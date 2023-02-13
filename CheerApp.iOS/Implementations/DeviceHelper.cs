using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using Foundation;
using UIKit;

namespace CheerApp.iOS.Implementations
{
    public class DeviceHelper : IDeviceHelper
    {
        public DeviceHelper()
        {
        }

        public void FillDeviceDetails(DeviceDetail deviceDetail)
        {
            deviceDetail.Name = UIDevice.CurrentDevice.Name;
            deviceDetail.Description = UIDevice.CurrentDevice.Description;
            deviceDetail.Version = UIDevice.CurrentDevice.SystemVersion;
            deviceDetail.AppVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
        }
    }
}