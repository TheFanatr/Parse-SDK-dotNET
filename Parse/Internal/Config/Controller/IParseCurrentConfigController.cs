using System.Threading.Tasks;

namespace Parse.Core.Internal
{
    public interface IParseCurrentConfigController
    {
        Task<ParseConfig> GetCurrentConfigAsync();

        Task SetCurrentConfigAsync(ParseConfig config);

        Task ClearCurrentConfigAsync();

        Task ClearCurrentConfigInMemoryAsync();
    }
}
