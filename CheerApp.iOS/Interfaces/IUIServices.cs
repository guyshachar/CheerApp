using System;
using System.Threading.Tasks;
using CheerApp.Common;
using UIKit;

namespace CheerApp.iOS.Interfaces
{
	public interface IUIServices
	{
		Task PaintScreenAsync(UIViewController viewController, UIColor color);

		Task ToggleFlashAsync(bool turnOn);

		Task SyncActivityAsync(DateTime startTime);
	}
}