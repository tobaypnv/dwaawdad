// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.IO;
using Microsoft.Git.CredentialManager;

namespace Microsoft.AzureRepos
{
    internal static class AzureDevOpsConstants
    {
        public const string AzReposDataDirectoryName = "azure-repos";
        public const string AzReposDataStoreName = "store.ini";

        // Azure DevOps's resource ID
        public const string AadResourceId = "499b84ac-1321-427f-aa17-267ca6975798";

        // Visual Studio's client ID
        // We share this to be able to consume existing access tokens from the VS caches
        public const string AadClientId = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1";

        // Standard redirect URI for native client 'v1 protocol' applications
        // https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-protocols-oauth-code#request-an-authorization-code
        public static readonly Uri AadRedirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");

        public const string VstsHostSuffix = ".visualstudio.com";
        public const string AzureDevOpsHost = "dev.azure.com";

        public const string VssResourceTenantHeader = "X-VSS-ResourceTenant";

        public static class PersonalAccessTokenScopes
        {
            public const string ReposWrite = "vso.code_write";
            public const string ArtifactsRead = "vso.packaging";
        }

        public static IniFileValueStore CreateIniDataStore(IFileSystem fs)
        {
            EnsureArgument.NotNull(fs, nameof(fs));

            string storePath = Path.Combine(
                fs.UserDataDirectoryPath,
                AzReposDataDirectoryName,
                AzReposDataStoreName);

            return new IniFileValueStore(fs, new IniSerializer(), storePath);
        }
    }
}
