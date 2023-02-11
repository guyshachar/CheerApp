using System;
using System.Threading.Tasks;
using CheerApp.Common;
using UIKit;

namespace CheerApp.Common.Interfaces
{
	public interface IUIServices
	{
		void PaintScreen(UIViewController viewController, UIColor color);

		Task ToggleFlashAsync(bool turnOn);
	}
}