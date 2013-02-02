using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DI.FM.Common
{
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var duration = double.Parse(value.ToString());
            if (duration < 0) duration = 0;

            var time = TimeSpan.FromSeconds(duration);

            string min = time.Minutes.ToString();
            string sec = time.Seconds.ToString();

            if (min.Length == 1) min = "0" + min;
            if (sec.Length == 1) sec = "0" + sec;

            if (time.Hours == 0) return string.Format("{0}:{1}", min, sec);

            string hours = time.Hours.ToString();
            if (hours.Length == 1) hours = "0" + hours;

            return string.Format("{0}:{1}:{2}", hours, min, sec);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class FavoritesHeaderVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || ((IList<ChannelItem>)value).Count == 0) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
