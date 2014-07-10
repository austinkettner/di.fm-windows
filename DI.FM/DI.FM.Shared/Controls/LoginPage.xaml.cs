using DI.FM.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Web.Http;

namespace DI.FM.Controls
{
    public sealed partial class LoginPage : UserControl
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            Progress.IsActive = true;
            TextEmail.IsEnabled = false;
            TextPass.IsEnabled = false;
            ButtonLogin.IsEnabled = false;
            TextError.Visibility = Visibility.Collapsed;

            var data = await LogIn(TextEmail.Text, TextPass.Password);

            Progress.IsActive = false;
            TextEmail.IsEnabled = true;
            TextPass.IsEnabled = true;
            ButtonLogin.IsEnabled = true;

            if (data != null)
            {
                var json = JObject.Parse(data);

                // Set the listen key
                var model = App.Main;
                model.ListenKey = (string)json["listen_key"];

                // Set account details
                ApplicationData.Current.LocalSettings.Values["AccountEmail"] = (string)json["email"];
                ApplicationData.Current.LocalSettings.Values["FullName"] = (string)json["first_name"] + " " + (string)json["last_name"];
                var parent = Parent as Popup;
                parent.IsOpen = false;
            }
            else
            {
                TextError.Visibility = Visibility.Visible;
            }
        }

        private async Task<string> LogIn(string user, string pass)
        {
            var client = new HttpClient();
            var content = new HttpFormUrlEncodedContent(new Dictionary<string, string>
            {
                {"username", user},
                {"password", pass}
            });

            try
            {
                var result = await client.PostAsync(new Uri("https://api.audioaddict.com/v1/di/members/authenticate"), content);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://di.fm/join"));
        }
    }
}
