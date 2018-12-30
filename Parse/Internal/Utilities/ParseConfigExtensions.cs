using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public static class ParseConfigExtensions
    {
        public static ParseConfig Create(IDictionary<string, object> fetchedConfig) => new ParseConfig(fetchedConfig);
    }
}
