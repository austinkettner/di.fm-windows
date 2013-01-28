using DI.FM.View;
using DI.FM.ViewModel;
using GalaSoft.MvvmLight;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM
{
    sealed partial class App : Application
    {
        #region Variables

        public static MediaElement MediaPlayer;
        public static NowPlayingItem NowPlaying;

        public class NowPlayingItem : ViewModelBase
        {
            private MainViewModel.ChannelItem _playingItem;
            public MainViewModel.ChannelItem PlayingItem
            {
                get { return _playingItem; }
                set
                {
                    _playingItem = value;
                    if (_playingItem != null)
                    {
                        MediaPlayer.Source = new Uri(_playingItem.Streams[0]);
                        MediaControl.AlbumArt = new Uri("ms-appx:///" + _playingItem.Image.Substring(3));
                        MediaControl.TrackName = _playingItem.Name;
                        MediaControl.ArtistName = _playingItem.Description;
                        MediaControl.IsPlaying = true;
                    }
                    else
                    {
                        MediaPlayer.Source = null;
                        MediaControl.IsPlaying = false;
                    }

                    RaisePropertyChanged("PlayingItem");
                }
            }
        }

        #endregion

        #region Constructor

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            // Init the now playing item
            NowPlaying = new NowPlayingItem();
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
            await MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (MediaPlayer.CurrentState == MediaElementState.Playing)
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
            await MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayer.Source = new Uri(NowPlaying.PlayingItem.Streams[0]);
                MediaControl.IsPlaying = true;
            });
        }

        private async void MediaControl_PausePressed(object sender, object e)
        {
            await MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            });
        }

        private async void MediaControl_StopPressed(object sender, object e)
        {
            await MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            });
        }

        #endregion

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

            //MC.MetroGridHelper.MetroGridHelper.CreateGrid();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
