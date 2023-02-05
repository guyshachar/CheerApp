using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheerApp.Common;
using CheerApp.iOS.Implementations;
using CheerApp.iOS.Interfaces;
using CorePush.Interfaces;
using UIKit;
using Xamarin.Essentials;
using Command = CheerApp.Common.Command;
using DependencyAttribute = Xamarin.Forms.DependencyAttribute;

[assembly: Dependency(typeof(UIServices))]
namespace CheerApp.iOS.Implementations
{
    public class UIServices : IUIServices
    {
        private readonly IJsonHelper JsonHelper;

        public UIServices(IJsonHelper jsonHelper)
        {
            JsonHelper = jsonHelper;
        }

        public void PaintScreen(UIViewController viewController, UIColor color)
        {
            Startup.InvokeUIAction(viewController, () => viewController.View.BackgroundColor = color);
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
    }
}