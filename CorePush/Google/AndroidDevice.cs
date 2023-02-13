using System;
using CorePush.Google;
using CorePush.Interfaces;

namespace CorePush.Google
{
    public class AndroidDevice : IDevice
    {
        public string GetIdentifier()
        {
            return Guid.NewGuid().ToString();// Settings.Secure.GetString(Forms.Context.ContentResolver, Settings.Secure.AndroidId);
        }
    }
}