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
using System.Linq;

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

        public IEnumerable<ChannelItem> MainFavoriteChannels
        {
            get { return this.FavoriteChannels.Take(5); }
        }

        private DispatcherTimer nowPlayingRefresh;

        private ChannelItem _nowPlayingItem;
        public ChannelItem NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set
            {
                _nowPlayingItem = value;
                if (_nowPlayingItem != null)
                {
                    NowPlayingRefresh_Tick(null, null);
                    nowPlayingRefresh.Start();
                }
                else
                {
                    nowPlayingRefresh.Stop();
                }

                RaisePropertyChanged("NowPlayingItem");
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

            private int _position;
            public int Position
            {
                get { return _position; }
                set
                {
                    _position = value;
                    RaisePropertyChanged("Position");
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
                FavoriteChannels.CollectionChanged += (sender, e) => { RaisePropertyChanged("MainFavoriteChannels"); };
                // Init the timer
                nowPlayingRefresh = new DispatcherTimer();
                nowPlayingRefresh.Interval = TimeSpan.FromSeconds(1);
                nowPlayingRefresh.Tick += NowPlayingRefresh_Tick;
                // Load the channels
                LoadAllChannels();
            }
        }

        private void NowPlayingRefresh_Tick(object sender, object e)
        {
            if (NowPlayingItem.NowPlaying == null || NowPlayingItem.NowPlaying.Started == -1)
            {
                nowPlayingRefresh.Stop();
                return;
            }

            var currentPosition = ConvertFromUnixTime(NowPlayingItem.NowPlaying.Started);
            if (currentPosition > NowPlayingItem.NowPlaying.Duration)
            {
                LoadTrackHistory(NowPlayingItem);
                //NowPlayingItem.NowPlaying.Started = -1;
                NowPlayingItem.NowPlaying.Position = 0;
            }
            else
            {
                NowPlayingItem.NowPlaying.Position = (int)currentPosition;
            }
        }

        #endregion

        #region Helpers

        private double ConvertFromUnixTime(long seconds)
        {
            var unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            unixTime = unixTime.AddSeconds(seconds).ToLocalTime();
            return DateTime.Now.Subtract(unixTime).TotalSeconds;
        }

        private async Task<string> DownloadJson(string url)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
            try { return await client.GetStringAsync(url); }
            catch { return null; }
        }

        #endregion

        #region Process

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

            await LoadFavoriteChannels();
        }

        private async Task LoadFavoriteChannels()
        {
            StorageFile file = null;
            try { file = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.txt"); }
            catch { }

            if (file != null)
            {
                var reader = new StreamReader(await file.OpenStreamForReadAsync());
                var array = await reader.ReadToEndAsync();

                foreach (var channel in AllChannels)
                {
                    if (array.Contains(channel.Key))
                    {
                        FavoriteChannels.Add(channel);
                    }
                }

                reader.Dispose();
            }
        }

        public async Task SaveFavoriteChannels()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("favorites.txt", CreationCollisionOption.OpenIfExists);

            var writer = new StreamWriter(await file.OpenStreamForWriteAsync());

            foreach (var channel in FavoriteChannels)
            {
                await writer.WriteLineAsync(channel.Key);
            }

            writer.Dispose();
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
            if (data == null) return;

           /* if (channel.TrackHistory == null) channel.TrackHistory = new ObservableCollection<TrackItem>();
            else channel.TrackHistory.Clear();*/

            var tempTracks = new List<TrackItem>();

            var index = 0;
            var tracks = JsonConvert.DeserializeObject(data) as dynamic;
            foreach (var track in tracks)
            {
                if (track["type"] == "track")
                {
                    tempTracks.Add(new TrackItem()
                    {
                        Index = index + 1,
                        Track = track["track"],
                        Started = track["started"],
                        Duration = track["duration"]
                    });
                    
                    if (index == 4) break;
                    index++;
                }
            }

            if (channel.TrackHistory != null && channel.TrackHistory.Count > 0 && tempTracks.Count > 0 && channel.TrackHistory[0].Started == tempTracks[0].Started)
            {
                channel.TrackHistory[0].Started = -1;
                return;
            }

            if (tempTracks.Count > 0)
            {
                channel.NowPlaying = tempTracks[0];
                channel.TrackHistory = new ObservableCollection<TrackItem>(tempTracks);
            }
        }

        #endregion
    }
}
