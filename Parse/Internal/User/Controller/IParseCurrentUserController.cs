using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseCurrentUserController : IParseObjectCurrentController<ParseUser>
    {
        Task<string> GetCurrentSessionTokenAsync(CancellationToken cancellationToken);

        Task LogOutAsync(CancellationToken cancellationToken);
    }
}
