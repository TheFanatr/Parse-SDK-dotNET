using Parse.Core.Internal;

namespace Parse.Analytics.Internal
{
    public class ParseAnalyticsPlugins : IParseAnalyticsPlugins
    {
        static object InstanceMutex { get; } = new object { };

        object Mutex { get; } = new object { };

        public void Reset()
        {
            lock (Mutex)
            {
                CorePlugins = null;
                Controller = null;
            }
        }

        static IParseAnalyticsPlugins _Instance;

        public static IParseAnalyticsPlugins Instance
        {
            get
            {
                lock (InstanceMutex)
                    return _Instance = _Instance ?? new ParseAnalyticsPlugins { };
            }
            set
            {
                lock (InstanceMutex)
                    _Instance = value;
            }
        }

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

        IParseAnalyticsController _Controller;

        public IParseAnalyticsController Controller
        {
            get
            {
                lock (Mutex)
                    return _Controller = _Controller ?? new ParseAnalyticsController(CorePlugins.CommandRunner);
            }
            set
            {
                lock (Mutex)
                    _Controller = value;
            }
        }
    }
}