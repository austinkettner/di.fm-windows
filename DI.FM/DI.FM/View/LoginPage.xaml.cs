using DI.FM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DI.FM.View
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            this.Loaded += (sender, e) =>
            {
                var val = ApplicationData.Current.LocalSettings.Values["ListenKey"];
                if(val != null) TextListenKey.Text = val.ToString();
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["ListenKey"] = TextListenKey.Text;

            var loc = App.Current.Resources["Locator"] as ViewModelLocator;
            loc.Main.GetIsPremium();
            loc.Main.ReUpdateChannelStreams();
        }

        private void ButtonRemoveKey_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Remove("ListenKey");
        }
    }
}
