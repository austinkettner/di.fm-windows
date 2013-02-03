using DI.FM.Common;
using DI.FM.ViewModel;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class ChannelPage : LayoutAwarePage
    {
        private MainViewModel Model;

        public ChannelPage()
        {
            this.InitializeComponent();
            // Get the model
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            // Bind the model
            this.DataContext = Model;
            CheckTrackStates();
            // Hook up media events
            this.Loaded += (sender, e) => { App.PlayingMedia.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged; };
            this.Unloaded += (sender, e) => { App.PlayingMedia.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged; };
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (App.PlayingMedia.PlayingItem == Model.NowPlayingItem)
            {
                if (App.PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing)
                {
                    ButtonPlayStop.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
                }
                else
                {
                    ButtonPlayStop.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
                }

                ButtonPlayStop1.Style = ButtonPlayStop.Style;
            }
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            if (App.PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing && Model.NowPlayingItem == App.PlayingMedia.PlayingItem)
            {
                App.PlayingMedia.MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            }
            else
            {
                App.PlayingMedia.PlayingItem = Model.NowPlayingItem;
            }
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            Model.NowPlayingItem = Model.NowPlayingItem.Prev;
            CheckTrackStates();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            Model.NowPlayingItem = Model.NowPlayingItem.Next;
            CheckTrackStates();
        }

        private void CheckTrackStates()
        {
            if (Model.NowPlayingItem != null && Model.NowPlayingItem == App.PlayingMedia.PlayingItem && App.PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing)
            {
                ButtonPlayStop.Style = App.Current.Resources["StopIconButtonStyle"] as Style;
            }
            else
            {
                ButtonPlayStop.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
            }

            ButtonPlayStop1.Style = ButtonPlayStop.Style;

            if (Model.FavoriteChannels.Contains(Model.NowPlayingItem))
            {
                ButtonFavorite.Style = App.Current.Resources["UnfavoriteAppBarButtonStyle"] as Style;
            }
            else
            {
                ButtonFavorite.Style = App.Current.Resources["FavoriteAppBarButtonStyle"] as Style;
            }
        }

        private async void ButtonFavorite_Click(object sender, RoutedEventArgs e)
        {
            this.BottomAppBar.IsOpen = false;

            if (Model.FavoriteChannels.Contains(Model.NowPlayingItem))
            {
                Model.FavoriteChannels.Remove(Model.NowPlayingItem);
                ButtonFavorite.Style = App.Current.Resources["FavoriteAppBarButtonStyle"] as Style;
            }
            else
            {
                Model.FavoriteChannels.Insert(0, Model.NowPlayingItem);
                ButtonFavorite.Style = App.Current.Resources["UnfavoriteAppBarButtonStyle"] as Style;
            }

            await Model.SaveFavoriteChannels();
        }
    }
}
