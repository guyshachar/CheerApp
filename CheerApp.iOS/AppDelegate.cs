using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Plugin.PushNotification;
using Xamarin.Forms.Platform.iOS;
using static SystemConfiguration.NetworkReachability;
using Notification = CheerApp.Common.Notification;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using CheerApp.Common;
using System.Security.Cryptography;

namespace CheerApp.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //---- declarations
        private UIWindow window;
        private static IUIAlertViewDelegate uIAlertViewDelegate = null;
        private IPushNotificationHandler pushNotificationHandler;

        public AppDelegate()
        {
        }

        // This method is invoked when the application has loaded its UI and it is ready to run
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Startup();
            this.pushNotificationHandler = DependencyService.Get<IPushNotificationHandler>();

            //global::Xamarin.Forms.Forms.Init();
            //LoadApplication(new App());

            PushNotificationManager.Initialize(options, pushNotificationHandler, true);

            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            // Handle when your app starts
            Task.Run(async () => await TokenRefreshAsync(null, new PushNotificationTokenEventArgs(CrossPushNotification.Current.Token)));
            CrossPushNotification.Current.OnTokenRefresh += async (s, p) => await TokenRefreshAsync(s, p);

            this.window = new UIWindow(UIScreen.MainScreen.Bounds);

            //---- instantiate a new navigation controller
            var rootNavigationController = new UINavigationController();
            //---- add the home screen to the navigation controller (it'll be the top most screen)
            rootNavigationController.PushViewController(DependencyServiceExtension.Get(typeof(HomeScreen)), false);

            //---- set the root view controller on the window. the nav controller will handle the rest
            this.window.RootViewController = rootNavigationController;

            this.window.MakeKeyAndVisible();

            //return base.FinishedLaunching(app, options);
            return true;
        }

        [Export("application:didReceiveLocalNotification:")]
        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            // show an alert
            var body = JsonSerializer.Deserialize<Notification>(notification.AlertBody);
            new UIAlertView(notification.AlertAction, notification.AlertBody, uIAlertViewDelegate, "OK", null).Show();
            Assembly asm = typeof(AppDelegate).Assembly;
            Type screenType = asm.GetType(body.ScreenName);
            var uiViewController = DependencyServiceExtension.Get(screenType);

            var uiServices = DependencyService.Resolve<IUIServices>();
            if (body.Action == "ResetShow")
            {
                uiServices.ResetShowAsync(uiViewController, body.Json);
            }

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber--;
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

        private void Startup()
        {
            DependencyService.Register<IPushNotificationHandler, PushNotificationHandler>();
            DependencyService.Register<IUIServices, UIServices>();
            DependencyServiceExtension.Register<HomeScreen>();
            DependencyServiceExtension.Register<CheerAppScreen>();
            DependencyServiceExtension.Register<HelloUniverseScreen>();
        }

        private async Task TokenRefreshAsync(object s, PushNotificationTokenEventArgs p)
        {
            System.Diagnostics.Debug.WriteLine($"TOKEN REC: {p.Token}");
            var text =
                $"Name: {DeviceInfo.Name}{System.Environment.NewLine}" +
                $"Device Token: {p.Token}";
            await Clipboard.SetTextAsync(text);
            new UIAlertView("New Device Token", "Please paste new Token to Guy", uIAlertViewDelegate, "OK", null).Show();
        }
    }
}