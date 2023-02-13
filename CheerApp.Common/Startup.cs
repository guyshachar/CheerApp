using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common.Implementations;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using CheerApp.iOS.Implementations;
using CorePush.Apple;
using CorePush.Google;
using CorePush.Interfaces;
using CorePush.Interfaces.Apple;
using CorePush.Interfaces.Google;
using CorePush.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CheerApp.Common
{
    public static class Startup
    {
        public const string TOPIC_ALL = "ALL";

        public const string APN_BODY = "aps.alert.body";
        public const string APN_TITLE = "aps.alert.title";

        private static IDictionary<string, CancellationTokenSource> cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();

        public static string DeviceId { get; private set; }

        static Startup()
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} In Ctor...");
        }

        public static IHost Start(
            Action<HostBuilderContext, IServiceCollection> additionalServices,
            Type startPage, string deviceId, params object[] pushHandlerParameters)
        {
            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} Before Forms Init...");

            DeviceId = deviceId;

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

            var uiServices = host.Services.GetService<IUIServices>();
            var startPageInstance = host.Services.GetService(startPage);
            uiServices.Navigate(startPageInstance);

            System.Diagnostics.Debug.WriteLine($"{nameof(Startup)} {nameof(Start)} End...");

            return host;
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddSingleton<IJsonHelper, JsonHelper>();
            services.AddSingleton<IFcmSettings, FcmSettings>();
            services.AddSingleton<IApnSettings, ApnSettings>();
            services.AddSingleton<IFcmSettings, FcmSettings>();
            services.AddTransient<IApnSender, ApnSender>();
            services.AddTransient<IFcmSender, FcmSender>();
            services.AddSingleton<IDbService, DbService>();
        }
    }
}