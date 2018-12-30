using System.Threading.Tasks;

namespace Parse.Push.Internal
{
    public interface IDeviceInfoController
    {
        string DeviceType { get; }

        string DeviceTimeZone { get; }

        string AppBuildVersion { get; }

        string AppIdentifier { get; }

        string AppName { get; }

        Task ExecuteParseInstallationSaveHookAsync(ParseInstallation installation);

        void Initialize();
    }
}
