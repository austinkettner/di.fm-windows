using DI.FM.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DI.FM.View
{
    public sealed partial class SearchPage : DI.FM.Common.LayoutAwarePage
    {
        string lastQuery = "";
        ObservableCollection<MainViewModel.ChannelItem> results;

        MainViewModel model;

        public SearchPage()
        {
            this.InitializeComponent();

            var x = App.Current.Resources["Locator"] as ViewModelLocator;
            model = x.Main;

            results = new ObservableCollection<MainViewModel.ChannelItem>(model.AllChannels);

            this.DefaultViewModel.Add("Results", results);


            SearchPane.GetForCurrentView().QueryChanged += SearchPage_QueryChanged;
        }

        void SearchPage_QueryChanged(SearchPane sender, SearchPaneQueryChangedEventArgs args)
        {
            this.DefaultViewModel["QueryText"] = args.QueryText;
            Search(args.QueryText.ToLower());
            //results.Clear();
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var queryText = navigationParameter as string;
            if(queryText != null) Search(queryText.ToLower());

            /*var filterList = new List<Filter>();
            filterList.Add(new Filter("All", 0, true));

            // Communicate results through the view model
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            this.DefaultViewModel["Filters"] = filterList;
            this.DefaultViewModel["ShowFilters"] = filterList.Count > 1;*/
        }

        private void Search(string query)
        {
            if (query.Contains(lastQuery))
            {
                for (int i = 0; i < results.Count; i++)
                {
                    if (!results[i].Name.ToLower().Contains(query))
                    {
                        results.RemoveAt(i);
                        i--;
                    }
                }
            }
            else
            {
                foreach(var channel in model.AllChannels)
                {
                    if (channel.Name.ToLower().Contains(query) && !results.Contains(channel))
                    {
                        results.Add(channel);
                    }
                }
            }

            lastQuery = query;
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
