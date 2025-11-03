using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ycToolkit;
using ycToolkit.Core;
using static ycToolkit.Mina.PakFormat;

namespace ycToolkit.Mina;

public class AnbFormat
{
    public struct FormatInfo
    {
        uint NodeType; // 0x00
        uint PaletteNameLength;

        uint NodeType2;
        uint OffsetToPaletteName;

        uint Unk;
        uint Unk2;
    }
    public struct Animation
    {
        public long ADDRESS;

        public uint Unk; // 0x00
        public uint NameLength;

        public uint NodeTypeUnk; // 0x10
        public uint OffsetToName;

        public uint Unk2; //maybe count?

        public uint Unk3;
        public uint Unk4;
        public uint Unk5;
        public uint Unk6;

        public long Pad1;
        public long Pad2;

        public uint Unk7;
        public uint Unk8;
    }

    public struct AnimationStructEntry
    {
        public const uint Size = 0x18;
        public uint ID; // idk
        public uint Unk;
        public uint Unk2;
        public uint Unk3;
        public uint Unk4;
        public uint Unk5;
    }
    public struct FrameInfoEntry
    {
        public long ADDRESS = 0;

        public uint Width;
        public uint Height;
        public uint Unk1;
        public uint Unk2;
        public uint wfLZSize;
        public uint Unk3;
        public uint nodeType;
        public uint offsetToFrameData;

        public FrameInfoEntry(ycBinaryReader br)
        {
            ADDRESS = br.Position;

            Width = br.ReadUInt32();
            Height = br.ReadUInt32();
            Unk1 = br.ReadUInt32();
            Unk2 = br.ReadUInt32();
            wfLZSize = br.ReadUInt32();
            Unk3 = br.ReadUInt32();
            nodeType = br.ReadUInt32();
            offsetToFrameData = br.ReadUInt32();
        }
    }

    public static bool Read(string filepath, out AnbFormat? anbFormat)
    {
        anbFormat = null;
        bool result = false;

        using FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using ycBinaryReader br = new ycBinaryReader(fs);

        result = Read(br, out anbFormat);

        br.Close();

        return result;
    }

    public class tempEventArgs(uint width, uint height, long wflzAddr, byte[] rawPixelData) : EventArgs
    {
        public uint Width = width;
        public uint Height = height;
        public byte[] RawPixelData = rawPixelData;
        public long WFLZAddr = wflzAddr;
    }
    public static event EventHandler<tempEventArgs> Createbitmap;

    public string PalettePath = "";


    public struct wfLZBlockData(long address, uint size)
    {
        public long Address = address;
        public uint Size = size;
    }

    public List<wfLZBlockData> WFLZBlocks = [];

    public List<FrameInfoEntry> FrameEntries = new List<FrameInfoEntry>();

    public static bool Read(ycBinaryReader br, out AnbFormat? anbFormat)
    {
        anbFormat = null;

        if (!br.ReadMagic())
            return false;

        anbFormat = new AnbFormat();

        var fileOffset = br.ReadUInt32();

        br.BaseStream.Position = fileOffset;
        br.ReadUInt32(); // Unknown? alwaus 0?
        var palettePathLen = br.ReadUInt32();

        br.ReadUInt32(); // Node type? dunno.
        br.BaseStream.Position += (br.ReadUInt32() - 0x4); // OffsetToPaletteInfo

        anbFormat!.PalettePath = br.ReadString(palettePathLen);

        br.BaseStream.Position = 0;


        // Caster: Okay I don't know the format exactly so we are going to just kinda feel around the binary for wfLZ chunks
        // which are probably image data. probably.
        uint wflzSignature = 0x5A4C4657;

        Console.WriteLine("START ================");
        long lastPos = -1;
        while (true)
        {
            bool found = false;
            while (true)
            {

                if ((br.BaseStream.Position + 4) > br.BaseStream.Length)
                {
                    break;
                }
                if (br.ReadUInt32() == wflzSignature)
                {
                    Console.WriteLine($"FOUND WFLZ @ {(br.BaseStream.Position - 4):X4}");
                    found = true;
                    break;
                }
                else
                    br.BaseStream.Position -= 4;

                br.BaseStream.Position++;
                // read until end of stream or WFLZ found
            }



            if (lastPos == -1)
            {
                lastPos = br.BaseStream.Position;
                continue;
            }

            anbFormat.WFLZBlocks.Add(new(lastPos - 4, (uint)(br.BaseStream.Position - lastPos)));
            var size = br.BaseStream.Position - lastPos;
            //Console.WriteLine($"@{(lastPos - 4):X4} SIZE: {size:X4}");

            lastPos = br.BaseStream.Position;

            if ((br.BaseStream.Position + 4) > br.BaseStream.Length)
                break;
        }
        Console.WriteLine("END ================");
        if (anbFormat.WFLZBlocks.Count == 0)
        {
            Console.WriteLine("wtf no frames??");
            return true;
        }

        anbFormat.FrameEntries = new List<FrameInfoEntry>(anbFormat.WFLZBlocks.Count);

        var frameStartAddr = (anbFormat.WFLZBlocks[0].Address - (anbFormat.WFLZBlocks.Count * 0x20));
        // go to first instance
        br.BaseStream.Position = frameStartAddr;

        for (var i = 0; i < anbFormat.WFLZBlocks.Count; i++)
        {
            anbFormat.FrameEntries.Add(new FrameInfoEntry(br));

            // Console.WriteLine($"{(anbFormat.FrameEntries[anbFormat.FrameEntries.Count - 1].ADDRESS + anbFormat.FrameEntries[anbFormat.FrameEntries.Count - 1].offsetToFrameData):X4}");
        }

        if (br.BaseStream.Position != anbFormat.WFLZBlocks[0].Address)
        {
            Console.Write("FUICK");
        }

        foreach (var entry in anbFormat.FrameEntries)
        {
            //Console.WriteLine($"{entry.ADDRESS}:X4");
        }

        return true;

        for (var i = 0; i < anbFormat.WFLZBlocks.Count; i++)
        {
            var wflzAddr = anbFormat.WFLZBlocks[i].Address;
            var wflzSize = anbFormat.WFLZBlocks[i].Size ;
            var frameInfo = anbFormat.FrameEntries[i];


            // I know this code is terrible
            // just bear with me for a bit <3

            var dir = $"batchDecompression/ycgSplash.anb.yc";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            byte[] workMem = new byte[wfLZ.GetWorkMemSize()];

            for (var j = 0; j < anbFormat.WFLZBlocks.Count; i++)
            {
                br.BaseStream.Position = anbFormat.WFLZBlocks[i].Address;

                byte[] source = br.ReadBytes((int)anbFormat.WFLZBlocks[i].Size);

                byte[] dst = new byte[wfLZ.GetDecompressedSize(source)];

                if (dst.Length == 0)
                {
                    Console.WriteLine($"Invalid wfLZ: {anbFormat.WFLZBlocks[i].Address:X4}");
                    continue;
                }

                Console.Write($"Decompressing: {i}/{anbFormat.WFLZBlocks.Count} - {anbFormat.WFLZBlocks[i].Address:X4} ");
                wfLZ.Decompress(source, dst);


                Console.Write($"\n");

                File.WriteAllBytes($"{dir}/{anbFormat.WFLZBlocks[i].Address:X4}", dst);

                Createbitmap?.Invoke(null, new tempEventArgs(frameInfo.Width, frameInfo.Height, anbFormat.WFLZBlocks[i].Address, dst));


            }

            }


        return true;
    }
}
