
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CheerApp.Common.Models;

namespace CheerApp.Common.Interfaces
{
	public interface IPageActions
	{
        List<CancellationTokenSource> CancellationTokenSources { get; set; }
        Task ReceivedMessageAsync(Message message);
	}
}