using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parse.Utilities;

namespace Parse.Common.Internal
{
    [Preserve(AllMembers = true, Conditional = false)]
    public class FlexibleListWrapper<TOut, TIn> : IList<TOut>
    {
        IList<TIn> Target { get; }

        public FlexibleListWrapper(IList<TIn> toWrap) => Target = toWrap;

        public int IndexOf(TOut item) => Target.IndexOf((TIn) ConversionHelpers.Downcast<TIn>(item));

        public void Insert(int index, TOut item) => Target.Insert(index, (TIn) ConversionHelpers.Downcast<TIn>(item));

        public void RemoveAt(int index) => Target.RemoveAt(index);

        public TOut this[int index]
        {
            get => (TOut) ConversionHelpers.Downcast<TOut>(Target[index]);
            set => Target[index] = (TIn) ConversionHelpers.Downcast<TIn>(value);
        }

        public void Add(TOut item) => Target.Add((TIn) ConversionHelpers.Downcast<TIn>(item));

        public void Clear() => Target.Clear();

        public bool Contains(TOut item) => Target.Contains((TIn) ConversionHelpers.Downcast<TIn>(item));

        public void CopyTo(TOut[] array, int arrayIndex) => Target.Select(item => (TOut) ConversionHelpers.Downcast<TOut>(item)).ToList().CopyTo(array, arrayIndex);

        public int Count => Target.Count;

        public bool IsReadOnly => Target.IsReadOnly;

        public bool Remove(TOut item) => Target.Remove((TIn) ConversionHelpers.Downcast<TIn>(item));

        public IEnumerator<TOut> GetEnumerator()
        {
            foreach (object item in (IEnumerable) Target)
                yield return (TOut) ConversionHelpers.Downcast<TOut>(item);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
