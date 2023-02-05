using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common;
using CheerApp.Common.Models;
using CheerApp.iOS.Models;
using CorePush.Google;
using CorePush.Interfaces;
using CorePush.Interfaces.Apple;
using CorePush.Interfaces.Google;
using UIKit;
using Command = CheerApp.Common.Command;

namespace CheerApp.iOS
{
    public partial class SendPushNotification : UIViewController
    {
        private readonly IRepository<DeviceDetails> DeviceDetailsRepository;
        private readonly IRepository<Topic> TopicRepository;
        private readonly IApnSender ApnSender;
        private readonly IFcmSender FcmSender;
        
        //loads the HelloUniverseScreen.xib file and connects it to this object
        public SendPushNotification(
            IRepository<DeviceDetails> deviceDetailsRepository,
            IRepository<Topic> topicRepository,
            IApnSender apnSender,
            IFcmSender fcmSender) : base(nameof(SendPushNotification), null)
        {
            DeviceDetailsRepository = deviceDetailsRepository;
            TopicRepository = topicRepository;
            ApnSender = apnSender;
            FcmSender = fcmSender;

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
            var notification = new ShowRoomNotification
            {
                ScreenName = nameof(ShowRoom),
                Action = "Show",
                StartTime = DateTime.UtcNow.AddSeconds(15),
                Json = json
            };

            var deviceDetails = await DeviceDetailsRepository.GetAsync(Startup.DeviceId);

            /*
             * var payload = new AppleNotification(
                               Guid.NewGuid(),
                               JsonSerializer.Serialize(notification),
                               this.txtMessageTitle.Text);
            */

            var payload = new FcmMessage(deviceDetails.FcmToken, "New notification from Cheer App", this.txtMessageTitle.Text, notification);

            var tasks = new List<Task>();
            var topicIds = (string.IsNullOrEmpty(txtTopics.Text) ? Startup.TOPIC_ALL : txtTopics.Text).Split(',');
            var deviceIds = new List<string>();
            foreach (var topicId in topicIds)
            {
                var topic = await TopicRepository.GetAsync(topicId);
                if (topic == null)
                    continue;
                deviceIds.AddRange(topic.DeviceIds.Split(','));
            }
            foreach (var deviceId in deviceIds.Distinct().Where(x => !string.IsNullOrEmpty(x)))
            {
                var deviceDetail = await DeviceDetailsRepository.GetAsync(deviceId);
                var fcmToken = deviceDetail?.FcmToken;
                if (fcmToken == null || fcmToken == "<null>")
                    continue;
                tasks.Add(FcmSender.SendAsync(deviceDetail.FcmToken, payload, CancellationToken.None));
            };
            var task = Task.WhenAll(tasks);

            //Startup.ShowAlert(this, "Info", $"{tasks.Count} Push Notification(s) sent...", ("OK", UIAlertActionStyle.Default, null));
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