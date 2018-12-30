using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class ParseUserController : IParseUserController
    {
        IParseCommandRunner CommandRunner { get; }

        public ParseUserController(IParseCommandRunner commandRunner) => CommandRunner = commandRunner;

        public Task<IObjectState> SignUpAsync(IObjectState state, IDictionary<string, IParseFieldOperation> operations, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand("classes/_User", "POST", data: ParseObject.ToJSONObjectForSaving(operations)), cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance).MutatedClone(mutableClone => mutableClone.IsNew = true));

        public Task<IObjectState> LogInAsync(string username, string password, CancellationToken cancellationToken)
        {
            Dictionary<string, object> data = new Dictionary<string, object>
            {
                ["username"] = username,
                ["password"] = password
            };

            return CommandRunner.RunCommandAsync(new ParseCommand($"login?{ParseClient.BuildQueryString(data)}", "GET", data: null), cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance).MutatedClone(mutableClone => mutableClone.IsNew = t.Result.Item1 == System.Net.HttpStatusCode.Created));
        }

        public Task<IObjectState> LogInAsync(string authType, IDictionary<string, object> data, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand("users", "POST", data: new Dictionary<string, object> { ["authData"] = new Dictionary<string, object> { [authType] = data } }), cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance).MutatedClone(mutableClone => mutableClone.IsNew = t.Result.Item1 == System.Net.HttpStatusCode.Created));

        public Task<IObjectState> GetUserAsync(string sessionToken, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand("users/me", "GET", sessionToken, data: null), cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance));

        public Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand("requestPasswordReset", "POST", data: new Dictionary<string, object> { ["email"] = email }), cancellationToken: cancellationToken);
    }
}
