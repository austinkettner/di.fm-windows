using Callisto.Controls;
using DI.FM.Common;
using DI.FM.Controls;
using DI.FM.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace DI.FM.View
{
    public sealed partial class ChannelPage : LayoutAwarePage
    {
        private bool Direction;
        private MainViewModel Model;
        private ChannelItem SelectedItem;

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Model = (App.Current.Resources["Locator"] as ViewModelLocator).Main;
            SelectedItem = e.Parameter as ChannelItem;

            this.DefaultViewModel.Add("Model", Model);
            this.DefaultViewModel.Add("Channel", SelectedItem);

            UpdatePlayStatus();
            UpdateFavoriteStatus();
        }

        private void ButtonPlayStop_Click(object sender, RoutedEventArgs e)
        {
            if (Model.NowPlayingItem == SelectedItem)
            {
                Model.TogglePlay();
            }
            else
            {
                Model.PlayChannel(SelectedItem);
                UpdatePlayStatus();
            }
        }

        private void ButtonPrev_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = SelectedItem.Prev;

            Direction = true;

            FadeOutRightStory.Begin();
            /*this.DefaultViewModel["Channel"] = SelectedItem;

            UpdatePlayStatus();
            UpdateFavoriteStatus();*/
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            SelectedItem = SelectedItem.Next;

            Direction = false;

            FadeOutLeftStory.Begin();

            /*this.DefaultViewModel["Channel"] = SelectedItem;

            UpdatePlayStatus();
            UpdateFavoriteStatus();*/
        }

        private void ButtonPrev1_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Channel"] = SelectedItem;

            UpdatePlayStatus();
            UpdateFavoriteStatus();
        }

        private void ButtonNext1_Click(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Channel"] = SelectedItem;

            UpdatePlayStatus();
            UpdateFavoriteStatus();
        }

        private void UpdateFavoriteStatus()
        {
            if (Model.FavoriteChannels.Contains(SelectedItem))
            {
                ButtonFavorite.Style = App.Current.Resources["UnfavoriteAppBarButtonStyle"] as Style;
            }
            else
            {
                ButtonFavorite.Style = App.Current.Resources["FavoriteAppBarButtonStyle"] as Style;
            }

            // Add to the live update list
            Model.LiveUpdateList.Clear();
            Model.LiveUpdateList.Add(SelectedItem);
        }

        private void UpdatePlayStatus()
        {
            if (Model.NowPlayingItem == SelectedItem)
            {
                var binding = new Binding();
                binding.Path = new PropertyPath("Model.IsPlaying");
                binding.Converter = new PlayStopButtonConverter();
                ButtonPlayStop.SetBinding(FrameworkElement.StyleProperty, binding);
                ButtonPlayStop1.SetBinding(FrameworkElement.StyleProperty, binding);
            }
            else
            {
                ButtonPlayStop.Style = App.Current.Resources["PlayIconButtonStyle"] as Style;
                ButtonPlayStop1.Style = ButtonPlayStop.Style;
            }
        }

        private async void ButtonFavorite_Click(object sender, RoutedEventArgs e)
        {
            this.BottomAppBar.IsOpen = false;

            if (Model.FavoriteChannels.Contains(Model.NowPlayingItem)) Model.FavoriteChannels.Remove(Model.NowPlayingItem);
            else Model.FavoriteChannels.Insert(0, Model.NowPlayingItem);
            UpdateFavoriteStatus();

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

        private void FadeOutStory_Completed(object sender, object e)
        {
            this.DefaultViewModel["Channel"] = SelectedItem;

            UpdatePlayStatus();
            UpdateFavoriteStatus();

            if (Direction) FadeInRightStory.Begin();
            else FadeInLeftStory.Begin();
        }
    }
}

