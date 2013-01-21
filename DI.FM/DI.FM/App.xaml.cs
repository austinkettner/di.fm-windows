using DI.FM.View;
using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DI.FM
{
    sealed partial class App : Application
    {
        public static MediaElement MediaPlayer;

        private static MainViewModel.ChannelItem _playingItem;
        public static MainViewModel.ChannelItem PlayingItem
        {
            get { return _playingItem; }
            set
            {
                _playingItem = value;
                if (_playingItem != null)
                {
                    MediaControl.AlbumArt = new Uri("ms-appx:///" + _playingItem.Image.Substring(3));
                    MediaControl.TrackName = _playingItem.Name;
                    MediaControl.ArtistName = _playingItem.Description;
                }
            }
        }

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            // Init the windows 8 player buttons
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
        }

        private async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await MediaPlayer.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (MediaPlayer.CurrentState == MediaElementState.Playing) MediaControl_PausePressed(sender, e);
                else MediaControl_PlayPressed(sender, e);
            });
        }

        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await MediaPlayer.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayer.Source = new Uri(PlayingItem.Streams[0]);
            });
        }

        private async void MediaControl_PausePressed(object sender, object e)
        {
            await MediaPlayer.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayer.Source = null;
            });
        }

        private async void MediaControl_StopPressed(object sender, object e)
        {
            await MediaPlayer.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayer.Source = null;
            });
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Intialize MarkedUp Analytics Client
            MarkedUp.AnalyticClient.Initialize("1404b1d4-45d7-40dd-b259-d3ec3c0bb684");

            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
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
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
