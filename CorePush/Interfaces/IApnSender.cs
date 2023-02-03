using CorePush.Apple;
using System.Threading;
using System.Threading.Tasks;

namespace CorePush.Interfaces.Apple
{
    public interface IApnSender
    {
        Task<ApnsResponse> SendAsync(
            object notification,
            string deviceToken,
            string apnsId = null,
            int apnsExpiration = 0,
            int apnsPriority = 10,
            ApnPushType apnPushType = ApnPushType.Alert,
            ApnServerType apnServerType = ApnServerType.Development,
            CancellationToken cancellationToken = default);
    }
}