using System;
using System.Threading.Tasks;
using UIKit;

namespace CheerApp.iOS
{
	public interface IUIServices
	{
		Task PaintScreenAsync(UIViewController viewController, UIColor color);

		Task ToggleFlashAsync(bool turnOn);

		Task ResetShowAsync(UIViewController uIViewController, string jsonShow);

		Task SyncActivityAsync(DateTime startTime);
	}
}