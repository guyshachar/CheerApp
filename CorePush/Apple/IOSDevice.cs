﻿using CorePush.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(IOSDevice))]
namespace CorePush.iOS
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using CoreFoundation;
    using Foundation;

    public class IOSDevice : IDevice
    {
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern IntPtr IOServiceMatching(string s);
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);
        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        private static extern int IOObjectRelease(uint o);
        public string GetIdentifier()
        {
            string serial = string.Empty;
            uint platformExpert = IOServiceGetMatchingService(0, IOServiceMatching("IOPlatformExpertDevice"));
            if (platformExpert != 0)
            {
                var key = (NSString)"IOPlatformSerialNumber";
                IntPtr serialNumber = IORegistryEntryCreateCFProperty(platformExpert, key.Handle, IntPtr.Zero, 0);
                if (serialNumber != IntPtr.Zero)
                {
                    serial = CFString.FromHandle(serialNumber);
                }
                IOObjectRelease(platformExpert);
            }
            return serial;
        }
    }
}