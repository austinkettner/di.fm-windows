using System;
using Windows.UI.Xaml.Data;

namespace DI.FM.Common
{
    public class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var duration = double.Parse(value.ToString());

            if (duration < 0)
                duration = 0;

            return TimeSpan.FromSeconds(duration).ToString(@"hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}