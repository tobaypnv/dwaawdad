// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Git.CredentialManager
{
    public static class Constants
    {
        public const string PersonalAccessTokenUserName = "PersonalAccessToken";
        public const string MicrosoftAuthHelperName = "Microsoft.Authentication.Helper";

        public const string ProviderIdAuto  = "auto";
        public const string AuthorityIdAuto = "auto";

        public static class EnvironmentVariables
        {
            public const string GcmTrace           = "GCM_TRACE";
            public const string GcmTraceSecrets    = "GCM_TRACE_SECRETS";
            public const string GcmTraceMsAuth     = "GCM_TRACE_MSAUTH";
            public const string GcmDebug           = "GCM_DEBUG";
            public const string GcmProvider        = "GCM_PROVIDER";
            public const string GcmAuthority       = "GCM_AUTHORITY";
            public const string GitTerminalPrompts = "GIT_TERMINAL_PROMPT";
            public const string GcmAllowWia        = "GCM_ALLOW_WINDOWSAUTH";
            public const string CurlAllProxy       = "ALL_PROXY";
            public const string CurlHttpProxy      = "HTTP_PROXY";
            public const string CurlHttpsProxy     = "HTTPS_PROXY";
            public const string GcmHttpProxy       = "GCM_HTTP_PROXY";
            public const string GitSslNoVerify     = "GIT_SSL_NO_VERIFY";
        }

        public static class Http
        {
            public const string WwwAuthenticateBasicScheme     = "Basic";
            public const string WwwAuthenticateBearerScheme    = "Bearer";
            public const string WwwAuthenticateNegotiateScheme = "Negotiate";
            public const string WwwAuthenticateNtlmScheme      = "NTLM";

            public const string MimeTypeJson = "application/json";
        }

        public static class GitConfiguration
        {
            public static class Credential
            {
                public const string SectionName = "credential";
                public const string Helper      = "helper";
                public const string Provider    = "provider";
                public const string Authority   = "authority";
                public const string AllowWia    = "allowWindowsAuth";
                public const string HttpProxy   = "httpProxy";
                public const string HttpsProxy  = "httpsProxy";
            }

            public static class Http
            {
                public const string SectionName = "http";
                public const string Proxy = "proxy";
                public const string SslVerify = "sslVerify";
            }
        }

        public static class HelpUrls
        {
            public const string GcmProjectUrl          = "https://aka.ms/gcmcore";
            public const string GcmAuthorityDeprecated = "https://aka.ms/gcmcore-authority";
            public const string GcmHttpProxyGuide      = "https://aka.ms/gcmcore-httpproxy";
            public const string GcmTlsVerification     = "https://aka.ms/gcmcore-tlsverify";
        }

        private static string _gcmVersion;

        /// <summary>
        /// The current version of Git Credential Manager.
        /// </summary>
        public static string GcmVersion
        {
            get
            {
                if (_gcmVersion is null)
                {
                    _gcmVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                }

                return _gcmVersion;
            }
        }

        /// <summary>
        /// Get standard program header title for Git Credential Manager, including the current version and OS information.
        /// </summary>
        /// <returns>Standard program header.</returns>
        public static string GetProgramHeader()
        {
            PlatformInformation info = PlatformUtils.GetPlatformInformation();

            return $"Git Credential Manager version {GcmVersion} ({info.OperatingSystemType}, {info.ClrVersion})";
        }

        /// <summary>
        /// Get the HTTP user-agent for Git Credential Manager.
        /// </summary>
        /// <returns>User-agent string for HTTP requests.</returns>
        public static string GetHttpUserAgent()
        {
            PlatformInformation info = PlatformUtils.GetPlatformInformation();
            string osType     = info.OperatingSystemType;
            string cpuArch    = info.CpuArchitecture;
            string clrVersion = info.ClrVersion;

            return string.Format($"Git-Credential-Manager/{GcmVersion} ({osType}; {cpuArch}) CLR/{clrVersion}");
        }
    }
}
