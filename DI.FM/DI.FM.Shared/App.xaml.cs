using DI.FM.Controls;
using DI.FM.View;
using DI.FM.ViewModel;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
            InitializeComponent();
            Suspending += OnSuspending;
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.Style = Resources["RootFrameStyle"] as Style;

                // Init and update the model
                Model = (Resources["Locator"] as ViewModelLocator).Main;
                await Model.CheckPremiumStatus();
                await Model.UpdateChannels();

                // When the frame is loaded set the model media player
                rootFrame.Loaded += (sender, e) =>
                {
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    Model.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
                };

                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }

                Window.Current.Activate();
            }

            if (!args.TileId.Equals("App"))
            {
                var channel = Model.AllChannels.FirstOrDefault(item => item.Key == args.TileId);
                if (channel != null) rootFrame.Navigate(typeof(ChannelPage), channel);
            }
        }

        private static void ShowAccountWindow()
        {

            var frame = Window.Current.Content as Frame;

            frame.Navigate(typeof(AccountPage));
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
                Model = (Resources["Locator"] as ViewModelLocator).Main;
                await Model.CheckPremiumStatus();
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
