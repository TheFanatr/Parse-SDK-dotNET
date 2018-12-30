using System;
using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public class PointerOrLocalIdEncoder : ParseEncoder
    {
        public static PointerOrLocalIdEncoder Instance { get; } = new PointerOrLocalIdEncoder();

        protected override IDictionary<string, object> EncodeParseObject(ParseObject value)
        {
            // TODO: Handle local identifier.
            if (value.ObjectId == null)
                throw new ArgumentException("Cannot create a pointer to an object without an objectId");

            return new Dictionary<string, object>
            {
                ["__type"] = "Pointer",
                ["className"] = value.ClassName,
                ["objectId"] = value.ObjectId
            };
        }
    }
}
