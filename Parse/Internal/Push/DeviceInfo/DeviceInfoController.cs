using System;
using System.Reflection;
using System.Threading.Tasks;
using static Parse.ParseClient.Configuration;

namespace Parse.Push.Internal
{
    public class DeviceInfoController : IDeviceInfoController
    {
        public string DeviceType { get; } = ParseClient.CurrentConfiguration.VersionInfo.OSVersion ?? Environment.OSVersion.ToString();

        public string DeviceTimeZone => TimeZoneInfo.Local.StandardName;

        public string AppBuildVersion { get; } = ParseClient.CurrentConfiguration.VersionInfo.BuildVersion ?? Assembly.GetEntryAssembly().GetName().Version.Build.ToString();

        public string AppIdentifier { get; } = GetIdentifierOrName(() => AppDomain.CurrentDomain.FriendlyName);

        public string AppName { get; } = GetIdentifierOrName(() => Assembly.GetEntryAssembly().GetName().Name);

        public Task ExecuteParseInstallationSaveHookAsync(ParseInstallation installation) => Task.FromResult<object>(null);

        public void Initialize() { }

        static string GetIdentifierOrName(Func<string> fallback) => ParseClient.CurrentConfiguration.StorageConfiguration is MetadataBasedStorageConfiguration storageMetadata ? storageMetadata.ProductName : (ParseClient.CurrentConfiguration.StorageConfiguration is IdentifierBasedStorageConfiguration storageIdentifier ? storageIdentifier.Identifier : fallback.Invoke());
    }
}