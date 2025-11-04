using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ycToolkit.Mina;

public sealed class FileFormatHeader
{
    /// <summary>
    /// "YCD\0"
    /// </summary>
    public const uint YCDSignature = 0x00444359;

    public class UnknownHeaderArrayEntry(byte[] value)
    {
        public byte[] Data { get; private set; } = value;

    }


    public uint Signature;
    /// <summary>
    /// Unknown! Possibly version information?
    /// </summary>
    public uint Field_0x04;
    /// <summary>
    /// The size of the header (in bytes)
    /// </summary>
    public ulong HeaderLength;
    /// <summary>
    /// Unknown! Possible some sort of hash?
    /// </summary>
    public ulong Field_0x10;

    public uint Field_0x18;
    public uint Field_0x1C;
    public uint Field_0x20;

    /// <summary>
    /// Number of elements, which are 16 bytes each, of the array, which currently contains an unknown member
    /// </summary>
    public uint UnknownArrayCount;
    /// <summary>
    /// Appears to be the offset in the file where some unknown table starts (this appears to be the same as the header size in all vanilla files)
    /// </summary>
    public ulong UnknownTableOffset;

    /// <summary>
    /// Appears to be the offset in the file where actual data starts
    /// </summary>
    public ulong UnknownInfoOffset;

    /// <summary>
    /// Unknown, appears to be set to the end of the file when not being used
    /// </summary>
    public ulong UnknownOffset3;
    /// <summary>
    /// Unknown, appears to be set to the end of the file when not being used
    /// </summary>
    public ulong UnknownOffset4;
    /// <summary>
    /// Unknown, always seems to be set to 8
    /// </summary>
    public ulong Field_0x48;


    /// <summary>
    /// Unknown block of memory, seems to always be set at 0x50 at a size of 0x30, following 12 0xFF and 12 0x00
    /// </summary>
    public byte[] UnknownBlock = new byte[0x30];

    public UnknownHeaderArrayEntry[] UnknownArray = [];

    /// <summary>
    /// Unknown block of memory, probably just padding? Always seems to be 0x20
    /// </summary>
    public byte[] HeaderPad = new byte[0x20];

    public FileFormatHeader()
    {

    }

    public bool Read(ycBinaryReader br)
    {
        var supposedSignature = br.ReadUInt32();

        if (supposedSignature != YCDSignature)
        {
            Debug.WriteLine($"Signature: {supposedSignature:X4} does not match YCDSignature: {YCDSignature:X4}");
            return false;
        }

        Signature = supposedSignature;

        Field_0x04 = br.ReadUInt32();

        Debug.WriteLineIf(Field_0x04 != 0x08, $"Field_0x48 was different: {Field_0x04:X4}");

        HeaderLength = br.ReadUInt64();

        Field_0x10 = br.ReadUInt64();
        Field_0x18 = br.ReadUInt32();
        Field_0x1C = br.ReadUInt32();
        Field_0x20 = br.ReadUInt32();

        UnknownArrayCount = br.ReadUInt32();
        UnknownTableOffset = br.ReadUInt64();
        UnknownInfoOffset = br.ReadUInt64();
        UnknownOffset3 = br.ReadUInt64();
        UnknownOffset4 = br.ReadUInt64();

        Field_0x48 = br.ReadUInt64();

        Debug.WriteLineIf(Field_0x48 != 0x08, $"Field_0x48 was different: {Field_0x48:X4}");

        for (var i = 0; i < 0x18; i++)
        {
            UnknownBlock[i] = br.ReadByte();

            Debug.WriteLineIf(UnknownBlock[i] != 0xFF, $"UnknownBlock[{i}] was different: {UnknownBlock[i]:X4}");
        }
        for (var i = 0; i < 0x18; i++)
        {
            UnknownBlock[i + 0x18] = br.ReadByte();

            Debug.WriteLineIf(UnknownBlock[i + 0x18] != 0x00, $"UnknownBlock[{i + 0x18}] was different: {UnknownBlock[i + 0x18]:X4}");
        }

        UnknownArray = new UnknownHeaderArrayEntry[UnknownArrayCount];
        for (var i = 0; i < UnknownArrayCount; i++)
        {
            UnknownArray[i] = new(br.ReadBytes(0x10));
        }

        for (var i = 0; i < 0x20; i++)
        {
            HeaderPad[i] = br.ReadByte();
            Debug.WriteLineIf(HeaderPad[i] != 0x00, $"HeaderPad[{i}] was different: {HeaderPad[i]:X4}");
        }
        return true;
    }

}
