using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseSessionController
    {
        Task<IObjectState> GetSessionAsync(string sessionToken, CancellationToken cancellationToken);

        Task RevokeAsync(string sessionToken, CancellationToken cancellationToken);

        Task<IObjectState> UpgradeToRevocableSessionAsync(string sessionToken, CancellationToken cancellationToken);

        bool IsRevocableSessionToken(string sessionToken);
    }
}
