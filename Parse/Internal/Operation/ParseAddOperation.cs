using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Parse.Utilities;

namespace Parse.Core.Internal
{
    public class ParseAddOperation : IParseFieldOperation
    {
        public ParseAddOperation(IEnumerable<object> objects) => Objects = new ReadOnlyCollection<object>(objects.ToList());

        public object Encode() => new Dictionary<string, object>
        {
            ["__op"] = "Add",
            ["objects"] = PointerOrLocalIdEncoder.Instance.Encode(Objects)
        };

        public IParseFieldOperation MergeWithPrevious(IParseFieldOperation previous)
        {
            switch (previous)
            {
                case null:
                    return this;
                case ParseDeleteOperation _:
                    return new ParseSetOperation(Objects.ToList());
                case ParseSetOperation setOp:
                    return new ParseSetOperation(ConversionHelpers.DowncastValue<IList<object>>(setOp.Value).Concat(Objects).ToList());
                case ParseAddOperation _:
                    return new ParseAddOperation(((ParseAddOperation) previous).Objects.Concat(Objects));
                default:
                    throw new InvalidOperationException("Operation is invalid after previous operation.");
            }
        }

        public object Apply(object oldValue, string key) => oldValue == null ? Objects.ToList() : ConversionHelpers.DowncastValue<IList<object>>(oldValue).Concat(Objects).ToList();

        public IEnumerable<object> Objects { get; }
    }
}
