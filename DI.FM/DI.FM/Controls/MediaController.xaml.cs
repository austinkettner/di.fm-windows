using DI.FM.ViewModel;
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
    }
}
