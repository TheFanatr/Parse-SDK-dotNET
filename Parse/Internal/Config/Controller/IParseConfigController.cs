using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseConfigController
    {
        IParseCurrentConfigController CurrentConfigController { get; }

        Task<ParseConfig> FetchConfigAsync(string sessionToken, CancellationToken cancellationToken);
    }
}
