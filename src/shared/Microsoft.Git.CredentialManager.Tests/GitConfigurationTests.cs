// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Git.CredentialManager.Tests.Objects;
using Xunit;

namespace Microsoft.Git.CredentialManager.Tests
{
    public class GitConfigurationTests
    {
        [Fact]
        public void GitProcess_GetConfiguration_ReturnsConfiguration()
        {
            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath);
            var config = git.GetConfiguration();
            Assert.NotNull(config);
        }

        [Fact]
        public void GitConfiguration_Enumerate_CallbackReturnsTrue_InvokesCallbackForEachEntry()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local foo.name lancelot").AssertSuccess();
            Git(repoPath, workDirPath, "config --local foo.quest seek-holy-grail").AssertSuccess();
            Git(repoPath, workDirPath, "config --local foo.favcolor blue").AssertSuccess();

            var expectedVisitedEntries = new List<(string name, string value)>
            {
                ("foo.name", "lancelot"),
                ("foo.quest", "seek-holy-grail"),
                ("foo.favcolor", "blue")
            };

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            var actualVisitedEntries = new List<(string name, string value)>();

            bool cb(string name, string value)
            {
                if (name.StartsWith("foo."))
                {
                    actualVisitedEntries.Add((name, value));
                }

                // Continue enumeration
                return true;
            }

            config.Enumerate(cb);

            Assert.Equal(expectedVisitedEntries, actualVisitedEntries);
        }

        [Fact]
        public void GitConfiguration_Enumerate_CallbackReturnsFalse_InvokesCallbackForEachEntryUntilReturnsFalse()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local foo.name lancelot").AssertSuccess();
            Git(repoPath, workDirPath, "config --local foo.quest seek-holy-grail").AssertSuccess();
            Git(repoPath, workDirPath, "config --local foo.favcolor blue").AssertSuccess();

            var expectedVisitedEntries = new List<(string name, string value)>
            {
                ("foo.name", "lancelot"),
                ("foo.quest", "seek-holy-grail")
            };

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            var actualVisitedEntries = new List<(string name, string value)>();

            bool cb(string name, string value)
            {
                if (name.StartsWith("foo."))
                {
                    actualVisitedEntries.Add((name, value));
                }

                // Stop enumeration after 2 'foo' entries
                return actualVisitedEntries.Count < 2;
            }

            config.Enumerate(cb);

            Assert.Equal(expectedVisitedEntries, actualVisitedEntries);
        }

        [Fact]
        public void GitConfiguration_TryGetValue_Name_Exists_ReturnsTrueOutString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            bool result = config.TryGetValue("user.name", out string value);
            Assert.True(result);
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_TryGetValue_Name_DoesNotExists_ReturnsFalse()
        {
            string repoPath = CreateRepository();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string randomName = $"{Guid.NewGuid():N}.{Guid.NewGuid():N}";
            bool result = config.TryGetValue(randomName, out string value);
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void GitConfiguration_TryGetValue_SectionProperty_Exists_ReturnsTrueOutString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            bool result = config.TryGetValue("user", "name", out string value);
            Assert.True(result);
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_TryGetValue_SectionProperty_DoesNotExists_ReturnsFalse()
        {
            string repoPath = CreateRepository();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string randomSection = Guid.NewGuid().ToString("N");
            string randomProperty = Guid.NewGuid().ToString("N");
            bool result = config.TryGetValue(randomSection, randomProperty, out string value);
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void GitConfiguration_TryGetValue_SectionScopeProperty_Exists_ReturnsTrueOutString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.example.com.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            bool result = config.TryGetValue("user", "example.com", "name", out string value);
            Assert.True(result);
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_TryGetValue_SectionScopeProperty_NullScope_ReturnsTrueOutUnscopedString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            bool result = config.TryGetValue("user", null, "name", out string value);
            Assert.True(result);
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_TryGetValue_SectionScopeProperty_DoesNotExists_ReturnsFalse()
        {
            string repoPath = CreateRepository();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string randomSection = Guid.NewGuid().ToString("N");
            string randomScope = Guid.NewGuid().ToString("N");
            string randomProperty = Guid.NewGuid().ToString("N");
            bool result = config.TryGetValue(randomSection, randomScope, randomProperty, out string value);
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void GitConfiguration_GetString_Name_Exists_ReturnsString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string value = config.GetValue("user.name");
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_GetString_Name_DoesNotExists_ThrowsException()
        {
            string repoPath = CreateRepository();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string randomName = $"{Guid.NewGuid():N}.{Guid.NewGuid():N}";
            Assert.Throws<KeyNotFoundException>(() => config.GetValue(randomName));
        }

        [Fact]
        public void GitConfiguration_GetString_SectionProperty_Exists_ReturnsString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string value = config.GetValue("user", "name");
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_GetString_SectionProperty_DoesNotExists_ThrowsException()
        {
            string repoPath = CreateRepository();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string randomSection = Guid.NewGuid().ToString("N");
            string randomProperty = Guid.NewGuid().ToString("N");
            Assert.Throws<KeyNotFoundException>(() => config.GetValue(randomSection, randomProperty));
        }

        [Fact]
        public void GitConfiguration_GetString_SectionScopeProperty_Exists_ReturnsString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.example.com.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string value = config.GetValue("user", "example.com", "name");
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_GetString_SectionScopeProperty_NullScope_ReturnsUnscopedString()
        {
            string repoPath = CreateRepository(out string workDirPath);
            Git(repoPath, workDirPath, "config --local user.name john.doe").AssertSuccess();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string value = config.GetValue("user", null, "name");
            Assert.NotNull(value);
            Assert.Equal("john.doe", value);
        }

        [Fact]
        public void GitConfiguration_GetString_SectionScopeProperty_DoesNotExists_ThrowsException()
        {
            string repoPath = CreateRepository();

            string gitPath = GetGitPath();
            var trace = new NullTrace();
            var git = new GitProcess(trace, gitPath, repoPath);
            IGitConfiguration config = git.GetConfiguration();

            string randomSection = Guid.NewGuid().ToString("N");
            string randomScope = Guid.NewGuid().ToString("N");
            string randomProperty = Guid.NewGuid().ToString("N");
            Assert.Throws<KeyNotFoundException>(() => config.GetValue(randomSection, randomScope, randomProperty));
        }

        #region Test helpers

        private static string GetGitPath()
        {
            ProcessStartInfo psi;
            if (PlatformUtils.IsWindows())
            {
                psi = new ProcessStartInfo(
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.System),
                        "where.exe"),
                    "git.exe"
                );
            }
            else
            {
                psi = new ProcessStartInfo("/usr/bin/which", "git");
            }

            psi.RedirectStandardOutput = true;

            using (var which = new Process {StartInfo = psi})
            {
                which.Start();
                which.WaitForExit();

                if (which.ExitCode != 0)
                {
                    throw new Exception("Failed to locate Git");
                }

                string data = which.StandardOutput.ReadLine();

                if (string.IsNullOrWhiteSpace(data))
                {
                    throw new Exception("Failed to locate Git on the PATH");
                }

                return data;
            }
        }

        private static string CreateRepository() => CreateRepository(out _);

        private static string CreateRepository(out string workDirPath)
        {
            string tempDirectory = Path.GetTempPath();
            string repoName = $"repo-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            workDirPath = Path.Combine(tempDirectory, repoName);
            string gitDirPath = Path.Combine(workDirPath, ".git");

            if (Directory.Exists(workDirPath))
            {
                Directory.Delete(workDirPath);
            }

            Directory.CreateDirectory(workDirPath);

            Git(gitDirPath, workDirPath, "init").AssertSuccess();

            return gitDirPath;
        }

        private static GitResult Git(string repositoryPath, string workingDirectory, string command)
        {
            var procInfo = new ProcessStartInfo("git", command)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory
            };

            procInfo.Environment["GIT_DIR"] = repositoryPath;

            Process proc = Process.Start(procInfo);
            if (proc is null)
            {
                throw new Exception("Failed to start Git process");
            }

            proc.WaitForExit();

            var result = new GitResult
            {
                ExitCode = proc.ExitCode,
                StandardOutput = proc.StandardOutput.ReadToEnd(),
                StandardError = proc.StandardError.ReadToEnd()
            };

            return result;
        }

        private struct GitResult
        {
            public int ExitCode;
            public string StandardOutput;
            public string StandardError;

            public void AssertSuccess()
            {
                Assert.Equal(0, ExitCode);
            }
        }

        #endregion
    }
}
