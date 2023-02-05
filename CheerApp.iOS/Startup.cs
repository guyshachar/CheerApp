using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CheerApp.Common;
using CheerApp.Common.Models;
using CheerApp.iOS.Implementations;
using CheerApp.iOS.Interfaces;
using CheerApp.iOS.Models;
using CoreAudioKit;
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

        public const string APN_BODY = "aps.alert.body";
        public const string APN_TITLE = "aps.alert.title";

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

        private IDictionary<string, CancellationTokenSource> cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();

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

        [Export("application:didDidRefreshRegistrationToken:")]
        public void DidRefreshRegistrationToken(Messaging messaging, string fcmToken)
        {
            System.Diagnostics.Debug.WriteLine($"FCM Token: {fcmToken}");
            DbService.SendDeviceDetailsToServerAsync(fcmToken: fcmToken).GetAwaiter();
        }

        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            Console.WriteLine($"Firebase registration token: {fcmToken}");

            // TODO: If necessary send token to application server.
            // Note: This callback is fired at each app startup and whenever a new token is generated.
            DbService.SendDeviceDetailsToServerAsync(fcmToken: fcmToken).GetAwaiter();
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

                var notification = new CheerAppNotification(p.Data);
                System.Diagnostics.Debug.WriteLine($"Received: {notification.Body}");
                if (notification.ScreenName == nameof(ShowRoom) && typeof(ShowRoom).GetInterface(nameof(IScreenActions)) != null)
                {
                    var showRoom = (UIViewController)ServiceProvider.GetService(typeof(ShowRoom));
                    var topViewController = ((UINavigationController)Window.RootViewController).TopViewController;
                    var cancelToken = false;
                    if (cancellationTokenSources.ContainsKey(notification.ScreenName))
                        cancelToken = true;
                    else
                        cancellationTokenSources.Add(notification.ScreenName, new CancellationTokenSource());

                    if (topViewController != null && topViewController != showRoom)
                        ShowAlert(
                            this.Window.RootViewController,
                            "New ShowRoom Notification Arrived",
                            "Do you want to play ?",
                            ("OK", UIAlertActionStyle.Default,
                                async (alert) =>
                                {
                                    if (cancelToken)
                                        cancellationTokenSources[notification.ScreenName].Cancel();
                                    topViewController.NavigationController.PushViewController(showRoom, false);
                                    await ((IScreenActions)showRoom).ReceivedNotificationAsync(notification, cancellationTokenSources[notification.ScreenName].Token);
                                }
                        ),
                            ("Cancel", UIAlertActionStyle.Cancel, null));
                    else
                    {
                        if (cancelToken)
                            cancellationTokenSources[notification.ScreenName].Cancel();
                        await ((IScreenActions)showRoom).ReceivedNotificationAsync(notification, cancellationTokenSources[notification.ScreenName].Token);
                    }
                }
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

        public static void ShowAlert(UIViewController viewController, string title, string description, params (string actionName, UIAlertActionStyle alertActionStyle, Action<UIAlertAction> action)[] actions)
        {
            //Create Alert
            var okCancelAlertController = UIAlertController.Create(title, description, UIAlertControllerStyle.Alert);

            //Add Actions
            foreach (var action in actions)
                okCancelAlertController.AddAction(UIAlertAction.Create(action.actionName, action.alertActionStyle, action.action));
  
            //Present Alert
            viewController.PresentViewController(okCancelAlertController, true, null);
        }

        public static void SetMessageText(object obj, string text)
        {
            InvokeUIAction(null, () =>
            {
                if (obj is UILabel)
                    ((UILabel)obj).Text = text;
                else if (obj is UITextField)
                    ((UITextField)obj).Text = text;
            });
        }

        public static void InvokeUIAction(UIViewController viewController, Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }
    }
}