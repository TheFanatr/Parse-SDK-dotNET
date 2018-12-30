using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;
using Parse.Utilities;

namespace Parse.Core.Internal
{
    public class ParseObjectController : IParseObjectController
    {
        // TODO: move this out to a class to be used by Analytics
        const int MaximumBatchSize = 50;

        IParseCommandRunner CommandRunner { get; }

        public ParseObjectController(IParseCommandRunner commandRunner) => CommandRunner = commandRunner;

        public Task<IObjectState> FetchAsync(IObjectState state, string sessionToken, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand($"classes/{Uri.EscapeDataString(state.ClassName)}/{Uri.EscapeDataString(state.ObjectId)}", "GET", sessionToken, data: null), cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance));

        public Task<IObjectState> SaveAsync(IObjectState state, IDictionary<string, IParseFieldOperation> operations, string sessionToken, CancellationToken cancellationToken)
        {
            ParseCommand command = new ParseCommand(state.ObjectId is null ? $"classes/{Uri.EscapeDataString(state.ClassName)}" : $"classes/{Uri.EscapeDataString(state.ClassName)}/{state.ObjectId}", state.ObjectId is null ? "POST" : "PUT", sessionToken, data: ParseObject.ToJSONObjectForSaving(operations));
            return CommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken).OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result.Item2, ParseDecoder.Instance).MutatedClone(mutableClone => mutableClone.IsNew = t.Result.Item1 == System.Net.HttpStatusCode.Created));
        }

        public IList<Task<IObjectState>> SaveAllAsync(IList<IObjectState> states, IList<IDictionary<string, IParseFieldOperation>> operationsList, string sessionToken, CancellationToken cancellationToken)
        {
            List<ParseCommand> requests = states.Zip(operationsList, (item, ops) => new ParseCommand(item.ObjectId == null ? $"classes/{Uri.EscapeDataString(item.ClassName)}" : $"classes/{Uri.EscapeDataString(item.ClassName)}/{Uri.EscapeDataString(item.ObjectId)}", item.ObjectId is null ? "POST" : "PUT", data: ParseObject.ToJSONObjectForSaving(ops))).ToList();

            List<Task<IObjectState>> stateTasks = new List<Task<IObjectState>> { };
            foreach (Task<IDictionary<string, object>> task in ExecuteBatchRequests(requests, sessionToken, cancellationToken))
                stateTasks.Add(task.OnSuccess(t => ParseObjectCoder.Instance.Decode(t.Result, ParseDecoder.Instance)));

            return stateTasks;
        }

        public Task DeleteAsync(IObjectState state, string sessionToken, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand(String.Format("classes/{0}/{1}", state.ClassName, state.ObjectId), "DELETE", sessionToken, data: null), cancellationToken: cancellationToken);

        public IList<Task> DeleteAllAsync(IList<IObjectState> states, string sessionToken, CancellationToken cancellationToken) => ExecuteBatchRequests(states.Where(item => item.ObjectId != null).Select(item => new ParseCommand($"classes/{Uri.EscapeDataString(item.ClassName)}/{Uri.EscapeDataString(item.ObjectId)}", "DELETE", data: null)).ToList(), sessionToken, cancellationToken).Cast<Task>().ToList();

        internal IList<Task<IDictionary<string, object>>> ExecuteBatchRequests(IList<ParseCommand> requests, string sessionToken, CancellationToken cancellationToken = default)
        {
            List<Task<IDictionary<string, object>>> tasks = new List<Task<IDictionary<string, object>>> { };
            IEnumerable<ParseCommand> remaining = requests;
            int batchSize = requests.Count;

            while (batchSize > MaximumBatchSize)
            {
                List<ParseCommand> process = remaining.Take(MaximumBatchSize).ToList();
                remaining = remaining.Skip(MaximumBatchSize);
                tasks.AddRange(ExecuteBatchRequest(process, sessionToken, cancellationToken));
                batchSize = remaining.Count();
            }

            tasks.AddRange(ExecuteBatchRequest(remaining.ToList(), sessionToken, cancellationToken));
            return tasks;
        }

        IList<Task<IDictionary<string, object>>> ExecuteBatchRequest(IList<ParseCommand> requests, string sessionToken, CancellationToken cancellationToken)
        {
            List<Task<IDictionary<string, object>>> tasks = new List<Task<IDictionary<string, object>>> { };
            List<TaskCompletionSource<IDictionary<string, object>>> sources = new List<TaskCompletionSource<IDictionary<string, object>>> { };
            int batchSize = requests.Count;

            for (int i = 0; i < batchSize; ++i)
            {
                TaskCompletionSource<IDictionary<string, object>> tcs = new TaskCompletionSource<IDictionary<string, object>> { };
                sources.Add(tcs);
                tasks.Add(tcs.Task);
            }

            List<object> encodedRequests = requests.Select(r =>
            {
                Dictionary<string, object> results = new Dictionary<string, object>
                {
                    ["method"] = r.Method,
                    ["path"] = r.Uri.AbsolutePath,
                };

                if (r.DataObject != null)
                    results["body"] = r.DataObject;

                return results;
            }).Cast<object>().ToList();

            ParseCommand command = new ParseCommand("batch", "POST", sessionToken, data: new Dictionary<string, object> { ["requests"] = encodedRequests });

            CommandRunner.RunCommandAsync(command, cancellationToken: cancellationToken).ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    foreach (TaskCompletionSource<IDictionary<string, object>> tcs in sources)
                        if (t.IsFaulted)
                            tcs.TrySetException(t.Exception);
                        else if (t.IsCanceled)
                            tcs.TrySetCanceled();

                    return;
                }

                IList<object> resultsArray = ConversionHelpers.DowncastReference<IList<object>>(t.Result.Item2["results"]);

                if (resultsArray.Count is int resultLength && resultLength != batchSize)
                {
                    foreach (TaskCompletionSource<IDictionary<string, object>> tcs in sources)
                        tcs.TrySetException(new InvalidOperationException($"Batch command result count expected: {batchSize} but was: {resultLength}."));

                    return;
                }

                for (int i = 0; i < batchSize; ++i)
                {
                    Dictionary<string, object> result = resultsArray[i] as Dictionary<string, object>;
                    TaskCompletionSource<IDictionary<string, object>> tcs = sources[i];

                    if (result.ContainsKey("success"))
                        tcs.TrySetResult(result["success"] as IDictionary<string, object>);
                    else if (result.ContainsKey("error"))
                    {
                        IDictionary<string, object> error = result["error"] as IDictionary<string, object>;
                        long errorCode = (long) error["code"];
                        tcs.TrySetException(new ParseException((ParseException.ErrorCode) errorCode, error["error"] as string));
                    }
                    else
                        tcs.TrySetException(new InvalidOperationException("Invalid batch command response."));
                }
            });

            return tasks;
        }
    }
}
