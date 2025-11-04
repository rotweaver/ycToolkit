using System.Diagnostics;
using ycToolkit;

namespace ycToolkit.Mina;

public class PakFormat : FileFormatBase
{    
    public struct FileInfo
    {
        public const int Size = 0x20;
        public long ADDRESS;

        public uint NameLength;
        public long NameAddress;
        public uint FileSize;
        public long FileAddress;

        public FileInfo(ycBinaryReader br)
        {
            Read(br);
        }

        public void Write(ycBinaryWriter bw)
        {

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


    // test

    public static void CreateFromDirectory(string filepath)
    {
        var files = Directory.GetFiles(filepath, "*", SearchOption.AllDirectories);

        using FileStream fs = new($"{Path.GetFileName(filepath)}.yc.pak", FileMode.OpenOrCreate, FileAccess.Write);
        using ycBinaryWriter bw = new ycBinaryWriter(fs);

        bw.WriteNullTerminatedString("YCD"); // Signature
        bw.Write(0x00000008);

        bw.Write((ulong)0xC0); // header length (always 0xC for pak?)

        bw.Write(0x00DABFA453C79641); // no idea
        bw.Write((ulong)0x00);

        bw.Write((uint)0x00); // dunno

        bw.Write((uint)0x02); // Array length, always 2 for pak?

        bw.Write((ulong)0xC0); // table offset (always 0xC for pak?)

        long fileDataOffsetPointer = bw.Position;
        // just save for later
        bw.Write((ulong)0x00);
        bw.Write((ulong)0x00);
        bw.Write((ulong)0x00);

        bw.Write((ulong)0x08); // always 0x8 for pak? iunno

        for (var i = 0; i < 3; i++)
            bw.Write(0xFFFFFFFFFFFFFFFF);

        for (var i = 0; i < 3; i++)
            bw.Write((ulong)0x0);

        // I have no clue dude lol
        bw.Write(0x00DABFA453C79641);
        bw.Write(0x1B5907DF0CD4CD7A);

        bw.Write(0x4622A9C9CA71C362);
        bw.Write(0x1FF0A395F9C3D373);

        for (var i = 0; i < 4; i++)
            bw.Write((ulong)0x00); // Pad? dunno!

        // Write the file meta table
        bw.Write((uint)(files.Length * 0x10));
        bw.Write((uint)(files.Length * 0x4));

        // Maybe an offset node?
        bw.Write((uint)0x10);
        bw.Write((uint)0x08);

        FileInfo[] list = new FileInfo[files.Length];

        bw.Position += files.Length * FileInfo.Size;
        for (var i = 0; i < list.Length; i++)
        {
            var name = files[i].Substring(filepath.Length + 1);
            var fileNodeOffset = (0xC + 0x10) + (FileInfo.Size * i);

            list[i].NameLength = (uint)name.Length;
            list[i].NameAddress = bw.Position;

            bw.WriteNullTerminatedString(name);
        }

        bw.Position = bw.Position & ~0b1111; // Round down to nearest multiple of 16
        bw.Position += 0x10; // get to next multiple

        bw.Position += 0x10; // pad

        long fileDataStartAddress = bw.Position;

        for (var i = 0; i < files.Length; i++)
        {
            var fileData = File.ReadAllBytes(files[i]);
            long dataAddress = bw.Position;

            list[i].FileSize = (uint)fileData.Length;

            bw.Write(fileData);

            long jumpbackAddress = bw.Position;

            bw.Position = (0xC + 0x10) + (FileInfo.Size * i);

            // Write the meta data
            bw.Write((uint)0x00);
            bw.Write(list[i].NameLength);

            bw.Write((uint)0x10);
            bw.Write((uint)(dataAddress - (bw.Position - 0x4))); // offset from 0x10

            bw.Write((uint)fileData.Length);
            bw.Write((uint)0x40);

            if (i == files.Length - 1)
                bw.Write((uint)0xFFFFFFFF);
            else
                bw.Write((uint)0x10);

            bw.Write((uint)(list[i].NameAddress - (bw.Position - 0x4))); // offset from 0x10
        }

        bw.Close();

    }
}
