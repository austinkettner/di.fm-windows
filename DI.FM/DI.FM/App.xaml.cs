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
using Windows.UI.Core;
using Windows.UI.Notifications;
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
                    _playingItem = value;

                    if (MediaPlayer != null)
                    {
                        if (_playingItem != null)
                        {
                            MediaPlayer.Source = new Uri(_playingItem.Streams[0]);
                            MediaControl.AlbumArt = new Uri(_playingItem.Image);
                            MediaControl.TrackName = _playingItem.Name;
                            MediaControl.ArtistName = _playingItem.Description;
                            MediaControl.IsPlaying = true;
                        }
                        else
                        {
                            MediaPlayer.Source = null;
                            MediaControl.IsPlaying = false;
                        }
                    }

                    OnPropertyChanged("PlayingItem");

                    // Save last played
                    if (_playingItem == null) ApplicationData.Current.RoamingSettings.Values.Remove("LastPlayedChannel");
                    else ApplicationData.Current.RoamingSettings.Values["LastPlayedChannel"] = _playingItem.Key;

                    // Set live tile
                    SetLiveTile(_playingItem);
                }
            }

            public void SetSilentNowPlayingItem(ChannelItem item)
            {
                _playingItem = item;
                OnPropertyChanged("PlayingItem");
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
                update.Update(new TileNotification(smallXml));

                var wideXml = WideLiveTile(channel);
                update.Update(new TileNotification(wideXml));
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
                textNodes[1].InnerText = channel.NowPlaying.Track;

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
                textNodes[1].InnerText = channel.NowPlaying.Track;

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
            MarkedUp.AnalyticClient.Initialize("1404b1d4-45d7-40dd-b259-d3ec3c0bb684");

            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Load all channels when first started
                var model = this.Resources["Locator"] as ViewModelLocator;
                await model.Main.LoadAllChannels();

                // Load last played channel when first started
                var channelKey = ApplicationData.Current.RoamingSettings.Values["LastPlayedChannel"];
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
