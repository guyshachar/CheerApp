using System;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using CheerApp.iOS.Extensions;
using CheerApp.Models;
using CorePush;
using CorePush.Apple;
using CorePush.Google;
using CorePush.Interfaces;
using Firebase.CloudFirestore;
using Firebase.CloudMessaging;
using Foundation;
using Plugin.PushNotification;
using UIKit;
using UserNotifications;
using Xamarin.Essentials;
using Xamarin.Forms;
using Notification = CheerApp.Common.Notification;

namespace CheerApp.iOS
{
    [Register(nameof(AppDelegate))]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        //---- declarations
        private UIWindow window;
        private static IUIAlertViewDelegate uIAlertViewDelegate = null;
        private IPushNotificationHandler pushNotificationHandler;

        public AppDelegate()
        {
        }

        public static string DeviceId { get; set; }
        public static string FcmToken { get; set; }
        public static string DeviceToken { get; set; }

        // This method is invoked when the application has loaded its UI and it is ready to run
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Startup();

            DeviceId = UIDevice.CurrentDevice.IdentifierForVendor.ToString();// DependencyService.Get<IDevice>().GetIdentifier();

            this.pushNotificationHandler = DependencyService.Get<IPushNotificationHandler>();

            //global::Xamarin.Forms.Forms.Init();
            //LoadApplication(new App());

            PushNotificationManager.Initialize(options, pushNotificationHandler, true);
            try
            {
                Firebase.Core.App.Configure();
            }
            catch (Exception e)
            {
            }
                Messaging.SharedInstance.Delegate = this;
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
            // Register your app for remote notifications.
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {

                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;

                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
                {
                    Console.WriteLine(granted);
                });
            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }

            UIApplication.SharedApplication.RegisterForRemoteNotifications();
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            // Handle when your app starts
            //Task.Run(async () => await TokenRefreshAsync(null, new PushNotificationTokenEventArgs(CrossPushNotification.Current.Token)));
            CrossPushNotification.Current.OnTokenRefresh += async (s, p) => await TokenRefreshAsync(s, p);

            Services.FirestoreService.Init();
    
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

        [Export("application:didDidRefreshRegistrationToken:")]
        public void DidRefreshRegistrationToken(Messaging messaging, string fcmToken)
        {
            System.Diagnostics.Debug.WriteLine($"FCM Token: {fcmToken}");
            AppDelegate.FcmToken = fcmToken;
        }

        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            Console.WriteLine($"Firebase registration token: {fcmToken}");
            AppDelegate.FcmToken = fcmToken;

            // TODO: If necessary send token to application server.
            // Note: This callback is fired at each app startup and whenever a new token is generated.
            SendDeviceTokenToServerAsync("CheerAppFcmTokens", DeviceId, fcmToken).GetAwaiter();
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
            //DependencyService.Register<IJsonHelper, JsonHelper>();
            DependencyService.Register<IPushNotificationHandler, PushNotificationHandler>();
            DependencyService.Register<IUIServices, UIServices>();
            DependencyService.Register<IApnSettings, ApnSettings>();
            DependencyService.Register<IFcmSettings, FcmSettings>();
            DependencyService.Register<IFcmSender, FcmSender>();
            DependencyServiceExtension.Register<CheerAppScreen>();
            DependencyServiceExtension.Register<SendPushNotification>();
            DependencyServiceExtension.Register<HomeScreen>();
        }

        private async Task TokenRefreshAsync(object s, PushNotificationTokenEventArgs p)
        {
            if (DeviceToken == p.Token)
                return;
            DeviceToken = p.Token;
            await SendDeviceTokenToServerAsync("CheerAppDeviceTokens", DeviceId ,p.Token);

            System.Diagnostics.Debug.WriteLine($"TOKEN REC: {p.Token}");
            return;

            var text =
                $"Name: {DeviceInfo.Name}{System.Environment.NewLine}" +
                $"Device Token: {p.Token}";
            await Clipboard.SetTextAsync(text);

            new UIAlertView("New Device Token", "Please paste new Token to Guy", uIAlertViewDelegate, "OK", null).Show();
        }

        private async Task SendDeviceTokenToServerAsync(string collectionName, string id, string token)
        {
            var data = new System.Collections.Generic.Dictionary<object, object>();
            data.Add("Token", token);
            await DependencyService.Get<IRepository<Event>>().AddUpdateOneAsync(collectionName, id, data);
        }
    }
}