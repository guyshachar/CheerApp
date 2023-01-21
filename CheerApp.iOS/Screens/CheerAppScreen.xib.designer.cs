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
	[Register ("CheerAppScreen")]
	partial class CheerAppScreen
	{
		[Outlet]
		UIKit.UILabel lblMain { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblMain != null) {
				lblMain.Dispose ();
				lblMain = null;
			}
		}
	}
}
