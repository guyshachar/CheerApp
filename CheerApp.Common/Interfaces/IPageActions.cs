using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common.Models;

namespace CheerApp.Common.Interfaces
{
	public interface IPageActions
	{
		Task ReceivedNotificationAsync(Message message, CancellationToken? cancellationToken = null);
	}
}