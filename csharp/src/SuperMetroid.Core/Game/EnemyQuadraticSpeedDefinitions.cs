namespace SuperMetroid.Core.Game;

/// <summary>Exact NTSC shared enemy quadratic-speed records, including their authored truncation.</summary>
public static class EnemyQuadraticSpeedDefinitions
{
    /// <summary>$A0:838F, CommonEnemySpeeds_QuadraticallyIncreasing, fraction/whole pairs of both signs.</summary>
    public const int ReferenceAddress = 0xa0838f;

    /// <summary>$A0:CBC7, CommonEnemyProjectileSpeeds_QuadraticallyIncreasing, the identical NTSC projectile copy.</summary>
    public const int ProjectileReferenceAddress = 0xa0cbc7;

    /// <summary>The pinned NTSC table contains records zero through 94.</summary>
    public const int RecordCount = 95;

    /// <summary>Each record stores positive fraction/whole followed by negative fraction/whole.</summary>
    public const int RecordSize = 8;

    /// <summary>Native fractional multiplier $0109 applied only to the triangle number's low byte.</summary>
    private const int FractionMultiplier = 0x0109;

    /// <summary>Reads a native word, including odd-byte windows used by initial hop estimates.</summary>
    public static ushort ReadWord(int byteOffset)
    {
        if (byteOffset < 0 || byteOffset > RecordCount * RecordSize - 2)
            throw new InvalidDataException($"Enemy quadratic-speed word offset ${byteOffset:X} is outside the authored NTSC records.");
        return (ushort)(ReadByte(byteOffset) | ReadByte(byteOffset + 1) << 8);
    }

    /// <summary>Reads a fraction-first signed 16.16 displacement without rounding or aligned-index coercion.</summary>
    public static int ReadDisplacement(int byteOffset)
    {
        ushort fraction = ReadWord(byteOffset);
        short whole = unchecked((short)ReadWord(byteOffset + 2));
        return (whole << 16) | fraction;
    }

    private static byte ReadByte(int offset)
    {
        int record = offset / RecordSize;
        int triangle = record * (record + 1) / 2;
        // The cartridge's table generator lost the carry from the fractional
        // multiplication. Preserve that discontinuity rather than smoothing gravity.
        int velocity = (triangle >> 8 << 16) | ((triangle & 255) * FractionMultiplier & 65535);
        int component = offset % RecordSize;
        if (component >= 4)
            velocity = -velocity;
        return unchecked((byte)(velocity >> ((component & 3) * 8)));
    }
}
