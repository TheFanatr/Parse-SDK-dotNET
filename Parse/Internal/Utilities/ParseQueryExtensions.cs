using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public static class ParseQueryExtensions
    {
        public static string GetClassName<T>(this ParseQuery<T> query) where T : ParseObject => query.ClassName;

        public static IDictionary<string, object> BuildParameters<T>(this ParseQuery<T> query) where T : ParseObject => query.BuildParameters(false);

        public static object GetConstraint<T>(this ParseQuery<T> query, string key) where T : ParseObject => query.GetConstraint(key);
    }
}
