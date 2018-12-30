using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public interface IParseCorePlugins
    {
        void Reset();

        IHttpClient HttpClient { get; }

        IParseCommandRunner CommandRunner { get; }

        IStorageController StorageController { get; }

        IParseCloudCodeController CloudCodeController { get; }

        IParseConfigController ConfigController { get; }

        IParseFileController FileController { get; }

        IParseObjectController ObjectController { get; }

        IParseQueryController QueryController { get; }

        IParseSessionController SessionController { get; }

        IParseUserController UserController { get; }

        IObjectSubclassingController SubclassingController { get; }

        IParseCurrentUserController CurrentUserController { get; }

        IInstallationIdController InstallationIdController { get; }
    }
}