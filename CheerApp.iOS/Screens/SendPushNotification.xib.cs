using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CheerApp.Common;
using Command = CheerApp.Common.Command;
using CorePush.Interfaces;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using CorePush.Apple;
using CorePush.Utils;
using CorePush;
using System.Threading;
using CorePush.Google;
using Plugin.PushNotification;
using CheerApp.Models;
using CheerApp;
using System.Collections;

namespace CheerApp.iOS
{
    public partial class SendPushNotification : UIViewController
    {
        private readonly string[] deviceTokens = new string[]
            {
            };

        //loads the HelloUniverseScreen.xib file and connects it to this object
        public SendPushNotification() : base(nameof(SendPushNotification), null)
        {
            this.Title = "Send Push Notification";
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            btnSend.TouchUpInside += async (sender, e) =>
            {
                var json = CreateJson();
                this.lblMessageBody.Text = json;
                var notification = new Notification
                {
                    ScreenName = nameof(SendPushNotification),
                    Action = "Show",
                    StartTime = DateTime.UtcNow.AddSeconds(5),
                    Json = json
                };

                /*
                 * var payload = new AppleNotification(
                                   Guid.NewGuid(),
                                   JsonSerializer.Serialize(notification),
                                   this.txtMessageTitle.Text);
                */

                var payload = new FcmMessage(AppDelegate.FcmToken, this.txtMessageTitle.Text, JsonSerializer.Serialize(notification));
                
                var tasks = new List<Task>();
                var tokens = await DependencyService.Get<IRepository<Event>>().GetAllAsync("CheerAppFcmTokens");

                foreach (var token in tokens)
                {
                    var fcmSender = DependencyService.Get<IFcmSender>(DependencyFetchTarget.NewInstance);
                    tasks.Add(fcmSender.SendAsync(token.Token, payload, CancellationToken.None));
                };
                var task = Task.WhenAll(tasks);
            };
        }

        private static string CreateJson()
        {
            var rnd = new Random();
            var commands = new List<Command>();
            for (var i = 0; i < 30; i++)
            {
                var t = rnd.Next(1000, 2000);
                var f = rnd.Next(2);
                if (f == 0)
                {
                    var o = rnd.Next(2);
                    commands.Add(new Command
                    {
                        SkipTime = t,
                        Feature = FeatureEnum.Flash,
                        CommandDetails = JsonSerializer.Serialize(o == 1)
                    });
                }
                else
                {
                    var c = GetColorRGB();
                    commands.Add(new Command
                    {
                        SkipTime = t,
                        Feature = FeatureEnum.Screen,
                        CommandDetails = JsonSerializer.Serialize(c)
                    }); ;
                }
            }

            return JsonSerializer.Serialize(commands);

            RGB GetColorRGB()
            {
                var r = rnd.Next(255);
                var g = rnd.Next(255);
                var b = rnd.Next(255);
                return new RGB(r, g, b);
            }
        }
    }
}