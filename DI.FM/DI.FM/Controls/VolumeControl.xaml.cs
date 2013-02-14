using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace DI.FM.Controls
{
    public sealed partial class VolumeControl : UserControl
    {
        public VolumeControl()
        {
            this.InitializeComponent();
            CheckMuteState();
            CheckVolumeState();
        }

        private void ButtonMute_Click(object sender, RoutedEventArgs e)
        {
            App.PlayingMedia.MediaPlayer.IsMuted = !App.PlayingMedia.MediaPlayer.IsMuted;
            CheckMuteState();
        }

        private void CheckMuteState()
        {
            if (App.PlayingMedia.MediaPlayer.IsMuted)
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
            App.PlayingMedia.MediaPlayer.Volume = e.NewValue / 100;
        }

        private void CheckVolumeState()
        {
            SliderVolume.Value = 100 * App.PlayingMedia.MediaPlayer.Volume;
        }
    }
}
