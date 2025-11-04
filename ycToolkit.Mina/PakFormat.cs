using System.Diagnostics;
using ycToolkit;

namespace ycToolkit.Mina;

public class PakFormat : FileFormatBase
{    
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


    public override bool Read(ycBinaryReader br)
    {
        if (!base.Read(br))
        {
            return false;
        }


        br.Seek((long)Header.UnknownTableOffset, SeekOrigin.Begin);

        var fileCount = br.ReadUInt32(); // x * 10 for some reason ig
        var nodeCount = br.ReadUInt32();

        br.ReadUInt32(); // unk
        br.ReadUInt32(); // unk2


        for (var i = 0; i < nodeCount; i += 4)
        {
            FileInfos.Add(new FileInfo(br));

            var pos = br.Position;

            br.Position = FileInfos[FileInfos.Count - 1].NameAddress;

            tempFileNames.Add(br.ReadString(FileInfos[FileInfos.Count - 1].NameLength));

            br.Position = pos;
        }

        Debug.WriteLineIf((tempFileNames.Count * 0x10) != fileCount, "Mismatch in filecount with tempfilenames count!");


        return true;
    }

    //public static bool Read(ycBinaryReader br, out PakFormat? pakFormat)
    //{
    //    pakFormat = null;

    //    if (!br.ReadMagic())
    //        return false;

    //    pakFormat = new PakFormat();


    //    pakFormat.Header.Read(br);

    //    br.Seek((long)pakFormat.Header.FileInfoOffset, SeekOrigin.Begin);

    //    var nodeEndAddress = br.ReadUInt32();
    //    var nodeCount = br.ReadUInt32();

    //    br.ReadUInt32(); // unk
    //    br.ReadUInt32(); // unk2


    //    for (var i = 0; i < nodeCount; i += 4)
    //    {
    //        pakFormat.FileInfos.Add(new FileInfo(br));

    //        var pos = br.Position;

    //        br.Position = pakFormat.FileInfos[pakFormat.FileInfos.Count - 1].NameAddress;

    //        pakFormat.tempFileNames.Add(br.ReadString(pakFormat.FileInfos[pakFormat.FileInfos.Count - 1].NameLength));

    //        br.Position = pos;
    //    }



    //    //br.Position = ((long)pakFormat.Header.FileArrayOffset) + 0x4;

    //    //var lastPos = br.Position;

    //    //int ycdFound = 0;
    //    //while (true)
    //    //{ 
    //    //    while (br.ReadUInt32() != YCD0)
    //    //    {
    //    //        if (br.EndOfStream)
    //    //            break;
    //    //    }
    //    //    var size = (br.Position - lastPos);

    //    //    Console.WriteLine($"[{ycdFound}] = {(lastPos - 0x4):X4} - SIZE: {size:X4}");
    //    //    ycdFound++;
    //    //    lastPos = br.Position;

    //    //    if (br.EndOfStream)
    //    //        break;
    //    //}

    //    //Console.WriteLine($"YCD FOUND: {ycdFound}");
    //    return true;
    //}

    public List<FileInfo> FileInfos = [];
    public List<string> tempFileNames = [];


}
