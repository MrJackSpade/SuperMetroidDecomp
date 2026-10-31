namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The codec of the segment selector the cartridge keeps in a body segment's direction word (#1473): an even byte
/// offset 0 through 62 into <c>BotwoonsBodyTail_InstListPointers</c> ($86:E9F1). Word index bit 4 selects the tail,
/// bit 3 the hidden half of a region and bits 0-2 the facing octant. The game holds typed segment programs; checks
/// that walk the cartridge's selector table translate through this codec.
/// </summary>
[EncodingOwner("Botwoon segment selector byte offsets")]
internal static class BotwoonSegmentSelectorCodec
{
    /// <summary>The bytes of one selector-table entry.</summary>
    public const int EntryBytes = 2;

    /// <summary>One past the last selector.</summary>
    public const int SelectorEnd = 64;

    /// <summary>The segment program selector <paramref name="byteOffset"/> names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The selector is odd or outside the table.</exception>
    public static BotwoonSegmentProgram Program(int byteOffset)
    {
        if (byteOffset % EntryBytes != 0 || byteOffset is < 0 or >= SelectorEnd)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset), byteOffset, "Botwoon segment selector must be even and zero through 62.");
        }
        int entry = byteOffset / EntryBytes;
        if ((entry & 8) != 0)
            return BotwoonSegmentProgram.Hidden;
        return new((entry & 16) != 0 ? BotwoonSegmentProgramKind.Tail : BotwoonSegmentProgramKind.Body,
            BotwoonAnimations.FacingOfOctant(entry & 7));
    }
}
