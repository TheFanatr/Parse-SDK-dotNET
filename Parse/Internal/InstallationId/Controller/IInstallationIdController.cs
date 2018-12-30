using System;
using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IInstallationIdController
    {
        Task SetAsync(Guid? installationId);

        Task<Guid?> GetAsync();

        Task ClearAsync();
    }
}
