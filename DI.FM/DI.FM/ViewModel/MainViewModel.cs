using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using System.IO;
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;

namespace DI.FM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private const string SOURCE_URL = "http://api.audioaddict.com/v1/di/mobile/batch_update?stream_set_key=public3";
        private const string TRACK_URL = "http://api.audioaddict.com/v1/di/track_history/channel/{0}.json";
        private const string USER = "ephemeron";
        private const string PASS = "dayeiph0ne@pp";

        



        private ChannelItem _nowPlayingItem;
        public ChannelItem NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set
            {
                _nowPlayingItem = value;
                RaisePropertyChanged("NowPlayingItem");
            }
        }






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

        public class TrackItem : ObservableObject
        {
            private string _track;
            public string Track
            {
                get { return _track; }
                set
                {
                    _track = value;
                    RaisePropertyChanged("Track");
                }
            }

            private int _duration;
            public int Duration
            {
                get { return _duration; }
                set
                {
                    _duration = value;
                    RaisePropertyChanged("Duration");
                }
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


                    SaveAsync(new Uri(_imageUrl), ApplicationData.Current.LocalFolder, Name + ".jpg");
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

            private ObservableCollection<TrackItem> _trackHistory;
            public ObservableCollection<TrackItem> TrackHistory
            {
                get { return _trackHistory; }
                set
                {
                    _trackHistory = value;
                    RaisePropertyChanged("TrackHistory");
                }
            }

            private List<string> _streams;
            public List<string> Streams
            {
                get { return _streams; }
                set
                {
                    _streams = value;
                    RaisePropertyChanged("Streams");
                }
            }


            public async static void SaveAsync(Uri fileUri, StorageFolder folder, string fileName)
            {
                try
                {
                    var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
                    var downloader = new BackgroundDownloader();
                    var download = downloader.CreateDownload(fileUri, file);
                    var res = await download.StartAsync();
                }
                catch
                {
                }
            }
        }

        public MainViewModel()
        {
            if (IsInDesignMode) return;
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
                LoadChannelPl(item, json);
                AllChannels.Add(item);
                //FavoriteChannels.Add(item);
            }
        }

        private void LoadChannelPl(ChannelItem channel, dynamic json)
        {
            channel.Streams = new List<string>();
            var channels = json["streamlists"]["public3"]["channels"];
            foreach (var c in channels)
            {
                if (c["id"] == channel.ID)
                {
                    foreach(var st in c["streams"])
                    {
                        channel.Streams.Add(st["url"].ToString());
                    }
                    return;
                }
            }
        }

        private async void LoadChannelInfo(ChannelItem channel)
        {
            var data = await DownloadJson(string.Format(TRACK_URL, channel.ID));
            channel.TrackHistory = new ObservableCollection<TrackItem>();
            var tracks = JsonConvert.DeserializeObject(data) as dynamic;
            foreach (var track in tracks)
            {
                if (track["type"] == "track")
                {
                    channel.TrackHistory.Add(new TrackItem()
                    {
                        Track = track["track"],
                        Duration = track["duration"]
                    });
                    channel.NowPlaying = track["track"];
                    
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
