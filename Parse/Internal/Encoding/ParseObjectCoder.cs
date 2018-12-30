using System;
using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public class ParseObjectCoder
    {
        public static ParseObjectCoder Instance { get; } = new ParseObjectCoder { };
        
        ParseObjectCoder() { }

        public IDictionary<string, object> Encode<T>(T state, IDictionary<string, IParseFieldOperation> operations, ParseEncoder encoder) where T : IObjectState
        {
            Dictionary<string, object> result = new Dictionary<string, object> { };
            foreach (KeyValuePair<string, IParseFieldOperation> pair in operations)
                result[pair.Key] = encoder.Encode(pair.Value);

            return result;
        }

        public IObjectState Decode(IDictionary<string, object> data, ParseDecoder decoder)
        {
            IDictionary<string, object> serverData = new Dictionary<string, object> { };
            Dictionary<string, object> mutableData = new Dictionary<string, object>(data);
            DateTime? createdAt = ExtractFromDictionary<DateTime?>(mutableData, "createdAt", (obj) => ParseDecoder.ParseDate(obj as string));
            DateTime? updatedAt = ExtractFromDictionary<DateTime?>(mutableData, "updatedAt", (obj) => ParseDecoder.ParseDate(obj as string));

            if (mutableData.ContainsKey("ACL"))
                serverData["ACL"] = ExtractFromDictionary(mutableData, "ACL", (obj) => new ParseACL(obj as IDictionary<string, object>));

            if (createdAt != null && updatedAt is null)
                updatedAt = createdAt;
            
            foreach (KeyValuePair<string, object> pair in mutableData)
            {
                if (pair.Key == "__type" || pair.Key == "className")
                    continue;

                serverData[pair.Key] = decoder.Decode(pair.Value);
            }

            return new MutableObjectState
            {
                ObjectId = ExtractFromDictionary(mutableData, "objectId", (obj) => obj as string),
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ServerData = serverData
            };
        }

        T ExtractFromDictionary<T>(IDictionary<string, object> data, string key, Func<object, T> action)
        {
            T result = default;
            if (data.ContainsKey(key))
            {
                result = action(data[key]);
                data.Remove(key);
            }

            return result;
        }
    }
}
