using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVFoundation;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Drawing;
using System.Text.Json.Serialization;

using System.Text.Json;
using Plugin.PushNotification;
using CoreAudioKit;
using CheerApp.Common;
using Command = CheerApp.Common.Command;
using static Xamarin.Forms.Internals.GIFBitmap;
using CheerApp.iOS.Models;
using CorePush;
using CheerApp.iOS.Interfaces;
using CorePush.Interfaces;
using CorePush.Utils;

namespace CheerApp.iOS
{
    public partial class ShowRoom : UIViewController, IScreenActions
    {
        private readonly IUIServices UIServices;
        private readonly IRepository<DeviceDetails> DeviceDetailsRepository;
        private readonly IDbService DbService;
        private readonly IJsonHelper JsonHelper;

        private Notification lastNotification;

        //loads the HelloWorldScreen.xib file and connects it to this object
        public ShowRoom(
            IUIServices uIServices,
            IRepository<DeviceDetails> DeviceDetailsRepository,
            IDbService dbService,
            IJsonHelper jsonHelper) : base(nameof(ShowRoom), null)
        {
            this.UIServices = uIServices;
            this.DeviceDetailsRepository = DeviceDetailsRepository;
            this.DbService = dbService;
            this.JsonHelper = jsonHelper;

            this.Title = "Cheer App by Guy Shachar";
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (lastNotification != null && lblMain.Text != lastNotification.Title)
                Startup.SetMessageText(lblMain, lastNotification.Title);

            var deviceDetails = await DeviceDetailsRepository.GetAsync(Startup.DeviceId);
            this.txtTopics.Text = deviceDetails.Topics;
            this.btnTopics.TouchUpInside += async (sender, e) => await SendAsync(sender, e);
        }
   
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            UIServices.ToggleFlashAsync(false);
            this.btnTopics.TouchUpInside -= async (sender, e) => await SendAsync(sender, e);
        }

        private async Task SendAsync(object sender, EventArgs e)
        {
            await DbService.SendDeviceDetailsToServerAsync(topics: txtTopics.Text);
        }

        private void CreateLocalNotification()
        {
            var localNotification = new UILocalNotification();
            //var remoteNotification = new UIRemoteNotificatio

            //---- set the fire date (the date time in which it will fire)
            var fireDate = DateTime.Now.AddSeconds(20);
            localNotification.FireDate = (NSDate)fireDate;
            localNotification.HasAction = false;

            //---- configure the alert stuff
            localNotification.AlertAction = "View Alert";
            var notificationBody = new Notification
            {
                ScreenName = typeof(ShowRoom).FullName,
                Action = "ResetShow",
                //Json = CreateJson()
            };
            localNotification.AlertBody = JsonSerializer.Serialize(notificationBody);

            //---- modify the badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber++;
            localNotification.ApplicationIconBadgeNumber = UIApplication.SharedApplication.ApplicationIconBadgeNumber;

            //---- set the sound to be the default sound
            localNotification.SoundName = UILocalNotification.DefaultSoundName;

            //				notification.UserInfo = new NSDictionary();
            //				notification.UserInfo[new NSString("Message")] = new NSString("Your 5 secs notification has fired!");

            //---- schedule it
            UIApplication.SharedApplication.ScheduleLocalNotification(localNotification);
        }

        private async Task StartBackgroundProcess(string json)
        {
            var commands = JsonSerializer.Deserialize<IEnumerable<Command>>(json);

            foreach (var command in commands)
            {
                await Task.Delay(command.SkipTime);

                switch (command.Feature)
                {
                    case FeatureEnum.Flash:
                        var onOff = JsonSerializer.Deserialize<bool>(command.CommandDetails);
                        await UIServices.ToggleFlashAsync(onOff);
                        break;
                    case FeatureEnum.Screen:
                        var rgb = JsonSerializer.Deserialize<RGB>(command.CommandDetails);
                        var color = UIColor.FromRGB(rgb.R, rgb.G, rgb.B);
                        await UIServices.PaintScreenAsync(this, color);
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task ReceivedNotificationAsync(Notification notification)
        {
            this.lastNotification = notification;
            Startup.SetMessageText(lblMain, notification.Title);
            await PlayShowRoomAsync(notification);
        }

        private async Task PlayShowRoomAsync(Notification notification)
        {
            await UIServices.SyncActivityAsync(notification.StartTime);

            string jsonShow = notification.Json;
            var commands = JsonHelper.Deserialize<IEnumerable<Command>>(jsonShow);

            foreach (var command in commands)
            {
                await Task.Delay(command.SkipTime);

                switch (command.Feature)
                {
                    case FeatureEnum.Flash:
                        var onOff = JsonHelper.Deserialize<bool>(command.CommandDetails);
                        await UIServices.ToggleFlashAsync(onOff);
                        break;
                    case FeatureEnum.Screen:
                        var rgb = JsonHelper.Deserialize<RGB>(command.CommandDetails);
                        var color = UIColor.FromRGB(rgb.R, rgb.G, rgb.B);
                        await UIServices.PaintScreenAsync(this, color);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}