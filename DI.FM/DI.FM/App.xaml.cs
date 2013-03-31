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
        private MainViewModel Model;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Show the loading screen
            var extendedSplash = new ExtendedSplash(args.SplashScreen);
            Window.Current.Content = extendedSplash;
            Window.Current.Activate();

            // Init and update the model
            Model = (this.Resources["Locator"] as ViewModelLocator).Main;
            await Model.UpdateChannels();

            // Init the root frame
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) rootFrame = new Frame();
            rootFrame.Style = Resources["RootFrameStyle"] as Style;

            // When the frame is loaded set the model media player
            rootFrame.Loaded += (sender, e) =>
            {
                var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                Model.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
            };

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
            SettingsPane.GetForCurrentView().CommandsRequested += SettingsPage_CommandsRequested;

            // Intialize MarkedUp Analytics Client
            MarkedUp.AnalyticClient.Initialize("94e3584b-f3c5-4ef3-ac7b-383630ef6731");
        }

        private void SettingsPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(onSettingsCommand);

            if (Model.ListenKey == null)
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
                    var dialog = new MessageDialog("If you log out from your premium account you will not have access anymore to premium streams", "Logout from your DI.FM account");
                    dialog.Commands.Add(new UICommand("Logout") { Id = 1 });
                    dialog.Commands.Add(new UICommand("Stay logged in") { Id = 2 });
                    dialog.DefaultCommandIndex = 1;

                    var result = await dialog.ShowAsync();

                    if ((int)result.Id == 1)
                    {
                        Model.ListenKey = null;
                        Model.CheckPremiumStatus();
                        Model.UpdateChannelsStreams();
                    }
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
                    var grid = page.Content as Grid;
                    grid.Children.Add(new LoginPage());
                }
            }
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
                Model = (this.Resources["Locator"] as ViewModelLocator).Main;
                await Model.UpdateChannels();

                frame = new Frame();
                frame.Style = Resources["RootFrameStyle"] as Style;

                // When the frame is loaded set the model media player
                frame.Loaded += (sender, e) =>
                {
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    Model.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
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
