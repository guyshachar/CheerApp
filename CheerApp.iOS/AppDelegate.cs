using System;
using System.Reflection;
using CheerApp.Common.Models;
using CheerApp.iOS.Implementations;
using CheerApp.iOS.Interfaces;
using CheerApp.iOS.Models;
using CorePush.Apple;
using CorePush.Google;
using CorePush.Interfaces;
using CorePush.Interfaces.Apple;
using CorePush.Interfaces.Google;
using CorePush.Utils;
using Firebase.CloudMessaging;
using Foundation;
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
            Host = new HostBuilder()
               .ConfigureServices(ConfigureServices)
               .ConfigureServices(services => services.AddSingleton<Startup>())
               .Build();
            var startup = Host.Services
               .GetService<Startup>();
            dbService = Host.Services
               .GetService<IDbService>();

            System.Diagnostics.Debug.WriteLine($"{nameof(AppDelegate)} {nameof(FinishedLaunching)} After Host build...");
            startup.Start(this, this, app, options);
            System.Diagnostics.Debug.WriteLine($"{nameof(AppDelegate)} {nameof(FinishedLaunching)} After Start call...");

            return true;
        }

        [Export("application:didReceiveLocalNotification:")]
        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            // show an alert
            var body = new ShowRoomNotification();
            //var a = new UIAlertView(notification.AlertAction, body.Body, uIAlertViewDelegate, "OK", null).Show();
            Assembly asm = typeof(AppDelegate).Assembly;
            Type screenType = asm.GetType(body.ScreenName);
            var uiViewController = DependencyServiceExtension.Get(screenType);

            var uiServices = DependencyService.Resolve<IUIServices>();
            if (body.Action == "ResetShow")
            {
                //uiServices.ResetShowAsync(uiViewController, notification);
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

        private void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddSingleton<IJsonHelper, JsonHelper>();
            services.AddSingleton<IFcmSettings, FcmSettings>();
            services.AddSingleton<IPushNotificationHandler, PushNotificationHandler>();
            services.AddSingleton<IUIServices, UIServices>();
            services.AddSingleton<IApnSettings, ApnSettings>();
            services.AddSingleton<IFcmSettings, FcmSettings>();
            services.AddTransient<IApnSender, ApnSender>();
            services.AddTransient<IFcmSender, FcmSender>();
            services.AddSingleton<ShowRoom>();
            services.AddSingleton<SendPushNotification>();
            services.AddSingleton<HomeScreen>();
            services.AddSingleton<IRepository<DeviceDetails>, DeviceDetailsRepository>();
            services.AddSingleton<IRepository<Topic>, TopicRepository>();
            services.AddSingleton<IDbService, DbService>();
        }
    }
}