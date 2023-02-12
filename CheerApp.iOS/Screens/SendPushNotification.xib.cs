using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common;
using CheerApp.Common.Implementations;
using CheerApp.Common.Interfaces;
using CheerApp.Common.Models;
using CorePush.Google;
using CorePush.Interfaces.Apple;
using CorePush.Interfaces.Google;
using UIKit;
using Xamarin.Forms;
using Command = CheerApp.Common.Command;

namespace CheerApp.iOS
{
    public partial class SendPushNotification : UIViewController
    {
        private readonly IFirestoreProvider _firestoreProvider;
        private readonly IApnSender _apnSender;
        private readonly IFcmSender _fcmSender;
        
        //loads the HelloUniverseScreen.xib file and connects it to this object
        public SendPushNotification(
            IFirestoreProvider firestoreProvider,
            IApnSender apnSender,
            IFcmSender fcmSender) : base(nameof(SendPushNotification), null)
        {
            _firestoreProvider = firestoreProvider;
            _apnSender = apnSender;
            _fcmSender = fcmSender;

            this.Title = "Send Push Notification";
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            btnSend.TouchUpInside += async (sender, e) => await SendAsync(sender, e);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            btnSend.TouchUpInside -= async (sender, e) => await SendAsync(sender, e);
        }

        private async Task SendAsync(object sender, EventArgs e)
        {
            var json = CreateJson();
            this.lblMessageBody.Text = json;

            var message = new Message
            {
                Title = txtMessageTitle.Text,
                Page = typeof(ShowRoom).FullName,
                Action = "Play",
                StartTime = DateTime.UtcNow.AddSeconds(15).ToString("u"),
                Json = json
            };

            await _firestoreProvider.AddUpdateAsync(message);

            var deviceDetails = await _firestoreProvider.GetAsync<DeviceDetail>(Startup.DeviceId);

            //var payload = new FcmMessage(deviceDetails.FcmToken, "New notification from Cheer App", this.txtMessageTitle.Text, notification);

            var tasks = new List<Task>();
            var topicIds = (string.IsNullOrEmpty(txtTopics.Text) ? Startup.TOPIC_ALL : txtTopics.Text).Split(',').ToList();
            var deviceIds = new List<string>();
            foreach (var topicId in topicIds)
            {
                var topic = await _firestoreProvider.GetAsync<Topic>(topicId);
                if (topic == null)
                    continue;
                deviceIds.AddRange(topic.DeviceIds);
            }
            deviceIds = deviceIds.Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
            foreach (var deviceId in deviceIds)
            {
                var deviceDetail = await _firestoreProvider.GetAsync<DeviceDetail>(deviceId);
                deviceDetail.NewMessageIds.Add(message.Id);
                if (deviceDetail == null)
                    continue;
                //var fcmToken = deviceDetail?.FcmToken;
                //if (fcmToken == null || fcmToken == "<null>")
                //    continue;
                //tasks.Add(_fcmSender.SendAsync(deviceDetail.FcmToken, payload, CancellationToken.None));
                tasks.Add(_firestoreProvider.AddUpdateAsync(deviceDetail));
            };
            var task = Task.WhenAll(tasks);

            if (!deviceIds.Contains(Startup.DeviceId))
                UIServices.ShowAlert(this, "Info", $"Message sent to {tasks.Count} Devices", ("OK", UIAlertActionStyle.Default, null));
        }

        private static string CreateJson()
        {
            var rnd = new Random();
            var commands = new List<Command>();
            for (var i = 0; i < 30; i++)
            {
                var t = rnd.Next(500, 2000);
                var f = rnd.Next(2);
                if (f == 0)
                {
                    var o = rnd.Next(2);
                    commands.Add(new Command
                    {
                        Feature = FeatureEnum.Flash,
                        CommandDetails = JsonSerializer.Serialize(o == 1),
                        CommandTime = t
                    });
                }
                else
                {
                    var c = GetColorRGB();
                    commands.Add(new Command
                    {
                        CommandTime = t,
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