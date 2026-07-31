using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LibMSPSharp;

public sealed partial class LibMSP : IDisposable {
    private bool _disposed; // to avoid double-free in the native part

    /// <summary>
    /// Initializes native libmsp
    /// </summary>
    /// <param name="libMspNativePath">Path to native library, e.g. "./libmsp.so"</param>
    /// <exception cref="Exception">Cannot initialize native library</exception>
    public LibMSP(string libMspNativePath) {
        LibMSPInternal.LibraryPath = libMspNativePath;
        if (!LibMSPInternal.Init()) {
            throw new InvalidOperationException($"Failed to initialize {libMspNativePath}");
        }
    }

    public void Dispose() {
        DeinitNative();
        GC.SuppressFinalize(this);
    }

    // In case if user forget to use "using" - free native part in the destructor
    ~LibMSP() {
        DeinitNative();
    }

    public bool Play(string filePath) {
        return LibMSPInternal.Play(filePath);
    }

    public bool TogglePause() {
        return LibMSPInternal.TogglePause();
    }
    
    /// <summary>
    /// Opens file, reads its metadata.
    /// Returns a dictionary where keys are requested metadata keys. Values could be null.
    /// </summary>
    /// <param name="fileName">file from which we want metadata</param>
    /// <param name="keys">each key could be e.g. "artist", "title" etc., case-insensitive.</param>
    /// <returns>A dictionary where keys are requested metadata keys, passed as parameters.
    /// Values could be null</returns>
    /// <exception cref="ArgumentException">if keys count is zero</exception>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global - depends on native lib loading via constructor
    public Dictionary<string, string?> GetMetadata(string fileName, string[] keys) {
        if (keys.Length == 0) {
            throw new ArgumentException("You should provide at least one key to search");
        }

        nint resultValuesPtr = LibMSPInternal.GetMetadata(fileName, keys, (ulong)keys.Length);
        var res = new Dictionary<string, string?>(keys.Length, StringComparer.OrdinalIgnoreCase);

        if (resultValuesPtr == nint.Zero) {
            foreach (string k in keys) {
                res[k] = null;
            }

            return res;
        }

        try {
            var valuePtrs = new nint[keys.Length];
            Marshal.Copy(resultValuesPtr, valuePtrs, 0, keys.Length);

            for (var i = 0; i < keys.Length; i++) {
                nint strPtr = valuePtrs[i];
                res[keys[i]] = strPtr != nint.Zero ? Marshal.PtrToStringUTF8(strPtr) : null;
            }

            return res;
        }
        finally {
            LibMSPInternal.FreeMetadataResult(resultValuesPtr, (ulong)keys.Length);
        }
    }

    private void DeinitNative() {
        if (_disposed) return;
        LibMSPInternal.Deinit();
        _disposed = true;
    }

    /// <summary>
    /// This is actual native library bindings class. Made private to hide any unmanaged code from user.
    /// </summary>
    private static partial class LibMSPInternal {
        public static string? LibraryPath;
        private const string LibraryName = "libmsp";

        static LibMSPInternal() {
            NativeLibrary.SetDllImportResolver(typeof(LibMSPInternal).Assembly, DllImportResolver);
        }

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly,
            DllImportSearchPath? searchPath) {
            if (libraryName == LibraryName && !string.IsNullOrWhiteSpace(LibraryPath)) {
                return NativeLibrary.Load(LibraryPath);
            }

            return IntPtr.Zero;
        }

        [LibraryImport(LibraryName, EntryPoint = "msp_init")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool Init();

        [LibraryImport(LibraryName, EntryPoint = "msp_deinit")]
        public static partial void Deinit();

        [LibraryImport(LibraryName, EntryPoint = "msp_play")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool Play([MarshalAs(UnmanagedType.LPUTF8Str)] string fileName);

        [LibraryImport(LibraryName, EntryPoint = "msp_toggle_pause")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool TogglePause();
        
        [LibraryImport(LibraryName, EntryPoint = "msp_get_metadata")]
        public static partial nint GetMetadata(
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string fileName,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)]
            string[] keys,
            ulong keysCount);

        [LibraryImport(LibraryName, EntryPoint = "msp_free_metadata_result")]
        public static partial void FreeMetadataResult(nint values, ulong keysCount);
    }
}