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
    public sealed partial class App : Application
    {
        public static MainViewModel Main
        {
            get
            {
                return (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            }
        }

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

        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.Style = Resources["RootFrameStyle"] as Style;

                // Init and update the model
                await Main.CheckPremiumStatus();
                await Main.UpdateChannels();

                // When the frame is loaded set the model media player
                rootFrame.Loaded += (sender, e) =>
                {
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    Main.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
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
                var channel = Main.AllChannels.FirstOrDefault(item => item.Key == args.TileId);
                if (channel != null) rootFrame.Navigate(typeof(ChannelPage), channel);
            }
        }
    }
}
