using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace DI.FM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Constants

        private const string SOURCE_URL = "http://api.audioaddict.com/v1/di/mobile/batch_update?stream_set_key=public3";
        private const string TRACK_URL = "http://api.audioaddict.com/v1/di/track_history/channel/{0}.json";
        private const string USER = "ephemeron";
        private const string PASS = "dayeiph0ne@pp";

        #endregion

        #region Properties

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

        private DispatcherTimer nowPlayingRefresh;

        private ChannelItem _nowPlayingItem;
        public ChannelItem NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set
            {
                if (value != null)
                {
                    if (_nowPlayingItem != value)
                    {
                        _nowPlayingItem = value;
                        nowPlayingRefresh.Start();
                        NowPlayingPosition = 0;
                    }
                }
                else
                {
                    nowPlayingRefresh.Stop();
                }

                RaisePropertyChanged("NowPlayingItem");
            }
        }

        private double _nowPlayingPosition;
        public double NowPlayingPosition
        {
            get { return _nowPlayingPosition; }
            set
            {
                _nowPlayingPosition = value;
                RaisePropertyChanged("NowPlayingPosition");
            }
        }

        #endregion

        #region Classes

        public class TrackItem : ObservableObject
        {
            private int _index;
            public int Index
            {
                get { return _index; }
                set
                {
                    _index = value;
                    RaisePropertyChanged("Index");
                }
            }

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

            private long _started;
            public long Started
            {
                get { return _started; }
                set
                {
                    _started = value;
                    RaisePropertyChanged("Started");
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
                    _imageUrl = "ms-appdata:///local/" + Name + ".jpg";
                    SaveAsync(new Uri(value), ApplicationData.Current.LocalFolder, Name + ".jpg");
                }
            }

            private TrackItem _nowPlaying;
            public TrackItem NowPlaying
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


            public async void SaveAsync(Uri fileUri, StorageFolder folder, string fileName)
            {
                try
                {
                    var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.FailIfExists);
                    var downloader = new BackgroundDownloader();
                    var download = downloader.CreateDownload(fileUri, file);
                    var res = await download.StartAsync();
                }
                catch { }

                RaisePropertyChanged("ImageUrl");
            }

            private ChannelItem _prev;
            public ChannelItem Prev
            {
                get { return _prev; }
                set
                {
                    _prev = value;
                    RaisePropertyChanged("Prev");
                }
            }

            private ChannelItem _next;
            public ChannelItem Next
            {
                get { return _next; }
                set
                {
                    _next = value;
                    RaisePropertyChanged("Next");
                }
            }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            if (!IsInDesignMode)
            {
                // Init the arrays
                AllChannels = new ObservableCollection<ChannelItem>();
                FavoriteChannels = new ObservableCollection<ChannelItem>();
                // Init the timer
                nowPlayingRefresh = new DispatcherTimer();
                nowPlayingRefresh.Interval = TimeSpan.FromSeconds(1);
                nowPlayingRefresh.Tick += nowPlayingRefresh_Tick;
                // Load the channels
                LoadAllChannels();
            }
        }

        #endregion




        private double ConvertFromUnixTime(long seconds)
        {
            var unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            unixTime = unixTime.AddSeconds(seconds).ToLocalTime();
            return DateTime.Now.Subtract(unixTime).TotalSeconds;
        }


        void nowPlayingRefresh_Tick(object sender, object e)
        {
            var currentPosition = ConvertFromUnixTime(NowPlayingItem.NowPlaying.Started);
            if (currentPosition > NowPlayingItem.NowPlaying.Duration)
            {
                //refresh
                LoadTrackHistory(NowPlayingItem);
                currentPosition = 0;
            }
            NowPlayingPosition = currentPosition;
        }

        private async void LoadAllChannels()
        {
            StorageFile file = null;
            try { file = await ApplicationData.Current.LocalFolder.GetFileAsync("channels.json"); }
            catch { }

            var data = "";


            if (file == null)
            {
                data = await DownloadJson(SOURCE_URL);
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync("channels.json");
                var writer = new StreamWriter(await file.OpenStreamForWriteAsync());
                await writer.WriteAsync(data);
                writer.Dispose();
            }
            else
            {
                var reader = new StreamReader(await file.OpenStreamForReadAsync());
                data = await reader.ReadToEndAsync();
                reader.Dispose();
            }

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
                LoadTrackHistory(item);
                LoadChannelStreams(item, json);
                AllChannels.Add(item);
                //FavoriteChannels.Add(item);
            }

            if (AllChannels.Count > 2)
            {
                AllChannels[0].Next = AllChannels[1];
                AllChannels[AllChannels.Count - 1].Prev = AllChannels[AllChannels.Count - 2];
            }

            for (int i = 1; i < AllChannels.Count - 1; i++)
            {
                AllChannels[i].Prev = AllChannels[i - 1];
                AllChannels[i].Next = AllChannels[i + 1];
            }
        }

        private void LoadChannelStreams(ChannelItem channel, dynamic json)
        {
            channel.Streams = new List<string>();
            var channels = json["streamlists"]["public3"]["channels"];

            foreach (var c in channels)
            {
                if (c["id"] == channel.ID)
                {
                    foreach (var st in c["streams"])
                    {
                        channel.Streams.Add(st["url"].ToString());
                    }
                    return;
                }
            }
        }

        private async void LoadTrackHistory(ChannelItem channel)
        {
            var data = await DownloadJson(string.Format(TRACK_URL, channel.ID));

            if (channel.TrackHistory == null) channel.TrackHistory = new ObservableCollection<TrackItem>();
            else channel.TrackHistory.Clear();

            var index = 0;
            var tracks = JsonConvert.DeserializeObject(data) as dynamic;
            foreach (var track in tracks)
            {
                if (track["type"] == "track")
                {
                    channel.TrackHistory.Add(new TrackItem()
                    {
                        Index = index + 1,
                        Track = track["track"],
                        Started = track["started"],
                        Duration = track["duration"]
                    });
                    if(index == 0) channel.NowPlaying = channel.TrackHistory[0];
                    if (index == 4) break;
                    index++;
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
