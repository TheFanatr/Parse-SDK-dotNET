using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseAuthenticationProvider
    {
        Task<IDictionary<string, object>> AuthenticateAsync(CancellationToken cancellationToken);

        void Deauthenticate();

        bool RestoreAuthentication(IDictionary<string, object> authData);

        string AuthType { get; }
    }
}
