// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System.IO;

namespace Microsoft.Git.CredentialManager
{
    /// <summary>
    /// Represents a file system and operations that can be performed.
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Check if two paths are the same for the current platform and file system. Symbolic links are not followed.
        /// </summary>
        /// <param name="a">File path.</param>
        /// <param name="b">File path.</param>
        /// <returns>True if both file paths are the same, false otherwise.</returns>
        bool IsSamePath(string a, string b);

        /// <summary>
        /// Check if a file exists at the specified path.
        /// </summary>
        /// <param name="path">Full path to file to test.</param>
        /// <returns>True if a file exists, false otherwise.</returns>
        bool FileExists(string path);

        /// <summary>
        /// Check if a directory exists at the specified path.
        /// </summary>
        /// <param name="path">Full path to directory to test.</param>
        /// <returns>True if a directory exists, false otherwise.</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Get the path to the current directory of the currently executing process.
        /// </summary>
        /// <returns>Current process directory.</returns>
        string GetCurrentDirectory();

        /// <summary>
        /// Open a file stream at the specified path with the given access and mode settings.
        /// </summary>
        /// <param name="path">Full file path.</param>
        /// <param name="fileMode">File mode settings.</param>
        /// <param name="fileAccess">File access settings.</param>
        /// <param name="fileShare">File share settings.</param>
        /// <returns></returns>
        Stream OpenFileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
    }

    /// <summary>
    /// The real file system.
    /// </summary>
    public abstract class FileSystem : IFileSystem
    {
        public abstract bool IsSamePath(string a, string b);

        public bool FileExists(string path) => File.Exists(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public string GetCurrentDirectory() => Directory.GetCurrentDirectory();

        public Stream OpenFileStream(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
            => File.Open(path, fileMode, fileAccess, fileShare);
    }
}
