using DI.FM.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            /*ApplicationData.Current.LocalSettings.Values["ListenKey"] = TextListenKey.Text;

            var loc = App.Current.Resources["Locator"] as ViewModelLocator;
            loc.Main.GetIsPremium();
            loc.Main.ReUpdateChannelStreams();*/


             var client = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"username", TextEmail.Text},
                {"password", TextPass.Password}
            });

            var x = await client.PostAsync("https://api.audioaddict.com/v1/di/members/authenticate", content);
            if (x.IsSuccessStatusCode)
            {
                var data = await x.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject(data) as JContainer;
                var lk = res.Value<string>("listen_key");

                var loc = App.Current.Resources["Locator"] as ViewModelLocator;
                loc.Main.ListenKey = lk;
                loc.Main.GetIsPremium();
                loc.Main.GetChannelsStresms();
            }
            else
            {
                await new MessageDialog("Error").ShowAsync();
            }
        }
    }
}
