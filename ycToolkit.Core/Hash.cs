using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ycToolkit;

// shovel knight hashing algorithm
public static class Hash
{
    public static uint ROL4(uint n, int d) => ((n << d) | (n >> (32 - d)));
    public static uint ROR4(uint n, int d) => ((n >> d) | (n << (32 - d)));

    public static uint CreateHash(string text, int initialHash = 123456789)
    {
        var stringBytes = Encoding.UTF8.GetBytes(text);

        int index = 0;

        uint hashA = (uint)(text.Length + initialHash + 0xDEADBEEF);
        uint hashB = (uint)(text.Length + initialHash + 0xDEADBEEF);
        uint hashC = (uint)(text.Length + initialHash + 0xDEADBEEF);

        uint a, a1, b, b1, c, c1, d, d1, e, f;

        uint length = (uint)text.Length;

        while (length > 12)
        {
            uint sHashA = hashA;
            uint sHashB = hashB;
            uint sHashC = hashC;

            Update(ref sHashC);
            Update(ref sHashB);
            Update(ref sHashA);


            a = (sHashC - sHashA) ^ ROL4(sHashA, 4);
            a1 = sHashB + sHashA;
            b = (sHashB - a) ^ ROL4(a, 6);
            b1 = a1 + a;
            c = (a1 - b) ^ ROL4(b, 8);
            c1 = b1 + b;
            d = ((b1 - c)) ^ ROL4(c, 16);
            d1 = c1 + c;
            e = (c1 - d) ^ ROR4(d, 13);
            hashC = d1 + d;
            hashA = (d1 - e) ^ ROL4(e, 4);
            hashB = hashC + e;
        }

        if (length > 0)
        {
            Update(ref hashC);
            Update(ref hashB);
            Update(ref hashA);
        }

        a = (hashB ^ hashA) - ROL4(hashB, 14);
        b = (hashC ^ a) - ROL4(a, 11);
        c = (b ^ hashB) - ROR4(b, 7);
        d = (c ^ a) - ROL4(c, 16);
        e = (((b ^ d) - ROL4(d, 4)) ^ c) - ROL4((b ^ d) - ROL4(d, 4), 14);
        f = (e ^ d) - ROR4(e, 8);

        return f;

        void Update(ref uint value)
        {
            for (var i = 0; i < 4 && length > 0; i++)
            {
                value += (uint)stringBytes[index++] << ((i & 3) << 3);
                length--;
            }
        }

    }

}
