using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DI.FM.ViewModel
{
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

        public double StartedTime
        {
            get { return ConvertFromUnixTime(this.Started); }
        }

        private int _position;
        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                RaisePropertyChanged("Position");
                RaisePropertyChanged("Progress");
            }
        }

        public double Progress
        {
            get
            {
                return 100 * Position / (double)Duration;
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

        public static double ConvertFromUnixTime(long seconds)
        {
            var unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            unixTime = unixTime.AddSeconds(seconds).ToLocalTime();
            return DateTime.Now.Subtract(unixTime).TotalSeconds;
        }
    }

    public class ChannelItem : ObservableObject
    {
        // Channel info

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

        public void GetChannelInfo()
        {
        }

        // Channel theme

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

        // Channel streams

        public List<string> Streams;

        public void GetStreams()
        {
        }

        // Track history

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

        public void GetTrackHistory()
        {
        }
    }
}
