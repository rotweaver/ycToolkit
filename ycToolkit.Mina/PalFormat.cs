using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ycToolkit;
using static ycToolkit.Mina.PakFormat;

namespace ycToolkit.Mina;

public class PalFormat
{
    public static bool Read(string filepath, out PalFormat? palFormat)
    {
        palFormat = null;
        bool result = false;

        using FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        using ycBinaryReader br = new ycBinaryReader(fs);

        result = Read(br, out palFormat);

        br.Close();

        return result;
    }
    public static bool Read(ycBinaryReader br, out PalFormat? palFormat)
    {
        palFormat = null;

        if (!br.ReadMagic())
            return false;

        palFormat = new PalFormat();

        var dataAddr = br.ReadInt64();


        br.BaseStream.Position = dataAddr;

        br.ReadUInt32(); // No fucking clue
        var palLength = br.ReadUInt32();
        br.ReadUInt32(); // no idea, always F0 0F 00 00
        br.ReadUInt32(); // no idea, always FC 03 00 00  (maybe length related?? cuase FF * 4 = 3FC)

        br.ReadUInt32(); // Node type, should alwayus be 0x10

        var palDataAddress = br.ReadUInt32() + (br.BaseStream.Position - 0x8);

        br.BaseStream.Position = palDataAddress;

        for (var i = 0; i < 0xFF; i++)
        {
            var r = br.ReadByte();
            var g = br.ReadByte();
            var b = br.ReadByte();
            var a = br.ReadByte();

            palFormat!.Colors[i] = Color.FromArgb(a, r, g, b);
        }

        return true;
    }


    public Color[] Colors = new Color[0xFF];
}
