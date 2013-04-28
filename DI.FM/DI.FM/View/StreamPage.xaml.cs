using DI.FM.ViewModel;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using System.Linq;

namespace DI.FM.View
{
    public sealed partial class StreamPage : Page
    {
        private object InitialFormat;
        private MainViewModel Model;

        public StreamPage()
        {
            this.InitializeComponent();

            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            InitialFormat = ApplicationData.Current.LocalSettings.Values["StreamFormat"];

            var list = Model.IsPremium ? ChannelsHelper.PremiumStreamFormats : ChannelsHelper.FreeStreamFormats;
            ComboFormats.ItemsSource = list;

            if (InitialFormat != null)
            {
                var item = list.FirstOrDefault(i => i[1] == InitialFormat.ToString());
                if (item != null) ComboFormats.SelectedItem = item;
                else ComboFormats.SelectedIndex = 0;
            }
            else ComboFormats.SelectedIndex = 0;
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
            if (InitialFormat != ApplicationData.Current.LocalSettings.Values["StreamFormat"])
            {
                Model.UpdateChannelsStreams();
            }
        }
    }
}
