using DI.FM.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
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
                AnimateInStory.Begin();
                TextEmail.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            };

            AnimateOutStory.Completed += async (sneder, e) =>
            {
                await Task.Delay(1000);
                var parent = this.Parent as Grid;
                if (parent != null)
                {
                    parent.Children.Remove(this);
                }
            };
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            Progress.IsActive = true;

            TextEmail.IsEnabled = false;
            TextPass.IsEnabled = false;
            ButtonCancel.IsEnabled = false;
            ButtonLogin.IsEnabled = false;

            TextError.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            var data = await LogIn(TextEmail.Text, TextPass.Password);

            Progress.IsActive = false;

            TextEmail.IsEnabled = true;
            TextPass.IsEnabled = true;
            ButtonCancel.IsEnabled = true;
            ButtonLogin.IsEnabled = true;

            if (data != null)
            {
                var json = JsonConvert.DeserializeObject(data) as JContainer;

                // Set the listen key
                var locator = App.Current.Resources["Locator"] as ViewModelLocator;
                locator.Main.ListenKey = json.Value<string>("listen_key");

                // Set account details
                ApplicationData.Current.LocalSettings.Values["AccountEmail"] = json.Value<string>("email");
                ApplicationData.Current.LocalSettings.Values["FullName"] = json.Value<string>("first_name") + " " + json.Value<string>("last_name");

                // Close the login window
                AnimateOutStory.Begin();
            }
            else
            {
                TextError.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            AnimateOutStory.Begin();
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
            await Launcher.LaunchUriAsync(new Uri("https://www.di.fm/join"));
        }
    }
}
