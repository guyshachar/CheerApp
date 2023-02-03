using System;
using System.Threading.Tasks;
using CheerApp.Common;

namespace CheerApp.iOS.Interfaces
{
	public interface IScreenActions
	{
		Task ReceivedNotificationAsync(Notification notification);
	}
}