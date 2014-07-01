using DI.FM.ViewModel;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DI.FM.View
{
    public sealed partial class FavoritePage : Page
    {
        private MainViewModel Model;

        public FavoritePage()
        {
            this.InitializeComponent();
            // Get the model
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            // Bind the model
            //this.DefaultViewModel.Add("Favorites", Model.FavoriteChannels);
            //this.DefaultViewModel.Add("NowPlaying", App.PlayingMedia);

            this.Loaded += (sender, e) =>
            {
                Model.LiveUpdateList.Clear();
                foreach (var item in Model.FavoriteChannels) Model.LiveUpdateList.Add(item);
            };
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            /*App.PlayingMedia.TogglePlayStop();*/
        }

        private void GridViewFavorites_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ChannelItem;
            if (item != null) this.Frame.Navigate(typeof(ChannelPage), item);
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


                foreach (ChannelItem item in GridViewFavorites1.SelectedItems) items.Add(item);

                foreach (ChannelItem item in GridViewFavorites.SelectedItems) items.Add(item);

            foreach (var item in items) Model.FavoriteChannels.Remove(item);
            await Model.SaveFavoriteChannels();
        }

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
             GridViewFavorites1.SelectAll();
             GridViewFavorites.SelectAll();
        }

        private void ButtonSelectNone_Click(object sender, RoutedEventArgs e)
        {
           GridViewFavorites1.SelectedItems.Clear();
            GridViewFavorites.SelectedItems.Clear();
        }
    }
}
