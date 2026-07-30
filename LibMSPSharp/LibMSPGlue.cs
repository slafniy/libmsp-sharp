using System.Runtime.InteropServices;

namespace LibMSPSharp;

public static partial class LibMSPGlue {
    private const string LibmspNativePath = "/mnt/data/code/libmsp/cmake-build-release/libmsp.so";

    [LibraryImport(LibmspNativePath, EntryPoint = "msp_init")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool Init();


    [LibraryImport(LibmspNativePath, EntryPoint = "msp_deinit")]
    public static partial void Deinit();

    [LibraryImport(LibmspNativePath, EntryPoint = "msp_play")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool Play([MarshalAs(UnmanagedType.LPUTF8Str)] string fileName);
}