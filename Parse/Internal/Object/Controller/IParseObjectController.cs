using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseObjectController
    {
        Task<IObjectState> FetchAsync(IObjectState state, string sessionToken, CancellationToken cancellationToken);

        Task<IObjectState> SaveAsync(IObjectState state, IDictionary<string, IParseFieldOperation> operations, string sessionToken, CancellationToken cancellationToken);

        IList<Task<IObjectState>> SaveAllAsync(IList<IObjectState> states, IList<IDictionary<string, IParseFieldOperation>> operationsList, string sessionToken, CancellationToken cancellationToken);

        Task DeleteAsync(IObjectState state, string sessionToken, CancellationToken cancellationToken);

        IList<Task> DeleteAllAsync(IList<IObjectState> states, string sessionToken, CancellationToken cancellationToken);
    }
}
