using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ycToolkit;

// old from sk 
public class skBinaryWriter : BinaryWriter
{
    public skBinaryWriter(Stream output) : base(output)
    {

    }

    public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
    public long Length { get => BaseStream.Length; }
    public bool EndOfStream => BaseStream.Length <= BaseStream.Position;
    public void Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

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
