using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LibMSPSharp;

public sealed partial class LibMSP : IDisposable {
    private bool _disposed; // to avoid double-free in the native part

    public enum Status {
        Uninitialized,
        Idle,
        Playing,
        Paused,
        Error
    }

    /// <summary>
    /// Initializes native libmsp
    /// </summary>
    /// <exception cref="Exception">Cannot initialize native library</exception>
    public LibMSP() {
        if (!LibMSPInternal.Init()) {
            throw new InvalidOperationException($"Failed to initialize libmsp");
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

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public bool Play(string filePath) {
        return LibMSPInternal.Play(filePath);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public bool TogglePause() {
        return LibMSPInternal.TogglePause();
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public bool Stop() {
        return LibMSPInternal.Stop();
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public bool SetVolume(float volume) {
        return LibMSPInternal.SetVolume(volume);
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public bool SetPosition(uint positionMs) {
        return LibMSPInternal.SetPosition(positionMs);
    }

    /// <summary>
    /// Get current playback position.
    /// </summary>
    /// <returns>Position in milliseconds, or null if it cannot obtain</returns>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public uint? GetPositionMs() {
        long pos = LibMSPInternal.GetPosition();
        return pos < 0 ? null : (uint)pos;
    }

    /// <summary>
    /// Get current track total duration
    /// </summary>
    /// <returns>Duration in milliseconds, null if it cannot obtain</returns>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public uint? GetDurationMs() {
        long dur = LibMSPInternal.GetDuration();
        return dur < 0 ? null : (uint)dur;
    }

    /// <summary>
    /// Gets current playback status.
    /// </summary>
    /// <returns>LibMSP.Status enum value</returns>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public Status GetStatus() {
        return (Status)LibMSPInternal.GetStatus();
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
        private const string LibraryName = "libmsp";

        // bool msp_init(void);
        [LibraryImport(LibraryName, EntryPoint = "msp_init")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool Init();

        // void msp_deinit(void);
        [LibraryImport(LibraryName, EntryPoint = "msp_deinit")]
        public static partial void Deinit();

        // bool msp_play(const char *filename);
        [LibraryImport(LibraryName, EntryPoint = "msp_play")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool Play([MarshalAs(UnmanagedType.LPUTF8Str)] string fileName);

        // bool msp_toggle_pause(void);
        [LibraryImport(LibraryName, EntryPoint = "msp_toggle_pause")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool TogglePause();

        // bool msp_stop(void);
        [LibraryImport(LibraryName, EntryPoint = "msp_stop")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool Stop();

        // bool msp_set_volume(float volume);
        [LibraryImport(LibraryName, EntryPoint = "msp_set_volume")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool SetVolume(float volume);

        // bool msp_set_position(uint32_t position_ms);
        [LibraryImport(LibraryName, EntryPoint = "msp_set_position")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool SetPosition(uint positionMs);

        // int64_t msp_get_position();
        [LibraryImport(LibraryName, EntryPoint = "msp_get_position")]
        public static partial long GetPosition();

        // int64_t msp_get_duration();
        [LibraryImport(LibraryName, EntryPoint = "msp_get_duration")]
        public static partial long GetDuration();

        // player_status_t msp_get_status();
        /* typedef enum {
            MSP_STATUS_UNINITIALIZED = 0, // special case to return when the context does not exist yet/anymore
            MSP_STATUS_IDLE,
            MSP_STATUS_PLAYING,
            MSP_STATUS_PAUSED,
            MSP_STATUS_ERROR
        } player_status_t; */
        [LibraryImport(LibraryName, EntryPoint = "msp_get_status")]
        public static partial int GetStatus();

        // char **msp_get_metadata(const char *filename, const char **keys, uint64_t keys_count);
        [LibraryImport(LibraryName, EntryPoint = "msp_get_metadata")]
        public static partial nint GetMetadata(
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            string fileName,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPUTF8Str)]
            string[] keys,
            ulong keysCount);

        // void msp_free_metadata_result(char **values, uint64_t keys_count);
        [LibraryImport(LibraryName, EntryPoint = "msp_free_metadata_result")]
        public static partial void FreeMetadataResult(nint values, ulong keysCount);
    }
}