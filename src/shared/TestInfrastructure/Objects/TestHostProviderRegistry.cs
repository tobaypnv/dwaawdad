// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
namespace Microsoft.Git.CredentialManager.Tests.Objects
{
    public class TestHostProviderRegistry : IHostProviderRegistry
    {
        public IHostProvider Provider { get; set; }

        #region IHostProviderRegistry

        void IHostProviderRegistry.Register(params IHostProvider[] hostProviders)
        {
        }

        IHostProvider IHostProviderRegistry.GetProvider(InputArguments input)
        {
            return Provider;
        }

        #endregion

        public void Dispose()
        {
            Provider?.Dispose();
        }
    }
}
