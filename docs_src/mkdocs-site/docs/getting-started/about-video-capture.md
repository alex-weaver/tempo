# About the Video Capture Sample

The Examples/VideoCapture folder shows how continuous code can be used to create a convenient
API and manage memory heavy resources. The sample uses the Media Foundation .NET project
(http://mfnet.sourceforge.net/) to
use the Windows Media Foundation COM interfaces. Some modifications have been made to some of the signatures
to fix minor issues. The full modified source is included. The sample is split into three parts:

* MFLib - the modified Media Foundation .NET library
* VideoCaptureLib - Implementation of a simple library exposing a list of video capture devices, and allows
a live feed from each device to be observed.
* AllCamerasSample - an application making use of the VideoCaptureLib to show a live feed of every available
camera.

The AllCamerasSample application shows that by tracking lifetimes and list changes, it is possible to
express a `map` -like construct even when the inner function has side-effects. In this case, it is the
`WithEach` method which provides the application with a continuous scope for each camera, which allows
the body of WithEach to activate the camera as long as it remains plugged in.

A convenient consequence of using a more direct construct like `WithEach` (rather than a more operational
approach like giving CaptureDevice a 'start' and 'stop' method) is that some edge cases are handled automatically.
For example, the continuous scope in the body of WithEach is ended when the list item is removed. The MediaCaptureApi
object removes cameras from the list when they are unplugged from the system, so this sample automatically adds and
removes video feeds from the UI whenever cameras are plugged or unplugged.

The sample uses WriteableBitmap to display VideoFrame objects in the WPF interface. Since
WriteableBitmap does not provide any way to manually release the resources it consumes, the sample uses an object
pool to avoid allocating a large number of WriteableBitmap objects.
