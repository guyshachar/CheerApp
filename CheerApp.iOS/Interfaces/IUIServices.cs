using System;
using System.Threading.Tasks;
using CheerApp.Common;
using UIKit;

namespace CheerApp.iOS.Interfaces
{
	public interface IUIServices
	{
		void PaintScreen(UIViewController viewController, UIColor color);

		Task ToggleFlashAsync(bool turnOn);
	}
}