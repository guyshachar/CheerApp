using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CheerApp.Common;
using CheerApp.iOS.Implementations;
using CheerApp.iOS.Interfaces;
using CheerApp.iOS.Models;
using CorePush.Interfaces;
using Firebase.CloudMessaging;
using Foundation;
using Plugin.PushNotification;
using UIKit;
using UserNotifications;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CheerApp.iOS
{
    public class Startup
    {
        public const string TOPIC_ALL = "ALL";

        private const string APN_BODY = "aps.alert.body";
        private const string APN_TITLE = "aps.alert.title";

        private static IUIAlertViewDelegate uIAlertViewDelegate = null;
        public UIWindow Window { get; private set; }

        private IUNUserNotificationCenterDelegate UserNotificationCenterDelegate;
        private IMessagingDelegate MessagingDelegate;

        private readonly IServiceProvider ServiceProvider;
        private readonly IPushNotificationHandler PushNotificationHandler;
        private readonly IRepository<DeviceDetails> DeviceDetailsRepository;
        private readonly IRepository<Topic> TopicRepository;
        private readonly HomeScreen HomeScreen;
        private readonly IDbService DbService;
        private readonly IUIServices UIServices;

        public static string DeviceId => UIDevice.CurrentDevice.IdentifierForVendor.ToString();

        public Startup(
            IServiceProvider serviceProvider,
            IPushNotificationHandler pushNotificationHandler,
            IRepository<DeviceDetails> deviceDetailsRepository,
            IRepository<Topic> topicRepository,
            HomeScreen homeScreen,
            IDbService dbService,
            IUIServices uiServices)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} In Ctor...");

            ServiceProvider = serviceProvider;
            PushNotificationHandler = pushNotificationHandler;
            DeviceDetailsRepository = deviceDetailsRepository;
            TopicRepository = topicRepository;
            HomeScreen = homeScreen;
            DbService = dbService;
            UIServices = uiServices;
        }

        public void Start(
            IMessagingDelegate messagingDelegate,
            IUNUserNotificationCenterDelegate userNotificationCenterDelegate,
            UIApplication app,
            NSDictionary options)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} Before Forms Init...");
            Xamarin.Forms.Forms.Init();

            this.Window = new UIWindow(UIScreen.MainScreen.Bounds);

            MessagingDelegate = messagingDelegate;
            UserNotificationCenterDelegate = userNotificationCenterDelegate;

            //global::Xamarin.Forms.Forms.Init();
            //LoadApplication(new App());

            PushNotificationManager.Initialize(options, PushNotificationHandler, true);
            try
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} Before Firebase app configure ...");
                Firebase.Core.App.Configure();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} InFirebase app configure exception {e.Message}");
            }

            Messaging.SharedInstance.Delegate = MessagingDelegate;
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
            // Register your app for remote notifications.
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {

                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = UserNotificationCenterDelegate;

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

            InitiatePushHandlers();

            //Implementations.FirestoreService.Init();

            //---- instantiate a new navigation controller
            var rootNavigationController = new UINavigationController();
            //---- add the home screen to the navigation controller (it'll be the top most screen)
            rootNavigationController.PushViewController(HomeScreen, false);

            //---- set the root view controller on the window. the nav controller will handle the rest
            this.Window.RootViewController = rootNavigationController;

            this.Window.MakeKeyAndVisible();
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} End...");
        }

        private void InitiatePushHandlers()
        {
            //await Clipboard.SetTextAsync(CrossPushNotification.Current.Token);
            //SetMessageText(lblMain, $"{this.lblMain.Text} TOKEN REC: {CrossPushNotification.Current.Token}");

            CrossPushNotification.Current.OnTokenRefresh += async (s, p) => await TokenRefreshAsync(s, p);

            CrossPushNotification.Current.OnNotificationReceived += async (s, p) => await NotificationReceivedAsync(s, p);

            CrossPushNotification.Current.OnNotificationOpened += async (s, p) => await NotificationOpenedAsync(s, p);

            CrossPushNotification.Current.OnNotificationDeleted += async (s, p) => await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine("Dismissed");
            });
        }

        private async Task TokenRefreshAsync(object s, PushNotificationTokenEventArgs p)
        {
            await DbService.SendDeviceDetailsToServerAsync(apnToken: p.Token);

            System.Diagnostics.Debug.WriteLine($"TOKEN REC: {p.Token}");
            return;

            var text =
                $"Name: {DeviceInfo.Name}{System.Environment.NewLine}" +
                $"Device Token: {p.Token}";
            await Clipboard.SetTextAsync(text);

            new UIAlertView("New Device Token", "Please paste new Token to Guy", uIAlertViewDelegate, "OK", null).Show();
        }

        private async Task NotificationReceivedAsync(object s, PushNotificationDataEventArgs p)
        {
            try
            {
                var duration = TimeSpan.FromSeconds(2);
                Vibration.Vibrate(duration);

                if (p.Data.ContainsKey(APN_BODY))
                {
                    System.Diagnostics.Debug.WriteLine($"Received: {p.Data[APN_BODY]}");
                    var notification = JsonSerializer.Deserialize<Notification>(p.Data[APN_BODY].ToString());
                    if (p.Data.ContainsKey(APN_TITLE))
                    {
                        notification.Title = p.Data[APN_TITLE].ToString();
                    }
                    if (notification.ScreenName == nameof(ShowRoom) && typeof(ShowRoom).GetInterface(nameof(IScreenActions)) != null)
                    {
                        var showRoom = (UIViewController)ServiceProvider.GetService(typeof(ShowRoom));
                        var task = Task.Run(() => ((IScreenActions)showRoom).ReceivedNotificationAsync(notification));

                        var topViewController = ((UINavigationController)Window.RootViewController).TopViewController;
                        if (topViewController != null && topViewController != showRoom)
                            ShowOKCancelAlert("New ShowRoom Notification Arrived",
                                "Do you want to move to participate ?",
                                notification,
                                alert => topViewController.NavigationController.PushViewController(showRoom, false));
                    }
                }
                else
                    System.Diagnostics.Debug.WriteLine("Received empty body");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task NotificationOpenedAsync(object s, PushNotificationResponseEventArgs p)
        {
            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine("Opened");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }
            });
        }

        public void ShowOKCancelAlert(string title, string description, Notification notification, Action<UIAlertAction> action)
        {
            //Create Alert
            var okCancelAlertController = UIAlertController.Create(title, description, UIAlertControllerStyle.Alert);

            //Add Actions
            okCancelAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action));
            okCancelAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, alert => Console.WriteLine("Cancel was clicked")));

            //Present Alert
            this.Window.RootViewController.PresentViewController(okCancelAlertController, true, null);
        }

        public static void SetMessageText(object obj, string text)
        {
            InvokeFromUI(() =>
            {
                if (obj is UILabel)
                    ((UILabel)obj).Text = text;
                else if (obj is UITextField)
                    ((UITextField)obj).Text = text;
            });
        }

        public static void InvokeFromUI(Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }
    }
}