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
	[Register ("ShowRoom")]
	partial class ShowRoom
	{
		[Outlet]
		UIKit.UIButton btnTopics { get; set; }

		[Outlet]
		UIKit.UILabel lblMain { get; set; }

		[Outlet]
		UIKit.UITextField txtDeviceToken { get; set; }

		[Outlet]
		UIKit.UITextField txtTopics { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnTopics != null) {
				btnTopics.Dispose ();
				btnTopics = null;
			}

			if (lblMain != null) {
				lblMain.Dispose ();
				lblMain = null;
			}

			if (txtDeviceToken != null) {
				txtDeviceToken.Dispose ();
				txtDeviceToken = null;
			}

			if (txtTopics != null) {
				txtTopics.Dispose ();
				txtTopics = null;
			}
		}
	}
}
