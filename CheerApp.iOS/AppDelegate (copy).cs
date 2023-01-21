using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Foundation;
using Hello_MultiScreen_iPhone;
using UIKit;
using Xamarin.Forms;

namespace CheerApp
{
    [Register(nameof(AppDelegate1))]
    public partial class AppDelegate1 : UIApplicationDelegate, IAppDelegateExtension
    {
        //---- declarations
        private UIWindow window;
        private readonly Dictionary<string, UIViewController> viewControllersDic = new Dictionary<string, UIViewController>();
        public Dictionary<string, UIViewController> ViewControllersDic => this.viewControllersDic;

        // This method is invoked when the application has loaded its UI and it is ready to run
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            DependencyService.Register<IUIServices, UIServices>();
            //DependencyService.Register<CheerAppShowScreen>();
            //DependencyService.Register<HelloUniverseScreen>();

            Xamarin.Forms.Forms.Init();

            this.window = new UIWindow(UIScreen.MainScreen.Bounds);

            //---- instantiate a new navigation controller
            var rootNavigationController = new UINavigationController();
            //---- instantiate a new home screen
            var homeScreen = new HomeScreen();
            //this.ViewControllersDic.Add(nameof(HomeScreen), homeScreen);

            //---- add the home screen to the navigation controller (it'll be the top most screen)
            rootNavigationController.PushViewController(homeScreen, false);

            //---- set the root view controller on the window. the nav controller will handle the rest
            this.window.RootViewController = rootNavigationController;

            this.window.MakeKeyAndVisible();

            return true;
        }

        [Export("application:didReceiveLocalNotification:")]
        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            // show an alert
            IUIAlertViewDelegate del = null;
            var body = JsonSerializer.Deserialize<Notification>(notification.AlertBody);
            new UIAlertView(notification.AlertAction, notification.AlertBody, del, "OK", null).Show();
            var uiViewController = this.ViewControllersDic[body.UIViewControllerName];

            var uiServices = DependencyService.Resolve<IUIServices>();
            if (body.Action  == "ResetShow")
            {
                uiServices.ResetShowAsync(uiViewController, body.Json);
            }

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {

        }
    }
}