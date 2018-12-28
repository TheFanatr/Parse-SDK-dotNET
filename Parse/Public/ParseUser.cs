// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using Parse.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Parse.Common.Internal;

namespace Parse
{
    /// <summary>
    /// Represents a user for a Parse application.
    /// </summary>
    [ParseClassName("_User")]
    public class ParseUser : ParseObject
    {
        /// <summary>
        /// Gets the currently logged in ParseUser with a valid session, either from memory or disk
        /// if necessary.
        /// </summary>
        public static ParseUser CurrentUser
        {
            get
            {
                Task<ParseUser> userTask = GetCurrentUserAsync();
                // TODO (hallucinogen): this will without a doubt fail in Unity. How should we fix it?
                userTask.Wait();
                return userTask.Result;
            }
        }

        /// <summary>
        /// Constructs a <see cref="ParseQuery{ParseUser}"/> for ParseUsers.
        /// </summary>
        public static ParseQuery<ParseUser> Query => new ParseQuery<ParseUser> { };

        /// <summary>
        /// Sets the email address.
        /// </summary>
        [ParseFieldName("email")]
        public string Email
        {
            get => GetProperty<string>("Email", null);
            set => SetProperty(value, "Email");
        }

        /// <summary>
        /// Whether the ParseUser has been authenticated on this device. Only an authenticated
        /// ParseUser can be saved and deleted.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                lock (Mutex)
                    return SessionToken != null && CurrentUser != null && CurrentUser.ObjectId == ObjectId;
            }
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        [ParseFieldName("password")]
        public string Password
        {
            private get => GetProperty<string>("Password", null);
            set => SetProperty(value, "Password");
        }

        /// <summary>
        /// A token representing the current session.
        /// </summary>
        public string SessionToken => State.ContainsKey("sessionToken") ? State["sessionToken"] as string : null;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [ParseFieldName("username")]
        public string Username
        {
            get => GetProperty<string>("Username", null);
            set => SetProperty(value, "Username");
        }

        internal static IParseCurrentUserController CurrentUserController => ParseCorePlugins.Instance.CurrentUserController;

        internal static IParseUserController UserController => ParseCorePlugins.Instance.UserController;

        /// <summary>
        /// Logs in a user with a username and password. On success, this saves the session to disk so you
        /// can retrieve the currently logged in user using <see cref="CurrentUser"/>.
        /// </summary>
        /// <param name="sessionToken">The session token to authorize with</param>
        /// <returns>The user if authorization was successful</returns>
        public static Task<ParseUser> BecomeAsync(string sessionToken) => BecomeAsync(sessionToken, CancellationToken.None);

        /// <summary>
        /// Logs in a user with a username and password. On success, this saves the session to disk so you
        /// can retrieve the currently logged in user using <see cref="CurrentUser"/>.
        /// </summary>
        /// <param name="sessionToken">The session token to authorize with</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The user if authorization was successful</returns>
        public static Task<ParseUser> BecomeAsync(string sessionToken, CancellationToken cancellationToken) => UserController.GetUserAsync(sessionToken, cancellationToken).OnSuccess(t =>
        {
            ParseUser user = FromState<ParseUser>(t.Result, "_User");
            return SaveCurrentUserAsync(user).OnSuccess(_ => user);
        }).Unwrap();

        /// <summary>
        /// Logs in a user with a username and password. On success, this saves the session to disk so you
        /// can retrieve the currently logged in user using <see cref="CurrentUser"/>.
        /// </summary>
        /// <param name="username">The username to log in with.</param>
        /// <param name="password">The password to log in with.</param>
        /// <returns>The newly logged-in user.</returns>
        public static Task<ParseUser> LogInAsync(string username, string password) => LogInAsync(username, password, CancellationToken.None);

        /// <summary>
        /// Logs in a user with a username and password. On success, this saves the session to disk so you
        /// can retrieve the currently logged in user using <see cref="CurrentUser"/>.
        /// </summary>
        /// <param name="username">The username to log in with.</param>
        /// <param name="password">The password to log in with.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The newly logged-in user.</returns>
        public static Task<ParseUser> LogInAsync(string username, string password, CancellationToken cancellationToken) => UserController.LogInAsync(username, password, cancellationToken).OnSuccess(t =>
        {
            ParseUser user = FromState<ParseUser>(t.Result, "_User");
            return SaveCurrentUserAsync(user).OnSuccess(_ => user);
        }).Unwrap();

        /// <summary>
        /// Logs out the currently logged in user session. This will remove the session from disk, log out of
        /// linked services, and future calls to <see cref="CurrentUser"/> will return <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Typically, you should use <see cref="LogOutAsync()"/>, unless you are managing your own threading.
        /// </remarks>
        public static void LogOut() => LogOutAsync().Wait();

        /// <summary>
        /// Logs out the currently logged in user session. This will remove the session from disk, log out of
        /// linked services, and future calls to <see cref="CurrentUser"/> will return <c>null</c>.
        ///
        /// This is preferable to using <see cref="LogOut()"/>, unless your code is already running from a
        /// background thread.
        /// </summary>
        public static Task LogOutAsync(CancellationToken cancellationToken = default) => GetCurrentUserAsync().OnSuccess(t =>
        {
            LogOutWithProviders();
            return t.Result is ParseUser user ? user.Queue.Enqueue(toAwait => user.LogOutAsync(toAwait, cancellationToken), cancellationToken) : Task.FromResult(0);
        }).Unwrap();

        /// <summary>
        /// Requests a password reset email to be sent to the specified email address associated with the
        /// user account. This email allows the user to securely reset their password on the Parse site.
        /// </summary>
        /// <param name="email">The email address associated with the user that forgot their password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default) => UserController.RequestPasswordResetAsync(email, cancellationToken);

        /// <summary>
        /// Removes a key from the object's data if it exists.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <exception cref="System.ArgumentException">Cannot remove the username key.</exception>
        public override void Remove(string key)
        {
            if (key == "username")
            {
                throw new ArgumentException("Cannot remove the username key.");
            }
            base.Remove(key);
        }

        /// <summary>
        /// Signs up a new user. This will create a new ParseUser on the server and will also persist the
        /// session on disk so that you can access the user using <see cref="CurrentUser"/>. A username and
        /// password must be set before calling SignUpAsync.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task SignUpAsync(CancellationToken cancellationToken = default) => Queue.Enqueue(toAwait => SignUpAsync(toAwait, cancellationToken), cancellationToken);

        internal static string CurrentSessionToken
        {
            get
            {
                Task<string> sessionTokenTask = GetCurrentSessionTokenAsync();
                sessionTokenTask.Wait();
                return sessionTokenTask.Result;
            }
        }

        /// <summary>
        /// Gets the authData for this user.
        /// </summary>
        internal IDictionary<string, IDictionary<string, object>> AuthData
        {
            get => TryGetValue("authData", out IDictionary<string, IDictionary<string, object>> authData) ? authData : null;
            private set => this["authData"] = value;
        }

        internal static void ClearInMemoryUser() => CurrentUserController.ClearFromMemory();

        internal static Task<string> GetCurrentSessionTokenAsync(CancellationToken cancellationToken = default) => CurrentUserController.GetCurrentSessionTokenAsync(cancellationToken);

        /// <summary>
        /// Gets the currently logged in ParseUser with a valid session, either from memory or disk
        /// if necessary, asynchronously.
        /// </summary>
        internal static Task<ParseUser> GetCurrentUserAsync(CancellationToken cancellationToken = default) => CurrentUserController.GetAsync(cancellationToken);

        internal static Task<ParseUser> LogInWithAsync(string authType, IDictionary<string, object> data, CancellationToken cancellationToken)
        {
            ParseUser user = null;

            return UserController.LogInAsync(authType, data, cancellationToken).OnSuccess(t =>
            {
                user = FromState<ParseUser>(t.Result, "_User");

                lock (user.Mutex)
                {
                    if (user.AuthData == null)
                        user.AuthData = new Dictionary<string, IDictionary<string, object>> { };

                    user.AuthData[authType] = data;
                    user.SynchronizeAllAuthData();
                }

                return SaveCurrentUserAsync(user);
            }).Unwrap().OnSuccess(t => user);
        }

        internal static Task<ParseUser> LogInWithAsync(string authType, CancellationToken cancellationToken) => GetProvider(authType).AuthenticateAsync(cancellationToken).OnSuccess(authData => LogInWithAsync(authType, authData.Result, cancellationToken)).Unwrap();

        internal static void RegisterProvider(IParseAuthenticationProvider provider)
        {
            Authentication[provider.AuthType] = provider;
            CurrentUser?.SynchronizeAuthData(provider);
        }

        // If this is already the current user, refresh its state on disk.
        internal override Task<ParseObject> FetchAsyncInternal(Task toAwait, CancellationToken cancellationToken) => base.FetchAsyncInternal(toAwait, cancellationToken).OnSuccess(t => !CurrentUserController.IsCurrent(this) ? Task.FromResult(t.Result) : SaveCurrentUserAsync(this).OnSuccess(_ => t.Result)).Unwrap();

        internal override void HandleSave(IObjectState serverState)
        {
            base.HandleSave(serverState);

            SynchronizeAllAuthData();
            CleanupAuthData();

            MutateState(mutableClone => mutableClone.ServerData.Remove("password"));
        }

        /// <summary>
        /// Checks whether a user is linked to a service.
        /// </summary>
        internal bool IsLinked(string authType)
        {
            lock (Mutex)
                return AuthData != null && AuthData.ContainsKey(authType) && AuthData[authType] != null;
        }

        internal Task LinkWithAsync(string authType, IDictionary<string, object> data, CancellationToken cancellationToken) => Queue.Enqueue(toAwait =>
        {
            IDictionary<string, IDictionary<string, object>> authData = AuthData;
            if (authData == null)
            {
                authData = AuthData = new Dictionary<string, IDictionary<string, object>>();
            }
            authData[authType] = data;
            AuthData = authData;
            return SaveAsync(cancellationToken);
        }, cancellationToken);

        internal Task LinkWithAsync(string authType, CancellationToken cancellationToken) => GetProvider(authType).AuthenticateAsync(cancellationToken).OnSuccess(t => LinkWithAsync(authType, t.Result, cancellationToken)).Unwrap();

        // TODO (hallucinogen): this will without a doubt fail in Unity. But what else can we do?
        internal Task LogOutAsync(Task toAwait, CancellationToken cancellationToken)
        {
            string oldSessionToken = SessionToken;
            if (oldSessionToken == null)
                return Task.FromResult(0);

            // Cleanup in-memory session.
            MutateState(mutableClone => mutableClone.ServerData.Remove("sessionToken"));
            return Task.WhenAll(ParseSession.RevokeAsync(oldSessionToken, cancellationToken), CurrentUserController.LogOutAsync(cancellationToken));
        }

        internal Task SetSessionTokenAsync(string newSessionToken, CancellationToken cancellationToken = default)
        {
            MutateState(mutableClone => mutableClone.ServerData["sessionToken"] = newSessionToken);
            return SaveCurrentUserAsync(this);
        }

        internal Task SignUpAsync(Task toAwait, CancellationToken cancellationToken = default)
        {
            if (AuthData == null)
            {
                if (String.IsNullOrEmpty(Username))
                    return Task.FromException(new InvalidOperationException("Cannot sign up user with an empty name."));
                if (String.IsNullOrEmpty(Password))
                    return Task.FromException(new InvalidOperationException("Cannot sign up user with an empty password."));
            }
            if (!String.IsNullOrEmpty(ObjectId))
                return Task.FromException(new InvalidOperationException("Cannot sign up a user that already exists."));

            IDictionary<string, IParseFieldOperation> currentOperations = StartSave();

            return toAwait.OnSuccess(_ => UserController.SignUpAsync(State, currentOperations, cancellationToken)).Unwrap().ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                    HandleFailedSave(currentOperations);
                else
                    HandleSave(t.Result);

                return t;
            }).Unwrap().OnSuccess(_ => SaveCurrentUserAsync(this)).Unwrap();
        }

        /// <summary>
        /// Unlinks a user from a service.
        /// </summary>
        internal Task UnlinkFromAsync(string authType, CancellationToken cancellationToken) => LinkWithAsync(authType, null, cancellationToken);

        /// <summary>
        /// Checks if the given <paramref name="key"/> is mutable on this object.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected override bool IsKeyMutable(string key) => !ReadOnlyKeys.Contains(key);

        protected override Task SaveAsync(Task toAwait, CancellationToken cancellationToken = default)
        {
            lock (Mutex)
                return ObjectId is null ? throw new InvalidOperationException("You must call SignUpAsync before calling SaveAsync.") : base.SaveAsync(toAwait, cancellationToken).OnSuccess(_ => !CurrentUserController.IsCurrent(this) ? Task.FromResult(0) : SaveCurrentUserAsync(this)).Unwrap();
        }

        static IDictionary<string, IParseAuthenticationProvider> Authentication { get; } = new Dictionary<string, IParseAuthenticationProvider> { };

        static HashSet<string> ReadOnlyKeys { get; } = new HashSet<string> { "sessionToken", "isNew" };
        private static IParseAuthenticationProvider GetProvider(string providerName) => Authentication.TryGetValue(providerName, out IParseAuthenticationProvider provider) ? provider : null;

        private static void LogOutWithProviders()
        {
            foreach (IParseAuthenticationProvider provider in Authentication.Values)
                provider.Deauthenticate();
        }
        static Task SaveCurrentUserAsync(ParseUser user, CancellationToken cancellationToken = default) => CurrentUserController.SetAsync(user, cancellationToken);
        #region Legacy / Revocable Session Tokens

        /// <summary>
        /// Tells server to use revocable session on LogIn and SignUp, even when App's Settings
        /// has "Require Revocable Session" turned off. Issues network request in background to
        /// migrate the sessionToken on disk to revocable session.
        /// </summary>
        /// <returns>The Task that upgrades the session.</returns>
        public static Task EnableRevocableSessionAsync(CancellationToken cancellationToken = default)
        {
            IsRevocableSessionEnabled = true;
            return GetCurrentUserAsync(cancellationToken).OnSuccess(t => t.Result.UpgradeToRevocableSessionAsync(cancellationToken));
        }

        internal static bool IsRevocableSessionEnabled
        {
            get
            {
                lock (RevocableSessionMutex)
                    return _IsRevocableSessionEnabled;
            }
            set
            {
                lock (RevocableSessionMutex)
                    _IsRevocableSessionEnabled = value;
            }
        }

        internal Task UpgradeToRevocableSessionAsync(CancellationToken cancellationToken = default) => Queue.Enqueue(toAwait => UpgradeToRevocableSessionAsync(toAwait, cancellationToken), cancellationToken);

        internal Task UpgradeToRevocableSessionAsync(Task toAwait, CancellationToken cancellationToken)
        {
            string sessionToken = SessionToken; // NOTE: Inlining could cause race condition if for some reason the session token changes before all the queued tasks here finish.

            return toAwait.OnSuccess(_ => ParseSession.UpgradeToRevocableSessionAsync(sessionToken, cancellationToken)).Unwrap().OnSuccess(t => SetSessionTokenAsync(t.Result)).Unwrap();
        }

        static bool _IsRevocableSessionEnabled;
        static object RevocableSessionMutex { get; } = new object { };
        #endregion
        /// <summary>
        /// Removes null values from authData (which exist temporarily for unlinking)
        /// </summary>
        private void CleanupAuthData()
        {
            lock (Mutex)
            {
                if (!CurrentUserController.IsCurrent(this))
                    return;

                IDictionary<string, IDictionary<string, object>> authData = AuthData;

                if (authData == null)
                    return;

                foreach (KeyValuePair<string, IDictionary<string, object>> pair in new Dictionary<string, IDictionary<string, object>>(authData))
                    if (pair.Value == null)
                        authData.Remove(pair.Key);
            }
        }

        void SynchronizeAllAuthData()
        {
            lock (Mutex)
            {
                IDictionary<string, IDictionary<string, object>> authData = AuthData;

                if (authData == null)
                    return;

                foreach (KeyValuePair<string, IDictionary<string, object>> pair in authData)
                    SynchronizeAuthData(GetProvider(pair.Key));
            }
        }

        private void SynchronizeAuthData(IParseAuthenticationProvider provider)
        {
            bool restorationSuccess = false;
            lock (Mutex)
            {
                IDictionary<string, IDictionary<string, object>> authData = AuthData;
                if (authData == null || provider == null)
                {
                    return;
                }
                if (authData.TryGetValue(provider.AuthType, out IDictionary<string, object> data))
                {
                    restorationSuccess = provider.RestoreAuthentication(data);
                }
            }

            if (!restorationSuccess)
            {
                UnlinkFromAsync(provider.AuthType, CancellationToken.None);
            }
        }
    }
}
