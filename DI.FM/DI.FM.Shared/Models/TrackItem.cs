using System;
using DI.FM.Common;

namespace DI.FM.ViewModel
{
    public class TrackItem : BindableBase
    {
        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }

        private string _track;
        public string Track
        {
            get { return _track; }
            set
            {
                _track = value;
                OnPropertyChanged();
            }
        }

        private long _started;
        public long Started
        {
            get { return _started; }
            set
            {
                _started = value;
                OnPropertyChanged();
            }
        }

        public double StartedTime
        {
            get { return ConvertFromUnixTime(Started); }
        }

        private double _position;
        public double Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged();
            }
        }

        public double Progress
        {
            get
            {
                return 100 * Position / Duration;
            }
        }

        private double _duration;
        public double Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        public static double ConvertFromUnixTime(long seconds)
        {
            var unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            unixTime = unixTime.AddSeconds(seconds).ToLocalTime();
            return DateTime.Now.Subtract(unixTime).TotalSeconds;
        }
    }
}