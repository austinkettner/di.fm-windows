using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace DI.FM.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Variables

        private DispatcherTimer nowPlayingRefresh;

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
            get { return this.FavoriteChannels.Take(3).ToList(); }
        }

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
                //LoadAllChannels();
            }
        }

        #endregion

        #region Load + Save

        public async Task LoadAllChannels(bool forceDownload = false)
        {
            AllChannels.Clear();
            FavoriteChannels.Clear();

            StorageFile file = null;
            try { file = await ApplicationData.Current.LocalFolder.GetFileAsync("channels.json"); }
            catch { }

            string data = null;

            if (file == null || forceDownload)
            {
                data = await ChannelsHelper.DownloadJson(ChannelsHelper.CHANNELS_URL);
                if (data != null)
                {
                    file = await ApplicationData.Current.LocalFolder.CreateFileAsync("channels.json", CreationCollisionOption.ReplaceExisting);
                    var writer = new StreamWriter(await file.OpenStreamForWriteAsync());
                    await writer.WriteAsync(data);
                    writer.Dispose();
                }
            }
            else
            {
                var reader = new StreamReader(await file.OpenStreamForReadAsync());
                data = await reader.ReadToEndAsync();
                reader.Dispose();
            }

            if (data == null) return;

            var channels = JsonConvert.DeserializeObject(data) as JContainer;

            foreach (var asset in ChannelsHelper.ChannelsAssets)
            {
                var item = new ChannelItem();
                item.Key = asset.Key;
                item.Image = asset.Value[0];
                item.Color1 = asset.Value[1];
                item.Color2 = asset.Value[2];

                var channel = channels.FirstOrDefault((element) => element["key"].Value<string>() == asset.Key);

                if (channel != null)
                {
                    item.ID = channel.Value<int>("id");
                    item.Name = channel.Value<string>("name");
                    item.Description = channel.Value<string>("description");
                }

                LoadChannelStreams(item);
                LoadTrackHistory(item);

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

        private async void LoadChannelStreams(ChannelItem channel)
        {
            channel.Streams = new List<string>();
            var data = await ChannelsHelper.DownloadJson(ChannelsHelper.CHANNELS_URL + "/" + channel.Key);

            if (data == null) return;

            var streams = JsonConvert.DeserializeObject(data) as JContainer;

            foreach (var stream in streams)
            {
                channel.Streams.Add(stream.ToObject<string>());
            }
        }

        private async void LoadTrackHistory(ChannelItem channel)
        {
            var data = await ChannelsHelper.DownloadJson(string.Format(ChannelsHelper.TRACK_URL, channel.ID));
            if (data == null) return;

            var tempTracks = new List<TrackItem>();

            var index = 0;
            var tracks = JsonConvert.DeserializeObject(data) as JContainer;

            foreach (var track in tracks)
            {
                if (track["type"].Value<string>() == "track")
                {
                    tempTracks.Add(new TrackItem()
                    {
                        Index = index + 1,
                        Track = track.Value<string>("track"),
                        Started = track.Value<long>("started"),
                        Duration = track.Value<int>("duration")
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

        private async Task LoadFavoriteChannels()
        {
            StorageFile file = null;

            try { file = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.txt"); }
            catch { }

            if (file != null)
            {
                using (var reader = new StreamReader(await file.OpenStreamForReadAsync()))
                {
                    var favorites = await reader.ReadToEndAsync();
                    var array = favorites.Split(';');

                    foreach (var channel in AllChannels)
                    {
                        if (array.Contains(channel.Key))
                        {
                            FavoriteChannels.Add(channel);
                        }
                    }
                }
            }
        }

        public async Task SaveFavoriteChannels()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("favorites.txt", CreationCollisionOption.ReplaceExisting);

            using (var writer = new StreamWriter(await file.OpenStreamForWriteAsync()))
            {
                foreach (var channel in FavoriteChannels)
                {
                    await writer.WriteAsync(channel.Key + ";");
                }

                await writer.FlushAsync();
            }
        }

        #endregion

        private void NowPlayingRefresh_Tick(object sender, object e)
        {
            if (NowPlayingItem.NowPlaying == null || NowPlayingItem.NowPlaying.Started == -1)
            {
                // Reload one more time if last reload was not successful
                LoadTrackHistory(NowPlayingItem);
                nowPlayingRefresh.Stop();
                return;
            }

            var currentPosition = NowPlayingItem.NowPlaying.StartedTime;
            if (currentPosition > NowPlayingItem.NowPlaying.Duration)
            {
                // Reload now playing if music ended and set position to maximum
                NowPlayingItem.NowPlaying.Position = NowPlayingItem.NowPlaying.Duration;
                LoadTrackHistory(NowPlayingItem);
            }
            else
            {
                NowPlayingItem.NowPlaying.Position = (int)currentPosition;
            }
        }
    }
}
