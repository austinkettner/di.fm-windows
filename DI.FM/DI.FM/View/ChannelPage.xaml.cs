using Callisto.Controls;
using DI.FM.Common;
using DI.FM.Controls;
using DI.FM.ViewModel;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace DI.FM.View
{
    public sealed partial class ChannelPage : LayoutAwarePage
    {
        private MainViewModel Model;
        private ChannelItem SelectedItem;

        public ChannelPage()
        {
            this.InitializeComponent();
            // Get the model
            
            // Bind the model
            //this.DataContext = Model;
            
            // Hook up media events
           /* this.Loaded += (sender, e) => { App.PlayingMedia.MediaPlayer.CurrentStateChanged += MediaPlayer_CurrentStateChanged; };
            this.Unloaded += (sender, e) => { App.PlayingMedia.MediaPlayer.CurrentStateChanged -= MediaPlayer_CurrentStateChanged; };*/
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            SelectedItem = e.Parameter as ChannelItem;

            this.DefaultViewModel.Add("Model", Model);
            this.DefaultViewModel.Add("Channel", SelectedItem);
            CheckTrackStates();
        }

        private void MediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            /*if (App.PlayingMedia.PlayingItem == Model.NowPlayingItem)
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
            }*/
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
           /* if (App.PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing && Model.NowPlayingItem == App.PlayingMedia.PlayingItem)
            {
                App.PlayingMedia.MediaPlayer.Source = null;
                MediaControl.IsPlaying = false;
            }
            else
            {
                App.PlayingMedia.PlayingItem = Model.NowPlayingItem;
            }*/

            if (Model.NowPlayingItem == SelectedItem)
            {
                Model.TogglePlay();
            }
            else
            {
                Model.PlayChannel(SelectedItem);
                CheckTrackStates();
            }
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = SelectedItem.Prev;
            this.DefaultViewModel["Channel"] = SelectedItem;
            CheckTrackStates();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = SelectedItem.Next;
            this.DefaultViewModel["Channel"] = SelectedItem;
            CheckTrackStates();
        }

        private void CheckTrackStates()
        {
            if (Model.NowPlayingItem == SelectedItem)
            {
                var binding = new Binding();
                binding.Path = new PropertyPath("Model.IsPlaying");
                binding.Converter = new PlayStopButtonConverter();
                ButtonPlayStop.SetBinding(FrameworkElement.StyleProperty, binding);
            }
            else
            {
                //ButtonPlayStop.SetBinding(FrameworkElement.StyleProperty, null);
                ButtonPlayStop.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
            }

            /*if (Model.NowPlayingItem != null && Model.NowPlayingItem == App.PlayingMedia.PlayingItem && App.PlayingMedia.MediaPlayer.CurrentState == MediaElementState.Playing)
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
            }*/
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

        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new Flyout()
            {
                Content = new VolumeControl(),
                Placement = PlacementMode.Top,
                PlacementTarget = sender as UIElement,
                IsOpen = true
            };
        }
    }
}
