using DI.FM.Common;
using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Search;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class MainPage : LayoutAwarePage
    {
        public MainViewModel Model
        {
            get { return this.DataContext as MainViewModel; }
        }

        public MainPage()
        {
            this.InitializeComponent();

            var search = SearchPane.GetForCurrentView();
            search.VisibilityChanged += search_VisibilityChanged;
            search.QueryChanged += MainPage_QueryChanged;

            this.Loaded += (sender, e) =>
            {
                if (App.MediaPlayer == null)
                {
                    var rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                    App.MediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
                    App.MediaPlayer.RealTimePlayback = true;
                    App.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                    App.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                    App.MediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
                }
            };
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            this.Model.NowPlayingItem = App.PlayingItem;
        }

        void MediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            bpp.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
        }

        void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            bpp.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
        }

        void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            bpp.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
        }


        private void search_VisibilityChanged(SearchPane sender, SearchPaneVisibilityChangedEventArgs args)
        {
            //args.Visible
        }

        private void MainPage_QueryChanged(SearchPane sender, SearchPaneQueryChangedEventArgs args)
        {

        }

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            //this.Frame.Navigate(typeof(FavoritePage));
            if (!e.IsSourceZoomedInView)
            {
                var sz = sender as SemanticZoom;
                var zil = sz.ZoomedInView as GridView;
                var zol = sz.ZoomedOutView as GridView;
                var index = zol.Items.IndexOf(e.SourceItem.Item);
                e.DestinationItem.Item = zil.Items[index];
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
            var btn = sender as Button;
            if (App.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                btn.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
                App.MediaPlayer.Source = null;
            }
            else
            {
                btn.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
                App.MediaPlayer.Source = new Uri(this.Model.NowPlayingItem.Streams[0]);
            }
        }

        private void ButtonFavorites_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FavoritePage), this.Model);
        }

        private List<MainViewModel.ChannelItem> TempFavorite = new List<MainViewModel.ChannelItem>();
        private List<MainViewModel.ChannelItem> TempUnFavorite = new List<MainViewModel.ChannelItem>();

        private void ListViewChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as MainViewModel.ChannelItem;

                if (this.Model.FavoriteChannels.Contains(item))
                {
                    if (!TempUnFavorite.Contains(item))
                    {
                        TempUnFavorite.Add(item);
                    }
                }
                else
                {
                    if (!TempFavorite.Contains(item))
                    {
                        TempFavorite.Add(item);
                    }
                }
            }

            if (e.RemovedItems.Count > 0)
            {
                var item = e.RemovedItems[0] as MainViewModel.ChannelItem;
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
                for(int i=0;i<TempUnFavorite.Count;i++)
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
    }
}
