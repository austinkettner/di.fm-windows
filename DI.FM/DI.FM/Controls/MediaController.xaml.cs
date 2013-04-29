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
            this.InitializeComponent();
        }

        private void ButtonPlayStop_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var model = this.DataContext as MainViewModel;
            if (model != null)
            {
                model.TogglePlay();
            }
        }

        private void StackPanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            var model = this.DataContext as MainViewModel;

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
