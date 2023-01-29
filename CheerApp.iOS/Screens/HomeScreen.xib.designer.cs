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
	[Register ("HomeScreen")]
	partial class HomeScreen
	{
		[Outlet]
		UIKit.UIButton btnSendPush { get; set; }

		[Outlet]
		UIKit.UIButton btnShowRoom { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnSendPush != null) {
				btnSendPush.Dispose ();
				btnSendPush = null;
			}

			if (btnShowRoom != null) {
				btnShowRoom.Dispose ();
				btnShowRoom = null;
			}
		}
	}
}
