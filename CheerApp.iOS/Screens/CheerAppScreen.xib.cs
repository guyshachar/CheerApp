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

            this.Title = "יאללה אופק ביטון";
        }

        int y = 0;
        bool viewIsFocused = false;

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            this.viewIsFocused = true;
            var sv = base.View.Subviews.FirstOrDefault();
            if (sv == null)
                return;

            y += 30;
            var lbl = new UILabel();
            lbl.Text = $"guy shachar {DateTime.Now.Ticks}";
            lbl.TextColor = UIColor.Brown;
            lbl.Frame = new CoreGraphics.CGRect(20, y, 200, 50);
            View.Add(lbl);

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
            SetMessageText($"{this.lblMain.Text} TOKEN REC: {CrossPushNotification.Current.Token}");

            // Handle when your app starts
            CrossPushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine($"TOKEN REC: {p.Token}");
                SetMessageText($"TOKEN REC: {p.Token}");
            };

            CrossPushNotification.Current.OnNotificationReceived += async (s, p) => await NotificationReceivedAsync(s, p);

            CrossPushNotification.Current.OnNotificationOpened += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Opened");
                foreach (var data in p.Data)
                {
                    System.Diagnostics.Debug.WriteLine($"{data.Key} : {data.Value}");
                }

                if (!string.IsNullOrEmpty(p.Identifier))
                {
                    SetMessageText(p.Identifier);
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
                    SetMessageText($"{p.Data["aps.alert.title"]}");
                }
            };
            CrossPushNotification.Current.OnNotificationDeleted += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine("Dismissed");
            };
        }

        private void SetMessageText(string text)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lblMain.Text = text;
            });
        }

        private async Task NotificationReceivedAsync(object s, PushNotificationDataEventArgs p)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Received");
                if (p.Data.ContainsKey(APN_TITLE))
                {
                    SetMessageText($"{p.Data[APN_TITLE]}");
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