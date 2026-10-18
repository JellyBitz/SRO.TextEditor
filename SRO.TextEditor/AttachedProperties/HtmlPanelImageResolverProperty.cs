using System;
using System.Windows;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.WPF;

namespace SRO.TextEditor
{
    public class HtmlPanelImageResolverProperty : BaseAttachedProperty<HtmlPanelImageResolverProperty, Func<string, BitmapImage>>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // Make sure is a panel attachment
            if (!(sender is HtmlPanel panel))
                return;

            // Set event tracking
            panel.ImageLoad -= OnImageLoad;
            if (!(e.NewValue is Func<string, BitmapImage> resolver))
                return;
            panel.ImageLoad += OnImageLoad;
        }
        private void OnImageLoad(object sender, RoutedEventArgs<HtmlImageLoadEventArgs> e)
        {
            if (!(sender is DependencyObject d))
                return;
            var resolver = GetValue(d);
            if (resolver == null)
                return;
            BitmapImage decodedImage = resolver(e.Data.Src);
            if (decodedImage != null)
                e.Data.Callback(decodedImage);
        }
    }
}