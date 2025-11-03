using System.Diagnostics;
using ycToolkit;

namespace ycToolkit.Mina;

public class PakFormat
{
    public struct PakHeader
    {
        public ulong field_00;
        public ulong field_08;
        public ulong field_10;
        public uint field_18;
        public uint field_1C;
        public ulong FileInfoOffset; // 0x20
        public ulong FileArrayOffset; // 0x28
        public ulong field_2C;
        public ulong field_34;
        public ulong field_3C;
        public ulong field_44;

        public void Read(ycBinaryReader br)
        {
            field_00 = br.ReadUInt64();
            field_08 = br.ReadUInt64();
            field_10 = br.ReadUInt64();

            field_18 = br.ReadUInt32();
            field_1C = br.ReadUInt32();

            FileInfoOffset = br.ReadUInt64();
            FileArrayOffset = br.ReadUInt64();

            field_34 = br.ReadUInt64();
            field_3C = br.ReadUInt64();
            field_44 = br.ReadUInt64();
        }
    }

    
    public struct FileInfo
    {
        public long ADDRESS;

        public uint NameLength;
        public long NameAddress;
        public uint FileSize;
        public long FileAddress;

        public FileInfo(ycBinaryReader br)
        {
            Read(br);
        }

        public void Read(ycBinaryReader br)
        {
            ADDRESS = br.Position;

            br.ReadUInt32();
            NameLength = br.ReadUInt32();

            br.ReadUInt32();
            NameAddress = br.ReadUInt32() + (br.Position - 0x8);

            FileSize = br.ReadUInt32();
            br.ReadUInt32();

            br.ReadUInt32();
            FileAddress = br.ReadUInt32() + (br.Position - 0x8);

        }

    }


    struct Node
    {
        uint type;
        uint RelativePointer;
    }

    public static bool Read(string filepath, out PakFormat? pakFormat)
    {
        pakFormat = null;
        bool result = false;

        using FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using ycBinaryReader br = new ycBinaryReader(fs);

        result = Read(br, out pakFormat);

        br.Close();

        return result;
    }
    public static bool Read(ycBinaryReader br, out PakFormat? pakFormat)
    {
        pakFormat = null;

        if (!br.ReadMagic())
            return false;

        pakFormat = new PakFormat();
        pakFormat.Header = new PakHeader();

        pakFormat.Header.Read(br);

        br.Seek((long)pakFormat.Header.FileInfoOffset, SeekOrigin.Begin);

        var nodeEndAddress = br.ReadUInt32();
        var nodeCount = br.ReadUInt32();

        br.ReadUInt32(); // unk
        br.ReadUInt32(); // unk2


        for (var i = 0; i < nodeCount; i += 4)
        {
            pakFormat.FileInfos.Add(new FileInfo(br));

            var pos = br.Position;

            br.Position = pakFormat.FileInfos[pakFormat.FileInfos.Count - 1].NameAddress;

            pakFormat.tempFileNames.Add(br.ReadString(pakFormat.FileInfos[pakFormat.FileInfos.Count - 1].NameLength));

            br.Position = pos;
        }



        //br.Position = ((long)pakFormat.Header.FileArrayOffset) + 0x4;

        //var lastPos = br.Position;

        //int ycdFound = 0;
        //while (true)
        //{ 
        //    while (br.ReadUInt32() != YCD0)
        //    {
        //        if (br.EndOfStream)
        //            break;
        //    }
        //    var size = (br.Position - lastPos);

        //    Console.WriteLine($"[{ycdFound}] = {(lastPos - 0x4):X4} - SIZE: {size:X4}");
        //    ycdFound++;
        //    lastPos = br.Position;

        //    if (br.EndOfStream)
        //        break;
        //}

        //Console.WriteLine($"YCD FOUND: {ycdFound}");
        return true;
    }

    public PakHeader Header = new PakHeader();

    public List<FileInfo> FileInfos = [];
    public List<string> tempFileNames = [];


}
