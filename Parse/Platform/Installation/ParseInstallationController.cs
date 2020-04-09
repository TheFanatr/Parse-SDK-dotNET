// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class ParseInstallationController : IParseInstallationController
    {
        static string InstallationIdKey { get; } = "InstallationId";

        object Mutex { get; } = new object { };

        Guid? InstallationId { get; set; }

        IStorageController StorageController { get; }

        public ParseInstallationController(IStorageController storageController) => StorageController = storageController;

        public Task SetAsync(Guid? installationId)
        {
            lock (Mutex)
            {
#warning Should refactor here if this operates correctly.

                Task saveTask = installationId is { } ? StorageController.LoadAsync().OnSuccess(storage => storage.Result.AddAsync(InstallationIdKey, installationId.ToString())).Unwrap() : StorageController.LoadAsync().OnSuccess(storage => storage.Result.RemoveAsync(InstallationIdKey)).Unwrap();

                InstallationId = installationId;
                return saveTask;
            }
        }

        public Task<Guid?> GetAsync()
        {
            lock (Mutex)
            {
                if (InstallationId is { })
                {
                    return Task.FromResult(InstallationId);
                }
            }

            return StorageController.LoadAsync().OnSuccess(storageTask =>
            {
                storageTask.Result.TryGetValue(InstallationIdKey, out object id);

                try
                {
                    lock (Mutex)
                    {
                        return Task.FromResult(InstallationId = new Guid(id as string));
                    }
                }
                catch (Exception)
                {
                    Guid newInstallationId = Guid.NewGuid();
                    return SetAsync(newInstallationId).OnSuccess<Guid?>(_ => newInstallationId);
                }
            })
            .Unwrap();
        }

        public Task ClearAsync() => SetAsync(null);
    }
}