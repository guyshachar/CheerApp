using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CheerApp;
using CheerApp.Common;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Command = CheerApp.Common.Command;

namespace CheerApp.iOS
{
    public class UIServices : IUIServices
    {
        public async Task PaintScreenAsync(UIViewController viewController, UIColor color)
        {
            viewController.BeginInvokeOnMainThread(delegate ()
            {
                viewController.View.BackgroundColor = color;
            });
        }

        public async Task ToggleFlashAsync(bool turnOn)
        {
            {
                try
                {
                    if (turnOn)
                        await Flashlight.TurnOnAsync();
                    else
                        await Flashlight.TurnOffAsync();
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                }
                catch (PermissionException pEx)
                {
                    //await ShowAlert(pEx.Message);
                }
                catch (Exception ex)
                {
                    //await ShowAlert(ex.Message);
                }
            }
        }

        public void ShowAlert()
        {
            IUIAlertViewDelegate del = new UIAlertViewDelegate();
            new UIAlertView("Hello", "Message", del, "Cancel", null).Show();
        }

        public async Task ResetShowAsync(UIViewController uIViewController, string jsonShow)
        {
            var commands = JsonSerializer.Deserialize<IEnumerable<Command>>(jsonShow);

            foreach (var command in commands)
            {
                await Task.Delay(command.SkipTime);

                switch (command.Feature)
                {
                    case FeatureEnum.Flash:
                        var onOff = JsonSerializer.Deserialize<bool>(command.CommandDetails);
                        await this.ToggleFlashAsync(onOff);
                        break;
                    case FeatureEnum.Screen:
                        var rgb = JsonSerializer.Deserialize<RGB>(command.CommandDetails);
                        var color = UIColor.FromRGB(rgb.R, rgb.G, rgb.B);
                        await this.PaintScreenAsync(uIViewController, color);
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task SyncActivityAsync(DateTime startTime)
        {
            while (startTime > DateTime.UtcNow)
            { }
        }
    }
}