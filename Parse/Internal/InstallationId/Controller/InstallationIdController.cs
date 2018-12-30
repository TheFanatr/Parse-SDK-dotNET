// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse.Core.Internal
{
    public class InstallationIdController : IInstallationIdController
    {
        object Mutex { get; } = new object { };

        Guid? InstallationId { get; set; }

        IStorageController StorageController { get; }

        public InstallationIdController(IStorageController storageController) => StorageController = storageController;

        public Task SetAsync(Guid? installationId)
        {
            lock (Mutex)
            {
                Task saveTask = installationId is null ? StorageController.LoadAsync().OnSuccess(storage => storage.Result.RemoveAsync("InstallationId")).Unwrap() : StorageController.LoadAsync().OnSuccess(storage => storage.Result.AddAsync("InstallationId", installationId.ToString())).Unwrap();

                InstallationId = installationId;
                return saveTask;
            }
        }

        public Task<Guid?> GetAsync()
        {
            lock (Mutex)
                if (InstallationId != null)
                    return Task.FromResult(InstallationId);

            return StorageController.LoadAsync().OnSuccess(s =>
            {
                s.Result.TryGetValue("InstallationId", out object id);

                try
                {
                    lock (Mutex)
                        return Task.FromResult(InstallationId = new Guid((string) id));
                }
                catch (Exception)
                {
                    Guid newInstallationId = Guid.NewGuid();
                    return SetAsync(newInstallationId).OnSuccess<Guid?>(_ => newInstallationId);
                }
            }).Unwrap();
        }

        public Task ClearAsync() => SetAsync(null);
    }
}
