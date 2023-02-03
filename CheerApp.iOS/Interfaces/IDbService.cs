using System.Threading.Tasks;
using CheerApp.iOS.Models;

namespace CheerApp.iOS.Interfaces
{
    public interface IDbService
    {
        Task SendDeviceDetailsToServerAsync(string apnToken = null, string fcmToken = null, string topics = null);
    }
}