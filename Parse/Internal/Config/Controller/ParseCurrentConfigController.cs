using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    internal class ParseCurrentConfigController : IParseCurrentConfigController
    {
        const string Key = "CurrentConfig";

        TaskQueue OperationQueue { get; }

        ParseConfig CurrentConfig { get; set; }

        IStorageController StorageController { get; }

        public ParseCurrentConfigController(IStorageController storageController)
        {
            StorageController = storageController;
            OperationQueue = new TaskQueue { };
        }

        public Task<ParseConfig> GetCurrentConfigAsync() => OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ =>
        {
            if (CurrentConfig is null)
            {
                return StorageController.LoadAsync().OnSuccess(t =>
                {
                    t.Result.TryGetValue(Key, out object tmp);
                    return CurrentConfig = tmp is string propertiesString ? new ParseConfig(ParseClient.DeserializeJsonString(propertiesString)) : new ParseConfig { };
                });
            }

            return Task.FromResult(CurrentConfig);
        })).Unwrap();

        public Task SetCurrentConfigAsync(ParseConfig config) => OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ =>
        {
            CurrentConfig = config;
            return StorageController.LoadAsync().OnSuccess(t => t.Result.AddAsync(Key, ParseClient.SerializeJsonString(((IJsonConvertible) config).ToJSON())));
        }).Unwrap().Unwrap());

        public Task ClearCurrentConfigAsync() => OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ =>
        {
            CurrentConfig = null;
            return StorageController.LoadAsync().OnSuccess(t => t.Result.RemoveAsync(Key));
        }).Unwrap().Unwrap());

        public Task ClearCurrentConfigInMemoryAsync() => OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => { CurrentConfig = null; }));
    }
}
