using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    internal class ParseConfigController : IParseConfigController
    {
        IParseCommandRunner Runner { get; }

        public ParseConfigController(IParseCommandRunner commandRunner, IStorageController storageController)
        {
            Runner = commandRunner;
            CurrentConfigController = new ParseCurrentConfigController(storageController);
        }

        // TODO: Investigate if this can be removed outright before the refactor.
        public IParseCommandRunner CommandRunner { get; internal set; }

        public IParseCurrentConfigController CurrentConfigController { get; internal set; }

        public Task<ParseConfig> FetchConfigAsync(string sessionToken, CancellationToken cancellationToken)
        {
            ParseCommand command = new ParseCommand("config", method: "GET", sessionToken: sessionToken, data: null);

            return Runner.RunCommandAsync(command, cancellationToken: cancellationToken).OnSuccess(task =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return new ParseConfig(task.Result.Item2);
            }).OnSuccess(task =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                CurrentConfigController.SetCurrentConfigAsync(task.Result);
                return task;
            }).Unwrap();
        }
    }
}
