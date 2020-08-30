// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
using System;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.UnmanagedType;
using static Microsoft.Git.CredentialManager.Interop.U8StringMarshaler;

namespace Microsoft.Git.CredentialManager.Interop.Native
{
    public static partial class LibGit2
    {
        /*
         * The CLR will apply the following rules when locating the library on each platform:
         *
         *   Windows
         *     - Append ".dll" extension
         *
         *      Final name: git2-SHA.dll
         *
         *   macOS (libgit2-SHA.dylib)
         *     - Prepend the "lib" prefix
         *     - Append ".dylib" extension
         *
         *      Final name: libgit2-SHA.dylib
         *
         *   Linux (libgit2-SHA.so)
         *     - Prepend the "lib" prefix
         *     - Append ".so" extension
         *
         *      Final name: libgit2-SHA.so
         */
        private const string LibraryName = "git2-572e4d8";

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int git_libgit2_init();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int git_libgit2_shutdown();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern git_error git_error_last();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void git_buf_dispose(git_buf buf);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int git_repository_discover(
            git_buf @out,
            [MarshalAs(CustomMarshaler, MarshalCookie = NativeCookie, MarshalTypeRef = typeof(U8StringMarshaler))]
            string start_path,
            bool across_fs,
            [MarshalAs(CustomMarshaler, MarshalCookie = NativeCookie, MarshalTypeRef = typeof(U8StringMarshaler))]
            string ceiling_dirs);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int git_config_add_file_ondisk(
            git_config* cfg,
            [MarshalAs(CustomMarshaler, MarshalCookie = NativeCookie, MarshalTypeRef = typeof(U8StringMarshaler))]
            string path,
            git_config_level_t level,
            git_repository* repo,
            int force);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void git_config_free(git_config* cfg);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int git_config_get_string(
            [MarshalAs(CustomMarshaler, MarshalCookie = ManagedCookie, MarshalTypeRef = typeof(U8StringMarshaler))]
            out string @out,
            git_config* cfg,
            [MarshalAs(CustomMarshaler, MarshalCookie = NativeCookie, MarshalTypeRef = typeof(U8StringMarshaler))]
            string name);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int git_config_open_default(git_config** @out);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int git_config_snapshot(git_config** @out, git_config* config);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class git_error
    {
        public unsafe byte* message;
        public int klass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class git_buf
    {
        public unsafe byte* ptr;
        public UIntPtr asize;
        public UIntPtr size;

        public override unsafe string ToString()
        {
            return U8StringConverter.ToManaged(ptr);
        }
    }

    public enum git_config_level_t
    {
        GIT_CONFIG_LEVEL_PROGRAMDATA = 1,
        GIT_CONFIG_LEVEL_SYSTEM      = 2,
        GIT_CONFIG_LEVEL_XDG         = 3,
        GIT_CONFIG_LEVEL_GLOBAL      = 4,
        GIT_CONFIG_LEVEL_LOCAL       = 5,
        GIT_CONFIG_LEVEL_APP         = 6,
        GIT_CONFIG_HIGHEST_LEVEL     = -1,
    }

    public struct git_repository { /* opaque */ }
    public struct git_config { /* opaque */ }
}
