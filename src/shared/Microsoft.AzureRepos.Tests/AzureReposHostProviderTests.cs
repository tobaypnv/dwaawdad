// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Git.CredentialManager;
using Microsoft.Git.CredentialManager.Authentication;
using Microsoft.Git.CredentialManager.Tests.Objects;
using Moq;
using Xunit;
using static Microsoft.Git.CredentialManager.Tests.TestHelpers;

namespace Microsoft.AzureRepos.Tests
{
    public class AzureReposHostProviderTests
    {
        private static readonly string AzDevUseHttpPathKey =
            $"{Constants.GitConfiguration.Credential.SectionName}.https://dev.azure.com.{Constants.GitConfiguration.Credential.UseHttpPath}";

        [Fact]
        public void AzureReposProvider_IsSupported_AzureHost_UnencryptedHttp_ReturnsTrue()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "http",
                ["host"] = "dev.azure.com",
                ["path"] = "org/proj/_git/repo",
            });

            var provider = new AzureReposHostProvider(new TestCommandContext());

            // We report that we support unencrypted HTTP here so that we can fail and
            // show a helpful error message in the call to `CreateCredentialAsync` instead.
            Assert.True(provider.IsSupported(input));
        }

        [Fact]
        public void AzureReposProvider_IsSupported_VisualStudioHost_UnencryptedHttp_ReturnsTrue()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "http",
                ["host"] = "org.visualstudio.com",
            });

            var provider = new AzureReposHostProvider(new TestCommandContext());

            // We report that we support unencrypted HTTP here so that we can fail and
            // show a helpful error message in the call to `CreateCredentialAsync` instead.
            Assert.True(provider.IsSupported(input));
        }

        [Fact]
        public void AzureReposProvider_IsSupported_AzureHost_WithPath_ReturnsTrue()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "https",
                ["host"] = "dev.azure.com",
                ["path"] = "org/proj/_git/repo",
            });

            var provider = new AzureReposHostProvider(new TestCommandContext());
            Assert.True(provider.IsSupported(input));
        }

        [Fact]
        public void AzureReposProvider_IsSupported_AzureHost_MissingPath_ReturnsTrue()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "https",
                ["host"] = "dev.azure.com",
            });

            var provider = new AzureReposHostProvider(new TestCommandContext());
            Assert.True(provider.IsSupported(input));
        }

        [Fact]
        public void AzureReposProvider_IsSupported_VisualStudioHost_ReturnsTrue()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "https",
                ["host"] = "org.visualstudio.com",
            });

            var provider = new AzureReposHostProvider(new TestCommandContext());
            Assert.True(provider.IsSupported(input));
        }

        [Fact]
        public void AzureReposProvider_IsSupported_VisualStudioHost_MissingOrgInHost_ReturnsFalse()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "https",
                ["host"] = "visualstudio.com",
            });

            var provider = new AzureReposHostProvider(new TestCommandContext());
            Assert.False(provider.IsSupported(input));
        }

        [Fact]
        public void AzureReposProvider_IsSupported_NonAzureRepos_ReturnsFalse()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "https",
                ["host"] = "example.com",
                ["path"] = "org/proj/_git/repo",
            });

            var provider = new AzureReposHostProvider(new TestCommandContext());
            Assert.False(provider.IsSupported(input));
        }

        [Fact]
        public async Task AzureReposProvider_GetCredentialAsync_UnencryptedHttp_ThrowsException()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "http",
                ["host"] = "dev.azure.com",
                ["path"] = "org/proj/_git/repo"
            });

            var context = new TestCommandContext();
            var azDevOps = Mock.Of<IAzureDevOpsRestApi>();
            var msAuth = Mock.Of<IMicrosoftAuthentication>();

            var provider = new AzureReposHostProvider(context, azDevOps, msAuth);

            await Assert.ThrowsAsync<Exception>(() => provider.GetCredentialAsync(input));
        }

        [Fact]
        public async Task AzureReposProvider_GetCredentialAsync_ReturnsCredential()
        {
            var input = new InputArguments(new Dictionary<string, string>
            {
                ["protocol"] = "https",
                ["host"] = "dev.azure.com",
                ["path"] = "org/proj/_git/repo"
            });

            var expectedOrgUri = new Uri("https://dev.azure.com/org");
            var remoteUri = new Uri("https://dev.azure.com/org/proj/_git/repo");
            var authorityUrl = "https://login.microsoftonline.com/common";
            var expectedClientId = AzureDevOpsConstants.AadClientId;
            var expectedRedirectUri = AzureDevOpsConstants.AadRedirectUri;
            var expectedResource = AzureDevOpsConstants.AadResourceId;
            var accessToken = CreateJwt("john.doe");
            var personalAccessToken = "PERSONAL-ACCESS-TOKEN";

            var context = new TestCommandContext();

            var azDevOpsMock = new Mock<IAzureDevOpsRestApi>();
            azDevOpsMock.Setup(x => x.GetAuthorityAsync(expectedOrgUri))
                        .ReturnsAsync(authorityUrl);
            azDevOpsMock.Setup(x => x.CreatePersonalAccessTokenAsync(expectedOrgUri, accessToken, It.IsAny<IEnumerable<string>>()))
                        .ReturnsAsync(personalAccessToken);

            var msAuthMock = new Mock<IMicrosoftAuthentication>();
            msAuthMock.Setup(x => x.GetAccessTokenAsync(authorityUrl, expectedClientId, expectedRedirectUri, expectedResource, remoteUri, null))
                      .ReturnsAsync(accessToken);

            var provider = new AzureReposHostProvider(context, azDevOpsMock.Object, msAuthMock.Object);

            ICredential credential = await provider.GetCredentialAsync(input);

            Assert.NotNull(credential);
            Assert.Equal(personalAccessToken, credential.Password);
            // We don't care about the username value
        }

        [Fact]
        public async Task AzureReposHostProvider_ConfigureAsync_UseHttpPathSetTrue_DoesNothing()
        {
            var provider = new AzureReposHostProvider(new TestCommandContext());

            var environment = new TestEnvironment();
            var git = new TestGit();
            git.GlobalConfiguration.Dictionary[AzDevUseHttpPathKey] = new List<string> {"true"};

            await provider.ConfigureAsync(
                environment, EnvironmentVariableTarget.User,
                git, GitConfigurationLevel.Global);

            Assert.Single(git.GlobalConfiguration.Dictionary);
            Assert.True(git.GlobalConfiguration.Dictionary.TryGetValue(AzDevUseHttpPathKey, out IList<string> actualValues));
            Assert.Single(actualValues);
            Assert.Equal("true", actualValues[0]);
        }

        [Fact]
        public async Task AzureReposHostProvider_ConfigureAsync_UseHttpPathSetFalse_SetsUseHttpPathTrue()
        {
            var provider = new AzureReposHostProvider(new TestCommandContext());

            var environment = new TestEnvironment();
            var git = new TestGit();
            git.GlobalConfiguration.Dictionary[AzDevUseHttpPathKey] = new List<string> {"false"};

            await provider.ConfigureAsync(
                environment, EnvironmentVariableTarget.User,
                git, GitConfigurationLevel.Global);

            Assert.Single(git.GlobalConfiguration.Dictionary);
            Assert.True(git.GlobalConfiguration.Dictionary.TryGetValue(AzDevUseHttpPathKey, out IList<string> actualValues));
            Assert.Single(actualValues);
            Assert.Equal("true", actualValues[0]);
        }

        [Fact]
        public async Task AzureReposHostProvider_ConfigureAsync_UseHttpPathUnset_SetsUseHttpPathTrue()
        {
            var provider = new AzureReposHostProvider(new TestCommandContext());

            var environment = new TestEnvironment();
            var git = new TestGit();

            await provider.ConfigureAsync(
                environment, EnvironmentVariableTarget.User,
                git, GitConfigurationLevel.Global);

            Assert.Single(git.GlobalConfiguration.Dictionary);
            Assert.True(git.GlobalConfiguration.Dictionary.TryGetValue(AzDevUseHttpPathKey, out IList<string> actualValues));
            Assert.Single(actualValues);
            Assert.Equal("true", actualValues[0]);
        }


        [Fact]
        public async Task AzureReposHostProvider_UnconfigureAsync_UseHttpPathSet_RemovesEntry()
        {
            var provider = new AzureReposHostProvider(new TestCommandContext());

            var environment = new TestEnvironment();
            var git = new TestGit();
            git.GlobalConfiguration.Dictionary[AzDevUseHttpPathKey] = new List<string> {"true"};

            await provider.UnconfigureAsync(
                environment, EnvironmentVariableTarget.User,
                git, GitConfigurationLevel.Global);

            Assert.Empty(git.GlobalConfiguration.Dictionary);
        }
    }
}
