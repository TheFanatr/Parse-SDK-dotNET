using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public static class ParseSessionExtensions
    {
        public static Task<string> UpgradeToRevocableSessionAsync(string sessionToken, CancellationToken cancellationToken) => ParseSession.UpgradeToRevocableSessionAsync(sessionToken, cancellationToken);

        public static Task RevokeAsync(string sessionToken, CancellationToken cancellationToken) => ParseSession.RevokeAsync(sessionToken, cancellationToken);
    }
}
