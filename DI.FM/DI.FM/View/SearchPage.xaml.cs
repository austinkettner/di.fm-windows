using DI.FM.Common;
using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Search;
using Windows.UI.Xaml.Controls;

namespace DI.FM.View
{
    public sealed partial class SearchPage : LayoutAwarePage
    {
        private string LastQuery = "";
        private ObservableCollection<ChannelItem> Results;
        private MainViewModel Model;

        public SearchPage()
        {
            this.InitializeComponent();
            // Get model
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            // Init the initial result list
            Results = new ObservableCollection<ChannelItem>(Model.AllChannels);
            // Bind the model
            this.DefaultViewModel.Add("Results", Results);
            // Hook up events
            SearchPane.GetForCurrentView().QueryChanged += SearchPage_QueryChanged;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var queryText = navigationParameter as string;
            if (queryText != null)
            {
                this.DefaultViewModel["QueryText"] = queryText;
                Search(queryText.ToLower());
            }
        }

        private void SearchPage_QueryChanged(SearchPane sender, SearchPaneQueryChangedEventArgs args)
        {
            this.DefaultViewModel["QueryText"] = args.QueryText;
            Search(args.QueryText.ToLower());
        }

        private void Search(string query)
        {
            if (query.Contains(LastQuery))
            {
                for (int i = 0; i < Results.Count; i++)
                {
                    if (!Results[i].Name.ToLower().Contains(query))
                    {
                        Results.RemoveAt(i);
                        i--;
                    }
                }
            }
            else
            {
                foreach (var channel in Model.AllChannels)
                {
                    if (channel.Name.ToLower().Contains(query) && !Results.Contains(channel))
                    {
                        Results.Add(channel);
                    }
                }
            }

            LastQuery = query;
        }

        private void GridViewResults_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as ChannelItem;
            if (item != null)
            {
                this.Frame.Navigate(typeof(ChannelPage), item);
            }
        }
    }
}
