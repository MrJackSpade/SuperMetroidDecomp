using System.Buffers.Binary;

namespace SuperMetroid.Core;

/// <summary>
/// The sixteen-bit word two consecutive bytes hold, low byte first: the layout of the recorded byte tables and byte-layout
/// blocks whose words the game reads across entry boundaries, one byte before or after an entry's own word.
/// </summary>
[EncodingOwner("little-endian sixteen-bit word of two bytes")]
internal static class LittleEndianWord
{
    /// <summary>The bits of the low byte, below the high byte.</summary>
    private const int ByteBits = 8;

    /// <summary>The word whose low byte is <paramref name="low"/> and high byte is <paramref name="high"/>, 0 through 65535.</summary>
    /// <param name="low">The first byte, 0 through 255; other values are rejected.</param>
    /// <param name="high">The second byte, 0 through 255; other values are rejected.</param>
    internal static int Unsigned(int low, int high) => RequireByte(high) << ByteBits | RequireByte(low);

    /// <summary>The word whose low byte is <paramref name="low"/> and high byte is <paramref name="high"/>, read as signed, -32768 through 32767.</summary>
    /// <param name="low">The first byte, 0 through 255; other values are rejected.</param>
    /// <param name="high">The second byte, 0 through 255; other values are rejected.</param>
    internal static int Signed(int low, int high) => unchecked((short)Unsigned(low, high));

    /// <summary>Writes <paramref name="word"/> into the first two bytes of <paramref name="destination"/>, low byte first.</summary>
    /// <param name="destination">The bytes the word occupies.</param>
    /// <param name="word">The word, 0 through 65535; other values are rejected.</param>
    internal static void Write(Span<byte> destination, int word) =>
        BinaryPrimitives.WriteUInt16LittleEndian(destination, checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, word)));

    /// <summary>A byte held as an int, checked to lie in 0 through 255.</summary>
    private static int RequireByte(int value) => Ensure.BetweenInclusive(0, byte.MaxValue, value);
}
