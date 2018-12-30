using System;
using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public class NoObjectsEncoder : ParseEncoder
    {
        public static NoObjectsEncoder Instance { get; } = new NoObjectsEncoder { };

        protected override IDictionary<string, object> EncodeParseObject(ParseObject value) => throw new ArgumentException("ParseObjects not allowed here.");
    }
}
