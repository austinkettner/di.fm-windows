using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DI.FM.View
{
    public sealed partial class FavoritePage : DI.FM.Common.LayoutAwarePage
    {
        public FavoritePage()
        {
            this.InitializeComponent();
        }

        private void ButtonPlayPause_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (App.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                btn.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
                App.MediaPlayer.Source = null;
            }
            else
            {
                btn.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
             //   App.MediaPlayer.Source = new Uri(this.Model.NowPlayingItem.Streams[0]);
            }
        }
    }
}
