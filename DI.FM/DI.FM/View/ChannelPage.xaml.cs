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
        public MainViewModel Model;

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Model = e.Parameter as MainViewModel;
            CheckTrackPlayingState();
            this.DataContext = Model;
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            //MediaControl.AlbumArt = new Uri(Model.NowPlayingItem.ImageUrl);
            //MediaControl.TrackName = Model.NowPlayingItem.NowPlaying.Track;
            if (App.MediaPlayer.CurrentState == MediaElementState.Playing && Model.NowPlayingItem == App.PlayingItem)
            {
                ButtonPlay.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
                App.MediaPlayer.Source = null;
                App.PlayingItem = null;
            }
            else
            {
                ButtonPlay.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
                App.MediaPlayer.Source = new Uri(Model.NowPlayingItem.Streams[0]);
                App.PlayingItem = Model.NowPlayingItem;
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
            if (Model.NowPlayingItem != null && Model.NowPlayingItem == App.PlayingItem &&
                App.MediaPlayer.CurrentState == MediaElementState.Playing) 
                ButtonPlay.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
            else ButtonPlay.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
        }
    }
}
