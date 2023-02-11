using System.Threading.Tasks;

namespace CheerApp.Common.Interfaces
{
    public interface IDbService
    {
        Task SendDeviceDetailsToServerAsync(string apnToken = null, string fcmToken = null, string topics = null);
    }
}