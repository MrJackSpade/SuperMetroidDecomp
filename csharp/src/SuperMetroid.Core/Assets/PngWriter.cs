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

    /// <summary>Writes a length/type/data/CRC PNG chunk.</summary>
    internal static void WriteChunk(Stream output, string name, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)data.Length);
        output.Write(length);
        byte[] type = Encoding.ASCII.GetBytes(name);
        output.Write(type);
        output.Write(data);

        // The CRC covers the four-byte type and payload, but not the length field.
        uint crc = 0xffffffff;
        foreach (byte value in type) crc = UpdateCrc(crc, value);
        foreach (byte value in data) crc = UpdateCrc(crc, value);
        Span<byte> checksum = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(checksum, ~crc);
        output.Write(checksum);
    }

    /// <summary>Advances PNG's reflected CRC-32 accumulator by one input byte using polynomial <c>0xedb88320</c>.</summary>
    /// <param name="crc">Unfinalized CRC accumulator before this byte is processed.</param>
    /// <param name="value">Next byte from the chunk type or payload.</param>
    /// <returns>The updated, still-unfinalized CRC accumulator.</returns>
    internal static uint UpdateCrc(uint crc, byte value)
    {
        crc ^= value;
        for (int bit = 0; bit < 8; bit++)
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xedb88320 : crc >> 1;
        return crc;
    }
}
