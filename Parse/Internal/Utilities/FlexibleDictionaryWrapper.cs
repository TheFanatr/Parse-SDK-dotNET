using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Parse.Utilities;

namespace Parse.Common.Internal
{
    [Preserve(AllMembers = true, Conditional = false)]
    public class FlexibleDictionaryWrapper<TOut, TIn> : IDictionary<string, TOut>
    {
        IDictionary<string, TIn> Target { get; }

        public FlexibleDictionaryWrapper(IDictionary<string, TIn> toWrap) => this.Target = toWrap;

        public void Add(string key, TOut value) => Target.Add(key, (TIn) ConversionHelpers.Downcast<TIn>(value));

        public bool ContainsKey(string key) => Target.ContainsKey(key);

        public ICollection<string> Keys => Target.Keys;

        public bool Remove(string key) => Target.Remove(key);

        public bool TryGetValue(string key, out TOut value)
        {
            bool result = Target.TryGetValue(key, out TIn outValue);
            value = (TOut) ConversionHelpers.Downcast<TOut>(outValue);
            return result;
        }

        public ICollection<TOut> Values => Target.Values.Select(item => (TOut) ConversionHelpers.Downcast<TOut>(item)).ToList();

        public TOut this[string key]
        {
            get => (TOut) ConversionHelpers.Downcast<TOut>(Target[key]);
            set => Target[key] = (TIn) ConversionHelpers.Downcast<TIn>(value);
        }

        public void Add(KeyValuePair<string, TOut> item) => Target.Add(new KeyValuePair<string, TIn>(item.Key, (TIn) ConversionHelpers.Downcast<TIn>(item.Value)));

        public void Clear() => Target.Clear();

        public bool Contains(KeyValuePair<string, TOut> item) => Target.Contains(new KeyValuePair<string, TIn>(item.Key, (TIn) ConversionHelpers.Downcast<TIn>(item.Value)));

        public void CopyTo(KeyValuePair<string, TOut>[] array, int arrayIndex) => (from pair in Target select new KeyValuePair<string, TOut>(pair.Key, (TOut) ConversionHelpers.Downcast<TOut>(pair.Value))).ToList().CopyTo(array, arrayIndex);

        public int Count => Target.Count;

        public bool IsReadOnly => Target.IsReadOnly;

        public bool Remove(KeyValuePair<string, TOut> item) => Target.Remove(new KeyValuePair<string, TIn>(item.Key, (TIn) ConversionHelpers.Downcast<TIn>(item.Value)));

        public IEnumerator<KeyValuePair<string, TOut>> GetEnumerator()
        {
            foreach (KeyValuePair<string, TIn> pair in Target)
                yield return new KeyValuePair<string, TOut>(pair.Key, (TOut) ConversionHelpers.Downcast<TOut>(pair.Value));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
