using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace DI.FM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Constants

        private const string CHANNELS_URL = "http://listen.di.fm/public3";
        private const string TRACK_URL = "http://api.v2.audioaddict.com/v1/di/track_history/channel/{0}.json";

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
                        nowPlayingRefresh.Start();
                        NowPlayingPosition = 0;
                    }
                }
                else
                {
                    nowPlayingRefresh.Stop();
                }

                _nowPlayingItem = value;
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
            private string _image;
            public string Image
            {
                get { return _image; }
                set
                {
                    _image = value;
                    RaisePropertyChanged("Image");
                }
            }

            private string _color1;
            public string Color1
            {
                get { return _color1; }
                set
                {
                    _color1 = value;
                    RaisePropertyChanged("Color1");
                }
            }

            private string _color2;
            public string Color2
            {
                get { return _color2; }
                set
                {
                    _color2 = value;
                    RaisePropertyChanged("Color2");
                }
            }



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

            private string _key;
            public string Key
            {
                get { return _key; }
                set
                {
                    _key = value;
                    RaisePropertyChanged("Key");
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
                data = await DownloadJson(CHANNELS_URL);
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

            var channels = JsonConvert.DeserializeObject(data) as dynamic;

            foreach (var channel in channels)
            {
                var item = new ChannelItem()
                {
                    ID = channel["id"],
                    Key = channel["key"],
                    Name = channel["name"],
                    Description = channel["description"]
                };

                var assets = ChannelsHelper.ChannelsAssets[item.Key];
                item.Image = assets[0];
                item.Color1 = assets[1];
                item.Color2 = assets[2];

                LoadTrackHistory(item);
                LoadChannelStreams(item);
                AllChannels.Add(item);

                if(FavoriteChannels.Count < 5) FavoriteChannels.Add(item);
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

        private async void LoadChannelStreams(ChannelItem channel)
        {
            channel.Streams = new List<string>();
            var data = await DownloadJson(CHANNELS_URL + "/" + channel.Key);
            var streams = JsonConvert.DeserializeObject(data) as dynamic;

            foreach (var c in streams)
            {
                channel.Streams.Add(c.ToString());
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
                    if (index == 0) channel.NowPlaying = channel.TrackHistory[0];
                    if (index == 4) break;
                    index++;
                }
            }
        }

        private async Task<string> DownloadJson(string url)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
            return await client.GetStringAsync(url);
        }
    }
}
