using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    internal class ParseQueryController : IParseQueryController
    {
        IParseCommandRunner CommandRunner { get; }

        public ParseQueryController(IParseCommandRunner commandRunner) => CommandRunner = commandRunner;

        public Task<IEnumerable<IObjectState>> FindAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken) where T : ParseObject => FindAsync(query.ClassName, query.BuildParameters(), user?.SessionToken, cancellationToken).OnSuccess(t => (from item in t.Result["results"] as IList<object> select ParseObjectCoder.Instance.Decode(item as IDictionary<string, object>, ParseDecoder.Instance)));

        public Task<int> CountAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken) where T : ParseObject
        {
            IDictionary<string, object> parameters = query.BuildParameters();
            parameters["limit"] = 0;
            parameters["count"] = 1;

            return FindAsync(query.ClassName, parameters, user?.SessionToken, cancellationToken).OnSuccess(t => Convert.ToInt32(t.Result["count"]));
        }

        public Task<IObjectState> FirstAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken) where T : ParseObject
        {
            IDictionary<string, object> parameters = query.BuildParameters();
            parameters["limit"] = 1;

            return FindAsync(query.ClassName, parameters, user?.SessionToken, cancellationToken).OnSuccess(t => (t.Result["results"] as IList<object>).FirstOrDefault() as IDictionary<string, object> is Dictionary<string, object> item && item != null ? ParseObjectCoder.Instance.Decode(item, ParseDecoder.Instance) : null);
        }

        Task<IDictionary<string, object>> FindAsync(string className, IDictionary<string, object> parameters, string sessionToken, CancellationToken cancellationToken) => CommandRunner.RunCommandAsync(new ParseCommand($"classes/{Uri.EscapeDataString(className)}?{ParseClient.BuildQueryString(parameters)}", "GET", sessionToken, data: null), cancellationToken: cancellationToken).OnSuccess(t => t.Result.Item2);
    }
}
