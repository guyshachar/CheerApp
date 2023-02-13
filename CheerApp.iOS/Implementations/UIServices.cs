using System;
using System.Threading.Tasks;
using CheerApp.Common.Interfaces;
using CorePush.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Drawing;
using UIKit;
using CheerApp.Common;

namespace CheerApp.iOS.Implementations
{
    public class UIServices : IUIServices
    {
        private readonly IJsonHelper JsonHelper;
        private UIWindow window;
        public object TopViewPage
        {
            get
            {
                if (window == null)
                {
                    window = new UIWindow(UIScreen.MainScreen.Bounds);

                    //---- instantiate a new navigation controller
                    var rootNavigationController = new UINavigationController();

                    //---- set the root view controller on the window. the nav controller will handle the rest
                    window.RootViewController = rootNavigationController;

                    window.MakeKeyAndVisible();
                }
                return ((UINavigationController)window.RootViewController.NavigationController?.TopViewController ??
                                (UINavigationController)window.RootViewController);
            }
        }

        public UIServices(IJsonHelper jsonHelper)
        {
            JsonHelper = jsonHelper;
        }

        public void PaintScreen(object page, System.Drawing.Color color)
        {
            var uiColor = UIColor.FromRGBA(color.R, color.G, color.B, color.A);
            InvokeUIAction(() => ((UIViewController)page).View.BackgroundColor = uiColor);
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

        public void ShowAlert(object page, string title, string description, params (string actionName, int alertActionStyle, Action<dynamic> action)[] actions)
        {
            //Create Alert
            var okCancelAlertController = UIAlertController.Create(title, description, UIAlertControllerStyle.Alert);

            //Add Actions
            foreach (var action in actions)
                okCancelAlertController.AddAction(UIAlertAction.Create(action.actionName, (UIAlertActionStyle)action.alertActionStyle, action.action));

            //Present Alert
            ((UIViewController)page).PresentViewController(okCancelAlertController, true, null);
        }

        public void SetMessageText(object obj, string text)
        {
            InvokeUIAction(() =>
            {
                if (obj is UILabel)
                    ((UILabel)obj).Text = text;
                else if (obj is UITextField)
                    ((UITextField)obj).Text = text;
            });
        }

        public void InvokeUIAction(Action action)
        {
            Device.BeginInvokeOnMainThread(() => action());
        }

        public void Navigate(object page, params object[] properties)
        {
            ((UINavigationController)TopViewPage)
                .PushViewController((UIViewController)page, properties.Length == 1 ? Convert.ToBoolean(properties[0]) : false);
        }
    }
}