using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

namespace DI.FM.View
{
    public sealed partial class ExtendedSplash : Page
    {
        public ExtendedSplash(SplashScreen splash)
        {
            InitializeComponent();

            // Resize logo on size changed
            SizeChanged += (sender, e) => { PositionElements(splash); };

            // Init the position
            PositionElements(splash);
        }

        private void PositionElements(SplashScreen splash)
        {
            // Logo
            extendedSplashImage.SetValue(Canvas.LeftProperty, splash.ImageLocation.X);
            extendedSplashImage.SetValue(Canvas.TopProperty, splash.ImageLocation.Y);
            extendedSplashImage.Height = splash.ImageLocation.Height;
            extendedSplashImage.Width = splash.ImageLocation.Width;

            // Progress ring
            extendedProgress.SetValue(Canvas.LeftProperty, splash.ImageLocation.X + splash.ImageLocation.Width / 2 - extendedProgress.Width / 2);
            extendedProgress.SetValue(Canvas.TopProperty, splash.ImageLocation.Y + splash.ImageLocation.Height + 50);
        }
    }
}
