using DI.FM.Common;
using DI.FM.View;
using DI.FM.ViewModel;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.Data.Xml.Dom;
using Windows.Media;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM
{
    sealed partial class App : Application
    {
        #region Variables

        public static NowPlayingItem PlayingMedia;

        public class NowPlayingItem : BindableBase
        {
            public MediaElement MediaPlayer { get; set; }

            private ChannelItem _playingItem;
            public ChannelItem PlayingItem
            {
                get { return _playingItem; }
                set
                {
                    SetSilentNowPlayingItem(value);

                    if (MediaPlayer != null)
                    {
                        if (_playingItem != null && _playingItem.Streams.Count > 0)
                        {
                            MediaPlayer.Source = new Uri(_playingItem.Streams[0]);
                            MediaControl.AlbumArt = new Uri(_playingItem.Image);
                            MediaControl.TrackName = _playingItem.Name;
                            MediaControl.ArtistName = _playingItem.NowPlaying.Track;
                            MediaControl.IsPlaying = true;
                        }
                        else
                        {
                            MediaPlayer.Source = null;
                            MediaControl.IsPlaying = false;
                        }
                    }

                    // Save last played
                    if (_playingItem == null) ApplicationData.Current.LocalSettings.Values.Remove("LastPlayedChannel");
                    else ApplicationData.Current.LocalSettings.Values["LastPlayedChannel"] = _playingItem.Key;
                }
            }

            public void SetSilentNowPlayingItem(ChannelItem item)
            {
                _playingItem = item;
                OnPropertyChanged("PlayingItem");
                SetLiveTile(item);
            }

            public void TogglePlayStop()
            {
                if (MediaPlayer == null) return;

                if (MediaPlayer.CurrentState == MediaElementState.Playing)
                {
                    MediaPlayer.Source = null;
                    MediaControl.IsPlaying = false;
                }
                else
                {
                    if (PlayingItem != null) MediaPlayer.Source = new Uri(PlayingItem.Streams[0]);
                    MediaControl.IsPlaying = true;
                }
            }

            public static void SetLiveTile(ChannelItem channel)
            {
                var update = TileUpdateManager.CreateTileUpdaterForApplication();

                if (channel == null)
                {
                    update.Clear();
                    return;
                }

                var smallXml = SmallLiveTile(channel);
                var smallTile = new TileNotification(smallXml);
                smallTile.ExpirationTime = DateTime.Now + TimeSpan.FromMinutes(30);
                update.Update(smallTile);

                var wideXml = WideLiveTile(channel);
                var wideTile = new TileNotification(wideXml);
                wideTile.ExpirationTime = DateTime.Now + TimeSpan.FromMinutes(30);
                update.Update(wideTile);
            }

            private static XmlDocument SmallLiveTile(ChannelItem channel)
            {
                var tileTemplate = TileTemplateType.TileSquarePeekImageAndText02;
                var tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);

                // Set notification image
                XmlNodeList imgNodes = tileXml.GetElementsByTagName("image");
                imgNodes[0].Attributes[1].NodeValue = channel.Image;

                // Set notification text
                XmlNodeList textNodes = tileXml.GetElementsByTagName("text");
                textNodes[0].InnerText = channel.Name;
                textNodes[1].InnerText = channel.Description;

                return tileXml;
            }

            private static XmlDocument WideLiveTile(ChannelItem channel)
            {
                var tileTemplate = TileTemplateType.TileWideSmallImageAndText04;
                var tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);

                // Set notification image
                XmlNodeList imgNodes = tileXml.GetElementsByTagName("image");
                imgNodes[0].Attributes[1].NodeValue = channel.Image;

                // Set notification text
                XmlNodeList textNodes = tileXml.GetElementsByTagName("text");
                textNodes[0].InnerText = channel.Name;
                textNodes[1].InnerText = channel.Description;

                return tileXml;
            }
        }

        #endregion

        #region Constructor

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            // Init the now playing item
            PlayingMedia = new NowPlayingItem();
            // Init the windows 8 mini player
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
        }

        #endregion

        #region MediaButtons

        private async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await PlayingMedia.MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing)
                {
                    MediaControl_PausePressed(sender, e);
                    MediaControl.IsPlaying = false;
                }
                else
                {
                    MediaControl_PlayPressed(sender, e);
                    MediaControl.IsPlaying = true;
                }
            });
        }

        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await PlayingMedia.MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlayingMedia.MediaPlayer.Source = new Uri(PlayingMedia.PlayingItem.Streams[0]);
                MediaControl.IsPlaying = true;
            });
        }

        private async void MediaControl_PausePressed(object sender, object e)
        {
            await PlayingMedia.MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlayingMedia.MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            });
        }

        private async void MediaControl_StopPressed(object sender, object e)
        {
            await PlayingMedia.MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlayingMedia.MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            });
        }

        #endregion

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Intialize MarkedUp Analytics Client
            MarkedUp.AnalyticClient.Initialize("94e3584b-f3c5-4ef3-ac7b-383630ef6731");

            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Load all channels when first started
                var model = this.Resources["Locator"] as ViewModelLocator;
                await model.Main.LoadAllChannels();

                // Load last played channel when first started
                var channelKey = ApplicationData.Current.LocalSettings.Values["LastPlayedChannel"];
                if (channelKey != null)
                {
                    foreach (var channel in model.Main.AllChannels)
                    {
                        if (channel.Key.Equals(channelKey))
                        {
                            App.PlayingMedia.SetSilentNowPlayingItem(channel);
                            model.Main.NowPlayingItem = channel;
                            break;
                        }
                    }
                }

                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                Window.Current.Content = rootFrame;
            }

            rootFrame.Style = Resources["RootFrameStyle"] as Style;

            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            Window.Current.Activate();

            SearchPane.GetForCurrentView().SearchHistoryEnabled = false;

            // Init the charms options
            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;
        }

        private void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(onSettingsCommand);

            SettingsCommand cmd2 = new SettingsCommand("ID_2", "Privacy", handler);
            args.Request.ApplicationCommands.Add(cmd2);
            SettingsCommand cmd3 = new SettingsCommand("ID_3", "Support", handler);
            args.Request.ApplicationCommands.Add(cmd3);
        }

        private async void onSettingsCommand(IUICommand command)
        {
            switch (command.Id.ToString())
            {
                case "ID_2":
                    await Launcher.LaunchUriAsync(new Uri("http://www.quixby.com/privacy"));
                    break;
                case "ID_3":
                    await Launcher.LaunchUriAsync(new Uri("mailto:support@quixby.com?subject=Feedback on DI.FM for Windows 8"));
                    break;
                default:
                    break;
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            // TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
            // event in OnWindowCreated to speed up searches once the application is already running

            // If the Window isn't already using Frame navigation, insert our own Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // If the app does not contain a top-level frame, it is possible that this 
            // is the initial launch of the app. Typically this method and OnLaunched 
            // in App.xaml.cs can call a common method.
            if (frame == null)
            {
                // Create a Frame to act as the navigation context and associate it with
                // a SuspensionManager key
                frame = new Frame();
                DI.FM.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await DI.FM.Common.SuspensionManager.RestoreAsync();
                    }
                    catch (DI.FM.Common.SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }
            }

            if (!(frame.Content is SearchPage)) frame.Navigate(typeof(SearchPage), args.QueryText);
            Window.Current.Content = frame;

            // Ensure the current window is active
            Window.Current.Activate();
        }
    }
}
