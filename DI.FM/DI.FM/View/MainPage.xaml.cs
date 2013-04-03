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

            // Main model
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;

            Model.PropertyChanged += Model_PropertyChanged;

            // Load saved settings
            ToggleShuffle.IsChecked = (bool?)ApplicationData.Current.RoamingSettings.Values["ShuffleChannels"];

            this.Loaded += (sender, e) =>
            {
                var showLogin = ApplicationData.Current.LocalSettings.Values["ShowMainLogin"] as bool?;
                if (!showLogin.HasValue && Model.ListenKey == null) LoginFeature.Visibility = Windows.UI.Xaml.Visibility.Visible;
            };
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ListenKey")
            {
                if (Model.ListenKey != null) LoginFeature.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Model.LiveUpdateList.Clear();
            Model.LiveUpdateList.Add(Model.NowPlayingItem);
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
            if (ToggleShuffle.IsChecked == true)
            {
                ShuffleChannel();
            }
            else
            {
                if (Model.NowPlayingItem != null && Model.NowPlayingItem.Prev != null)
                {
                    Model.PlayChannel(Model.NowPlayingItem.Prev);
                }
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleShuffle.IsChecked == true)
            {
                ShuffleChannel();
            }
            else
            {
                if (Model.NowPlayingItem != null && Model.NowPlayingItem.Next != null)
                {
                    Model.PlayChannel(Model.NowPlayingItem.Next);
                }
            }
        }

        private void ShuffleChannel()
        {
            var random = new Random();
            var index = random.Next(0, Model.AllChannels.Count);
            Model.PlayChannel(Model.AllChannels[index]);

            Model.LiveUpdateList.Clear();
            Model.LiveUpdateList.Add(Model.NowPlayingItem);
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
                    Model.FavoriteChannels.Insert(0, favItem);
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
                    Model.FavoriteChannels.Remove(unFavItem);
                }
            }

            ButtonFavorite.Tag = null;

            TempFavorite.Clear();
            TempUnFavorite.Clear();

            GridViewFavorites.SelectedItems.Clear();
            GridViewChannels.SelectedItems.Clear();
            GridViewFavorites1.SelectedItems.Clear();
            GridViewChannels1.SelectedItems.Clear();

            await Model.SaveFavoriteChannels();
        }

        #endregion

        private void ButtonNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ChannelPage), Model.NowPlayingItem);
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
                this.Frame.Navigate(typeof(ChannelPage), item);
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
                GridViewFavorites1.SelectedItems.Clear();
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
            await Model.UpdateChannels();
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            App.ShowLoginWindow();
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            Model.TogglePlay();
        }

        private void ButtonHideLogin_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["ShowMainLogin"] = false;
            LoginFeature.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
