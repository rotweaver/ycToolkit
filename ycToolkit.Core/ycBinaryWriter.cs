using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ycToolkit;

// old from sk 
public class ycBinaryWriter : BinaryWriter
{
    public ycBinaryWriter(Stream output) : base(output)
    {

    }

    public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
    public long Length { get => BaseStream.Length; }
    public bool EndOfStream => BaseStream.Length <= BaseStream.Position;
    public void Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

    public void WriteNullTerminatedString(string value)
    {
        for (var i = 0; i < value.Length; i++)
            Write(value[i]);

        Write('\0');
    }

    public void WriteAlignedString(string value)
    {
        long startPos = Position;

        for (var i = 0; i < value.Length; i++)
            Write(value[i]);

        long targetPos = (startPos + ((value.Length + 8) & ~0b111));
        while(Position != targetPos)
            Write('\0');
    }
}
