using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parse.Internal.Utilities;

namespace Parse.Common.Internal
{
    public class StorageController : IStorageController
    {
        private class StorageDictionary : IStorageDictionary<string, object>
        {
            object Mutex { get; } = new object { };

            Dictionary<string, object> InternalDictionary;

            FileInfo File { get; set; }

            public StorageDictionary(FileInfo file)
            {
                File = file;
                InternalDictionary = new Dictionary<string, object>();
            }

            internal Task SaveAsync()
            {
                string json;
                lock (Mutex)
                    json = JsonProcessor.Encode(InternalDictionary);

                return File.WriteToAsync(json);
            }

            internal Task LoadAsync() => File.ReadAllTextAsync().ContinueWith(t =>
            {
                string text = t.Result;
                Dictionary<string, object> result = null;
                try { result = JsonProcessor.Parse(text) as Dictionary<string, object>; }
                catch (Exception) { }

                lock (Mutex)
                    InternalDictionary = result ?? new Dictionary<string, object> { };
            });

            internal void Update(IDictionary<string, object> contents)
            {
                lock (Mutex)
                    InternalDictionary = contents.ToDictionary(p => p.Key, p => p.Value);
            }

            public Task AddAsync(string key, object value)
            {
                lock (Mutex)
                    InternalDictionary[key] = value;
                return SaveAsync();
            }

            public Task RemoveAsync(string key)
            {
                lock (Mutex)
                    InternalDictionary.Remove(key);
                return SaveAsync();
            }

            public bool ContainsKey(string key)
            {
                lock (Mutex)
                    return InternalDictionary.ContainsKey(key);
            }

            public IEnumerable<string> Keys
            {
                get
                {
                    lock (Mutex)
                        return InternalDictionary.Keys;
                }
            }

            public bool TryGetValue(string key, out object value)
            {
                lock (Mutex)
                    return InternalDictionary.TryGetValue(key, out value);
            }

            public IEnumerable<object> Values
            {
                get
                {
                    lock (Mutex)
                        return InternalDictionary.Values;
                }
            }

            public object this[string key]
            {
                get
                {
                    lock (Mutex)
                        return InternalDictionary[key];
                }
            }

            public int Count
            {
                get
                {
                    lock (Mutex)
                        return InternalDictionary.Count;
                }
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                lock (Mutex)
                    return InternalDictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                lock (Mutex)
                    return InternalDictionary.GetEnumerator();
            }
        }

        FileInfo File { get; }

        StorageDictionary Storage { get; set; }

        TaskQueue Queue { get; } = new TaskQueue { };

        public StorageController() => Storage = new StorageDictionary(File = StorageManager.PersistentStorageFileWrapper);

        public StorageController(FileInfo file) => File = file;

        public Task<IStorageDictionary<string, object>> LoadAsync() => Queue.Enqueue(toAwait => toAwait.ContinueWith(_ => Task.FromResult((IStorageDictionary<string, object>) Storage) ?? (Storage = new StorageDictionary(File)).LoadAsync().OnSuccess(__ => Storage as IStorageDictionary<string, object>)).Unwrap(), CancellationToken.None);

        public Task<IStorageDictionary<string, object>> SaveAsync(IDictionary<string, object> contents) => Queue.Enqueue(toAwait => toAwait.ContinueWith(_ =>
        {
            (Storage ?? (Storage = new StorageDictionary(File))).Update(contents);
            return Storage.SaveAsync().OnSuccess(__ => Storage as IStorageDictionary<string, object>);
        }).Unwrap());
    }
}
