using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaFoundation;
using MediaFoundation.Misc;
using System.Runtime.InteropServices;
using MediaFoundation.ReadWrite;
using TwistedOak.Util;

namespace VideoCaptureLib.MFCapture
{
    public static class CaptureBuilder
    {
        private static IMFActivate CreateGrabber(Action<DataBuffer> onSample, int width, int height)
        {
            IMFMediaType grabberType;

            MFError.ThrowExceptionForHR(MFExtern.MFCreateMediaType(out grabberType));

            MFError.ThrowExceptionForHR(grabberType.SetGUID(MFAttributesClsid.MF_MT_MAJOR_TYPE, MFMediaType.Video));
            MFError.ThrowExceptionForHR(grabberType.SetGUID(MFAttributesClsid.MF_MT_SUBTYPE, MFMediaType.RGB32));

            Marshal.ThrowExceptionForHR(
                MFFunctions.MFSetAttributeSize(grabberType, MFAttributesClsid.MF_MT_FRAME_SIZE, (uint)width, (uint)height));

            var callback = new SampleGrabberCallback(onSample);

            IMFActivate grabberActivate;
            MFError.ThrowExceptionForHR(
                MFExtern.MFCreateSampleGrabberSinkActivate(grabberType, callback, out grabberActivate));

            // Run as fast as possible (requires Windows 7)
            MFError.ThrowExceptionForHR(
                grabberActivate.SetUINT32(MFAttributesClsid.MF_SAMPLEGRABBERSINK_IGNORE_CLOCK, 1));

            Marshal.ReleaseComObject(grabberType);

            return grabberActivate;
        }


        private static IMFTopologyNode AddSourceNode(IMFTopology topology, IMFMediaSource source, IMFPresentationDescriptor pd, IMFStreamDescriptor sd)
        {
            IMFTopologyNode node;
            MFError.ThrowExceptionForHR(MFExtern.MFCreateTopologyNode(MFTopologyType.SourcestreamNode, out node));

            MFError.ThrowExceptionForHR(node.SetUnknown(MFAttributesClsid.MF_TOPONODE_SOURCE, source));
            MFError.ThrowExceptionForHR(node.SetUnknown(MFAttributesClsid.MF_TOPONODE_PRESENTATION_DESCRIPTOR, pd));
            MFError.ThrowExceptionForHR(node.SetUnknown(MFAttributesClsid.MF_TOPONODE_STREAM_DESCRIPTOR, sd));

            MFError.ThrowExceptionForHR(topology.AddNode(node));

            return node;
        }

        private static IMFTopologyNode AddOutputNode(IMFTopology topology, IMFActivate sinkActivate, int sinkStreamId)
        {
            IMFTopologyNode node;
            MFError.ThrowExceptionForHR(MFExtern.MFCreateTopologyNode(MFTopologyType.OutputNode, out node));

            MFError.ThrowExceptionForHR(node.SetObject(sinkActivate));
            MFError.ThrowExceptionForHR(node.SetUINT32(MFAttributesClsid.MF_TOPONODE_STREAMID, sinkStreamId));
            MFError.ThrowExceptionForHR(node.SetUINT32(MFAttributesClsid.MF_TOPONODE_NOSHUTDOWN_ON_REMOVE, 0));

            MFError.ThrowExceptionForHR(topology.AddNode(node));

            return node;
        }


        private static int FindVideoStream(IMFPresentationDescriptor pd)
        {
            int streamCount;
            MFError.ThrowExceptionForHR(pd.GetStreamDescriptorCount(out streamCount));

            for (int i = 0; i < streamCount; ++i)
            {
                bool selected;
                IMFStreamDescriptor sd;
                IMFMediaTypeHandler typeHandler;
                Guid majorType;

                MFError.ThrowExceptionForHR(pd.GetStreamDescriptorByIndex(i, out selected, out sd));
                MFError.ThrowExceptionForHR(sd.GetMediaTypeHandler(out typeHandler));
                MFError.ThrowExceptionForHR(typeHandler.GetMajorType(out majorType));

                Marshal.ReleaseComObject(typeHandler);
                Marshal.ReleaseComObject(sd);

                if (majorType.Equals(MFMediaType.Video))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void SetMediaType(IMFStreamDescriptor sd, FormatInfo format)
        {
            IMFMediaTypeHandler mtHandler;
            MFError.ThrowExceptionForHR(sd.GetMediaTypeHandler(out mtHandler));

            try
            {
                var mt = FormatEnum.GetMediaType(mtHandler, format);
                mtHandler.SetCurrentMediaType(mt);
            }
            finally
            {
                Marshal.ReleaseComObject(mtHandler);
            }
        }

        private static IMFTopology CreateTopology(IMFMediaSource source, IMFPresentationDescriptor pd, IMFStreamDescriptor sd, IMFActivate sinkActivate)
        {
            IMFTopology topology = null;
            IMFTopologyNode srcNode = null, outputNode = null;

            MFError.ThrowExceptionForHR(MFExtern.MFCreateTopology(out topology));
            try
            {
                srcNode = AddSourceNode(topology, source, pd, sd);
                outputNode = AddOutputNode(topology, sinkActivate, 0);
                MFError.ThrowExceptionForHR(srcNode.ConnectOutput(0, outputNode, 0));
            }
            finally
            {
                if (srcNode != null) Marshal.ReleaseComObject(srcNode);
                if (outputNode != null) Marshal.ReleaseComObject(srcNode);
            }

            return topology;
        }

        private static void RunSession(IMFMediaSession session, IMFTopology topology)
        {
            MFError.ThrowExceptionForHR(
                session.SetTopology(MFSessionSetTopologyFlags.None, topology));

            ConstPropVariant var = new ConstPropVariant();
            MFError.ThrowExceptionForHR(
                session.Start(Guid.Empty, var));
        }


        private static readonly Guid IID_MEDIASOURCE = new Guid("279A808D-AEC7-40C8-9C6B-A6B492C78A66");
        private static IMFMediaSource CreateSource(IMFActivate device)
        {
            object mediaSourceObj;
            Marshal.ThrowExceptionForHR(
                device.ActivateObject(IID_MEDIASOURCE, out mediaSourceObj));
            var mediaSource = (IMFMediaSource)mediaSourceObj;
            return mediaSource;
        }


        public static IEnumerable<FormatInfo> EnumFormats(IMFActivate device)
        {
            var source = CreateSource(device);

            IMFPresentationDescriptor pd;
            IMFStreamDescriptor sd;
            IMFMediaTypeHandler mtHandler;

            MFError.ThrowExceptionForHR(source.CreatePresentationDescriptor(out pd));

            int streamIndex = FindVideoStream(pd);
            bool selected;
            MFError.ThrowExceptionForHR(pd.GetStreamDescriptorByIndex(streamIndex, out selected, out sd));
            MFError.ThrowExceptionForHR(sd.GetMediaTypeHandler(out mtHandler));

            var formats = FormatEnum.EnumFormats(mtHandler);

            Marshal.ReleaseComObject(mtHandler);
            Marshal.ReleaseComObject(sd);
            Marshal.ReleaseComObject(pd);
            Marshal.ReleaseComObject(source);

            return formats;
        }


        public static void Init(Lifetime lifetime, IMFActivate device, FormatInfo format, Action<DataBuffer> onSample)
        {
            var source = CreateSource(device);

            IMFPresentationDescriptor pd;
            IMFStreamDescriptor sd;

            MFError.ThrowExceptionForHR(source.CreatePresentationDescriptor(out pd));

            int streamIndex = FindVideoStream(pd);
            bool selected;
            MFError.ThrowExceptionForHR(pd.GetStreamDescriptorByIndex(streamIndex, out selected, out sd));

            SetMediaType(sd, format);

            var grabberActivate = CreateGrabber(onSample, format.Width, format.Height);
            var topology = CreateTopology(source, pd, sd, grabberActivate);
            Marshal.ReleaseComObject(grabberActivate);


            IMFMediaSession mediaSession;
            MFError.ThrowExceptionForHR(MFExtern.MFCreateMediaSession(null, out mediaSession));
            RunSession(mediaSession, topology);
            lifetime.WhenDead(() =>
            {
                try
                {
                    MFError.ThrowExceptionForHR(mediaSession.Stop());
                    MFError.ThrowExceptionForHR(mediaSession.Close());
                    MFError.ThrowExceptionForHR(source.Shutdown());
                    MFError.ThrowExceptionForHR(mediaSession.Shutdown());
                }
                finally
                {
                    Marshal.ReleaseComObject(mediaSession);
                    Marshal.ReleaseComObject(source);
                }
            });

            Marshal.ReleaseComObject(topology);
            Marshal.ReleaseComObject(sd);
            Marshal.ReleaseComObject(pd);

        }
    }
}
