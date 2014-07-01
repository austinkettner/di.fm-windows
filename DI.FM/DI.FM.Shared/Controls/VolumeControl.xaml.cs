using DI.FM.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DI.FM.Controls
{
    public sealed partial class VolumeControl : UserControl
    {
        private MediaElement MediaPlayer;

        public VolumeControl()
        {
            this.InitializeComponent();

            var model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            MediaPlayer = model.MediaPlayer;

            CheckMuteState();
            CheckVolumeState();
        }

        private void ButtonMute_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.IsMuted = !MediaPlayer.IsMuted;
            CheckMuteState();
        }

        private void CheckMuteState()
        {
            if (MediaPlayer.IsMuted)
            {
                ButtonMute.Style = this.Resources["ButtonMutedStyle"] as Style;
            }
            else
            {
                ButtonMute.Style = this.Resources["ButtonUnMutedStyle"] as Style;
            }
        }

        private void SliderVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MediaPlayer.Volume = e.NewValue / 100;
        }

        private void CheckVolumeState()
        {
            SliderVolume.Value = 100 * MediaPlayer.Volume;
        }
    }
}
