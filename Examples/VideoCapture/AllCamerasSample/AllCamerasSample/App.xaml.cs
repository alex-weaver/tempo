using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Tempo;
using Tempo.Wpf;
using VideoCaptureLib;

namespace VideoTest
{
    public partial class App : Application
    {
        public App()
        {
            TempoApp.Init(this, true, WhileRunning);
        }

        private void WhileRunning()
        {
            // Initialize window, capture api and bitmap pool
            var window = new MainWindow();
            window.Show();

            var captureApi = new MediaCaptureApi();
            var bitmapPool = new WriteableBitmapPool(100);


            // This creates a reactive list of CameraView objects. There will be one CameraView
            // object for each connected camera, and it will show a video feed from the camera.
            // Using WithEach means that the list is automatically updated when devices are plugged and unplugged.
            var views = captureApi.Devices.WithEach(device =>
                {
                    if (!device.VideoFormats.Any())
                        return CellBuilder.Const<CameraView>(null);

                    // If there is any format with a width of 640, use it; otherwise, pick the first format.
                    var format = device.VideoFormats
                        .Where(x => x.Width == 640)
                        .Union(device.VideoFormats)
                        .First();

                    // Apply a continuous request for a video feed in the given format, and construct a CameraView
                    // from the resulting feed
                    var feed = device.ApplyVideoRequest(format);
                    return CellBuilder.Const(MakeFeedView(device.FriendlyName, feed, bitmapPool));
                });

            // Bind the list of CameraView objects to the list in the main window
            views.Bind(window.feedList.Items);
        }


        private static CameraView MakeFeedView(string name, ICellRead<VideoFrame> videoFeed, WriteableBitmapPool bitmapPool)
        {
            var view = new CameraView();

            view.name.Text = name;
            view.nameBg.Text = name;


            // Copy each new frame into a WriteableBitmap, and set it as the source for feedImage

            var bitmapFrame = videoFeed.WithLatestOrDefault(currentFrame =>
                    CellBuilder.Const(bitmapPool.AllocateWith(currentFrame)));

            bitmapFrame.Latest(value => view.feedImage.Source = value);

            Events.WhenEnded(() => view.feedImage.Source = null);

            return view;
        }
    }
}
