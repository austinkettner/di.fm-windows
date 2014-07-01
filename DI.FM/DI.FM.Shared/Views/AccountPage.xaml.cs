using DI.FM.ViewModel;
using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DI.FM.Controls
{
    public sealed partial class AccountPage : UserControl
    {
        MainViewModel Model;

        public AccountPage()
        {
            InitializeComponent();

            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;

            Loaded += (sender, e) =>
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

                    Stack1.Visibility = Visibility.Visible;
                }
                else
                {
                    Stack2.Visibility = Visibility.Visible;
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
            // Show the login window
            App.ShowLoginWindow();
            RemoveWindow();
        }
        private void RemoveWindow()
        {
            var parent = Parent as Grid;
            if (parent != null)
            {
                parent.Children.Remove(this);
            }
        }

        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            RemoveWindow();

            var dialog = new MessageDialog("If you log out from your premium account you will not have access anymore to premium streams.", "Logout from your account");
            dialog.Commands.Add(new UICommand("Logout") { Id = 1 });
            dialog.Commands.Add(new UICommand("Stay logged in") { Id = 2 });
            dialog.DefaultCommandIndex = 1;

            var result = await dialog.ShowAsync();

            if ((int)result.Id == 1)
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

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            Model.UpdateChannelsStreams();
            RemoveWindow();
        }
    }
}
