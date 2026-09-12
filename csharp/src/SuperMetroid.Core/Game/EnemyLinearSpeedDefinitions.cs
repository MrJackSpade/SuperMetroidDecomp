namespace SuperMetroid.Core.Game;

/// <summary>Compiled NTSC fixed-point speeds shared by enemy families.</summary>
public static class EnemyLinearSpeedDefinitions
{
    /// <summary>$A0:8187, CommonEnemySpeeds_LinearlyIncreasing; duplicated in the common enemy-bank prefix.</summary>
    public const int ReferenceAddress = 0xa08187;

    /// <summary>NTSC authored records zero through $40, inclusive; PAL uses different values/count.</summary>
    public const int RecordCount = 65;

    /// <summary>Each record stores positive whole/fraction followed by negative whole/fraction.</summary>
    public const int RecordSize = 8;

    /// <summary>Exact NTSC step $0000.1000, one sixteenth of a pixel per frame.</summary>
    private const int FixedPointStep = 0x1000;

    /// <summary>Reads a native byte-indexed whole/fraction pair, preserving unaligned byte assembly.</summary>
    public static (short Whole, ushort Fraction) Read(int byteOffset)
    {
        if (byteOffset < 0 || byteOffset > RecordCount * RecordSize - 4)
            throw new InvalidDataException($"Enemy linear-speed byte offset ${byteOffset:X} is outside the authored NTSC records.");
        return (unchecked((short)ReadWord(byteOffset)), ReadWord(byteOffset + 2));
    }

    private static ushort ReadWord(int offset) => (ushort)(ReadByte(offset) | ReadByte(offset + 1) << 8);

    private static byte ReadByte(int offset)
    {
        int velocity = offset / RecordSize * FixedPointStep;
        int component = offset % RecordSize;
        if (component >= 4)
            velocity = -velocity;
        // The record stores the high word first, but each word is little endian.
        int shift = (component & 3) switch { 0 => 16, 1 => 24, 2 => 0, _ => 8 };
        return unchecked((byte)(velocity >> shift));
    }
}
