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
    public sealed partial class MainPage : DI.FM.Common.LayoutAwarePage
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
                    App.MediaPlayer.AudioCategory = AudioCategory.BackgroundCapableMedia;
                }
            };
        }

        private void search_VisibilityChanged(SearchPane sender, SearchPaneVisibilityChangedEventArgs args)
        {
            //args.Visible
        }

        private void MainPage_QueryChanged(SearchPane sender, SearchPaneQueryChangedEventArgs args)
        {

        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
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

        private void ListView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as DI.FM.ViewModel.MainViewModel.ChannelItem;
            if (data != null && this.Model.NowPlayingItem != data)
            {
                MediaControl.AlbumArt = new Uri("ms-appdata:///local/" + data.Name + ".jpg");
                MediaControl.TrackName = data.NowPlaying;
                App.MediaPlayer.Source = new Uri(data.Streams[0]);
                this.Model.NowPlayingItem = data;
            }
        }

        private void ButtonPlayPause_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (App.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                btn.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
                if (App.MediaPlayer.CanPause) App.MediaPlayer.Pause();
                else App.MediaPlayer.Stop();
            }
            else
            {
                btn.Style = App.Current.Resources["PauseIconButtonStyle"] as Style;
                App.MediaPlayer.Play();
            }
        }
    }
}
