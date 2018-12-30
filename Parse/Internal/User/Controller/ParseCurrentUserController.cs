// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class ParseCurrentUserController : IParseCurrentUserController
    {
        object Mutex { get; } = new object { };

        TaskQueue OperationQueue { get; } = new TaskQueue { };

        IStorageController StorageController { get; }

        public ParseCurrentUserController(IStorageController storageController) => StorageController = storageController;

        ParseUser _CurrentUser;

        public ParseUser CurrentUser
        {
            get
            {
                lock (Mutex)
                    return _CurrentUser;
            }
            set
            {
                lock (Mutex)
                    _CurrentUser = value;
            }
        }

        public Task SetAsync(ParseUser user, CancellationToken cancellationToken) => OperationQueue.Enqueue(toAwait =>
        {
            return toAwait.ContinueWith(_ =>
            {
                Task saveTask = null;
                if (user == null)
                    saveTask = StorageController.LoadAsync().OnSuccess(t => t.Result.RemoveAsync("CurrentUser")).Unwrap();
                else
                {
                    // TODO: We need to use ParseCurrentCoder instead of this janky encoding.
                    IDictionary<string, object> data = user.ServerDataToJSONObjectForSerialization();
                    data["objectId"] = user.ObjectId;
                    if (user.CreatedAt != null)
                        data["createdAt"] = user.CreatedAt.Value.ToString(ParseClient.DateFormatStrings.First(), CultureInfo.InvariantCulture);
                    if (user.UpdatedAt != null)
                        data["updatedAt"] = user.UpdatedAt.Value.ToString(ParseClient.DateFormatStrings.First(), CultureInfo.InvariantCulture);

                    saveTask = StorageController.LoadAsync().OnSuccess(t => t.Result.AddAsync("CurrentUser", JsonProcessor.Encode(data))).Unwrap();
                }
                CurrentUser = user;

                return saveTask;
            }).Unwrap();
        }, cancellationToken);

        public Task<ParseUser> GetAsync(CancellationToken cancellationToken) => CurrentUser is ParseUser cachedCurrent ? Task.FromResult(cachedCurrent) : OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => StorageController.LoadAsync().OnSuccess(t =>
        {
            t.Result.TryGetValue("CurrentUser", out object temp);
            ParseUser user = null;

            if (temp is string userDataString)
                user = ParseObject.FromState<ParseUser>(ParseObjectCoder.Instance.Decode(JsonProcessor.Parse(userDataString) as IDictionary<string, object>, ParseDecoder.Instance), "_User");

            CurrentUser = user;
            return user;
        })).Unwrap(), cancellationToken);

        public Task<bool> ExistsAsync(CancellationToken cancellationToken) => CurrentUser is null ? OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => StorageController.LoadAsync().OnSuccess(t => t.Result.ContainsKey("CurrentUser"))).Unwrap(), cancellationToken) : Task.FromResult(true);

        public bool IsCurrent(ParseUser user) => CurrentUser == user;

        public void ClearFromMemory() => CurrentUser = null;

        public void ClearFromDisk()
        {
            lock (Mutex)
            {
                ClearFromMemory();
                OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => StorageController.LoadAsync().OnSuccess(t => t.Result.RemoveAsync("CurrentUser"))).Unwrap().Unwrap());
            }
        }

        public Task<string> GetCurrentSessionTokenAsync(CancellationToken cancellationToken) => GetAsync(cancellationToken).OnSuccess(t => t.Result?.SessionToken);

        public Task LogOutAsync(CancellationToken cancellationToken) => OperationQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => GetAsync(cancellationToken)).Unwrap().OnSuccess(t => ClearFromDisk()), cancellationToken);
    }
}
