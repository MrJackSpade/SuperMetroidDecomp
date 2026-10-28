using System.Buffers.Binary;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Reads where Kraid's ceiling-break rocks fall, by the byte offset its selector holds.</summary>
public static class KraidCeilingRocks
{
    /// <summary>The rock X at byte <paramref name="offset"/> of the position table.</summary>
    /// <remarks>
    /// Independently verified for #1165 against all nineteen original byte windows;
    /// offsets0..18 are accepted, and all other int inputs throw ArgumentOutOfRangeException.
    /// </remarks>
    public static ushort AtByteOffset(int offset)
    {
        if ((uint)offset > KraidCeilingRockPositionDefinitions.LastByteOffset) throw new ArgumentOutOfRangeException(nameof(offset));
        int index = offset >> 1;
        if ((offset & 1) == 0)
            return Word(index);
        // An odd offset reads the little-endian word straddling two table words.
        Span<byte> table = stackalloc byte[2 * sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(table, Word(index));
        BinaryPrimitives.WriteUInt16LittleEndian(table[sizeof(ushort)..], Word(index + 1));
        return BinaryPrimitives.ReadUInt16LittleEndian(table[1..]);
    }

    /// <summary>The table word at <paramref name="index"/>, as the reader's sixteen bits.</summary>
    private static ushort Word(int index) => unchecked((ushort)KraidCeilingRockPositionDefinitions.Words[index]);
}
