using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LibMSPSharp;

public sealed partial class LibMSP : IDisposable {
    private bool _disposed; // to avoid double-free in the native part
    private readonly nint _ctxHandle;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable - keep it to save from GC
    private readonly LibMSPInternal.StatusChangeCallbackDelegate? _callbackDelegate;
    public event Action<Status>? StatusChanged;
    
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
        _ctxHandle = LibMSPInternal.Init();
        if (_ctxHandle == IntPtr.Zero) {
            throw new InvalidOperationException("Failed to initialize libmsp");
        }

        _callbackDelegate = OnStatusChangeCallback;
        LibMSPInternal.RegisterStatusChangeCallback(_ctxHandle, _callbackDelegate, IntPtr.Zero);
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
        return LibMSPInternal.Play(_ctxHandle, filePath);
    }
    
    public bool TogglePause() {
        return LibMSPInternal.TogglePause(_ctxHandle);
    }
    
    public bool Stop() {
        return LibMSPInternal.Stop(_ctxHandle);
    }
    
    public bool SetVolume(float volume) {
        return LibMSPInternal.SetVolume(_ctxHandle, volume);
    }
    
    public bool SetPosition(uint positionMs) {
        return LibMSPInternal.SetPosition(_ctxHandle, positionMs);
    }

    /// <summary>
    /// Get current playback position.
    /// </summary>
    /// <returns>Position in milliseconds, or null if it cannot obtain</returns>
    public uint? GetPositionMs() {
        long pos = LibMSPInternal.GetPosition(_ctxHandle);
        return pos < 0 ? null : (uint)pos;
    }

    /// <summary>
    /// Get current track total duration
    /// </summary>
    /// <returns>Duration in milliseconds, null if it cannot obtain</returns>
    public uint? GetDurationMs() {
        long dur = LibMSPInternal.GetDuration(_ctxHandle);
        return dur < 0 ? null : (uint)dur;
    }

    /// <summary>
    /// Gets current playback status.
    /// </summary>
    /// <returns>LibMSP.Status enum value</returns>
    public Status GetStatus() {
        return (Status)LibMSPInternal.GetStatus(_ctxHandle);
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
    public static Dictionary<string, string?> GetMetadata(string fileName, string[] keys) {
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
        if (_disposed || _ctxHandle == IntPtr.Zero) return;
        LibMSPInternal.Deinit(_ctxHandle);
        _disposed = true;
    }

    // typedef void (*player_status_callback_t)(player_status_t new_status, void *user_data);
    private void OnStatusChangeCallback(int status, nint userData) {
        StatusChanged?.Invoke((Status)status);
    }
    
    /// <summary>
    /// This is actual native library bindings class. Made private to hide any unmanaged code from user.
    /// </summary>
    private static partial class LibMSPInternal {
        private const string LibraryName = "libmsp";

        // playback_context_t *msp_init(void)
        [LibraryImport(LibraryName, EntryPoint = "msp_init")]
        public static partial nint Init();

        // void msp_deinit(playback_context_t *ctx);
        [LibraryImport(LibraryName, EntryPoint = "msp_deinit")]
        public static partial void Deinit(nint ctxHandle);

        // bool msp_play(const playback_context_t *ctx, const char *filename);
        [LibraryImport(LibraryName, EntryPoint = "msp_play")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool Play(nint ctxHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string fileName);

        // bool msp_toggle_pause(const playback_context_t *ctx);
        [LibraryImport(LibraryName, EntryPoint = "msp_toggle_pause")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool TogglePause(nint ctxHandle);

        // bool msp_stop(const playback_context_t *ctx);
        [LibraryImport(LibraryName, EntryPoint = "msp_stop")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool Stop(nint ctxHandle);

        // bool msp_set_volume(const playback_context_t *ctx, float volume);
        [LibraryImport(LibraryName, EntryPoint = "msp_set_volume")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool SetVolume(nint ctxHandle, float volume);

        // bool msp_set_position(const playback_context_t *ctx, uint32_t position_ms);
        [LibraryImport(LibraryName, EntryPoint = "msp_set_position")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool SetPosition(nint ctxHandle, uint positionMs);

        // int64_t msp_get_position(const playback_context_t *ctx);
        [LibraryImport(LibraryName, EntryPoint = "msp_get_position")]
        public static partial long GetPosition(nint ctxHandle);

        // int64_t msp_get_duration(const playback_context_t *ctx);
        [LibraryImport(LibraryName, EntryPoint = "msp_get_duration")]
        public static partial long GetDuration(nint ctxHandle);

        // player_status_t msp_get_status(const playback_context_t *ctx);
        /* typedef enum {
            MSP_STATUS_UNINITIALIZED = 0, // special case to return when the context does not exist yet/anymore
            MSP_STATUS_IDLE,
            MSP_STATUS_PLAYING,
            MSP_STATUS_PAUSED,
            MSP_STATUS_ERROR
        } player_status_t; */
        [LibraryImport(LibraryName, EntryPoint = "msp_get_status")]
        public static partial int GetStatus(nint ctxHandle);

        // typedef void (*player_status_callback_t)(player_status_t new_status, void *user_data);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StatusChangeCallbackDelegate(int status, nint userData);
        
        // bool msp_register_on_status_change_callback(playback_context_t *ctx, player_status_callback_t callback, void *user_data);
        [LibraryImport(LibraryName, EntryPoint = "msp_register_on_status_change_callback")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool RegisterStatusChangeCallback(nint ctxHandle, StatusChangeCallbackDelegate cb, nint userData);
        
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