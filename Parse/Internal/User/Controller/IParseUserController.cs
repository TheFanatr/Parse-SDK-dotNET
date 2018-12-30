using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseUserController
    {
        Task<IObjectState> SignUpAsync(IObjectState state, IDictionary<string, IParseFieldOperation> operations, CancellationToken cancellationToken);

        Task<IObjectState> LogInAsync(string username, string password, CancellationToken cancellationToken);

        Task<IObjectState> LogInAsync(string authType, IDictionary<string, object> data, CancellationToken cancellationToken);

        Task<IObjectState> GetUserAsync(string sessionToken, CancellationToken cancellationToken);

        Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken);
    }
}
