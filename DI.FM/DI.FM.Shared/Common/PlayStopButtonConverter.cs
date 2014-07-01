using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DI.FM.Common
{
    public class PlayStopButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool && !(bool)value) return App.Current.Resources["PlayIconButtonStyle"] as Style;
            return App.Current.Resources["StopIconButtonStyle"] as Style;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
