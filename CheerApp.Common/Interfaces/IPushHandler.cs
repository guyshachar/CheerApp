using CheerApp.Common.Models;
using System.Threading.Tasks;

namespace CheerApp.Common.Interfaces
{
	public interface IPushHandler
    {
		void Start(params object[] pushHandlerHandlerParameters);

        Task HandleUpdatedDocumentAsync<T>(T obj) where T : ModelBase;
    }
}