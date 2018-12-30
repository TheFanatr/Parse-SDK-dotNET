using System;
using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public class ParseObjectIdComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object p1, object p2)
        {
            ParseObject parseObj2 = p2 as ParseObject;
            return p1 is ParseObject parseObj1 && parseObj2 != null ? Equals(parseObj1.ObjectId, parseObj2.ObjectId) : Equals(p1, p2);
        }

        public int GetHashCode(object p) => p is ParseObject parseObject ? parseObject.ObjectId.GetHashCode() : p.GetHashCode();
    }

    static class ParseFieldOperations
    {
        static ParseObjectIdComparer Comparer { get; set; }

        public static IParseFieldOperation Decode(IDictionary<string, object> json) => throw new NotImplementedException();

        public static IEqualityComparer<object> ParseObjectComparer => Comparer = Comparer ?? new ParseObjectIdComparer { };
    }
}
