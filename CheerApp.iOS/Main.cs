using System;
using System.Collections.Generic;
using System.Linq;
using CorePush.Apple;
using CorePush.Google;
using CorePush.Utils;
using Foundation;
using LocalAuthentication;
using Plugin.PushNotification;
using UIKit;
using Xamarin.Forms;

namespace CheerApp.iOS
{
    public class Application
    {
        static void Main(string[] args)
        {
           UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}