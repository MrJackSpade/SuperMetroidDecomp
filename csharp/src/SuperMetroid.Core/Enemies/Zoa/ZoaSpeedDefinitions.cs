namespace SuperMetroid.Core.Enemies.Zoa;

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
    public static int Displacement(int byteOffset)
    {
        if (byteOffset is < 0 or > 16)
            throw new ArgumentOutOfRangeException(nameof(byteOffset));
        ushort Word(int offset) => (ushort)(SnesLittleEndian.Word(Byte(offset), Byte(offset + 1)));
        return unchecked(unchecked((int)SnesSixteenSixteen.Compose(Word(byteOffset), Word(byteOffset + 2))));
    }

    /// <summary>Reads the NTSC speed table in native whole/fraction word order, including odd bytes and windows crossing records.</summary>
    /// <remarks>Initialization and the trailing record are stationary; the middle records are the shooting stages.</remarks>
    private static byte Byte(int offset)
    {
        uint speed = RecordSpeeds[offset / 4];
        uint nativeWords = (SnesSixteenSixteen.WholeOf(speed)) | (SnesSixteenSixteen.FromWhole(speed));
        return (byte)(nativeWords >> (8 * (offset & 3)));
    }
}
