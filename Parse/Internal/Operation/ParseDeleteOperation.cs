using System.Collections.Generic;

namespace Parse.Core.Internal
{
    public class ParseDeleteOperation : IParseFieldOperation
    {
        internal static object DeleteToken { get; } = new object { };

        public static ParseDeleteOperation Instance { get; } = new ParseDeleteOperation();

        ParseDeleteOperation() { }

        public object Encode() => new Dictionary<string, object> { ["__op"] = "Delete" };

        public IParseFieldOperation MergeWithPrevious(IParseFieldOperation previous) => this;

        public object Apply(object oldValue, string key) => DeleteToken;
    }
}
