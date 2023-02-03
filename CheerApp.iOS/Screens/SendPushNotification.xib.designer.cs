// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CheerApp.iOS
{
	[Register ("SendPushNotification")]
	partial class SendPushNotification
	{
		[Outlet]
		UIKit.UIButton btnSend { get; set; }

		[Outlet]
		UIKit.UILabel lblMessageBody { get; set; }

		[Outlet]
		UIKit.UITextField txtMessageTitle { get; set; }

		[Outlet]
		UIKit.UITextField txtTopics { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnSend != null) {
				btnSend.Dispose ();
				btnSend = null;
			}

			if (lblMessageBody != null) {
				lblMessageBody.Dispose ();
				lblMessageBody = null;
			}

			if (txtMessageTitle != null) {
				txtMessageTitle.Dispose ();
				txtMessageTitle = null;
			}

			if (txtTopics != null) {
				txtTopics.Dispose ();
				txtTopics = null;
			}
		}
	}
}
