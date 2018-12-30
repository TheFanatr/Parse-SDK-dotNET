using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Parse.Utilities;

namespace Parse.Core.Internal
{
    public class ParseDecoder
    {
        public static ParseDecoder Instance { get; } = new ParseDecoder { };

        ParseDecoder() { }

        public object Decode(object data)
        {
            if (data is null)
                return null;

            if (data is IDictionary<string, object> dict)
            {
                if (dict.ContainsKey("__op"))
                    return ParseFieldOperations.Decode(dict);

                dict.TryGetValue("__type", out object type);

                switch (type)
                {
                    case "Date":
                        return ParseDate(dict["iso"] as string);
                    case "Bytes":
                        return Convert.FromBase64String(dict["base64"] as string);
                    case "Pointer":
                        return DecodePointer(dict["className"] as string, dict["objectId"] as string);
                    case "File":
                        return new ParseFile(dict["name"] as string, new Uri(dict["url"] as string));
                    case "GeoPoint":
                        return new ParseGeoPoint(ConversionHelpers.DowncastValue<double>(dict["latitude"]),
                            ConversionHelpers.DowncastValue<double>(dict["longitude"]));
                    case "Object":
                        IObjectState state = ParseObjectCoder.Instance.Decode(dict, this);
                        return ParseObject.FromState<ParseObject>(state, dict["className"] as string);
                    case "Relation":
                        return ParseRelationBase.CreateRelation(null, null, dict["className"] as string);
                    case object anything when !(anything is string):
                        Dictionary<string, object> newDict = new Dictionary<string, object> { };
                        foreach (KeyValuePair<string, object> pair in dict)
                            newDict[pair.Key] = Decode(pair.Value);
                        return newDict;
                }

                Dictionary<string, object> converted = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> pair in dict)
                    converted[pair.Key] = Decode(pair.Value);

                return converted;
            }

            return data is IList<object> list ? (from item in list select Decode(item)).ToList() : data;
        }

        protected virtual object DecodePointer(string className, string objectId) => ParseObject.CreateWithoutData(className, objectId);

        public static DateTime ParseDate(string input) => DateTime.ParseExact(input, ParseClient.DateFormatStrings, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }
}
