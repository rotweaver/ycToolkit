using System;
using System.Buffers.Binary;
using System.Text;

namespace ycToolkit;

/// <summary>
/// Reads primitive data in formats commonly found in Yacht Club Games' binary files
/// </summary>
public class ycBinaryReader : BinaryReader
{
    public ycBinaryReader(Stream stream) : base(stream)
    {

    }


    public long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
    public long Length { get => BaseStream.Length; }

    public void Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

    public bool EndOfStream => BaseStream.Length <= BaseStream.Position;

    /// <summary>
    /// Reads a null-terminated string aligned to an 8 byte boundary relative to current position
    /// </summary>
    /// <returns></returns>
    public new string ReadString()
    {
        StringBuilder builder = new();

        var startPos = BaseStream.Position;

        while (true)
        {
            var @char = ReadChar();

            if (@char == '\0')
                break;

            builder.Append(@char);
        }

        // Go to next greater multiple of 8
        Seek(startPos + ((builder.Length + 8) & ~0b111), SeekOrigin.Begin);

        return builder.ToString();
    }


    public string ReadString(uint length)
    {
        StringBuilder builder = new();

        var startPos = BaseStream.Position;


        for (var i = 0; i < length; i++)
            builder.Append(ReadChar());

        Position++;


        return builder.ToString();
    }
}
