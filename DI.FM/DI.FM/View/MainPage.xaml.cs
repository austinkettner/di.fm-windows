using Callisto.Controls;
using DI.FM.Common;
using DI.FM.Controls;
using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class MainPage : LayoutAwarePage
    {
        private MainViewModel Model;

        public MainPage()
        {
            this.InitializeComponent();
            // Init the model
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            this.DefaultViewModel.Add("Model", Model);
            //this.DefaultViewModel.Add("NowPlaying", App.PlayingMedia);
            // Init the player
            /*this.Loaded += (sender, e) =>
            {
                if (App.PlayingMedia.MediaPlayer == null)
                {
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    App.PlayingMedia.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
                }
                App.PlayingMedia.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                MediaPlayer_CurrentStateChanged(null, null);
            };
            this.Unloaded += (sender, e) => { App.PlayingMedia.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged; };*/
            // Load saved settings
            ToggleShuffle.IsChecked = (bool?)ApplicationData.Current.RoamingSettings.Values["ShuffleChannels"];
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            //this.Model.NowPlayingItem = App.PlayingMedia.PlayingItem;
        }

        #region Next/Prev/Shuffle

        private void ToggleShuffle_Click(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            VisualStateManager.GoToState(button, button.IsChecked.Value ? "Checked" : "Unchecked", false);
            ApplicationData.Current.RoamingSettings.Values["ShuffleChannels"] = button.IsChecked;
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            /*if (ToggleShuffle.IsChecked == true)
            {
                ShuffleChannel();
            }
            else
            {
                var index = this.Model.AllChannels.IndexOf(App.PlayingMedia.PlayingItem);
                if (index != -1 && index > 0) App.PlayingMedia.PlayingItem = this.Model.AllChannels[index - 1];
            }

            this.Model.NowPlayingItem = App.PlayingMedia.PlayingItem;*/
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
           /* if (ToggleShuffle.IsChecked == true)
            {
                ShuffleChannel();
            }
            else
            {
                var index = this.Model.AllChannels.IndexOf(App.PlayingMedia.PlayingItem);
                if (index != -1 && index < this.Model.AllChannels.Count - 1) App.PlayingMedia.PlayingItem = this.Model.AllChannels[index + 1];
            }

            this.Model.NowPlayingItem = App.PlayingMedia.PlayingItem;*/
        }

        private void ShuffleChannel()
        {
            /*var random = new Random();
            var index = random.Next(0, this.Model.AllChannels.Count);
            App.PlayingMedia.PlayingItem = this.Model.AllChannels[index];*/
        }

        #endregion

        #region ButtonFavorite

        private List<ChannelItem> TempFavorite = new List<ChannelItem>();
        private List<ChannelItem> TempUnFavorite = new List<ChannelItem>();

        private void GridViewChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ChannelItem item in e.AddedItems)
            {
                if (this.Model.FavoriteChannels.Contains(item))
                {
                    TempUnFavorite.Add(item);
                }
                else
                {
                    TempFavorite.Add(item);
                }
            }

            foreach (ChannelItem item in e.RemovedItems)
            {
                TempUnFavorite.Remove(item);
                TempFavorite.Remove(item);
            }

            if (TempFavorite.Count > 0)
            {
                ButtonFavorite.Style = App.Current.Resources["FavoriteAppBarButtonStyle"] as Style;
                ButtonFavorite.Tag = true;
            }
            else if (TempUnFavorite.Count > 0)
            {
                ButtonFavorite.Style = App.Current.Resources["UnfavoriteAppBarButtonStyle"] as Style;
                ButtonFavorite.Tag = false;
            }

            if (TempFavorite.Count + TempUnFavorite.Count > 0)
            {
                StackSelectedOptions.Visibility = Visibility.Visible;
                this.BottomAppBar.IsSticky = true;
                this.BottomAppBar.IsOpen = true;
            }
            else
            {
                StackSelectedOptions.Visibility = Visibility.Collapsed;
                this.BottomAppBar.IsSticky = false;
                this.BottomAppBar.IsOpen = false;
            }
        }

        private async void ButtonFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonFavorite.Tag == null) return;

            if ((bool)ButtonFavorite.Tag)
            {
                foreach (var favItem in TempFavorite)
                {
                    this.Model.FavoriteChannels.Insert(0, favItem);
                }
            }
            else
            {
                List<ChannelItem> temp = new List<ChannelItem>();
                foreach (var unFavItem in TempUnFavorite)
                {
                    temp.Add(unFavItem);
                }
                foreach (var unFavItem in temp)
                {
                    this.Model.FavoriteChannels.Remove(unFavItem);
                }
            }

            ButtonFavorite.Tag = null;

            TempFavorite.Clear();
            TempUnFavorite.Clear();

            GridViewFavorites.SelectedItems.Clear();
            GridViewChannels.SelectedItems.Clear();

            await this.Model.SaveFavoriteChannels();
        }

        #endregion

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
           /* if (App.PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                ButtonPlayStop.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
            }
            else
            {
                ButtonPlayStop.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
            }

            ButtonPlayStop1.Style = ButtonPlayStop.Style;*/
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            //App.PlayingMedia.TogglePlayStop();
        }

        private void ButtonNowPlaying_Click(object sender, RoutedEventArgs e)
        {
           // Model.NowPlayingItem = App.PlayingMedia.PlayingItem;
            //this.Frame.Navigate(typeof(ChannelPage));
        }

        private void ButtonFavorites_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FavoritePage));
        }

        private void GridViewChannels_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ChannelItem;
            if (item != null)
            {
                Model.NowPlayingItem = item;
                this.Frame.Navigate(typeof(ChannelPage));
            }
        }

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (!e.IsSourceZoomedInView)
            {
                var semantic = sender as SemanticZoom;
                var view1 = semantic.ZoomedInView as GridView;
                var view2 = semantic.ZoomedOutView as GridView;
                var index = view2.Items.IndexOf(e.SourceItem.Item);
                e.DestinationItem.Item = view1.Items[index];

                Ad.Visibility = Visibility.Visible;
            }
            else
            {
                Ad.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonSelectNone_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                GridViewChannels1.SelectedItems.Clear();
            }
            else
            {
                GridViewFavorites.SelectedItems.Clear();
                GridViewChannels.SelectedItems.Clear();
            }
        }

        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new Flyout()
            {
                Content = new VolumeControl(),
                Placement = PlacementMode.Top,
                PlacementTarget = sender as UIElement,
                IsOpen = true
            };
        }

        private async void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.BottomAppBar.IsOpen = false;
            await Model.LoadAllChannels(true);
        }
    }
}
