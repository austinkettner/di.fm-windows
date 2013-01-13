using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System;
using Newtonsoft.Json;

namespace DI.FM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const string SOURCE_URL = "http://api.audioaddict.com/v1/di/mobile/batch_update?stream_set_key=public3";
        private const string TRACK_URL = "http://api.audioaddict.com/v1/di/track_history/channel/";
        private const string USER = "ephemeron";
        private const string PASS = "dayeiph0ne@pp";

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
            private int _id;
            public int ID
            {
                get { return _id; }
                set
                {
                    _id = value;
                    RaisePropertyChanged("ID");
                }
            }

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

            private string _description;
            public string Description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    RaisePropertyChanged("Description");
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
            var data = await DownloadJson(SOURCE_URL);

            var json = JsonConvert.DeserializeObject(data) as dynamic;
            var channels = json["channel_filters"][0]["channels"];
            foreach (var channel in channels)
            {
                var item = new ChannelItem()
                {
                    ID = channel["id"],
                    Name = channel["name"],
                    Description = channel["description"],
                    ImageUrl = channel["asset_url"]
                };
                LoadChannelInfo(item);
                AllChannels.Add(item);
                FavoriteChannels.Add(item);
            }
        }

        private async void LoadChannelInfo(ChannelItem channel)
        {
            var data = await DownloadJson(TRACK_URL + channel.ID + ".json");

            var tracks = JsonConvert.DeserializeObject(data) as dynamic;
            foreach (var track in tracks)
            {
                if (track["type"] == "track")
                {
                    channel.NowPlaying = track["track"];
                    break;
                }
            }
        }

        private async Task<string> DownloadJson(string url)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = CreateBasicHeader(USER, PASS);
            return await client.GetStringAsync(url);
        }

        public AuthenticationHeaderValue CreateBasicHeader(string username, string password)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(username + ":" + password);
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
}
