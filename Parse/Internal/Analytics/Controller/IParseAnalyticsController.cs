using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Analytics.Internal
{
    public interface IParseAnalyticsController
    {
        Task TrackEventAsync(string name, IDictionary<string, string> dimensions, string sessionToken, CancellationToken cancellationToken);

        Task TrackAppOpenedAsync(string pushHash, string sessionToken, CancellationToken cancellationToken);
    }
}
