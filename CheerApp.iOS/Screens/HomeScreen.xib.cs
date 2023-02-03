using System;
using System.Collections.Generic;
using System.Linq;
using CheerApp.iOS.Extensions;
using CheerApp.iOS.Interfaces;
using CheerApp.iOS.Models;
using Foundation;
using UIKit;

namespace CheerApp.iOS
{
	public partial class HomeScreen : UIViewController
	{
		private readonly ShowRoom ShowRoom;
		private readonly SendPushNotification SendPushNotification;

		//loads the HomeScreen.xib file and connects it to this object
		public HomeScreen(ShowRoom showRoom, SendPushNotification sendPushNotification)
			: base(nameof(HomeScreen), null)
		{
			ShowRoom = showRoom;
			SendPushNotification = sendPushNotification;
        }

        public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//---- when the hello world button is clicked
			this.btnShowRoom.TouchUpInside += (sender, e) =>
			{
				//---- instantiate a new hello world screen, if it's null (it may not be null if they've navigated
				// backwards from it
				//---- push our hello world screen onto the navigation controller and pass a true so it navigates
				this.NavigationController.PushViewController(ShowRoom, true);
			};

			//---- same thing, but for the hello universe screen
			this.btnSendPush.TouchUpInside += (sender, e) =>
			{
				this.NavigationController.PushViewController(SendPushNotification, true);
			};
		}

		/// <summary>
		/// Is called when the view is about to appear on the screen. We use this method to hide the
		/// navigation bar.
		/// </summary>
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			this.NavigationController.SetNavigationBarHidden(true, animated);
		}

		/// <summary>
		/// Is called when the another view will appear and this one will be hidden. We use this method
		/// to show the navigation bar again.
		/// </summary>
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			this.NavigationController.SetNavigationBarHidden(false, animated);
		}
	}
}