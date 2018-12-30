using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;
using Parse.Utilities;

namespace Parse.Core.Internal
{
    public class ParseCloudCodeController : IParseCloudCodeController
    {
        IParseCommandRunner Runner { get; }

        public ParseCloudCodeController(IParseCommandRunner commandRunner) => Runner = commandRunner;

        public Task<T> CallFunctionAsync<T>(string name, IDictionary<string, object> parameters, string sessionToken, CancellationToken cancellationToken) => Runner.RunCommandAsync(new ParseCommand(String.Format("functions/{0}", Uri.EscapeUriString(name)), "POST", sessionToken, data: NoObjectsEncoder.Instance.Encode(parameters) as IDictionary<string, object>), cancellationToken: cancellationToken).OnSuccess(t =>
        {
            // NOTE: Should fail by design if decoded is null according to unit test; do not inline.
            IDictionary<string, object> decoded = ParseDecoder.Instance.Decode(t.Result.Item2) as IDictionary<string, object>;
            return decoded.ContainsKey("result") ? ConversionHelpers.DowncastValue<T>(decoded["result"]) : default;
        });
    }
}
