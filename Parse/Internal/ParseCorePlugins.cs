using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class ParseCorePlugins : IParseCorePlugins
    {
        static object InstanceMutex { get; } = new object { };

        static IParseCorePlugins _Instance;

        public static IParseCorePlugins Instance
        {
            get
            {
                lock (InstanceMutex)
                    return _Instance = _Instance ?? new ParseCorePlugins { };
            }
            set
            {
                lock (InstanceMutex)
                    _Instance = value;
            }
        }

        object Mutex { get; } = new object { };
        
        public void Reset()
        {
            lock (Mutex)
            {
                HttpClient = null;
                CommandRunner = null;
                StorageController = null;

                CloudCodeController = null;
                FileController = null;
                ObjectController = null;
                SessionController = null;
                UserController = null;
                SubclassingController = null;

                CurrentUserController = null;
                InstallationIdController = null;
            }
        }

        IHttpClient _HttpClient;

        public IHttpClient HttpClient
        {
            get
            {
                lock (Mutex)
                    return _HttpClient = _HttpClient ?? new HttpClient();
            }
            set
            {
                lock (Mutex)
                    _HttpClient = value;
            }
        }

        IParseCommandRunner _CommandRunner;

        public IParseCommandRunner CommandRunner
        {
            get
            {
                lock (Mutex)
                    return _CommandRunner = _CommandRunner ?? new ParseCommandRunner(HttpClient, InstallationIdController);
            }
            set
            {
                lock (Mutex)
                    _CommandRunner = value;
            }
        }

        IStorageController _StorageController;

        public IStorageController StorageController
        {
            get
            {
                lock (Mutex)
                    return _StorageController = _StorageController ?? new StorageController { };
            }
            set
            {
                lock (Mutex)
                    _StorageController = value;
            }
        }

        IParseCloudCodeController _CloudCodeController;

        public IParseCloudCodeController CloudCodeController
        {
            get
            {
                lock (Mutex)
                    return _CloudCodeController = _CloudCodeController ?? new ParseCloudCodeController(CommandRunner);
            }
            set
            {
                lock (Mutex)
                    _CloudCodeController = value;
            }
        }

        IParseConfigController _ConfigController;

        public IParseFileController FileController
        {
            get
            {
                lock (Mutex)
                    return _FileController = _FileController ?? new ParseFileController(CommandRunner);
            }
            set
            {
                lock (Mutex)
                    _FileController = value;
            }
        }

        IParseFileController _FileController;

        public IParseConfigController ConfigController
        {
            get
            {
                lock (Mutex)
                    return _ConfigController ?? (_ConfigController = new ParseConfigController(CommandRunner, StorageController));
            }
            set
            {
                lock (Mutex)
                    _ConfigController = value;
            }
        }

        IParseObjectController _ObjectController;

        public IParseObjectController ObjectController
        {
            get
            {
                lock (Mutex)
                    return _ObjectController = _ObjectController ?? new ParseObjectController(CommandRunner);
            }
            set
            {
                lock (Mutex)
                    _ObjectController = value;
            }
        }

        IParseQueryController _QueryController;

        public IParseQueryController QueryController
        {
            get
            {
                lock (Mutex)
                    return _QueryController ?? (_QueryController = new ParseQueryController(CommandRunner));
            }
            set
            {
                lock (Mutex)
                    _QueryController = value;
            }
        }

        IParseSessionController _SessionController;

        public IParseSessionController SessionController
        {
            get
            {
                lock (Mutex)
                    return _SessionController = _SessionController ?? new ParseSessionController(CommandRunner);
            }
            set
            {
                lock (Mutex)
                    _SessionController = value;
            }
        }

        IParseUserController _UserController;

        public IParseUserController UserController
        {
            get
            {
                lock (Mutex)
                    return _UserController = _UserController ?? new ParseUserController(CommandRunner);
            }
            set
            {
                lock (Mutex)
                    _UserController = value;
            }
        }

        IObjectSubclassingController _SubclassingController;
        
        public IParseCurrentUserController CurrentUserController
        {
            get
            {
                lock (Mutex)
                    return _CurrentUserController = _CurrentUserController ?? new ParseCurrentUserController(StorageController);
            }
            set
            {
                lock (Mutex)
                    _CurrentUserController = value;
            }
        }

        IParseCurrentUserController _CurrentUserController;

        public IObjectSubclassingController SubclassingController
        {
            get
            {
                lock (Mutex)
                {
                    if (_SubclassingController == null)
                    {
                        _SubclassingController = new ObjectSubclassingController();
                        _SubclassingController.AddRegisterHook(typeof(ParseUser), () => CurrentUserController.ClearFromMemory());
                    }
                    return _SubclassingController;
                }
            }
            set
            {
                lock (Mutex)
                    _SubclassingController = value;
            }
        }

        IInstallationIdController _InstallationIdController;

        public IInstallationIdController InstallationIdController
        {
            get
            {
                lock (Mutex)
                    return _InstallationIdController = _InstallationIdController ?? new InstallationIdController(StorageController);
            }
            set
            {
                lock (Mutex)
                    _InstallationIdController = value;
            }
        }
    }
}
