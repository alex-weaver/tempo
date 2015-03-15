using MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoCaptureLib.MFCapture
{
    public static class DeviceEnumeration
    {
        /// <summary>
        /// Caller must release each element of the array
        /// </summary>
        /// <param name="symbolicLink"></param>
        /// <returns></returns>
        public static IMFActivate[] EnumVideoDevices(string symbolicLink)
        {
            IMFAttributes attributes;
            Marshal.ThrowExceptionForHR(
                MFExtern.MFCreateAttributes(out attributes, 1));

            Marshal.ThrowExceptionForHR(
                attributes.SetGUID(
                    MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE,
                    CLSID.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID));

            if (symbolicLink != null)
            {
                Marshal.ThrowExceptionForHR(
                    attributes.SetString(MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK, symbolicLink));
            }

            IMFActivate[] sources;
            int sourceCount;
            Marshal.ThrowExceptionForHR(
                MFExtern.MFEnumDeviceSources(attributes, out sources, out sourceCount));

            //.;//TODO: sourceCount may be > 1, but sources.Length will always be 1. Is the signature of MFEnumDeviceSources wrong?

            return sources == null ? new IMFActivate[0] : sources;
        }

        public static string GetFriendlyName(IMFActivate activationObject)
        {
            string name;
            int length;

            Marshal.ThrowExceptionForHR(
                activationObject.GetAllocatedString(
                    MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME,
                    out name,
                    out length));

            return name;
        }

        public static string GetSymbolicLink(IMFActivate activationObject)
        {
            string name;
            int length;

            Marshal.ThrowExceptionForHR(
                activationObject.GetAllocatedString(
                    MFAttributesClsid.MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK,
                    out name,
                    out length));

            return name;
        }
    }
}
