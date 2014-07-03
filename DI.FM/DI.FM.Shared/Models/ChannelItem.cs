using System.Collections.Generic;
using System.Collections.ObjectModel;
using DI.FM.Common;

namespace DI.FM.FM.Models
{
    public class ChannelItem : BindableBase
    {
        private int _id;
        public int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _key;
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _image;
        public string Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        private string _color1;
        public string Color1
        {
            get { return _color1; }
            set
            {
                _color1 = value;
                OnPropertyChanged();
            }
        }

        private string _color2;
        public string Color2
        {
            get { return _color2; }
            set
            {
                _color2 = value;
                OnPropertyChanged();
            }
        }

        private ChannelItem _previous;
        public ChannelItem Previous
        {
            get { return _previous; }
            set
            {
                _previous = value;
                OnPropertyChanged();
            }
        }

        private ChannelItem _next;
        public ChannelItem Next
        {
            get { return _next; }
            set
            {
                _next = value;
                OnPropertyChanged();
            }
        }

        public List<string> Streams;

        private TrackItem _nowPlaying;
        public TrackItem NowPlaying
        {
            get { return _nowPlaying; }
            set
            {
                _nowPlaying = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TrackItem> _trackHistory;
        public ObservableCollection<TrackItem> TrackHistory
        {
            get { return _trackHistory; }
            set
            {
                _trackHistory = value;
                OnPropertyChanged();
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }
    }
}
