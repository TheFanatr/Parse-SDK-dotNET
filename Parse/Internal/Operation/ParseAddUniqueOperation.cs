using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Parse.Utilities;

namespace Parse.Core.Internal
{
    public class ParseAddUniqueOperation : IParseFieldOperation
    {
        public ParseAddUniqueOperation(IEnumerable<object> objects) => Queue = new ReadOnlyCollection<object>(objects.Distinct().ToList());

        public object Encode() => new Dictionary<string, object>
        {
            ["__op"] = "AddUnique",
            ["objects"] = PointerOrLocalIdEncoder.Instance.Encode(Queue)
        };

        public IParseFieldOperation MergeWithPrevious(IParseFieldOperation previous)
        {
            switch (previous)
            {
                case null:
                    return this;
                case ParseDeleteOperation _:
                    return new ParseSetOperation(Queue.ToList());
                case ParseSetOperation setOp:
                    IList<object> oldList = ConversionHelpers.DowncastValue<IList<object>>(setOp.Value);
                    object result = Apply(oldList, null);
                    return new ParseSetOperation(result);
                case ParseAddUniqueOperation _:
                    return new ParseAddUniqueOperation((IList<object>) Apply(((ParseAddUniqueOperation) previous).Queue, null));
                default:
                    throw new InvalidOperationException("Operation is invalid after previous operation.");
            }
        }

        public object Apply(object oldValue, string key)
        {
            if (oldValue is null)
                return Queue.ToList();
            
            List<object> newList = ConversionHelpers.DowncastValue<IList<object>>(oldValue).ToList();
            IEqualityComparer<object> comparer = ParseFieldOperations.ParseObjectComparer;

            foreach (object objToAdd in Queue)
            {
                if (objToAdd is ParseObject)
                {
                    if (newList.FirstOrDefault(listObj => comparer.Equals(objToAdd, listObj)) is object matchedObj)
                        newList[newList.IndexOf(matchedObj)] = objToAdd;
                    else
                        newList.Add(objToAdd);
                }
                else if (!newList.Contains(objToAdd, comparer))
                    newList.Add(objToAdd);
            }

            return newList;
        }

        public IEnumerable<object> Queue { get; }
    }
}
