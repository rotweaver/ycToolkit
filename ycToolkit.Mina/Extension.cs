using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ycToolkit;

namespace ycToolkit.Mina;

public static class Extension
{
    public static char[] HeaderMagic = { 'Y', 'C', 'D', '\0', (char)0x08, '\0', '\0' , '\0' };
    public static bool ReadMagic(this ycBinaryReader br)
    {
        return br.ReadChars(HeaderMagic.Length).SequenceEqual(HeaderMagic);
    }
}
