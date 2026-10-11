namespace SuperMetroid.Core.Game;

/// <summary>NTSC Zoa horizontal velocities, retaining the native byte-offset selector.</summary>
public static class ZoaSpeedDefinitions
{

    /// <summary>
    /// The five four-byte 16.16 speed records reachable by a complete window of the definition,
    /// indexed by byte offset / 4: initialization's stationary record; the $A3:B429, $A3:B434 and
    /// $A3:B43F shooting stages (half, five eighths and two pixels per frame); and the trailing
    /// stationary record.
    /// </summary>
    private static readonly uint[] RecordSpeeds = [0, 1u << 15, 5u << 13, 2u << 16, 0];

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
        uint speed = RecordSpeeds[offset / 4];
        uint nativeWords = (speed >> 16) | (speed << 16);
        return (byte)(nativeWords >> (8 * (offset & 3)));
    }
}
