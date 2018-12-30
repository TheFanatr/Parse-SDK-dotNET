using Parse.Core.Internal;

namespace Parse.Analytics.Internal
{
    public interface IParseAnalyticsPlugins
    {
        void Reset();

        IParseCorePlugins CorePlugins { get; }
        IParseAnalyticsController Controller { get; }
    }
}