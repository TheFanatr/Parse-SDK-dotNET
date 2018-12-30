using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class ParseSessionController : IParseSessionController
    {
        IParseCommandRunner CommandRunner { get; }

        public ParseSessionController(IParseCommandRunner commandRunner) => CommandRunner = commandRunner;

        public Task<IObjectState> GetSessionAsync(string sessionToken, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand("sessions/me", "GET", sessionToken, data: null), cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance));

        public Task RevokeAsync(string sessionToken, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand("logout", "POST", sessionToken, data: new Dictionary<string, object> { }), cancellationToken: cancellationToken);

        public Task<IObjectState> UpgradeToRevocableSessionAsync(string sessionToken, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand("upgradeToRevocableSession", method: "POST", sessionToken: sessionToken, data: new Dictionary<string, object> { }), cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance));

        public bool IsRevocableSessionToken(string sessionToken) => sessionToken.Contains("r:");
    }
}
