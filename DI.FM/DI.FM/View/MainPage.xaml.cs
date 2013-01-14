using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Search;
using Windows.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class MainPage : DI.FM.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();

            var search = SearchPane.GetForCurrentView();
            search.VisibilityChanged += search_VisibilityChanged;
            search.QueryChanged += MainPage_QueryChanged;


            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
        }

        void MediaControl_PlayPressed(object sender, object e)
        {
            MediaPlayer.Play();
        }

        void MediaControl_PausePressed(object sender, object e)
        {
            MediaPlayer.Pause();
        }

        void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            if (MediaPlayer.CurrentState == MediaElementState.Playing) MediaPlayer.Pause();
            else MediaPlayer.Play();
        }

        void MediaControl_StopPressed(object sender, object e)
        {
            MediaPlayer.Stop();
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
            if (data != null)
            {
                MediaControl.AlbumArt = new Uri("ms-appdata:///local/" + data.Name + ".jpg");
                MediaControl.TrackName = data.NowPlaying;
                MediaPlayer.Source = new Uri(data.Streams[0]);
            }
        }
    }
}
