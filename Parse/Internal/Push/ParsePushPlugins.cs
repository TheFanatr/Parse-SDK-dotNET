using Parse.Core.Internal;

namespace Parse.Push.Internal
{
    public class ParsePushPlugins : IParsePushPlugins
    {
        static object InstanceMutex { get; } = new object { };

        static IParsePushPlugins _Instance;

        public static IParsePushPlugins Instance
        {
            get
            {
                lock (InstanceMutex)
                    return _Instance = _Instance ?? new ParsePushPlugins { };
            }
            set
            {
                lock (InstanceMutex)
                    _Instance = value;
            }
        }

        object Mutex { get; } = new object { };

        IParseCorePlugins _CorePlugins;

        public IParseCorePlugins CorePlugins
        {
            get
            {
                lock (Mutex)
                    return _CorePlugins = _CorePlugins ?? ParseCorePlugins.Instance;
            }
            set
            {
                lock (Mutex)
                    _CorePlugins = value;
            }
        }

        IParsePushChannelsController _PushChannelsController;

        public IParsePushChannelsController PushChannelsController
        {
            get
            {
                lock (Mutex)
                    return _PushChannelsController = _PushChannelsController ?? new ParsePushChannelsController { };
            }
            set
            {
                lock (Mutex)
                    _PushChannelsController = value;
            }
        }

        IParsePushController _PushController;

        public IParsePushController PushController
        {
            get
            {
                lock (Mutex)
                    return _PushController = _PushController ?? new ParsePushController(CorePlugins.CommandRunner, CorePlugins.CurrentUserController);
            }
            set
            {
                lock (Mutex)
                    _PushController = value;
            }
        }

        IParseCurrentInstallationController _CurrentInstallationController;

        public IParseCurrentInstallationController CurrentInstallationController
        {
            get
            {
                lock (Mutex)
                    return _CurrentInstallationController = _CurrentInstallationController ?? new ParseCurrentInstallationController(CorePlugins.InstallationIdController, CorePlugins.StorageController, ParseInstallationCoder.Instance);
            }
            set
            {
                lock (Mutex)
                    _CurrentInstallationController = value;
            }
        }

        IDeviceInfoController _DeviceInfoController;

        public IDeviceInfoController DeviceInfoController
        {
            get
            {
                lock (Mutex)
                    return _DeviceInfoController = _DeviceInfoController ?? new DeviceInfoController { };
            }
            set
            {
                lock (Mutex)
                    _DeviceInfoController = value;
            }
        }

        public void Reset()
        {
            lock (Mutex)
            {
                CorePlugins = null;
                PushChannelsController = null;
                PushController = null;
                CurrentInstallationController = null;
                DeviceInfoController = null;
            }
        }
    }
}
