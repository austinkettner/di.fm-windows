using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DI.FM.View
{
    public sealed partial class ExtendedSplash : Page
    {
        public ExtendedSplash(SplashScreen splash)
        {
            this.InitializeComponent();
            PositionImage(splash);
        }

        private void PositionImage(SplashScreen splash)
        {
            extendedSplashImage.SetValue(Canvas.LeftProperty, splash.ImageLocation.X);
            extendedSplashImage.SetValue(Canvas.TopProperty, splash.ImageLocation.Y);
            extendedSplashImage.Height = splash.ImageLocation.Height;
            extendedSplashImage.Width = splash.ImageLocation.Width;

            Progress.SetValue(Canvas.LeftProperty, splash.ImageLocation.X + splash.ImageLocation.Width / 2 - Progress.Width / 2);
            Progress.SetValue(Canvas.TopProperty, splash.ImageLocation.Y + splash.ImageLocation.Height + 50);
        }
    }
}
