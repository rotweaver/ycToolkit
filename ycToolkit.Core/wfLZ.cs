using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ycToolkit.Core;

/// <summary>
/// Native bindings for Shane's wfLZ library
/// </summary>
public static partial class wfLZ
{
    [LibraryImport("wfLZ.dll", EntryPoint = "wfLZ_GetWorkMemSize")]
    public static partial uint GetWorkMemSize();


    [LibraryImport("wfLZ.dll", EntryPoint = "wfLZ_GetMaxCompressedSize")]
    public static partial uint GetMaxCompressedSize(uint sourceSize);



    [LibraryImport("wfLZ.dll", EntryPoint = "wfLZ_CompressFast")]
    public static partial uint CompressFast(
        [MarshalAs(UnmanagedType.LPArray)] byte[] source, uint sourceSize,
        [MarshalAs(UnmanagedType.LPArray)] byte[] dest,
        [MarshalAs(UnmanagedType.LPArray)] byte[] workMem, 
        uint swapEndian);



    [LibraryImport("wfLZ.dll", EntryPoint = "wfLZ_GetDecompressedSize")]
    public static partial uint GetDecompressedSize([MarshalAs(UnmanagedType.LPArray)] byte[] source);


    [LibraryImport("wfLZ.dll", EntryPoint = "wfLZ_GetCompressedSize")]
    public static partial uint GetCompressedSize([MarshalAs(UnmanagedType.LPArray)] byte[] source);


    [LibraryImport("wfLZ.dll", EntryPoint = "wfLZ_Decompress")]
    public static partial uint Decompress([MarshalAs(UnmanagedType.LPArray)] byte[] source, [MarshalAs(UnmanagedType.LPArray)] byte[] dest);
}
