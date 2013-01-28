using DI.FM.Common;
using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class MainPage : LayoutAwarePage
    {
        #region Variables

        public MainViewModel Model
        {
            get;
            set; //{ return this.DataContext as MainViewModel; }
        }

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();
            // Init the model
            var model = App.Current.Resources["Locator"] as ViewModelLocator;
            this.Model = model.Main;
            this.DefaultViewModel.Add("Model", model.Main);
            this.DefaultViewModel.Add("NowPlaying", App.NowPlaying);
            // Init the player
            this.Loaded += (sender, e) =>
            {
                if (App.MediaPlayer == null)
                {
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    App.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
                    App.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                }
            };
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

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.Model.NowPlayingItem = App.NowPlaying.PlayingItem;
        }

        #endregion

        #region Next/Prev/Shuffle

        private void ToggleShuffle_Click(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            VisualStateManager.GoToState(button, button.IsChecked.Value ? "Checked" : "Unchecked", false);
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleShuffle.IsChecked == true)
            {
                var random = new Random();
                var index = random.Next(0, this.Model.AllChannels.Count);
                App.NowPlaying.PlayingItem = this.Model.AllChannels[index];
            }
            else
            {
                var index = this.Model.AllChannels.IndexOf(App.NowPlaying.PlayingItem);
                if (index != -1 && index > 0) App.NowPlaying.PlayingItem = this.Model.AllChannels[index - 1];
            }

            this.Model.NowPlayingItem = App.NowPlaying.PlayingItem;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleShuffle.IsChecked == true)
            {
                var random = new Random();
                var index = random.Next(0, this.Model.AllChannels.Count);
                App.NowPlaying.PlayingItem = this.Model.AllChannels[index];
            }
            else
            {
                var index = this.Model.AllChannels.IndexOf(App.NowPlaying.PlayingItem);
                if (index != -1 && index < this.Model.AllChannels.Count - 1) App.NowPlaying.PlayingItem = this.Model.AllChannels[index + 1];
            }

            this.Model.NowPlayingItem = App.NowPlaying.PlayingItem;
        }

        #endregion

        #region ButtonFavorite

        private List<MainViewModel.ChannelItem> TempFavorite = new List<MainViewModel.ChannelItem>();
        private List<MainViewModel.ChannelItem> TempUnFavorite = new List<MainViewModel.ChannelItem>();

        private void ListViewChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (MainViewModel.ChannelItem item in e.AddedItems)
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

            foreach (MainViewModel.ChannelItem item in e.RemovedItems)
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
                    this.Model.FavoriteChannels.Add(favItem);
                }
            }
            else
            {
                for (int i = 0; i < TempUnFavorite.Count; i++)
                {
                    if (this.Model.FavoriteChannels.Remove(TempUnFavorite[i]))
                    {
                        i--;
                    }
                }
            }

            ButtonFavorite.Tag = null;

            TempFavorite.Clear();
            TempUnFavorite.Clear();

            ListViewFavorite.SelectedItems.Clear();
            ListViewAllChannels.SelectedItems.Clear();

            // Save to file
            await this.Model.SaveFavoriteChannels();
        }

        #endregion

        #region Others

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (!e.IsSourceZoomedInView)
            {
                var sz = sender as SemanticZoom;
                var zil = sz.ZoomedInView as GridView;
                var zol = sz.ZoomedOutView as GridView;
                var index = zol.Items.IndexOf(e.SourceItem.Item);
                e.DestinationItem.Item = zil.Items[index];

                Ad.Visibility = Visibility.Visible;
            }
            else
            {
                Ad.Visibility = Visibility.Collapsed;
            }
        }

        private void ListViewChannels_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as MainViewModel.ChannelItem;
            if (data != null)
            {
                this.Model.NowPlayingItem = data;
                this.Frame.Navigate(typeof(ChannelPage), this.Model);
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
                App.NowPlaying.PlayingItem = Model.NowPlayingItem;
            }
        }

        private void ButtonFavorites_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FavoritePage), this.Model);
        }

        #endregion
    }
}
