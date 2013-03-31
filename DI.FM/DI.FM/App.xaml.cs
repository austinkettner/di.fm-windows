using DI.FM.Controls;
using DI.FM.View;
using DI.FM.ViewModel;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM
{
    sealed partial class App : Application
    {
        #region Variables

        /*public static NowPlayingItem PlayingMedia;

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
                if (smallXml != null)
                {
                    var smallTile = new TileNotification(smallXml);
                    smallTile.ExpirationTime = DateTime.Now + TimeSpan.FromMinutes(30);
                    update.Update(smallTile);
                }

                var wideXml = WideLiveTile(channel);
                if (wideXml != null)
                {
                    var wideTile = new TileNotification(wideXml);
                    wideTile.ExpirationTime = DateTime.Now + TimeSpan.FromMinutes(30);
                    update.Update(wideTile);
                }
            }

            private static XmlDocument SmallLiveTile(ChannelItem channel)
            {
                try
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
                catch { return null; }
            }

            private static XmlDocument WideLiveTile(ChannelItem channel)
            {
                try
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
                catch { return null; }
            }
        }*/

        #endregion

        #region Constructor

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        #endregion

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Show the loading screen
            var extendedSplash = new ExtendedSplash(args.SplashScreen);
            Window.Current.Content = extendedSplash;
            Window.Current.Activate();

            // Intialize MarkedUp Analytics Client
            MarkedUp.AnalyticClient.Initialize("94e3584b-f3c5-4ef3-ac7b-383630ef6731");

            // Init and update the model
            var model = this.Resources["Locator"] as ViewModelLocator;
            await model.Main.UpdateChannels();

            // Get the root frame
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) rootFrame = new Frame();

            // Set the frame style
            rootFrame.Style = Resources["RootFrameStyle"] as Style;

            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Remove the loading screen and set the frame as content
            Window.Current.Content = rootFrame;
            Window.Current.Activate();

            // Init the charms options
            SearchPane.GetForCurrentView().SearchHistoryEnabled = false;
            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;

            // When the frame is loaded set the model media player
            rootFrame.Loaded += (sender, e) =>
            {
                var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                model.Main.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
            };
        }

        private void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(onSettingsCommand);

            var model = this.Resources["Locator"] as ViewModelLocator;
            if (model.Main.ListenKey == null)
            {
                // No licence, not logged in (even if it's not premium)
                SettingsCommand cmd1 = new SettingsCommand("ID_1", "Login", handler);
                args.Request.ApplicationCommands.Add(cmd1);
            }
            else
            {
                SettingsCommand cmd2 = new SettingsCommand("ID_2", "Logout", handler);
                args.Request.ApplicationCommands.Add(cmd2);
            }

            SettingsCommand cmd3 = new SettingsCommand("ID_3", "Privacy", handler);
            args.Request.ApplicationCommands.Add(cmd3);
            SettingsCommand cmd4 = new SettingsCommand("ID_4", "Support", handler);
            args.Request.ApplicationCommands.Add(cmd4);
        }

        private async void onSettingsCommand(IUICommand command)
        {
            switch (command.Id.ToString())
            {
                case "ID_1":
                    ShowLoginWindow();
                    break;
                case "ID_2":
                    var locator = this.Resources["Locator"] as ViewModelLocator;
                    locator.Main.ListenKey = null;
                    locator.Main.CheckPremiumStatus();
                    locator.Main.GetChannelsStresms();
                    break;
                case "ID_3":
                    await Launcher.LaunchUriAsync(new Uri("http://www.quixby.com/privacy"));
                    break;
                case "ID_4":
                    await Launcher.LaunchUriAsync(new Uri("mailto:support@quixby.com?subject=Feedback on DI.FM for Windows 8"));
                    break;
                default:
                    break;
            }
        }

        public static void ShowLoginWindow()
        {
            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                var page = frame.Content as Page;
                if (page != null)
                {
                    var login = new LoginPage();
                    var grid = page.Content as Grid;
                    grid.Children.Add(login);
                }
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override async void OnSearchActivated(SearchActivatedEventArgs args)
        {
            var frame = Window.Current.Content as Frame;

            if (frame == null)
            {
                // Show the loading screen
                var extendedSplash = new ExtendedSplash(args.SplashScreen);
                Window.Current.Content = extendedSplash;
                Window.Current.Activate();

                // Init and update the model
                var model = this.Resources["Locator"] as ViewModelLocator;
                await model.Main.UpdateChannels();

                frame = new Frame();
                frame.Style = Resources["RootFrameStyle"] as Style;

                // When the frame is loaded set the model media player
                frame.Loaded += (sender, e) =>
                {
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    model.Main.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
                };
            }

            if (!(frame.Content is SearchPage))
            {
                frame.Navigate(typeof(SearchPage), args.QueryText);
            }

            Window.Current.Content = frame;
            Window.Current.Activate();
        }
    }
}
