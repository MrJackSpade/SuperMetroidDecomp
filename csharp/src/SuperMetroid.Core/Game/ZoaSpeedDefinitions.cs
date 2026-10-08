namespace SuperMetroid.Core.Game;

/// <summary>NTSC Zoa horizontal velocities, retaining the native byte-offset selector.</summary>
public static class ZoaSpeedDefinitions
{

    /// <summary>$A3:B429 selects byte offset 4: first shooting stage, half a pixel per frame.</summary>
    private const int FirstShootingRecord = 1;
    /// <summary>$A3:B434 selects byte offset 8: second shooting stage, five eighths of a pixel per frame.</summary>
    private const int SecondShootingRecord = 2;
    /// <summary>$A3:B43F selects byte offset 12: third shooting stage, two pixels per frame.</summary>
    private const int ThirdShootingRecord = 3;

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

    // NTSC stage selection; initialization and the trailing record are stationary.
    // Compose the native whole-word/fraction-word byte layout before taking a
    // byte, so odd offsets and windows crossing record boundaries remain exact.
    private static byte Byte(int offset)
    {
        uint speed = (offset / 4) switch
        {
            FirstShootingRecord => 1u << 15,
            SecondShootingRecord => 5u << 13,
            ThirdShootingRecord => 2u << 16,
            _ => 0,
        };
        uint nativeWords = (speed >> 16) | (speed << 16);
        return (byte)(nativeWords >> (8 * (offset & 3)));
    }
}
