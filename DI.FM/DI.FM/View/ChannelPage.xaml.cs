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
    public sealed partial class ChannelPage : DI.FM.Common.LayoutAwarePage
    {
        public MainViewModel Model;

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Model = e.Parameter as MainViewModel;
            this.DataContext = Model;
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            //MediaControl.AlbumArt = new Uri(Model.NowPlayingItem.ImageUrl);
            //MediaControl.TrackName = Model.NowPlayingItem.NowPlaying.Track;
            App.MediaPlayer.Source = new Uri(Model.NowPlayingItem.Streams[0]);
            App.PlayingItem = Model.NowPlayingItem;
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            Model.NowPlayingItem = Model.NowPlayingItem.Prev;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            Model.NowPlayingItem = Model.NowPlayingItem.Next;
        }
    }
}
