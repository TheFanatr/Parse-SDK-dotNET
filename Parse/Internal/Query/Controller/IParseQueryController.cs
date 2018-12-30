using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseQueryController
    {
        Task<IEnumerable<IObjectState>> FindAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken) where T : ParseObject;

        Task<int> CountAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken) where T : ParseObject;

        Task<IObjectState> FirstAsync<T>(ParseQuery<T> query, ParseUser user, CancellationToken cancellationToken) where T : ParseObject;
    }
}
