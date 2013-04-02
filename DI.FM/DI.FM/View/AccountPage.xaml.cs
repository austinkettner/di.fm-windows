using Callisto.Controls;
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
            };
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            // Close the charm
            (this.Parent as SettingsFlyout).IsOpen = false;

            // Show the login window
            App.ShowLoginWindow();
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            // Close the charm
            (this.Parent as SettingsFlyout).IsOpen = false;

            // Clear the key
            Model.ListenKey = null;

            // Clear user info
            ApplicationData.Current.LocalSettings.Values.Remove("AccountEmail");
            ApplicationData.Current.LocalSettings.Values.Remove("FullName");
        }
    }
}
