using GalaSoft.MvvmLight;
using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace DI.FM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const string MAIN_URL = "http://www.di.fm/";

        private ObservableCollection<ChannelItem> _allChannels;
        public ObservableCollection<ChannelItem> AllChannels
        {
            get { return _allChannels; }
            set
            {
                _allChannels = value;
                RaisePropertyChanged("AllChannels");
            }
        }

        private ObservableCollection<ChannelItem> _favoriteChannels;
        public ObservableCollection<ChannelItem> FavoriteChannels
        {
            get { return _favoriteChannels; }
            set
            {
                _favoriteChannels = value;
                RaisePropertyChanged("FavoriteChannels");
            }
        }

        public class ChannelItem : ObservableObject
        {
            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    RaisePropertyChanged("Name");
                }
            }

            private string _imageUrl;
            public string ImageUrl
            {
                get { return _imageUrl; }
                set
                {
                    _imageUrl = value;
                    RaisePropertyChanged("ImageUrl");
                }
            }

            private string _nowPlaying;
            public string NowPlaying
            {
                get { return _nowPlaying; }
                set
                {
                    _nowPlaying = value;
                    RaisePropertyChanged("NowPlaying");
                }
            }
        }

        public MainViewModel()
        {
            AllChannels = new ObservableCollection<ChannelItem>();
            FavoriteChannels = new ObservableCollection<ChannelItem>();
            LoadAllChannels();
        }

        private async void LoadAllChannels()
        {
            var html = await DownloadHtml();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var items = doc.GetElementbyId("channels").Descendants("li");

            foreach (var item in items)
            {
                var channel = new ChannelItem();
                channel.ImageUrl = item.Descendants("a").FirstOrDefault().Descendants("img").FirstOrDefault().GetAttributeValue("src", "");
                channel.Name = item.Descendants("p").Where(tr => tr.GetAttributeValue("class", "").Equals("channel")).FirstOrDefault().InnerText;
                //var x = item.Descendants("p").Where(tr => tr.GetAttributeValue("class", "").Equals("track")).FirstOrDefault();
                AllChannels.Add(channel);
            }

            for (int i = 0; i < 5; i++)
            {
                FavoriteChannels.Add(AllChannels[i]);
            }
        }

        private async Task<string> DownloadHtml()
        {
            var client = new HttpClient();
            return await client.GetStringAsync(MAIN_URL);
        }
    }
}