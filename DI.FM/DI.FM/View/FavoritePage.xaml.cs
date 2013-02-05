using DI.FM.Common;
using DI.FM.ViewModel;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class FavoritePage : LayoutAwarePage
    {
        private MainViewModel Model;

        public FavoritePage()
        {
            this.InitializeComponent();
            // Get the model
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            // Bind the model
            this.DefaultViewModel.Add("Favorites", Model.FavoriteChannels);
            this.DefaultViewModel.Add("NowPlaying", App.PlayingMedia);
            // Hook up media events
            this.Loaded += (sender, e) =>
            {
                MediaPlayer_CurrentStateChanged(null, null);
                App.PlayingMedia.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
            };
            this.Unloaded += (sender, e) => { App.PlayingMedia.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged; };
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (App.PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                ButtonPlayStop.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
            }
            else
            {
                ButtonPlayStop.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
            }
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            App.PlayingMedia.TogglePlayStop();
        }

        private void GridViewFavorites_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ChannelItem;
            if (item != null)
            {
                Model.NowPlayingItem = item;
                this.Frame.Navigate(typeof(ChannelPage));
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gridView = sender as GridView;

            if (gridView.SelectedItems.Count > 0)
            {
                ButtonUnfavorite.Visibility = Visibility.Visible;
                ButtonSelectNone.Visibility = Visibility.Visible;
                this.BottomAppBar.IsSticky = true;
                this.BottomAppBar.IsOpen = true;
            }
            else
            {
                ButtonUnfavorite.Visibility = Visibility.Collapsed;
                ButtonSelectNone.Visibility = Visibility.Collapsed;
                this.BottomAppBar.IsSticky = false;
                this.BottomAppBar.IsOpen = false;
            }
        }

        private async void ButtonUnfavorite_Click(object sender, RoutedEventArgs e)
        {
            List<ChannelItem> items = new List<ChannelItem>();
            
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                foreach (ChannelItem item in GridViewFavorites1.SelectedItems) items.Add(item);
            }
            else
            {
                foreach (ChannelItem item in GridViewFavorites.SelectedItems) items.Add(item);
            }

            foreach (var item in items) Model.FavoriteChannels.Remove(item);
            await Model.SaveFavoriteChannels();
        }

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.Value == ApplicationViewState.Snapped) GridViewFavorites1.SelectAll();
            else GridViewFavorites.SelectAll();
        }

        private void ButtonSelectNone_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.Value == ApplicationViewState.Snapped) GridViewFavorites1.SelectedItems.Clear();
            else GridViewFavorites.SelectedItems.Clear();
        }
    }
}
