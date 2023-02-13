using System;
using System.Reflection;
using CheerApp.Common;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using CheerApp.iOS.Implementations;
using Firebase.CloudMessaging;
using Foundation;
using Google.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plugin.PushNotification;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace CheerApp.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        public const string TOPIC_ALL = "ALL";

        private IHost Host;
        private IDbService dbService;
        private static IUIAlertViewDelegate uIAlertViewDelegate = null;

        public AppDelegate()
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(AppDelegate)} Start...");
        }

        // This method is invoked when the application has loaded its UI and it is ready to run
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(AppDelegate)} {nameof(FinishedLaunching)} Start...");
            Host = Startup.Start((context, serviceCollection)
                =>
            {
                serviceCollection.AddSingleton<IFirestoreProvider, FirestoreProvider>();
                serviceCollection.AddSingleton<IUIServices, UIServices>();
                serviceCollection.AddSingleton<IDeviceHelper, DeviceHelper>();
                serviceCollection.AddSingleton<HomeScreen>();
                serviceCollection.AddSingleton<ShowRoom>();
                serviceCollection.AddSingleton<SendPushNotification>();
                serviceCollection.AddSingleton<IPushHandler, PushHandler>();
                serviceCollection.AddSingleton<IPushNotificationHandler, PushNotificationHandler>();
            }, typeof(HomeScreen), UIDevice.CurrentDevice.IdentifierForVendor.ToString(), this, app, options);

            dbService = Host.Services
               .GetService<IDbService>();

            Xamarin.Forms.Forms.Init();

            System.Diagnostics.Debug.WriteLine($"{nameof(AppDelegate)} {nameof(FinishedLaunching)} After Host build...");
            System.Diagnostics.Debug.WriteLine($"{nameof(AppDelegate)} {nameof(FinishedLaunching)} After Start call...");

            return true;
        }

        [Export("application:didReceiveLocalNotification:")]
        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {/*
            // show an alert
            var body = new ShowRoomNotification();
            //var a = new UIAlertView(notification.AlertAction, body.Body, uIAlertViewDelegate, "OK", null).Show();
            Assembly asm = typeof(AppDelegate).Assembly;
            Type screenType = asm.GetType(body.ScreenName);
            //var uiViewController = DependencyServiceExtension.Get(screenType);

            var uiServices = DependencyService.Resolve<IUIServices>();
            if (body.Action == "ResetShow")
            {
                //uiServices.ResetShowAsync(uiViewController, notification);
            }

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber--;
        */
        }

        /// <summary>
        /// The iOS will call the APNS in the background and issue a device token to the device. when that's
        /// accomplished, this method will be called.
        ///
        /// Note: the device token can change, so this needs to register with your server application everytime
        /// this method is invoked, or at a minimum, cache the last token and check for a change.
        /// </summary>
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            PushNotificationManager.DidRegisterRemoteNotifications(deviceToken);
            deviceToken = deviceToken.ToString();
        }

        /// <summary>
        /// Registering for push notifications can fail, for instance, if the device doesn't have network access.
        ///
        /// In this case, this method will be called.
        /// </summary>
        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            PushNotificationManager.RemoteNotificationRegistrationFailed(error);
            new UIAlertView("Error registering push notifications", error.LocalizedDescription, uIAlertViewDelegate, "OK", null).Show();
        }

        // To receive notifications in foregroung on iOS 9 and below.
        // To receive notifications in background in any iOS version
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            PushNotificationManager.DidReceiveMessage(userInfo);
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
        }
    }
}