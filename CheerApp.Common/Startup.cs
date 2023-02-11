using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common.Implementations;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using CheerApp.iOS.Implementations;
using CoreBluetooth;
using CorePush.Apple;
using CorePush.Google;
using CorePush.Interfaces;
using CorePush.Interfaces.Apple;
using CorePush.Interfaces.Google;
using CorePush.Utils;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plugin.PushNotification;
using UIKit;
using UserNotifications;

namespace CheerApp.Common
{
    public static class Startup
    {
        public const string TOPIC_ALL = "ALL";

        public const string APN_BODY = "aps.alert.body";
        public const string APN_TITLE = "aps.alert.title";

        private static IUIAlertViewDelegate uIAlertViewDelegate = null;
        public static UIWindow Window { get; private set; }

        private static IDictionary<string, CancellationTokenSource> cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();

        public static string DeviceId => UIDevice.CurrentDevice.IdentifierForVendor.ToString();

        static Startup()
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} In Ctor...");
        }

        public static IHost Start(
            Action<HostBuilderContext, IServiceCollection> additionalServices,
            Type startPage, params object[] pushHandlerParameters)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} Before Forms Init...");

            var hostBuilder = new HostBuilder()
               .ConfigureServices(ConfigureServices);
            if (additionalServices != null)
                hostBuilder = hostBuilder.ConfigureServices(additionalServices);
            var host = hostBuilder
            .Build();

            var dbService = host.Services.GetService<IDbService>();
            Task.Run(async () => await dbService.SendDeviceDetailsToServerAsync());

            var pushHandler = host.Services.GetService<IPushHandler>();
            pushHandler.Start(pushHandlerParameters);

            var firestoreProvider = host.Services.GetService<IFirestoreProvider>();
            Task.Run(() => firestoreProvider.RegisterListener<DeviceDetail>(DeviceId, async (obj) => await pushHandler.HandleUpdatedDocumentAsync(obj)));

            var startPageInstance = (UIViewController)host.Services.GetService(startPage);

            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            //Implementations.FirestoreService.Init();

            //---- instantiate a new navigation controller
            var rootNavigationController = new UINavigationController();
            //---- add the home screen to the navigation controller (it'll be the top most screen)
            rootNavigationController.PushViewController(startPageInstance, false);

            //---- set the root view controller on the window. the nav controller will handle the rest
            Window.RootViewController = rootNavigationController;

            Window.MakeKeyAndVisible();
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} End...");

            return host;
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddSingleton<IJsonHelper, JsonHelper>();
            services.AddSingleton<IFcmSettings, FcmSettings>();
            services.AddSingleton<IUIServices, UIServices>();
            services.AddSingleton<IApnSettings, ApnSettings>();
            services.AddSingleton<IFcmSettings, FcmSettings>();
            services.AddTransient<IApnSender, ApnSender>();
            services.AddTransient<IFcmSender, FcmSender>();
            services.AddSingleton<IDbService, DbService>();
        }
    }
}