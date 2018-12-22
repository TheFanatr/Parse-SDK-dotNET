// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Parse.Common.Internal;
using Parse.Internal.Utilities;

namespace Parse
{
    /// <summary>
    /// ParseClient contains static functions that handle global
    /// configuration for the Parse library.
    /// </summary>
    public static partial class ParseClient
    {
        internal static readonly string[] DateFormatStrings =
        {
            // It's possible that the string converter server-side may trim trailing zeroes, so the extra format strings provide addition protection.
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ff'Z'",
            "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'f'Z'",
        };

        /// <summary>
        /// Represents the configuration of the Parse SDK.
        /// </summary>
        public struct Configuration
        {
            /// <summary>
            /// A unit that can generate a relative path to a persistent storage file.
            /// </summary>
            public interface IStorageConfiguration
            {
                /// <summary>
                /// The corresponding relative path generated by this <see cref="IStorageConfiguration"/>.
                /// </summary>
                string RelativeStorageFilePath { get; }
            }

            /// <summary>
            /// A configuration of the Parse SDK persistent storage location based on product metadata such as company name and product name.
            /// </summary>
            public struct MetadataBasedStorageConfiguration : IStorageConfiguration
            {
                /// <summary>
                /// An instance of <see cref="MetadataBasedStorageConfiguration"/> with inferred values based on the entry assembly. Should be used with <see cref="VersionInformation.Inferred"/>.
                /// </summary>
                /// <remarks>Should not be used with Unity.</remarks>
                public static MetadataBasedStorageConfiguration NoCompanyInferred { get; } = new MetadataBasedStorageConfiguration { CompanyName = Assembly.GetEntryAssembly().GetName().Name, ProductName = String.Empty };

                /// <summary>
                /// The name of the company that owns the product specified by <see cref="ProductName"/>.
                /// </summary>
                public string CompanyName { get; set; }

                /// <summary>
                /// The name of the product that is using the Parse .NET SDK.
                /// </summary>
                public string ProductName { get; set; }

                /// <summary>
                /// The corresponding relative path generated by this <see cref="IStorageConfiguration"/>.
                /// </summary>
                public string RelativeStorageFilePath => Path.Combine(CompanyName ?? "Parse", ProductName ?? "_global", $"{CurrentConfiguration.VersionInfo.DisplayVersion ?? "1.0.0.0"}.cachefile");
            }

            /// <summary>
            /// A configuration of the Parse SDK persistent storage location based on an identifier.
            /// </summary>
            public struct IdentifierBasedStorageConfiguration : IStorageConfiguration
            {
                internal static IdentifierBasedStorageConfiguration Fallback { get; } = new IdentifierBasedStorageConfiguration { IsFallback = true };

                /// <summary>
                /// Dictates whether or not this <see cref="IStorageConfiguration"/> instance should act as a fallback for when <see cref="ParseClient"/> has not yet been initialized but the storage path is needed.
                /// </summary>
                internal bool IsFallback { get; set; }

                /// <summary>
                /// The identifier that all Parse SDK cache files should be labelled with.
                /// </summary>
                public string Identifier { get; set; }

                /// <summary>
                /// The corresponding relative path generated by this <see cref="IStorageConfiguration"/>.
                /// </summary>
                /// <remarks>This will cause a .cachefile file extension to be added to the cache file in order to prevent the creation of files with unwanted extensions due to the value of <see cref="Identifier"/> containing periods.</remarks>
                public string RelativeStorageFilePath
                {
                    get
                    {
                        FileInfo file = default;
                        while ((file = StorageManager.GetWrapperForRelativePersistentStorageFilePath(GeneratePath())).Exists && IsFallback);

                        return file.FullName;
                    }
                }

                /// <summary>
                /// Generates a path for use in the <see cref="RelativeStorageFilePath"/> getter.
                /// </summary>
                /// <returns>A potential path to the cachefile</returns>
                string GeneratePath() => Path.Combine("Parse", IsFallback ? "_fallback" : "_global", $"{(IsFallback ? new Random { }.Next().ToString() : Identifier)}.cachefile");
            }

            /// <summary>
            /// In the event that you would like to use the Parse SDK
            /// from a completely portable project, with no platform-specific library required,
            /// to get full access to all of our features available on Parse Dashboard
            /// (A/B testing, slow queries, etc.), you must set the values of this struct
            /// to be appropriate for your platform.
            ///
            /// Any values set here will overwrite those that are automatically configured by
            /// any platform-specific migration library your app includes.
            /// </summary>
            public struct VersionInformation
            {
                /// <summary>
                /// An instance of <see cref="VersionInformation"/> with inferred values based on the entry assembly.
                /// </summary>
                /// <remarks>Should not be used with Unity.</remarks>
                public static VersionInformation Inferred { get; } = new VersionInformation { BuildVersion = Assembly.GetEntryAssembly().GetName().Version.Build.ToString(), DisplayVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(), OSVersion = Environment.OSVersion.ToString() };

                /// <summary>
                /// The build number of your app.
                /// </summary>
                public string BuildVersion { get; set; }

                /// <summary>
                /// The human friendly version number of your app.
                /// </summary>
                public string DisplayVersion { get; set; }

                /// <summary>
                /// The operating system version of the platform the SDK is operating in..
                /// </summary>
                public string OSVersion { get; set; }

                /// <summary>
                /// Gets a value for whether or not this instance of <see cref="VersionInformation"/> is populated with default values.
                /// </summary>
                internal bool IsDefault => BuildVersion is null && DisplayVersion is null && OSVersion is null;

                /// <summary>
                /// Gets a value for whether or not this instance of <see cref="VersionInformation"/> can currently be used for the generation of <see cref="MetadataBasedStorageConfiguration.NoCompanyInferred"/>.
                /// </summary>
                internal bool CanBeUsedForInference => !(IsDefault || String.IsNullOrWhiteSpace(DisplayVersion));
            }

            /// <summary>
            /// The App ID of your app.
            /// </summary>
            public string ApplicationID { get; set; }

            /// <summary>
            /// A URI pointing to the target Parse Server instance hosting the app targeted by <see cref="ApplicationID"/>.
            /// </summary>
            public string ServerURI { get; set; }

            /// <summary>
            /// The .NET Key for the Parse app targeted by <see cref="ApplicationID"/>.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// The Master Key for the Parse app targeted by <see cref="ApplicationID"/>.
            /// </summary>
            public string MasterKey { get; set; }

            /// <summary>
            /// Additional HTTP headers to be sent with network requests from the SDK.
            /// </summary>
            public IDictionary<string, string> AuxiliaryHeaders { get; set; }

            /// <summary>
            /// The version information of your application environment.
            /// </summary>
            public VersionInformation VersionInfo { get; set; }

            /// <summary>
            /// The <see cref="IStorageConfiguration"/> that Parse should use when generating cache files.
            /// </summary>
            public IStorageConfiguration StorageConfiguration { get; set; }
        }

        private static readonly object mutex = new object();


        // TODO: Investigate if the version string header can be changed to simple "net-".
        static ParseClient() => VersionString = "net-portable-" + Version; //ParseModuleController.Instance.ScanForModules();

        /// <summary>
        /// The current configuration that parse has been initialized with.
        /// </summary>
        public static Configuration CurrentConfiguration { get; internal set; }

        internal static Version Version => new AssemblyName(typeof(ParseClient).GetTypeInfo().Assembly.FullName).Version;

        internal static string VersionString { get; }

        /// <summary>
        /// Authenticates this client as belonging to your application. This must be
        /// called before your application can use the Parse library. The recommended
        /// way is to put a call to <c>ParseFramework.Initialize</c> in your
        /// Application startup.
        /// </summary>
        /// <param name="identifier">The Application ID provided in the Parse dashboard.
        /// </param>
        /// <param name="key">The .NET API Key provided in the Parse dashboard.
        /// </param>
        public static void Initialize(string identifier, string key) => Initialize(new Configuration { ApplicationID = identifier, Key = key });

        /// <summary>
        /// Authenticates this client as belonging to your application. This must be
        /// called before your application can use the Parse library. The recommended
        /// way is to put a call to <c>ParseFramework.Initialize</c> in your
        /// Application startup.
        /// </summary>
        /// <param name="configuration">The configuration to initialize Parse with.
        /// </param>
        public static void Initialize(Configuration configuration)
        {
            lock (mutex)
            {
                configuration.ServerURI = configuration.ServerURI ?? "https://api.parse.com/1/";
                
                switch (configuration.VersionInfo)
                {
                    case Configuration.VersionInformation info when info.CanBeUsedForInference:
                        break;
                    case Configuration.VersionInformation info when !info.IsDefault:
                        configuration.VersionInfo = new Configuration.VersionInformation { BuildVersion = info.BuildVersion, OSVersion = info.OSVersion, DisplayVersion = Configuration.VersionInformation.Inferred.DisplayVersion };
                        break;
                    default:
                        configuration.VersionInfo = Configuration.VersionInformation.Inferred;
                        break;
                }

                switch (configuration.StorageConfiguration)
                {
                    case IStorageController controller when !(controller is null):
                        break;
                    default:
                        configuration.StorageConfiguration = Configuration.MetadataBasedStorageConfiguration.NoCompanyInferred;
                        break;
                }

                CurrentConfiguration = configuration;

                ParseObject.RegisterSubclass<ParseUser>();
                ParseObject.RegisterSubclass<ParseRole>();
                ParseObject.RegisterSubclass<ParseSession>();

                ParseModuleController.Instance.ParseDidInitialize();
            }
        }

        /// <summary>
        /// Reflects a change in the Parse SDK storage configuration by copying all of the cached data into the new location.
        /// </summary>
        /// <param name="originalRelativePath"></param>
        /// <returns></returns>
        public static async Task ReflectStorageChangeAsync(string originalRelativePath) => await StorageManager.TransferAsync(StorageManager.GetWrapperForRelativePersistentStorageFilePath(originalRelativePath).FullName, StorageManager.PersistentStorageFilePath);

        internal static string BuildQueryString(IDictionary<string, object> parameters) => String.Join("&", (from pair in parameters let valueString = pair.Value as string select $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(String.IsNullOrEmpty(valueString) ? Json.Encode(pair.Value) : valueString)}").ToArray());

        internal static IDictionary<string, string> DecodeQueryString(string queryString)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string pair in queryString.Split('&'))
            {
                string[] parts = pair.Split(new char[] { '=' }, 2);
                dict[parts[0]] = parts.Length == 2 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : null;
            }
            return dict;
        }

        internal static IDictionary<string, object> DeserializeJsonString(string jsonData) => Json.Parse(jsonData) as IDictionary<string, object>;

        internal static string SerializeJsonString(IDictionary<string, object> jsonData) => Json.Encode(jsonData);
    }
}
