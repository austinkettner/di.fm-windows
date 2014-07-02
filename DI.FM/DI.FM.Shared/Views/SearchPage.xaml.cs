using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using DI.FM.Common;
using DI.FM.FM.Models;
using DI.FM.ViewModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace DI.FM.View
{
    public sealed partial class SearchPage : Page
    {
        private string LastQuery = "";
        private ObservableCollection<ChannelItem> Results;
        private MainViewModel Model;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }
        public SearchPage()
        {
            InitializeComponent();
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            Results = new ObservableCollection<ChannelItem>(Model.AllChannels);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string queryText = e.Parameter as string;

            if (queryText != null)
            {
                Search(queryText.ToLower());
            }
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
                Frame.Navigate(typeof(ChannelPage), item);
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
