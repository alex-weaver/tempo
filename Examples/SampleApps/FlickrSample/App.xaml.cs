using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tempo;
using Tempo.Wpf;

namespace FlickrSample
{
    public partial class App : Application
    {
        // To run this sample, you will need a Flickr API key. You can get one here:
        // https://www.flickr.com/services/apps/create/apply
        private const string FlickrApiKey = "%%YOUR_API_KEY_HERE";

        public App()
        {
            if (FlickrApiKey.StartsWith("%%")) throw new ApplicationException(
                "To run this sample, please change the FlickrApiKey constant to your own API key");

            TempoApp.Init(this, true, WhileRunning);
        }

        private void WhileRunning()
        {
            var window = new MainWindow();
            window.Show();

            var photos = new ListCell<Photo>();

            Op.Listen<KeyEventHandler, KeyEventArgs>(h => window.queryView.KeyDown += h, h => window.queryView.KeyDown -= h, args =>
                {
                    if (args.Key == Key.Return || args.Key == Key.Enter)
                    {
                        window.progressOverlay.Visibility = Visibility.Visible;

                        FlickrSearch.Search(FlickrApiKey, window.queryView.Text, 500, 1, result =>
                        {
                            photos.Clear();
                            photos.AddRange(result);
                            window.progressOverlay.Visibility = Visibility.Hidden;
                        });
                    }
                });

            
            var views = Op.WithEach(photos, x =>
                {
                    var imageView = new Image();
                    imageView.Width = 150;
                    imageView.Height = 150;

                    FlickrSearch.GetPhotoThumb(x, data =>
                        {
                            var frame = JpegDecoding.DecodeJpeg(data);
                            imageView.Source = frame;
                        });

                    return CellBuilder.Const(imageView);
                });

            SelectorBinding.Bind(views, window.photoListView);
        }
    }
}
