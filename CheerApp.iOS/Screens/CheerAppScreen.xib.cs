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

namespace CheerApp.iOS
{
    public partial class CheerAppScreen : UIViewController
    {
        private const string APN_BODY = "aps.alert.body";
        private const string APN_TITLE = "aps.alert.title";

        private readonly IUIServices uiServices;

        //loads the HelloWorldScreen.xib file and connects it to this object
        public CheerAppScreen() : base(nameof(CheerAppScreen), null)
        {
            this.uiServices = DependencyService.Get<IUIServices>();
            this.Title = "Cheer App by Guy Shachar";
        }

        bool viewIsFocused = false;

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            this.viewIsFocused = true;
            var sv = base.View.Subviews.FirstOrDefault();
            if (sv == null)
                return;

            await InitiatePushHandlersAsync();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            this.viewIsFocused = false;
            uiServices.ToggleFlashAsync(false);
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
                ScreenName = typeof(CheerAppScreen).FullName,
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

        private async Task InitiatePushHandlersAsync()
        {
            await Clipboard.SetTextAsync(CrossPushNotification.Current.Token);
            //SetMessageText(lblMain, $"{this.lblMain.Text} TOKEN REC: {CrossPushNotification.Current.Token}");

            CrossPushNotification.Current.OnNotificationReceived += async (s, p) => await NotificationReceivedAsync(s, p);

            CrossPushNotification.Current.OnNotificationOpened += async (s, p) => await NotificationOpenedAsync(s, p);

            CrossPushNotification.Current.OnNotificationDeleted += async (s, p) => 
            {
                System.Diagnostics.Debug.WriteLine("Dismissed");
            };
        }

        private void SetMessageText(object obj, string text)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (obj is UILabel)
                    ((UILabel)obj).Text = text;
                else if (obj is UITextField)
                    ((UITextField)obj).Text = text;
            });
        }

        private async Task NotificationReceivedAsync(object s, PushNotificationDataEventArgs p)
        {
            try
            {
                var duration = TimeSpan.FromSeconds(1);
                Vibration.Vibrate(duration);

                System.Diagnostics.Debug.WriteLine("Received");
                if (p.Data.ContainsKey(APN_TITLE))
                {
                    SetMessageText(lblMain, $"{p.Data[APN_TITLE]}");
                }
                if (p.Data.ContainsKey(APN_BODY))
                {
                    var body = JsonSerializer.Deserialize<Notification>(p.Data[APN_BODY].ToString());
                    await uiServices.SyncActivityAsync(body.StartTime);
                    await uiServices.ResetShowAsync(this, body.Json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task NotificationOpenedAsync(object s, PushNotificationResponseEventArgs p)
        {
            System.Diagnostics.Debug.WriteLine("Opened");
            foreach (var data in p.Data)
            {
                System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
            }

            if (!string.IsNullOrEmpty(p.Identifier))
            {
                SetMessageText(lblMain, p.Identifier);
            }
            else if (p.Data.ContainsKey("color"))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //mPage.Navigation.PushAsync(new ContentPage()
                    //{
                    //    BackgroundColor = Color.FromHex($"{p.Data["color"]}")
                    //});
                });
            }
            else if (p.Data.ContainsKey("aps.alert.title"))
            {
                SetMessageText(lblMain, $"{p.Data["aps.alert.title"]}");
            }
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
                        await uiServices.ToggleFlashAsync(onOff);
                        break;
                    case FeatureEnum.Screen:
                        var rgb = JsonSerializer.Deserialize<RGB>(command.CommandDetails);
                        var color = UIColor.FromRGB(rgb.R, rgb.G, rgb.B);
                        await uiServices.PaintScreenAsync(this, color);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}