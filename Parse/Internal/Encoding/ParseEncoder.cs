using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Parse.Common.Internal;
using Parse.Utilities;

namespace Parse.Core.Internal
{
    public abstract class ParseEncoder
    {
        public static bool IsValidType(object value) => value == null || ReflectionHelpers.IsPrimitive(value.GetType()) || value is string || value is ParseObject || value is ParseACL || value is ParseFile || value is ParseGeoPoint || value is ParseRelationBase || value is DateTime || value is byte[] || ConversionHelpers.DowncastReference<IDictionary<string, object>>(value) != null || ConversionHelpers.DowncastReference<IList<object>>(value) != null;

        public object Encode(object value)
        {
            switch (value)
            {
                case DateTime _:
                    return new Dictionary<string, object>
                    {
                        ["iso"] = ((DateTime) value).ToString(ParseClient.DateFormatStrings.First(), CultureInfo.InvariantCulture),
                        ["__type"] = "Date"
                    };
                case byte[] bytes:
                    return new Dictionary<string, object>
                    {
                        ["__type"] = "Bytes",
                        ["base64"] = Convert.ToBase64String(bytes)
                    };
                case ParseObject obj:
                    return EncodeParseObject(obj);
                case IJsonConvertible jsonConvertible:
                    return jsonConvertible.ToJSON();
                case object _ when ConversionHelpers.DowncastReference<IDictionary<string, object>>(value) is IDictionary<string, object> dict:
                    Dictionary<string, object> json = new Dictionary<string, object> { };
                    foreach (KeyValuePair<string, object> pair in dict)
                        json[pair.Key] = Encode(pair.Value);

                    return json;
                case object _ when ConversionHelpers.DowncastReference<IList<object>>(value) is IList<object> list:
                    return EncodeList(list);
                case IParseFieldOperation operation:
                    // TODO: Convert IParseFieldOperation to IJsonConvertible.
                    return operation.Encode();
                default:
                    return value;
            }
        }

        protected abstract IDictionary<string, object> EncodeParseObject(ParseObject value);

        object EncodeList(IList<object> list)
        {
            List<object> newArray = new List<object> { };
            foreach (object item in list)
            {
                if (!IsValidType(item))
                    throw new ArgumentException("Invalid type for value in an array");

                newArray.Add(Encode(item));
            }
            return newArray;
        }
    }
}
