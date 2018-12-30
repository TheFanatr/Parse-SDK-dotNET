using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Push.Internal
{
    internal class ParsePushChannelsController : IParsePushChannelsController
    {
        public Task SubscribeAsync(IEnumerable<string> channels, CancellationToken cancellationToken)
        {
            ParseInstallation installation = ParseInstallation.CurrentInstallation;
            installation.AddRangeUniqueToList("channels", channels);
            return installation.SaveAsync(cancellationToken);
        }

        public Task UnsubscribeAsync(IEnumerable<string> channels, CancellationToken cancellationToken)
        {
            ParseInstallation installation = ParseInstallation.CurrentInstallation;
            installation.RemoveAllFromList("channels", channels);
            return installation.SaveAsync(cancellationToken);
        }
    }
}
