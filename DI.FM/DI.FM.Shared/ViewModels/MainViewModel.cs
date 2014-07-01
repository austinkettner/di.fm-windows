using System.Text;
using Windows.ApplicationModel;
using Windows.Media;
using Windows.Storage.Streams;
using DI.FM.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace DI.FM.ViewModel
{
    public class MainViewModel : BindableBase
    {

        public List<ChannelItem> LiveUpdateList = new List<ChannelItem>();
        private DispatcherTimer LiveUpdateTimer;
        private SystemMediaTransportControls systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
        private int StreamIndex;

        private MediaElement _mediaPlayer;
        public MediaElement MediaPlayer
        {
            get { return _mediaPlayer; }
            set
            {
                _mediaPlayer = value;
                _mediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                _mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            }
        }

        private ObservableCollection<ChannelItem> _allChannels;
        public ObservableCollection<ChannelItem> AllChannels
        {
            get { return _allChannels; }
            set
            {
                _allChannels = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ChannelItem> _favoriteChannels;
        public ObservableCollection<ChannelItem> FavoriteChannels
        {
            get { return _favoriteChannels; }
            set
            {
                _favoriteChannels = value;
                OnPropertyChanged();
                if (_favoriteChannels != null)
                {
                    _favoriteChannels.CollectionChanged += (sender, e) =>
                    {
                        HasFavorites = FavoriteChannels.Count > 0;
                        OnPropertyChanged();
                    };
                }
            }
        }

        private bool _hasFavorites;
        public bool HasFavorites
        {
            get { return _hasFavorites; }
            set
            {
                _hasFavorites = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<ChannelItem> TopFavoriteChannels
        {
            get { return FavoriteChannels.Take(6).ToList(); }
        }

        private ChannelItem _nowPlayingItem;
        public ChannelItem NowPlayingItem
        {
            get { return _nowPlayingItem; }
            set
            {
                _nowPlayingItem = value;
                OnPropertyChanged();
                if (_nowPlayingItem != null)
                {
                    ApplicationData.Current.LocalSettings.Values["LastPlayedChannel"] = _nowPlayingItem.Key;
                }
            }
        }

        private bool _isBuffering;
        public bool IsBuffering
        {
            get { return _isBuffering; }
            set
            {
                if (_isBuffering != value)
                {
                    _isBuffering = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged();
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                }
            }
        }

        public string ListenKey
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values["ListenKey"] as string;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["ListenKey"] = value;
                OnPropertyChanged(); 
                CheckPremiumStatusSync();
            }
        }

        private bool _isPremium;
        public bool IsPremium
        {
            get { return _isPremium; }
            set
            {
                _isPremium = value;
                OnPropertyChanged();
                UpdateChannelsStreams();
            }
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get { return _isUpdating; }
            set
            {
                _isUpdating = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                CreateEmptyChannels();
                LoadFavoriteChannels();

                LiveUpdateTimer = new DispatcherTimer();
                LiveUpdateTimer.Interval = TimeSpan.FromSeconds(1);
                LiveUpdateTimer.Tick += NowPlayingRefresh_Tick;
                LiveUpdateTimer.Start();

                systemMediaControls.ButtonPressed += ButtonPressed;
                systemMediaControls.IsPlayEnabled = true;
                systemMediaControls.IsPauseEnabled = true;
                systemMediaControls.IsStopEnabled = true;
                systemMediaControls.IsNextEnabled = true;
                systemMediaControls.IsPreviousEnabled = true;
                systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
        }

        private async void ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            await MediaPlayer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        PlayChannel(NowPlayingItem);
                        MediaPlayer.Play();
                        break;

                    case SystemMediaTransportControlsButton.Pause:
                        MediaPlayer.Pause();
                        break;

                    case SystemMediaTransportControlsButton.Stop:
                        MediaPlayer.Stop();
                        break;

                    case SystemMediaTransportControlsButton.Next:
                        if (NowPlayingItem != null && NowPlayingItem.Prev != null)
                            PlayChannel(NowPlayingItem.Prev);
                        break;

                    case SystemMediaTransportControlsButton.Previous:
                        if (NowPlayingItem != null && NowPlayingItem.Next != null)
                            PlayChannel(NowPlayingItem.Next);
                        break;
                }

            });
        }

        #region Load + Save

        private async void LoadTrackHistory(ChannelItem channel)
        {
            channel.IsRefreshing = true;

            var data = await ChannelsHelper.DownloadJson(string.Format(ChannelsHelper.TRACK_HISTORY_URL, channel.ID));
            if (data == null)
            {
                channel.IsRefreshing = false;
                return;
            }

            TrackItem nowPl = null;
            var tempTracks = new List<TrackItem>();

            var index = 0;
            var tracks = JsonConvert.DeserializeObject(data) as JContainer;

            foreach (var track in tracks)
            {
                if (track["type"].Value<string>() == "track")
                {
                    var item = new TrackItem
                    {
                        Index = index,
                        Track = track.Value<string>("track"),
                        Started = track.Value<long>("started"),
                        Duration = track.Value<int>("length")
                    };

                    if (nowPl == null) nowPl = item;
                    else tempTracks.Add(item);

                    if (index == 5) break;
                    index++;
                }
            }

            channel.TrackHistory = new ObservableCollection<TrackItem>(tempTracks);

            if (channel.NowPlaying != null && nowPl != null && channel.NowPlaying.Started == nowPl.Started)
            {
                // Extend with 1 minute if now playing is the same
                channel.NowPlaying.Duration += channel.NowPlaying.StartedTime + 10;
            }
            else
            {
                channel.NowPlaying = nowPl;
            }

            channel.IsRefreshing = false;
        }

        private async void LoadFavoriteChannels()
        {
            FavoriteChannels = new ObservableCollection<ChannelItem>();

            StorageFile file = null;

            try { file = await ApplicationData.Current.LocalFolder.GetFileAsync("favorites.txt"); }
            catch { }

            if (file != null)
            {
                using (var reader = new StreamReader(await file.OpenStreamForReadAsync()))
                {
                    var favorites = await reader.ReadToEndAsync();
                    var channels = favorites.Split(';');

                    foreach (var item in AllChannels)
                    {
                        if (channels.Contains(item.Key))
                        {
                            FavoriteChannels.Add(item);
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

        #region RefreshStates

        private void NowPlayingRefresh_Tick(object sender, object e)
        {
            foreach (var item in LiveUpdateList)
            {
                // The item is null or it's refreshing -> ignore
                if (item == null || item.IsRefreshing) continue;

                if (item.NowPlaying == null || item.TrackHistory == null || item.TrackHistory.Count == 0)
                {
                    // Reload if now playing is null or no history is available
                    LoadTrackHistory(item);
                    continue;
                }

                var currentPosition = item.NowPlaying.StartedTime;
                if (currentPosition > item.NowPlaying.Duration)
                {
                    // The channel position > channel duration -> reload track history
                    item.NowPlaying.Position = item.NowPlaying.Duration;
                    LoadTrackHistory(item);
                }
                else
                {
                    // Update the current position
                    item.NowPlaying.Position = (int)currentPosition;
                }
            }
        }

        private async void MediaPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (NowPlayingItem != null)
            {
                StreamIndex++;

                if (StreamIndex < NowPlayingItem.Streams.Count)
                {
                    MediaPlayer.Source = new Uri(NowPlayingItem.Streams[StreamIndex]);
                }
                else
                {
                    var msg = new MessageDialog("Could not connect on any of available streams!", "Cannot play channel");
                    await msg.ShowAsync();
                }
            }
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            IsBuffering = MediaPlayer.CurrentState == MediaElementState.Buffering ||
                          MediaPlayer.CurrentState == MediaElementState.Opening;

            switch (MediaPlayer.CurrentState)
            {
                case MediaElementState.Paused:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;

                case MediaElementState.Playing:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    IsPlaying = true;
                    break;

                case MediaElementState.Stopped:
                    systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
            }
        }

        #endregion

        #region Public

        public void PlayChannel(ChannelItem channel)
        {
            if (channel != null && channel.Streams.Count > 0)
            {
                StreamIndex = 0;
                NowPlayingItem = channel;
                MediaPlayer.Source = new Uri(channel.Streams[0]);

                SystemMediaTransportControlsDisplayUpdater updater = systemMediaControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Music;
                updater.MusicProperties.AlbumArtist = channel.NowPlaying.Track;
                updater.MusicProperties.Title = channel.Name;
                updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(channel.Image));

                updater.Update();
                SetLiveTile(channel);
            }
        }

        public void TogglePlay()
        {
            if (MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                MediaPlayer.Source = null;
            }
            else
            {
                PlayChannel(NowPlayingItem);
            }
        }

        #endregion

        #region Channels + API

        private void CreateEmptyChannels()
        {
            AllChannels = new ObservableCollection<ChannelItem>();

            int i = 0;

            foreach (var item in ChannelsHelper.ChannelsAssets)
            {
                var chn = new ChannelItem
                {
                    Key = item.Key,
                    Image = item.Value[0],
                    Color1 = item.Value[1],
                    Color2 = item.Value[2],
                    Streams = new List<string>()
                };

                if (i > 0)
                {
                    chn.Prev = AllChannels[AllChannels.Count - 1];
                    AllChannels[AllChannels.Count - 1].Next = chn;
                }

                AllChannels.Add(chn);
                i++;
            }

            // Get load the last 
            var channelKey = ApplicationData.Current.LocalSettings.Values["LastPlayedChannel"];
            if (channelKey != null)
            {
                foreach (var channel in AllChannels)
                {
                    if (channel.Key.Equals(channelKey))
                    {
                        NowPlayingItem = channel;
                        break;
                    }
                }
            }
        }

        public async Task UpdateChannels()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.Authorization = CreateBasicHeader(ChannelsHelper.BATCH_USER, ChannelsHelper.BATCH_PASS);

            var format = GetStreamFormat();
            var url = string.Format(ChannelsHelper.BATCH_UPDATE_URL, format);

            string data = null;
            try { data = await client.GetStringAsync(url); }
            catch { }

            if (data != null)
            {
                var token = JsonConvert.DeserializeObject(data) as JContainer;
                await UpdateChannelsInfo(token["channel_filters"].First["channels"]);
                UpdateChannelsStreams(token["streamlists"], format);
                UpdateChannelsTrack(token["track_history"]);
            }
        }

        private string GetStreamFormat()
        {
            bool found = false;
            var format = ApplicationData.Current.LocalSettings.Values["StreamFormat"];

            if (IsPremium)
            {
                if (format == null) return ChannelsHelper.PremiumStreamFormats[0][1];

                foreach (var i in ChannelsHelper.PremiumStreamFormats)
                {
                    if (i[1] == format.ToString())
                    {
                        found = true;
                        break;
                    }
                }

                if (found) return format.ToString();
                return ChannelsHelper.PremiumStreamFormats[0][1];
            }
            else
            {
                if (format == null) return ChannelsHelper.FreeStreamFormats[0][1];

                foreach (var i in ChannelsHelper.FreeStreamFormats)
                {
                    if (i[1] == format.ToString())
                    {
                        found = true;
                        break;
                    }
                }

                if (found) return format.ToString();
                return ChannelsHelper.FreeStreamFormats[0][1];
            }
        }

        private Task UpdateChannelsInfo(JToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                foreach (var item in AllChannels)
                {
                    var jChannel = token.FirstOrDefault((e) => e.Value<string>("key") == item.Key);
                    if (jChannel != null)
                    {
                        item.ID = jChannel.Value<int>("id");
                        item.Name = jChannel.Value<string>("name");
                        item.Description = jChannel.Value<string>("description");
                    }
                }
            });
        }

        public void UpdateChannelsStreams(JToken token, string key)
        {
            var jSelection = token[key];

            if (jSelection != null)
            {
                var jChannels = jSelection["channels"];

                foreach (var item in AllChannels)
                {
                    item.Streams.Clear();

                    var jChannel = jChannels.FirstOrDefault((e) => e.Value<int>("id") == item.ID);
                    if (jChannel == null) continue;

                    var jUrls = jChannel["streams"];
                    if (jUrls == null) continue;

                    foreach (var url in jUrls)
                    {
                        if (IsPremium) item.Streams.Add(url.Value<string>("url") + "?" + ListenKey);
                        else item.Streams.Add(url.Value<string>("url"));
                    }
                }
            }
        }

        private void UpdateChannelsTrack(JToken token)
        {
            foreach (var item in AllChannels)
            {
                var jChannel = token.FirstOrDefault((e) => e.First.Value<int>("channel_id") == item.ID).First;

                if (jChannel != null)
                {
                    item.NowPlaying = new TrackItem
                    {
                        Track = jChannel.Value<string>("track"),
                        Started = jChannel.Value<int>("started"),
                        Duration = jChannel.Value<int>("length")
                    };
                }
            }
        }

        public async void UpdateChannelsStreams()
        {
            IsUpdating = true;

            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.Authorization = CreateBasicHeader(ChannelsHelper.BATCH_USER, ChannelsHelper.BATCH_PASS);

            var format = GetStreamFormat();
            var url = string.Format(ChannelsHelper.BATCH_UPDATE_URL, format);

            string data = null;
            try { data = await client.GetStringAsync(url); }
            catch { }

            if (data != null)
            {
                var token = JsonConvert.DeserializeObject(data) as JContainer;
                UpdateChannelsStreams(token["streamlists"], format);
            }

            IsUpdating = false;
        }

        public AuthenticationHeaderValue CreateBasicHeader(string username, string password)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(username + ":" + password);
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        #endregion

        #region Premium

        private string[] PremiumURLs = { "http://prem2.di.fm:80/deeptech", "http://prem1.di.fm:80/deeptech", "http://prem4.di.fm:80/deeptech", "http://prem2.di.fm:80/darkdnb", "http://prem4.di.fm:80/darkdnb", "http://prem1.di.fm:80/darkdnb", "http://prem2.di.fm:80/liquiddubstep", "http://prem1.di.fm:80/liquiddubstep", "http://prem4.di.fm:80/liquiddubstep", "http://prem2.di.fm:80/glitchhop", "http://prem1.di.fm:80/glitchhop", "http://prem4.di.fm:80/glitchhop", "http://prem2.di.fm:80/trance", "http://prem1.di.fm:80/trance", "http://prem4.di.fm:80/trance", "http://prem2.di.fm:80/classiceurodisco", "http://prem1.di.fm:80/classiceurodisco", "http://prem4.di.fm:80/classiceurodisco", "http://prem2.di.fm:80/vocaltrance", "http://prem1.di.fm:80/vocaltrance", "http://prem4.di.fm:80/vocaltrance", "http://prem2.di.fm:80/chillout", "http://prem1.di.fm:80/chillout", "http://prem4.di.fm:80/chillout", "http://prem2.di.fm:80/progressive", "http://prem1.di.fm:80/progressive", "http://prem4.di.fm:80/progressive", "http://prem2.di.fm:80/lounge", "http://prem1.di.fm:80/lounge", "http://prem4.di.fm:80/lounge", "http://prem2.di.fm:80/house", "http://prem1.di.fm:80/house", "http://prem4.di.fm:80/house", "http://prem2.di.fm:80/vocalchillout", "http://prem1.di.fm:80/vocalchillout", "http://prem4.di.fm:80/vocalchillout", "http://prem2.di.fm:80/minimal", "http://prem1.di.fm:80/minimal", "http://prem4.di.fm:80/minimal", "http://prem2.di.fm:80/harddance", "http://prem1.di.fm:80/harddance", "http://prem4.di.fm:80/harddance", "http://prem2.di.fm:80/electrohouse", "http://prem1.di.fm:80/electrohouse", "http://prem4.di.fm:80/electrohouse", "http://prem2.di.fm:80/eurodance", "http://prem1.di.fm:80/eurodance", "http://prem4.di.fm:80/eurodance", "http://prem2.di.fm:80/techhouse", "http://prem1.di.fm:80/techhouse", "http://prem4.di.fm:80/techhouse", "http://prem2.di.fm:80/psychill", "http://prem1.di.fm:80/psychill", "http://prem4.di.fm:80/psychill", "http://prem2.di.fm:80/goapsy", "http://prem1.di.fm:80/goapsy", "http://prem4.di.fm:80/goapsy", "http://prem2.di.fm:80/progressivepsy", "http://prem4.di.fm:80/progressivepsy", "http://prem1.di.fm:80/progressivepsy", "http://prem2.di.fm:80/hardcore", "http://prem1.di.fm:80/hardcore", "http://prem4.di.fm:80/hardcore", "http://prem2.di.fm:80/djmixes", "http://prem1.di.fm:80/djmixes", "http://prem4.di.fm:80/djmixes", "http://prem2.di.fm:80/ambient", "http://prem1.di.fm:80/ambient", "http://prem4.di.fm:80/ambient", "http://prem2.di.fm:80/drumandbass", "http://prem1.di.fm:80/drumandbass", "http://prem4.di.fm:80/drumandbass", "http://prem2.di.fm:80/classicelectronica", "http://prem1.di.fm:80/classicelectronica", "http://prem4.di.fm:80/classicelectronica", "http://prem2.di.fm:80/epictrance", "http://prem4.di.fm:80/epictrance", "http://prem1.di.fm:80/epictrance", "http://prem2.di.fm:80/ukgarage", "http://prem4.di.fm:80/ukgarage", "http://prem1.di.fm:80/ukgarage", "http://prem2.di.fm:80/cosmicdowntempo", "http://prem1.di.fm:80/cosmicdowntempo", "http://prem4.di.fm:80/cosmicdowntempo", "http://prem2.di.fm:80/breaks", "http://prem1.di.fm:80/breaks", "http://prem4.di.fm:80/breaks", "http://prem2.di.fm:80/techno", "http://prem1.di.fm:80/techno", "http://prem4.di.fm:80/techno", "http://prem2.di.fm:80/soulfulhouse", "http://prem1.di.fm:80/soulfulhouse", "http://prem4.di.fm:80/soulfulhouse", "http://prem2.di.fm:80/deephouse", "http://prem4.di.fm:80/deephouse", "http://prem1.di.fm:80/deephouse", "http://prem2.di.fm:80/tribalhouse", "http://prem1.di.fm:80/tribalhouse", "http://prem4.di.fm:80/tribalhouse", "http://prem2.di.fm:80/funkyhouse", "http://prem1.di.fm:80/funkyhouse", "http://prem4.di.fm:80/funkyhouse", "http://prem2.di.fm:80/deepnudisco", "http://prem1.di.fm:80/deepnudisco", "http://prem4.di.fm:80/deepnudisco", "http://prem2.di.fm:80/spacemusic", "http://prem1.di.fm:80/spacemusic", "http://prem4.di.fm:80/spacemusic", "http://prem2.di.fm:80/hardstyle", "http://prem1.di.fm:80/hardstyle", "http://prem4.di.fm:80/hardstyle", "http://prem2.di.fm:80/chilloutdreams", "http://prem1.di.fm:80/chilloutdreams", "http://prem4.di.fm:80/chilloutdreams", "http://prem2.di.fm:80/liquiddnb", "http://prem1.di.fm:80/liquiddnb", "http://prem4.di.fm:80/liquiddnb", "http://prem2.di.fm:80/classiceurodance", "http://prem1.di.fm:80/classiceurodance", "http://prem4.di.fm:80/classiceurodance", "http://prem2.di.fm:80/handsup", "http://prem1.di.fm:80/handsup", "http://prem4.di.fm:80/handsup", "http://prem2.di.fm:80/club", "http://prem1.di.fm:80/club", "http://prem4.di.fm:80/club", "http://prem2.di.fm:80/classictrance", "http://prem1.di.fm:80/classictrance", "http://prem4.di.fm:80/classictrance", "http://prem2.di.fm:80/classicvocaltrance", "http://prem1.di.fm:80/classicvocaltrance", "http://prem4.di.fm:80/classicvocaltrance", "http://prem2.di.fm:80/dubstep", "http://prem1.di.fm:80/dubstep", "http://prem4.di.fm:80/dubstep", "http://prem2.di.fm:80/clubdubstep", "http://prem4.di.fm:80/clubdubstep", "http://prem1.di.fm:80/clubdubstep", "http://prem2.di.fm:80/discohouse", "http://prem1.di.fm:80/discohouse", "http://prem4.di.fm:80/discohouse", "http://prem2.di.fm:80/futuresynthpop", "http://prem1.di.fm:80/futuresynthpop", "http://prem4.di.fm:80/futuresynthpop", "http://prem2.di.fm:80/latinhouse", "http://prem1.di.fm:80/latinhouse", "http://prem4.di.fm:80/latinhouse", "http://prem2.di.fm:80/oldschoolacid", "http://prem1.di.fm:80/oldschoolacid", "http://prem4.di.fm:80/oldschoolacid", "http://prem2.di.fm:80/chiptunes", "http://prem1.di.fm:80/chiptunes", "http://prem4.di.fm:80/chiptunes" };

        public async void CheckPremiumStatusSync()
        {
            await CheckPremiumStatus();
        }

        public async Task CheckPremiumStatus()
        {
            var key = ListenKey;

            if (key == null)
            {
                IsPremium = false;
                return;
            }

            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

            int i = 0, maxRetry = 3;
            var rand = new Random();

            while (i < maxRetry)
            {
                var url = PremiumURLs[rand.Next(0, PremiumURLs.Length - 1)] + "?" + key;

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    var response = await request.GetResponseAsync();
                    IsPremium = true;
                    break;
                }
                catch
                {
                    IsPremium = false;
                    i++;
                }
            }
        }

        #endregion

        #region LiveTile

        public static void SetLiveTile(ChannelItem channel)
        {
            var update = TileUpdateManager.CreateTileUpdaterForApplication();

            if (channel == null)
            {
                update.Clear();
                return;
            }

            var smallXml = SmallLiveTile(channel);
            if (smallXml != null)
            {
                var smallTile = new TileNotification(smallXml);
                smallTile.ExpirationTime = DateTime.Now + TimeSpan.FromMinutes(30);
                update.Update(smallTile);
            }

            var wideXml = WideLiveTile(channel);
            if (wideXml != null)
            {
                var wideTile = new TileNotification(wideXml);
                wideTile.ExpirationTime = DateTime.Now + TimeSpan.FromMinutes(30);
                update.Update(wideTile);
            }
        }

        private static XmlDocument SmallLiveTile(ChannelItem channel)
        {
            try
            {
                var tileTemplate = TileTemplateType.TileSquarePeekImageAndText02;
                var tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);

                // Set notification image
                XmlNodeList imgNodes = tileXml.GetElementsByTagName("image");
                imgNodes[0].Attributes[1].NodeValue = channel.Image;

                // Set notification text
                XmlNodeList textNodes = tileXml.GetElementsByTagName("text");
                textNodes[0].InnerText = channel.Name;
                textNodes[1].InnerText = channel.Description;

                return tileXml;
            }
            catch { return null; }
        }

        private static XmlDocument WideLiveTile(ChannelItem channel)
        {
            try
            {
                var tileTemplate = TileTemplateType.TileWideSmallImageAndText04;
                var tileXml = TileUpdateManager.GetTemplateContent(tileTemplate);

                // Set notification image
                XmlNodeList imgNodes = tileXml.GetElementsByTagName("image");
                imgNodes[0].Attributes[1].NodeValue = channel.Image;

                // Set notification text
                XmlNodeList textNodes = tileXml.GetElementsByTagName("text");
                textNodes[0].InnerText = channel.Name;
                textNodes[1].InnerText = channel.Description;

                return tileXml;
            }
            catch { return null; }
        }

        #endregion
    }
}
