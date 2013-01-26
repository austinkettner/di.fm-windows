using DI.FM.ViewModel;
using System;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class FavoritePage : DI.FM.Common.LayoutAwarePage
    {
        public FavoritePage()
        {
            this.InitializeComponent();
            // Init the model
            var model = App.Current.Resources["Locator"] as ViewModelLocator;
            this.DefaultViewModel.Add("Favorites", model.Main.FavoriteChannels);
            this.DefaultViewModel.Add("NowPlaying", App.NowPlaying);

            App.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (App.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                bpp.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
            }
            else
            {
                bpp.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
            }
        }

        private void ButtonPlayPause_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                App.MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            }
            else
            {
                if (App.NowPlaying.PlayingItem != null) App.MediaPlayer.Source = new Uri(App.NowPlaying.PlayingItem.Streams[0]);
                MediaControl.IsPlaying = true;
            }
        }

        private void GridViewFavorites_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MainViewModel.ChannelItem;
            var model = App.Current.Resources["Locator"] as ViewModelLocator;
            if (item != null && model != null)
            {
                model.Main.NowPlayingItem = item;
                this.Frame.Navigate(typeof(ChannelPage), model.Main);
            }
        }
    }
}
