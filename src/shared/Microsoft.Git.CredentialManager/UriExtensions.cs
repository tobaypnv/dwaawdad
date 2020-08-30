// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Net;

namespace Microsoft.Git.CredentialManager
{
    public static class UriExtensions
    {
        public static bool TryGetUserInfo(this Uri uri, out string userName, out string password)
        {
            EnsureArgument.NotNull(uri, nameof(uri));
            userName = null;
            password = null;

            if (string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                return false;
            }

            /* According to RFC 3986 section 3.2.1 (https://tools.ietf.org/html/rfc3986#section-3.2.1)
             * the user information component of a URI should look like:
             *
             *     url-encode(username):url-encode(password)
             */
            string[] split = uri.UserInfo.Split(new[] {':'}, count: 2);

            if (split.Length > 0)
            {
                userName = WebUtility.UrlDecode(split[0]);
            }
            if (split.Length > 1)
            {
                password = WebUtility.UrlDecode(split[1]);
            }

            return split.Length > 0;
        }

        public static IEnumerable<string> GetGitConfigurationScopes(this Uri uri)
        {
            EnsureArgument.NotNull(uri, nameof(uri));

            string schemeAndDelim = $"{uri.Scheme}{Uri.SchemeDelimiter}";
            string host = uri.Host.TrimEnd('/');
            string path = uri.AbsolutePath.Trim('/');

            // Unfold the path by component, right-to-left
            while (!string.IsNullOrWhiteSpace(path))
            {
                yield return $"{schemeAndDelim}{host}/{path}";

                // Trim off the last path component
                if (!TryTrimString(path, StringExtensions.TruncateFromLastIndexOf, '/', out path))
                {
                    break;
                }
            }

            // Unfold the host by sub-domain, left-to-right
            while (!string.IsNullOrWhiteSpace(host))
            {
                if (host.Contains(".")) // Do not emit just the TLD
                {
                    yield return $"{schemeAndDelim}{host}";
                }

                // Trim off the left-most sub-domain
                if (!TryTrimString(host, StringExtensions.TrimUntilIndexOf, '.', out host))
                {
                    break;
                }
            }
        }

        private static bool TryTrimString(string input, Func<string, char, string> func, char c, out string output)
        {
            output = func(input, c);
            return !StringComparer.Ordinal.Equals(input, output);
        }
    }
}
