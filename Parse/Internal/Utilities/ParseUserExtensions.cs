using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public static class ParseUserExtensions
    {
        public static IDictionary<string, IDictionary<string, object>> GetAuthData(this ParseUser user) => user.AuthData;

        public static Task UnlinkFromAsync(this ParseUser user, string authType, CancellationToken cancellationToken) => user.UnlinkFromAsync(authType, cancellationToken);

        public static Task<ParseUser> LogInWithAsync(string authType, CancellationToken cancellationToken) => ParseUser.LogInWithAsync(authType, cancellationToken);

        public static Task<ParseUser> LogInWithAsync(string authType, IDictionary<string, object> data, CancellationToken cancellationToken) => ParseUser.LogInWithAsync(authType, data, cancellationToken);

        public static Task LinkWithAsync(this ParseUser user, string authType, CancellationToken cancellationToken) => user.LinkWithAsync(authType, cancellationToken);

        public static Task LinkWithAsync(this ParseUser user, string authType, IDictionary<string, object> data, CancellationToken cancellationToken) => user.LinkWithAsync(authType, data, cancellationToken);

        public static Task UpgradeToRevocableSessionAsync(this ParseUser user, CancellationToken cancellationToken) => user.UpgradeToRevocableSessionAsync(cancellationToken);
    }
}
