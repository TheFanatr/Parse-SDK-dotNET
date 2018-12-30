using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Push.Internal
{
    public interface IParsePushChannelsController
    {
        Task SubscribeAsync(IEnumerable<string> channels, CancellationToken cancellationToken);

        Task UnsubscribeAsync(IEnumerable<string> channels, CancellationToken cancellationToken);
    }
}
