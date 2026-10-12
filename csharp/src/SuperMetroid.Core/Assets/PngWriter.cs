using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Minimal RGBA PNG encoder used to keep the reverse-engineering tools dependency-free.
/// Supporting one predictable format also makes generated assets deterministic.
/// </summary>
public static class PngWriter
{

    /// <summary>Writes a PNG chunk in network byte order with CRC over its four-byte type and payload, excluding length.</summary>
    internal static void WriteChunk(Stream output, string name, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        output.Write(length);
        byte[] type = Encoding.ASCII.GetBytes(name);
        output.Write(type);
        output.Write(data);

        uint crc = 0xffffffff;
        foreach (byte value in type) crc = UpdateCrc(crc, value);
        foreach (byte value in data) crc = UpdateCrc(crc, value);
        Span<byte> checksum = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksum, ~crc);
        output.Write(checksum);
    }

    /// <summary>Advances a reflected PNG CRC-32 accumulator through one byte using the standard polynomial.</summary>
    internal static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (int bit = 0; bit < 8; bit++)
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xedb88320 : crc >> 1;
        return crc;
    }
}
