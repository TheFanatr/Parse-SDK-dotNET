// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Parse.Utilities;

namespace Parse.Core.Internal
{
    public class ParseRemoveOperation : IParseFieldOperation
    {
        public ParseRemoveOperation(IEnumerable<object> objects) => Objects = new ReadOnlyCollection<object>(objects.Distinct().ToList());

        public object Encode() => new Dictionary<string, object>
        {
            ["__op"] = "Remove",
            ["objects"] = PointerOrLocalIdEncoder.Instance.Encode(Objects)
        };

        public IParseFieldOperation MergeWithPrevious(IParseFieldOperation previous)
        {
            switch (previous)
            {
                case null:
                    return this;
                case ParseDeleteOperation _:
                    return previous;
                case ParseSetOperation setOp:
                    return new ParseSetOperation(Apply(ConversionHelpers.DowncastReference<IList<object>>(setOp.Value), null));
                case ParseRemoveOperation oldOp:
                    return new ParseRemoveOperation(oldOp.Objects.Concat(Objects));
                default:
                    throw new InvalidOperationException("Operation is invalid after previous operation.");
            }
        }

        public object Apply(object oldValue, string key) => oldValue is null ? new List<object> { } : ConversionHelpers.DowncastReference<IList<object>>(oldValue).Except(Objects, ParseFieldOperations.ParseObjectComparer).ToList();

        public IEnumerable<object> Objects { get; }
    }
}
