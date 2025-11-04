using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ycToolkit;

// old (shovel knight not migrated yet)
public class PakFormat
{
    public class FileInfo
    {
        public FileInfo()
        {
            Length = 0;
            Unk1 = 0;
            NameHash = 0;
            Unk2 = 0;
            Unk3 = 0;
            Data = [];
        }
        public FileInfo(BinaryReader br)
        {
            Length = br.ReadUInt64();
            Unk1 = br.ReadUInt64();
            NameHash = br.ReadUInt32();
            Unk2 = br.ReadUInt32();
            Unk3 = br.ReadUInt64();
            Data = br.ReadBytes((int)(Length));
            Console.WriteLine($"Unk1:{Unk1:X8} Unk2:{Unk2:X8} Unk3:{Unk3:X8}");
        }
        public ulong Length;
        public ulong Unk1;
        public uint NameHash;
        public uint Unk2;
        public ulong Unk3;
        public byte[] Data;
    }

    uint unk1;
    uint FileCount;
    long FileOffsetTable;
    long FileNameOffsetTable;


    public Dictionary<string, FileInfo> Files = [];

    public void Read(ycBinaryReader br)
    {
        unk1 = br.ReadUInt32();
        FileCount = br.ReadUInt32();
        FileOffsetTable = br.ReadInt64();
        FileNameOffsetTable = br.ReadInt64();

        Console.WriteLine($"Found: {FileCount} files in pak");
        long[] fileOffsets = new long[FileCount];


        br.Seek(FileOffsetTable, SeekOrigin.Begin);

        for (var i = 0; i < FileCount; i++)
            fileOffsets[i] = br.ReadInt64();

        if (br.Position != fileOffsets[0])
            Console.WriteLine($"Expected Pos: {fileOffsets[0]:X8} but got {br.BaseStream.Position:X8}");


        br.Seek(FileNameOffsetTable, SeekOrigin.Begin);

        long[] fileNameOffsets = new long[FileCount];
        for (var i = 0; i < FileCount; i++)
            fileNameOffsets[i] = br.ReadInt64();


        string[] names = new string[FileCount];
        for (var i = 0; i < FileCount; i++)
        {
            if (br.Position != fileNameOffsets[i])
                Console.WriteLine($"Expected Pos: {fileNameOffsets[i]:X8} but got {br.BaseStream.Position:X8}");

            br.Seek(fileNameOffsets[i], SeekOrigin.Begin);
            names[i] = br.ReadString();

            Console.WriteLine($"Found file: {names[i]} @ {fileOffsets[i]:X8}");
        }


        // Read files

        for (var i = 0; i < FileCount; i++)
        {
            br.Seek(fileOffsets[i], SeekOrigin.Begin);

            Console.WriteLine($"Parsing: {names[i]}");
            Files.Add(names[i], new FileInfo(br));
        }
    }




    public static void CompileDirectory(string outputName, string path)
    {
        var entries = Directory.GetFiles(path, "*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal).ToArray();

        int pathToRemove = path.Length;
        if (!(path.EndsWith('\\') || path.EndsWith('/')))
            pathToRemove++;

        for (var i = 0; i < entries.Length; i++)
            entries[i] = entries[i].Replace('\\', '/');


        using FileStream fs = new FileStream(outputName, FileMode.OpenOrCreate, FileAccess.Write);
        using ycBinaryWriter bw = new ycBinaryWriter(fs);

        long fileInfoJumpTableAddress = 0;
        long fileNameJumpTableAddress = 0;

        WriteHeader();

        WriteFileDataChunk();
        WriteFilenameChunk();

        WriteHeader(); // Second pass

        bw.Close();
        fs.Close();

        return;

        void WriteHeader()
        {
            bw.Position = 0;
            bw.Write(0x00); // Always 0??
            bw.Write(entries.Length);
            bw.Write(fileInfoJumpTableAddress);
            bw.Write(fileNameJumpTableAddress);
        }

        void WriteFileDataChunk()
        {
            fileInfoJumpTableAddress = bw.Position;

            // Fill empty table
            bw.Position += (entries.Length * 8);

            long[] addresses = new long[entries.Length];

            for (var i = 0; i < entries.Length; i++)
            {
                addresses[i] = bw.Position;

                byte[] fileData = File.ReadAllBytes(entries[i]);

                var entryName = entries[i].Substring(pathToRemove);
                var hash = Hash.CreateHash(entryName);

                bw.Write((long)fileData.Length);
                bw.Write(0L); // Unk1
                bw.Write(Hash.CreateHash(entries[i].Substring(pathToRemove)));
                bw.Write(1u); // Unk2
                bw.Write(1L); // Unk3
                bw.Write(fileData);
            }

            // Fill table with actual addresses
            long jumpbackAddress = bw.Position;
            bw.Position = fileInfoJumpTableAddress;
            for (var i = 0; i < entries.Length; i++)
                bw.Write(addresses[i]);

            bw.Position = jumpbackAddress;
        }

        void WriteFilenameChunk()
        {
            fileNameJumpTableAddress = bw.Position;

            long[] nameAddresses = new long[entries.Length];

            // Make blank table
            bw.Position = fileNameJumpTableAddress + entries.Length * sizeof(long);

            for (var i = 0; i < entries.Length; i++)
            {
                nameAddresses[i] = bw.Position;
                bw.WriteAlignedString(entries[i].Substring(pathToRemove));
            }

            // Fill table with actual addresses
            bw.Position = fileNameJumpTableAddress;
            for (var i = 0; i < entries.Length; i++)
                bw.Write(nameAddresses[i]);



        }
    }
}
