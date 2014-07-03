using Windows.UI.Xaml.Controls.Primitives;
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

            Model = App.Main;

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
                Stack2.Visibility = Visibility.Collapsed;
            }
            else
            {
                Stack2.Visibility = Visibility.Visible;
                Stack1.Visibility = Visibility.Collapsed;
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
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            var parent = Parent as Popup;
            parent.IsOpen = false;
        }

        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("If you sign out from your premium account you will not have access anymore to premium streams.", "Sign Out");
            dialog.Commands.Add(new UICommand("Sign out") { Id = 1 });
            dialog.Commands.Add(new UICommand("Stay") { Id = 2 });
            dialog.DefaultCommandIndex = 1;
            var result = await dialog.ShowAsync();

            if ((int)result.Id == 1)
            {
                Model.ListenKey = null;
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

        private async void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
           await Model.UpdateChannels();
        }
    }
}
