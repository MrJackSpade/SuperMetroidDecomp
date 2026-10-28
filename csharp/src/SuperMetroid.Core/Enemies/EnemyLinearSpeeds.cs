using System.Buffers.Binary;
using static SuperMetroid.Core.Enemies.EnemyLinearSpeedDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>Reads the shared enemy linear-speed records as the native table presents them, byte window by byte window.</summary>
[EncodingOwner("enemy linear-speed table bytes")]
public static class EnemyLinearSpeeds
{
    /// <summary>Reads a native byte-indexed whole/fraction pair, preserving unaligned byte assembly.</summary>
    public static SignedSixteenSixteen Read(int byteOffset)
    {
        if (byteOffset is < 0 or > (RecordCount * RecordSize - 4))
            throw new InvalidDataException($"Enemy linear-speed byte offset ${byteOffset:X} is outside the authored NTSC records.");
        return SignedSixteenSixteen.FromParts(unchecked((short)ReadWord(byteOffset)), ReadWord(byteOffset + 2));
    }

    /// <summary>Assembles two consecutive native bytes into a little-endian word, including unaligned windows.</summary>
    private static ushort ReadWord(int offset) => BinaryPrimitives.ReadUInt16LittleEndian([ReadByte(offset), ReadByte(offset + 1)]);

    /// <summary>
    /// Calculates a signed 16.16 record byte, selecting the negative half after byte four.
    /// Each record places its high word first while encoding both words little-endian.
    /// </summary>
    private static byte ReadByte(int offset)
    {
        int velocity = offset / RecordSize * FixedPointStep;
        int component = offset % RecordSize;
        if (component >= 4)
            velocity = -velocity;
        int shift = (component & 3) switch { 0 => 16, 1 => 24, 2 => 0, _ => 8 };
        return unchecked((byte)(velocity >> shift));
    }
}
