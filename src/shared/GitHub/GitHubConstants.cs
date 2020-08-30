// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;

namespace GitHub
{
    public static class GitHubConstants
    {
        public const string GitHubBaseUrlHost = "github.com";
        public const string GistBaseUrlHost = "gist." + GitHubBaseUrlHost;

        public const string DefaultAuthenticationHelper = "GitHub.UI";

        public const string OAuthClientId = "0120e057bd645470c1ed";
        public const string OAuthClientSecret = "18867509d956965542b521a529a79bb883344c90";
        public static readonly Uri OAuthRedirectUri = new Uri("http://localhost/");
        public static readonly Uri OAuthAuthorizationEndpointRelativeUri = new Uri("/login/oauth/authorize", UriKind.Relative);
        public static readonly Uri OAuthTokenEndpointRelativeUri = new Uri("/login/oauth/access_token", UriKind.Relative);
        public static readonly Uri OAuthDeviceEndpointRelativeUri = new Uri("/login/device/code", UriKind.Relative);

        /// <summary>
        /// The GitHub required HTTP accepts header value
        /// </summary>
        public const string GitHubApiAcceptsHeaderValue = "application/vnd.github.v3+json";
        public const string GitHubOptHeader = "X-GitHub-OTP";

        /// <summary>
        /// Minimum GitHub Enterprise version that supports OAuth authentication with GCM Core.
        /// </summary>
        // TODO: update this with a real version number once the GCM OAuth application has been deployed to GHE
        public static readonly Version MinimumEnterpriseOAuthVersion = new Version("99.99.99");

        /// <summary>
        /// Supported authentication modes for GitHub.com.
        /// </summary>
        // TODO: remove Basic once the GCM OAuth app is whitelisted and does not require installation in every organization
        public const AuthenticationModes DotDomAuthenticationModes = AuthenticationModes.Basic | AuthenticationModes.OAuth;

        public static class TokenScopes
        {
            public const string Gist = "gist";
            public const string Repo = "repo";
        }

        public static class OAuthScopes
        {
            public const string Gist = "gist";
            public const string Repo = "repo";
            public const string Workflow = "workflow";
        }

        public static class EnvironmentVariables
        {
            public const string AuthenticationHelper = "GCM_GITHUB_HELPER";
            public const string AuthenticationModes = "GCM_GITHUB_AUTHMODES";
            public const string DevOAuthClientId = "GCM_DEV_GITHUB_CLIENTID";
            public const string DevOAuthClientSecret = "GCM_DEV_GITHUB_CLIENTSECRET";
            public const string DevOAuthRedirectUri = "GCM_DEV_GITHUB_REDIRECTURI";
        }

        public static class GitConfiguration
        {
            public static class Credential
            {
                public const string AuthenticationHelper = "gitHubHelper";
                public const string AuthenticationModes = "gitHubAuthModes";
                public const string DevOAuthClientId = "gitHubDevClientId";
                public const string DevOAuthClientSecret = "gitHubDevClientSecret";
                public const string DevOAuthRedirectUri = "gitHubDevRedirectUri";
            }
        }
    }
}
