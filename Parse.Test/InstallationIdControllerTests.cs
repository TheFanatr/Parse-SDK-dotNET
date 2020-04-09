using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Parse.Common.Internal;
using Parse.Core.Internal;
using Parse.Library;
using Parse.Management;

namespace Parse.Test
{
#warning Class refactoring may be required.

    [TestClass]
    public class InstallationIdControllerTests
    {
        ParseClient Client { get; } = new ParseClient(new ServerConnectionData { Test = true });

        [TestCleanup]
        public void TearDown() => (Client.Services as ServiceHub).Reset();

        [TestMethod]
        public void TestConstructor()
        {
            Mock<IStorageController> storageMock = new Mock<IStorageController>(MockBehavior.Strict);
            ParseInstallationController controller = new ParseInstallationController(storageMock.Object);

            // Make sure it didn't touch storageMock.

            storageMock.Verify();
        }

        [TestMethod]
        [AsyncStateMachine(typeof(InstallationIdControllerTests))]
        public Task TestGet()
        {
            Mock<IStorageController> storageMock = new Mock<IStorageController>(MockBehavior.Strict);
            Mock<IStorageDictionary<string, object>> storageDictionary = new Mock<IStorageDictionary<string, object>>();

            storageMock.Setup(s => s.LoadAsync()).Returns(Task.FromResult(storageDictionary.Object));

            ParseInstallationController controller = new ParseInstallationController(storageMock.Object);
            return controller.GetAsync().ContinueWith(installationIdTask =>
            {
                Assert.IsFalse(installationIdTask.IsFaulted);

                object verified = null;
                storageDictionary.Verify(s => s.TryGetValue("InstallationId", out verified));
                storageDictionary.Verify(s => s.AddAsync("InstallationId", It.IsAny<object>()));

                return controller.GetAsync().ContinueWith(newInstallationIdTask =>
                {
                    Assert.IsFalse(newInstallationIdTask.IsFaulted);

                    // Ensure nothing more has happened with our dictionary.
                    storageDictionary.VerifyAll();

                    Assert.AreEqual(installationIdTask.Result, newInstallationIdTask.Result);

                    return controller.ClearAsync();
                }).Unwrap().ContinueWith(clearTask =>
                {
                    Assert.IsFalse(clearTask.IsFaulted);

                    storageDictionary.Verify(storage => storage.RemoveAsync("InstallationId"));

                    return controller.GetAsync();
                }).Unwrap().ContinueWith(newInstallationIdTask =>
                {
                    Assert.IsFalse(newInstallationIdTask.IsFaulted);

                    Assert.AreNotEqual(installationIdTask.Result, newInstallationIdTask.Result);

                    storageDictionary.Verify(s => s.TryGetValue("InstallationId", out verified));
                    storageDictionary.Verify(s => s.AddAsync("InstallationId", It.IsAny<object>()));
                });
            }).Unwrap();
        }

        [TestMethod]
        [AsyncStateMachine(typeof(InstallationIdControllerTests))]
        public Task TestSet()
        {
            Mock<IStorageController> storageMock = new Mock<IStorageController>(MockBehavior.Strict);
            Mock<IStorageDictionary<string, object>> storageDictionary = new Mock<IStorageDictionary<string, object>>();

            storageMock.Setup(s => s.LoadAsync()).Returns(Task.FromResult(storageDictionary.Object));

            ParseInstallationController controller = new ParseInstallationController(storageMock.Object);

            return controller.GetAsync().ContinueWith(installationIdTask =>
            {
                Assert.IsFalse(installationIdTask.IsFaulted);

                object verified = null;
                storageDictionary.Verify(s => s.TryGetValue("InstallationId", out verified));
                storageDictionary.Verify(s => s.AddAsync("InstallationId", It.IsAny<object>()));

                Guid? installationId = installationIdTask.Result;
                Guid installationId2 = Guid.NewGuid();

                return controller.SetAsync(installationId2).ContinueWith(setTask =>
                {
                    Assert.IsFalse(setTask.IsFaulted);

                    storageDictionary.Verify(s => s.AddAsync("InstallationId", installationId2.ToString()));

                    return controller.GetAsync();
                }).Unwrap().ContinueWith(installationId3Task =>
                {
                    Assert.IsFalse(installationId3Task.IsFaulted);

                    storageDictionary.Verify(s => s.TryGetValue("InstallationId", out verified));

                    Guid? installationId3 = installationId3Task.Result;
                    Assert.AreEqual(installationId2, installationId3);

                    return controller.SetAsync(installationId);
                }).Unwrap().ContinueWith(setTask =>
                {
                    Assert.IsFalse(setTask.IsFaulted);

                    storageDictionary.Verify(s => s.AddAsync("InstallationId", installationId.ToString()));

                    return controller.ClearAsync();
                }).Unwrap().ContinueWith(clearTask =>
                {
                    Assert.IsFalse(clearTask.IsFaulted);

                    storageDictionary.Verify(s => s.RemoveAsync("InstallationId"));

                    return controller.SetAsync(installationId2);
                }).Unwrap().ContinueWith(setTask =>
                {
                    Assert.IsFalse(setTask.IsFaulted);

                    storageDictionary.Verify(s => s.AddAsync("InstallationId", installationId2.ToString()));
                });
            }).Unwrap();
        }
    }
}
