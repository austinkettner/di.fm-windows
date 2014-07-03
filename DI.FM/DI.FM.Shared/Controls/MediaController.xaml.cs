using Windows.UI.Xaml.Input;
using DI.FM.View;
using DI.FM.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DI.FM.Controls
{
    public sealed partial class MediaController : UserControl
    {
        public MediaController()
        {
            InitializeComponent();
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            App.Main.TogglePlay();
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            var model = DataContext as MainViewModel;

            if (frame != null && model != null)
            {
                if (!(frame.Content is ChannelPage))
                {
                    frame.Navigate(typeof(ChannelPage), model.NowPlayingItem);
                }
            }
        }
    }
}
