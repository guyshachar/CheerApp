using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common;
using CheerApp.Common.Models;
using CheerApp.Common.Interfaces;
using CoreAudioKit;
using CorePush.Google;
using CorePush.Interfaces;
using Foundation;
using Plugin.PushNotification;
using UIKit;
using Xamarin.Forms;
using Command = CheerApp.Common.Command;
using CheerApp.Common.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace CheerApp.iOS
{
    public partial class ShowRoom : UIViewController, IPageActions
    {
        private readonly IUIServices _uiServices;
        private readonly IFirestoreProvider _firestoreProvider;
        private readonly IDbService _dbService;
        private readonly IJsonHelper _jsonHelper;

        private Message lastMessage;
        public List<CancellationTokenSource> CancellationTokenSources { get; set; } = new List<CancellationTokenSource>();

        //loads the HelloWorldScreen.xib file and connects it to this object
        public ShowRoom(
            IUIServices uIServices,
            IFirestoreProvider firestoreProvider,
            IDbService dbService,
            IJsonHelper jsonHelper) : base(nameof(ShowRoom), null)
        {
            this._uiServices = uIServices;
            this._firestoreProvider = firestoreProvider;
            this._dbService = dbService;
            this._jsonHelper = jsonHelper;

            this.Title = "Cheer App by Guy Shachar";
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (lastMessage != null && lblMain.Text != lastMessage.Title)
                _uiServices.SetMessageText(lblMain, lastMessage.Title);

            var deviceDetails = await _firestoreProvider.GetAsync<DeviceDetail>(Startup.DeviceId);
            this.txtTopics.Text = string.Join(",", deviceDetails.Topics);
            this.btnTopics.TouchUpInside += async (sender, e) => await SendAsync(sender, e);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            _uiServices.ToggleFlashAsync(false);
            this.btnTopics.TouchUpInside -= async (sender, e) => await SendAsync(sender, e);

            CancelTokens();
        }

        private void CancelTokens()
        {
            foreach (var cancellationTokenSource in CancellationTokenSources)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            CancellationTokenSources.Clear();
        }

        private async Task SendAsync(object sender, EventArgs e)
        {
            await _dbService.SendDeviceDetailsToServerAsync(topics: txtTopics.Text);
            _uiServices.ShowAlert(this, "Info", "Topic updated", ("OK", (int)UIAlertActionStyle.Default, null));
        }

        private void CreateLocalNotification()
        {/*
            var localNotification = new UILocalNotification();
            //var remoteNotification = new UIRemoteNotificatio

            //---- set the fire date (the date time in which it will fire)
            var fireDate = DateTime.Now.AddSeconds(20);
            localNotification.FireDate = (NSDate)fireDate;
            localNotification.HasAction = false;

            //---- configure the alert stuff
            localNotification.AlertAction = "View Alert";
            var notificationBody = new ShowRoomNotification
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
        */}

        public async Task ReceivedMessageAsync(Message message)
        {
            lastMessage = message;

            _uiServices.SetMessageText(lblMain, lastMessage.Title);

            var showRoom = this;
            var cancellationTokenSource = new CancellationTokenSource();

            if (_uiServices.TopViewPage != null && _uiServices.TopViewPage != showRoom)
            {
                var okCancelAlertController = UIAlertController.Create("New ShowRoom message arrived", "Do you want to participate ?",
                    UIAlertControllerStyle.Alert);
                okCancelAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default,
                                            async (alert) =>
                                            {
                                                CancelTokens();
                                                CancellationTokenSources.Add(cancellationTokenSource);
                                                _uiServices.Navigate(showRoom);
                                                await PlayShowRoomAsync(lastMessage, cancellationTokenSource.Token);
                                            }));
                okCancelAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                ((UIViewController)_uiServices.TopViewPage).PresentViewController(okCancelAlertController, true, null);
            }
            else
            {
                CancelTokens();
                CancellationTokenSources.Add(cancellationTokenSource);
                await PlayShowRoomAsync(lastMessage, cancellationTokenSource.Token);
            }
        }

        private async Task PlayShowRoomAsync(Message message, CancellationToken? cancellationToken = null)
        {
            try
            {
                //message.Json = "[{\"FE\":0,\"CD\":\"false\",\"CT\":871},{\"FE\":0,\"CD\":\"true\",\"CT\":1934},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:57,\\u0022G\\u0022:245,\\u0022B\\u0022:8}\",\"CT\":1616},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:184,\\u0022G\\u0022:26,\\u0022B\\u0022:225}\",\"CT\":1180},{\"FE\":0,\"CD\":\"false\",\"CT\":1121},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:37,\\u0022G\\u0022:118,\\u0022B\\u0022:232}\",\"CT\":1028},{\"FE\":0,\"CD\":\"false\",\"CT\":897},{\"FE\":0,\"CD\":\"true\",\"CT\":1885},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:42,\\u0022G\\u0022:253,\\u0022B\\u0022:186}\",\"CT\":1738},{\"FE\":0,\"CD\":\"false\",\"CT\":1044},{\"FE\":0,\"CD\":\"true\",\"CT\":832},{\"FE\":0,\"CD\":\"true\",\"CT\":1060},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:183,\\u0022G\\u0022:16,\\u0022B\\u0022:114}\",\"CT\":1461},{\"FE\":0,\"CD\":\"false\",\"CT\":1449},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:5,\\u0022G\\u0022:25,\\u0022B\\u0022:239}\",\"CT\":1728},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:37,\\u0022G\\u0022:112,\\u0022B\\u0022:209}\",\"CT\":1149},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:34,\\u0022G\\u0022:23,\\u0022B\\u0022:73}\",\"CT\":1261},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:188,\\u0022G\\u0022:163,\\u0022B\\u0022:39}\",\"CT\":1273},{\"FE\":0,\"CD\":\"true\",\"CT\":1935},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:246,\\u0022G\\u0022:68,\\u0022B\\u0022:175}\",\"CT\":1155},{\"FE\":0,\"CD\":\"false\",\"CT\":1364},{\"FE\":0,\"CD\":\"true\",\"CT\":1782},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:248,\\u0022G\\u0022:160,\\u0022B\\u0022:108}\",\"CT\":685},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:18,\\u0022G\\u0022:169,\\u0022B\\u0022:241}\",\"CT\":1718},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:92,\\u0022G\\u0022:140,\\u0022B\\u0022:73}\",\"CT\":1798},{\"FE\":0,\"CD\":\"true\",\"CT\":805},{\"FE\":0,\"CD\":\"false\",\"CT\":1988},{\"FE\":0,\"CD\":\"true\",\"CT\":1176},{\"FE\":1,\"CD\":\"{\\u0022R\\u0022:41,\\u0022G\\u0022:0,\\u0022B\\u0022:145}\",\"CT\":1068},{\"FE\":0,\"CD\":\"true\",\"CT\":544}]";
                var commands = _jsonHelper.Deserialize<IEnumerable<Command>>(message.Json);

                var timeInShow = DateTime.Parse(message.StartTime).ToUniversalTime();

                foreach (var command in commands)
                {
                    if ((bool)(cancellationToken?.IsCancellationRequested))
                        return;

                    var delay = timeInShow - DateTime.UtcNow;
                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay);

                    timeInShow = timeInShow.AddMilliseconds(command.CommandTime);

                    switch (command.Feature)
                    {
                        case FeatureEnum.Flash:
                            var onOff = _jsonHelper.Deserialize<bool>(command.CommandDetails);
                            await _uiServices.ToggleFlashAsync(onOff);
                            break;
                        case FeatureEnum.Screen:
                            var rgb = _jsonHelper.Deserialize<RGB>(command.CommandDetails);
                            var color = System.Drawing.Color.FromArgb(rgb.A, rgb.R, rgb.G, rgb.B);
                            _uiServices.PaintScreen(this, color);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}