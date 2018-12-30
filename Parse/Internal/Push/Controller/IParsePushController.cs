using System.Threading;
using System.Threading.Tasks;

namespace Parse.Push.Internal
{
    public interface IParsePushController
    {
        Task SendPushNotificationAsync(IPushState state, CancellationToken cancellationToken);
    }
}
