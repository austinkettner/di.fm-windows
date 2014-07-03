using System.ComponentModel;
using Windows.UI.Xaml.Navigation;
using DI.FM.Controls;
using DI.FM.FM.Models;
using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;

namespace DI.FM.View
{
    public sealed partial class MainPage : Page
    {
        MainViewModel Model;

        public MainPage()
        {
            InitializeComponent();

            // Main model
            Model = App.Main;

            Model.PropertyChanged += Model_PropertyChanged;

            // Load saved settings
            ToggleShuffle.IsChecked = (bool?)ApplicationData.Current.RoamingSettings.Values["ShuffleChannels"];

            Loaded += (sender, e) =>
            {
                var showLogin = ApplicationData.Current.LocalSettings.Values["ShowMainLogin"] as bool?;
                if (!showLogin.HasValue && Model.ListenKey == null) LoginFeature.Visibility = Visibility.Visible;
                ButtonLogin.Visibility = Model.ListenKey == null ? Visibility.Visible : Visibility.Collapsed;
            };
        }

        void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ListenKey")
            {
                if (Model.ListenKey != null) LoginFeature.Visibility = Visibility.Collapsed;
                ButtonLogin.Visibility = Model.ListenKey == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Model.LiveUpdateList.Clear();
            Model.LiveUpdateList.Add(Model.NowPlayingItem);
        }

        #region Next/Previous/Shuffle

        private void ToggleShuffle_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.RoamingSettings.Values["ShuffleChannels"] = ToggleShuffle.IsChecked;
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleShuffle.IsChecked == true)
            {
                ShuffleChannel();
            }
            else
            {
                if (Model.NowPlayingItem != null && Model.NowPlayingItem.Previous != null)
                {
                    Model.PlayChannel(Model.NowPlayingItem.Previous);
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

        private IList<ChannelItem> TempFavorite = new List<ChannelItem>();
        private IList<ChannelItem> TempUnFavorite = new List<ChannelItem>();

        private void GridViewChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ChannelItem item in e.AddedItems)
            {
                if (Model.FavoriteChannels.Contains(item))
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
                ButtonFavorite.Icon = new SymbolIcon(Symbol.Favorite);
                ButtonFavorite.Visibility = Visibility.Visible;
                ButtonFavorite.Label = "Favorite";
                ButtonFavorite.Tag = true;
            }
            else if (TempUnFavorite.Count > 0)
            {
                ButtonFavorite.Icon = new SymbolIcon(Symbol.UnFavorite);
                ButtonFavorite.Label = "UnFavorite";
                ButtonFavorite.Tag = false;
            }

            if (TempFavorite.Count + TempUnFavorite.Count > 0)
            {
                ButtonFavorite.Visibility = Visibility.Visible;
                BottomAppBar.IsSticky = true;
                BottomAppBar.IsOpen = true;
            }
            else
            {
                ButtonFavorite.Visibility = Visibility.Collapsed;
                BottomAppBar.IsSticky = false;
                BottomAppBar.IsOpen = false;
            }
        }

        private void ButtonFavorite_Click(object sender, RoutedEventArgs e)
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
                Model.FavoriteChannels.Remove(TempUnFavorite[0]);
            }

            ButtonFavorite.Tag = null;
            TempFavorite.Clear();
            TempUnFavorite.Clear();
            GridViewFavorites.SelectedItems.Clear();
            GridViewChannels.SelectedItems.Clear();
            GridViewFavorites1.SelectedItems.Clear();
            GridViewChannels1.SelectedItems.Clear();
            Model.SaveFavoriteChannels();
        }

        #endregion

        private void ButtonNowPlaying_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ChannelPage), Model.NowPlayingItem);
        }

        private void ButtonFavorites_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(FavoritePage));
        }

        private void GridViewChannels_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ChannelItem;
            if (item != null)
            {
                Frame.Navigate(typeof(ChannelPage), item);
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
            }
        }

        private void ButtonSelectNone_Click(object sender, RoutedEventArgs e)
        {

            GridViewFavorites1.SelectedItems.Clear();
            GridViewChannels1.SelectedItems.Clear();

            GridViewFavorites.SelectedItems.Clear();
            GridViewChannels.SelectedItems.Clear();

        }

        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new Flyout();
            flyout.Content = new VolumeControl();
            flyout.Placement = FlyoutPlacementMode.Top;
            flyout.ShowAt(sender as FrameworkElement);
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginPopup.IsOpen = true;
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            Model.TogglePlay();
        }

        private void ButtonHideLogin_Click(object sender, RoutedEventArgs e)
        {
            BottomAppBar.IsOpen = false;
            ApplicationData.Current.LocalSettings.Values["ShowMainLogin"] = false;
            LoginFeature.Visibility = Visibility.Collapsed;
        }

        private void ButtonAccount_Click(object sender, RoutedEventArgs e)
        {
            AccountPopup.IsOpen = true;
        }
    }
}
