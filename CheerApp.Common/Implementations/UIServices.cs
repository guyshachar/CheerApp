using System;
using System.Threading.Tasks;
using CheerApp.Common.Interfaces;
using CorePush.Interfaces;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CheerApp.Common.Implementations
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
            UIServices.InvokeUIAction(viewController, () => viewController.View.BackgroundColor = color);
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

        public static void ShowAlert(UIViewController viewController, string title, string description, params (string actionName, UIAlertActionStyle alertActionStyle, Action<UIAlertAction> action)[] actions)
        {
            //Create Alert
            var okCancelAlertController = UIAlertController.Create(title, description, UIAlertControllerStyle.Alert);

            //Add Actions
            foreach (var action in actions)
                okCancelAlertController.AddAction(UIAlertAction.Create(action.actionName, action.alertActionStyle, action.action));

            //Present Alert
            viewController.PresentViewController(okCancelAlertController, true, null);
        }

        public static void SetMessageText(object obj, string text)
        {
            InvokeUIAction(null, () =>
            {
                if (obj is UILabel)
                    ((UILabel)obj).Text = text;
                else if (obj is UITextField)
                    ((UITextField)obj).Text = text;
            });
        }

        public static void InvokeUIAction(UIViewController viewController, Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }
    }
}