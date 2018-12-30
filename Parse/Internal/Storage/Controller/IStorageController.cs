using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.Common.Internal
{
    public interface IStorageController
    {
        Task<IStorageDictionary<string, object>> LoadAsync();

        Task<IStorageDictionary<string, object>> SaveAsync(IDictionary<string, object> contents);
    }

    public interface IStorageDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        int Count { get; }

        TValue this[TKey key] { get; }

        IEnumerable<TKey> Keys { get; }

        IEnumerable<TValue> Values { get; }

        bool ContainsKey(TKey key);

        bool TryGetValue(TKey key, out TValue value);

        Task AddAsync(TKey key, TValue value);

        Task RemoveAsync(TKey key);
    }
}