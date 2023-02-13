using System;
using System.Drawing;
using System.Threading.Tasks;
using Xamarin.Forms.PlatformConfiguration;

namespace CheerApp.Common.Interfaces
{
	public interface IUIServices
	{
		object TopViewPage { get; }

		void PaintScreen(object page, Color color);

		Task ToggleFlashAsync(bool turnOn);

		void ShowAlert(object page, string title, string description, params (string actionName, int alertActionStyle, Action<dynamic> action)[] actions);

		void SetMessageText(object obj, string text);

		void InvokeUIAction(Action action);

		void Navigate(object page, params object[] properties);
	}
}