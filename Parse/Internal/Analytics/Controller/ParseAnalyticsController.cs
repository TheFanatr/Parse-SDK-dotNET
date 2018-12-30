using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parse.Core.Internal;

namespace Parse.Analytics.Internal
{
    public class ParseAnalyticsController : IParseAnalyticsController
    {
        IParseCommandRunner Runner { get; }

        public ParseAnalyticsController(IParseCommandRunner commandRunner) => Runner = commandRunner;

        public Task TrackEventAsync(string name, IDictionary<string, string> dimensions, string sessionToken, CancellationToken cancellationToken)
        {
            IDictionary<string, object> data = new Dictionary<string, object>
            {
                ["at"] = DateTime.Now,
                ["name"] = name,
            };

            if (dimensions != null)
                data["dimensions"] = dimensions;

            return Runner.RunCommandAsync(new ParseCommand("events/" + name, "POST", sessionToken, data: PointerOrLocalIdEncoder.Instance.Encode(data) as IDictionary<string, object>), cancellationToken: cancellationToken);
        }

        public Task TrackAppOpenedAsync(string pushHash, string sessionToken, CancellationToken cancellationToken)
        {
            IDictionary<string, object> data = new Dictionary<string, object> { ["at"] = DateTime.Now };

            if (pushHash != null)
                data["push_hash"] = pushHash;

            return Runner.RunCommandAsync(new ParseCommand("events/AppOpened", "POST", sessionToken, data: PointerOrLocalIdEncoder.Instance.Encode(data) as IDictionary<string, object>), cancellationToken: cancellationToken);
        }
    }
}
