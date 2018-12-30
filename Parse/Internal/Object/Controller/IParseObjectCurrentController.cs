using System.Threading;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseObjectCurrentController<T> where T : ParseObject
    {
        Task SetAsync(T obj, CancellationToken cancellationToken);

        Task<T> GetAsync(CancellationToken cancellationToken);

        Task<bool> ExistsAsync(CancellationToken cancellationToken);

        bool IsCurrent(T obj);

        void ClearFromMemory();

        void ClearFromDisk();
    }
}
