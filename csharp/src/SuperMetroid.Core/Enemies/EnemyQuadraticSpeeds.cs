using System.Buffers.Binary;
using static SuperMetroid.Core.Enemies.EnemyQuadraticSpeedDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>Reads the shared enemy quadratic-speed records as the native table presents them, byte window by byte window.</summary>
[EncodingOwner("enemy quadratic-speed table bytes")]
public static class EnemyQuadraticSpeeds
{
    /// <summary>Reads a native word, including odd-byte windows used by initial hop estimates.</summary>
    public static ushort ReadWord(int byteOffset)
    {
        if (byteOffset is < 0 or > (RecordCount * RecordSize - 2))
            throw new InvalidDataException($"Enemy quadratic-speed word offset ${byteOffset:X} is outside the authored NTSC records.");
        return BinaryPrimitives.ReadUInt16LittleEndian([ReadByte(byteOffset), ReadByte(byteOffset + 1)]);
    }

    /// <summary>Reads a fraction-first signed 16.16 displacement without rounding or aligned-index coercion.</summary>
    public static int ReadDisplacement(int byteOffset)
    {
        int fraction = ReadWord(byteOffset);
        int whole = unchecked((short)ReadWord(byteOffset + 2));
        return (SixteenSixteen.FromWhole(whole)) | fraction;
    }

    /// <summary>Reconstructs a byte of the shared positive/negative 16.16 record, preserving the native discarded fractional carry.</summary>
    private static byte ReadByte(int offset)
    {
        int record = offset / RecordSize;
        int triangle = record * (record + 1) / 2;
        // The original values match discarded fractional carry. Preserve that
        // observed discontinuity without claiming provenance for its generator.
        int velocity = (triangle >> 8 << 16) | ((triangle & 255) * FractionMultiplier & 65535);
        int component = offset % RecordSize;
        if (component >= 4)
            velocity = -velocity;
        return unchecked((byte)(velocity >> ((component & 3) * 8)));
    }
}
