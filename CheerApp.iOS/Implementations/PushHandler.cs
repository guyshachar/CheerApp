using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common.Implementations;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using Firebase.CloudMessaging;
using Foundation;
using Plugin.PushNotification;
using UIKit;
using UserNotifications;
using Xamarin.Essentials;
using System.Linq;

namespace CheerApp.iOS.Implementations
{
    public class PushHandler: IPushHandler
    {
        public const string APN_BODY = "aps.alert.body";
        public const string APN_TITLE = "aps.alert.title";

        private static IUIAlertViewDelegate uIAlertViewDelegate = null;
        public UIWindow Window { get; private set; }

        private IUNUserNotificationCenterDelegate _userNotificationCenterDelegate;
        private IMessagingDelegate _messagingDelegate;
        private UIApplication _app;
        private NSDictionary _options;

        private readonly IServiceProvider _serviceProvider;
        private readonly IPushNotificationHandler _pushNotificationHandler;
        private readonly IDbService _dbService;
        private readonly IUIServices _uiServices;
        private readonly IFirestoreProvider _firestoreProvider;

        public static string DeviceId => UIDevice.CurrentDevice.IdentifierForVendor.ToString();
        private IDictionary<string, CancellationTokenSource> cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();

        public PushHandler(
            IServiceProvider serviceProvider,
            IPushNotificationHandler pushNotificationHandler,
            IDbService dbService,
            IUIServices uiServices,
            IFirestoreProvider firestoreProvider)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(PushHandler)} In Ctor...");

            _serviceProvider = serviceProvider;
            _pushNotificationHandler = pushNotificationHandler;
            _dbService = dbService;
            _uiServices = uiServices;
            _firestoreProvider = firestoreProvider;
        }

        public void Start(params object[] messageHandlerParameters)
        {
            _messagingDelegate = (IMessagingDelegate)messageHandlerParameters[0];
            _userNotificationCenterDelegate = (IUNUserNotificationCenterDelegate)messageHandlerParameters[0];
            _app = (UIApplication)messageHandlerParameters[1];
            _options = (NSDictionary)messageHandlerParameters[2];

            System.Diagnostics.Debug.WriteLine($"{nameof(PushHandler)} {nameof(Start)} Before Forms Init...");
     
            PushNotificationManager.Initialize(_options, _pushNotificationHandler, true);
            Messaging.SharedInstance.Delegate = _messagingDelegate;
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
            // Register your app for remote notifications.
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {

                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = _userNotificationCenterDelegate;

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

    
            InitiatePushHandlers();
            System.Diagnostics.Debug.WriteLine($"{nameof(PushHandler)} {nameof(Start)} End...");
        }

        private void InitiatePushHandlers()
        {
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
            _dbService.SendDeviceDetailsToServerAsync(fcmToken: fcmToken).GetAwaiter();
        }

        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            Console.WriteLine($"Firebase registration token: {fcmToken}");

            // TODO: If necessary send token to application server.
            // Note: This callback is fired at each app startup and whenever a new token is generated.
            _dbService.SendDeviceDetailsToServerAsync(fcmToken: fcmToken).GetAwaiter();
        }

        private ConcurrentDictionary<(Type, string), object> objectSnapshots = new ConcurrentDictionary<(Type, string), object>();
        public async Task HandleUpdatedDocumentAsync<T>(T obj) where T : ModelBase
        {
            if (objectSnapshots.TryGetValue((typeof(T), obj.Id), out var savedObj))
            {
                if (typeof(T).Equals(typeof(DeviceDetail)))
                {
                    var messageId = (obj as DeviceDetail).MessageIds.Except(((DeviceDetail)savedObj).MessageIds).LastOrDefault();
                    if (messageId == null)
                        return;
                    var message = await _firestoreProvider.GetAsync<Message>(messageId);
                    if (message == null)
                        return;
                    await TriggerShowRoomAsync(message);
                }
            }
        }

        private async Task TokenRefreshAsync(object s, PushNotificationTokenEventArgs p)
        {
            await _dbService.SendDeviceDetailsToServerAsync(apnToken: p.Token);

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

                //var notification = new CheerAppNotification(p.Data);
                var message = new Message();
                System.Diagnostics.Debug.WriteLine($"Received: {message.Json}");
                if (message.Page == nameof(ShowRoom) && typeof(ShowRoom).GetInterface(nameof(IPageActions)) != null)
                {
                    await TriggerShowRoomAsync(message);
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

        private async Task TriggerShowRoomAsync(Message message)
        {
            var showRoom = (UIViewController)_serviceProvider.GetService(typeof(ShowRoom));
            var topViewController = ((UINavigationController)Window.RootViewController).TopViewController;
            var cancelToken = false;
            if (cancellationTokenSources.ContainsKey(message.Page))
                cancelToken = true;
            else
                cancellationTokenSources.Add(message.Page, new CancellationTokenSource());

            if (topViewController != null && topViewController != showRoom)
                UIServices.ShowAlert(
                    this.Window.RootViewController,
                    "New ShowRoom Notification Arrived",
                    "Do you want to play ?",
                    ("OK", UIAlertActionStyle.Default,
                        async (alert) =>
                        {
                            if (cancelToken)
                                cancellationTokenSources[message.Page].Cancel();
                            topViewController.NavigationController.PushViewController(showRoom, false);
                            await ((IPageActions)showRoom).ReceivedNotificationAsync(message, cancellationTokenSources[message.Page].Token);
                        }
                ),
                    ("Cancel", UIAlertActionStyle.Cancel, null));
            else
            {
                if (cancelToken)
                    cancellationTokenSources[message.Page].Cancel();
                await((IPageActions)showRoom).ReceivedNotificationAsync(message, cancellationTokenSources[message.Page].Token);
            }
        }
    }
}