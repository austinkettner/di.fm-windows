using DI.FM.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DI.FM.Controls
{
    public sealed partial class LoginPage : UserControl
    {
        private const string AUTH_URL = "https://api.audioaddict.com/v1/di/members/authenticate";

        public LoginPage()
        {
            this.InitializeComponent();
            this.Loaded += (sender, e) =>
            {
                TextEmail.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            };
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            TextError.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            var data = await LogIn(TextEmail.Text, TextPass.Password);

            this.IsEnabled = true;

            if (data != null)
            {
                var json = JsonConvert.DeserializeObject(data) as JContainer;
                var key = json.Value<string>("listen_key");

                var locator = App.Current.Resources["Locator"] as ViewModelLocator;
                locator.Main.ListenKey = key;
                locator.Main.CheckPremiumStatus();
                locator.Main.GetChannelsStresms();

                RemoveWindow();
            }
            else
            {
                TextError.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            RemoveWindow();
        }

        private void RemoveWindow()
        {
            var parent = this.Parent as Grid;
            if (parent != null)
            {
                parent.Children.Remove(this);
            }
        }

        private async Task<string> LogIn(string user, string pass)
        {
            var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"username", user},
                {"password", pass}
            });

            try
            {
                var result = await client.PostAsync(AUTH_URL, content);
                if (!result.IsSuccessStatusCode) return null;
                return await result.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}
