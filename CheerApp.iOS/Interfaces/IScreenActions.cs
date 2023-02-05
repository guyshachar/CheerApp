using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common;
using CheerApp.Common.Models;
using CorePush.Google;

namespace CheerApp.iOS.Interfaces
{
	public interface IScreenActions
	{
		Task ReceivedNotificationAsync(CheerAppNotification notification, CancellationToken? cancellationToken = null);
	}
}