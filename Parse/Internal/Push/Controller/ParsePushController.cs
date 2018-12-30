using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;
using Parse.Core.Internal;

namespace Parse.Push.Internal
{
    internal class ParsePushController : IParsePushController
    {
        IParseCommandRunner CommandRunner { get; }

        IParseCurrentUserController CurrentUserController { get; }

        public ParsePushController(IParseCommandRunner commandRunner, IParseCurrentUserController currentUserController)
        {
            CommandRunner = commandRunner;
            CurrentUserController = currentUserController;
        }

        public Task SendPushNotificationAsync(IPushState state, CancellationToken cancellationToken) => CurrentUserController.GetCurrentSessionTokenAsync(cancellationToken).OnSuccess(sessionTokenTask => CommandRunner.RunCommandAsync(new ParseCommand("push", "POST", sessionTokenTask.Result, data: ParsePushEncoder.Instance.Encode(state)), cancellationToken: cancellationToken)).Unwrap();
    }
}
