using DI.FM.Common;
using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace DI.FM.View
{
    public sealed partial class ChannelPage : LayoutAwarePage
    {
        #region Properties

        public MainViewModel Model;

        #endregion

        #region Constructor

        public ChannelPage()
        {
            this.InitializeComponent();
            this.Loaded += (sender, e) => { App.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged; };
            this.Unloaded += (sender, e) => { App.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged; };
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (App.NowPlaying.PlayingItem == Model.NowPlayingItem)
            {
                if (App.MediaPlayer.CurrentState == MediaElementState.Playing)
                {
                    ButtonPlay.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
                }
                else
                {
                    ButtonPlay.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Model = e.Parameter as MainViewModel;
            CheckTrackPlayingState();
            this.DataContext = Model;
        }

        #endregion



        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (App.MediaPlayer.CurrentState == MediaElementState.Playing && Model.NowPlayingItem == App.NowPlaying.PlayingItem)
            {
                App.MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            }
            else
            {
                App.NowPlaying.PlayingItem = Model.NowPlayingItem;
            }
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            Model.NowPlayingItem = Model.NowPlayingItem.Prev;
            CheckTrackPlayingState();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            Model.NowPlayingItem = Model.NowPlayingItem.Next;
            CheckTrackPlayingState();
        }

        private void CheckTrackPlayingState()
        {
            if (Model.NowPlayingItem != null && Model.NowPlayingItem == App.NowPlaying.PlayingItem &&
                App.MediaPlayer.CurrentState == MediaElementState.Playing) 
                ButtonPlay.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
            else ButtonPlay.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
        }
    }
}
