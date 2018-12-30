using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public static class ParseObjectExtensions
    {
        public static T FromState<T>(IObjectState state, string defaultClassName) where T : ParseObject => ParseObject.FromState<T>(state, defaultClassName);

        public static IObjectState GetState(this ParseObject obj) => obj.State;

        public static void HandleFetchResult(this ParseObject obj, IObjectState serverState) => obj.HandleFetchResult(serverState);

        public static IDictionary<string, IParseFieldOperation> GetCurrentOperations(this ParseObject obj) => obj.CurrentOperations;

        public static IEnumerable<object> DeepTraversal(object root, bool traverseParseObjects = false, bool yieldRoot = false) => ParseObject.DeepTraversal(root, traverseParseObjects, yieldRoot);

        public static void SetIfDifferent<T>(this ParseObject obj, string key, T value) => obj.SetIfDifferent<T>(key, value);

        public static IDictionary<string, object> ServerDataToJSONObjectForSerialization(this ParseObject obj) => obj.ServerDataToJSONObjectForSerialization();

        public static void Set(this ParseObject obj, string key, object value) => obj.Set(key, value);
    }
}
