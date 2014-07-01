using DI.FM.ViewModel;
using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DI.FM.View
{
    public sealed partial class AccountPage : Page
    {
        MainViewModel Model;

        public AccountPage()
        {
            this.InitializeComponent();

            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;

            this.Loaded += (sender, e) =>
            {
                bool isSignedIn = Model.ListenKey != null;

                if (isSignedIn)
                {
                    if (ApplicationData.Current.LocalSettings.Values["AccountEmail"] != null)
                    {
                        TextEmail.Text = ApplicationData.Current.LocalSettings.Values["AccountEmail"].ToString();
                    }

                    if (ApplicationData.Current.LocalSettings.Values["FullName"] != null)
                    {
                        TextFullName.Text = ApplicationData.Current.LocalSettings.Values["FullName"].ToString();
                    }

                    TextIsPremium.Text = Model.IsPremium ? "Yes, premium experience" : "No, free experience";

                    Stack1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    Stack2.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }

                var list = Model.IsPremium ? ChannelsHelper.PremiumStreamFormats : ChannelsHelper.FreeStreamFormats;
                ComboFormats.ItemsSource = list;

                if (ApplicationData.Current.LocalSettings.Values["StreamFormat"] != null)
                {
                    var item = list.FirstOrDefault(i => i[1] == ApplicationData.Current.LocalSettings.Values["StreamFormat"].ToString());
                    if (item != null) ComboFormats.SelectedItem = item;
                    else ComboFormats.SelectedIndex = 0;
                }
                else ComboFormats.SelectedIndex = 0;
            };
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            // Close the charm
            (this.Parent as SettingsFlyout).Hide();;

            // Show the login window
            App.ShowLoginWindow();
        }

        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            // Close the charm
            (this.Parent as SettingsFlyout).Hide();

            var dialog = new MessageDialog("If you log out from your premium account you will not have access anymore to premium streams.", "Logout from your account");
            dialog.Commands.Add(new UICommand("Logout") { Id = "ID_1" });
            dialog.Commands.Add(new UICommand("Stay logged in") { Id = "ID_2" });
            dialog.DefaultCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((string)result.Id == "ID_1")
            {
                // Clear the key
                Model.ListenKey = null;

                // Clear user info
                ApplicationData.Current.LocalSettings.Values.Remove("AccountEmail");
                ApplicationData.Current.LocalSettings.Values.Remove("FullName");
            }
        }

        private void ComboFormats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ApplicationData.Current.LocalSettings.Values["StreamFormat"] = (e.AddedItems[0] as string[])[1];
            }
        }

        private void ButtonApply_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Model.UpdateChannelsStreams();
            (this.Parent as SettingsFlyout).Hide();
        }
    }
}
