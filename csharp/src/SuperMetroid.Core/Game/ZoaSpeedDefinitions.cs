namespace SuperMetroid.Core.Game;

/// <summary>NTSC Zoa horizontal velocities, retaining the native byte-offset selector.</summary>
public static class ZoaSpeedDefinitions
{
    /// <summary>$A3:B415, ZoaXSpeedTable: five whole/fraction records, including the trailing zero record.</summary>
    /// <remarks>
    /// Retained after independent #1165 review of NTSC J/U v1.0 and pinned bank_A3.asm.
    /// The speeds 0, 1/2, 5/8, 2, 0 are authored shooting-phase settings: neither constant
    /// differences/second differences nor a geometric ratio explains them. A piecewise
    /// formula would simply restate these five choices. Keep both zero records and the
    /// high-word-first/little-endian byte view, including all complete unaligned windows;
    /// do not replace this with an aligned phase switch or smooth an acceleration curve.
    /// </remarks>
    private static ReadOnlySpan<ushort> Words => [0, 0, 0, 0x8000, 0, 0xa000, 2, 0, 0, 0];

    /// <summary>
    /// Reads the native pair of potentially unaligned words and combines them as 16.16.
    /// Instructions normally select byte offsets 4, 8 and 12; initialization selects zero.
    /// All complete four-byte windows in the definition remain representable.
    /// </summary>
    public static int Displacement(ushort byteOffset)
    {
        if (byteOffset > 16)
            throw new ArgumentOutOfRangeException(nameof(byteOffset));
        ushort Word(int offset) => (ushort)(Byte(offset) | Byte(offset + 1) << 8);
        return unchecked((Word(byteOffset) << 16) | Word(byteOffset + 2));
    }

    private static byte Byte(int offset) => (byte)(Words[offset >> 1] >> ((offset & 1) * 8));
}
