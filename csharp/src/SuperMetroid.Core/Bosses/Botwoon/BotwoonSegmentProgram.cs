namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A body segment's animation: what it shows and which way it faces.</summary>
internal readonly record struct BotwoonSegmentProgram(BotwoonSegmentProgramKind Kind, BotwoonFacing Facing)
{
    /// <summary>The hidden segment. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonSegmentProgram Hidden => new(BotwoonSegmentProgramKind.Hidden, BotwoonFacing.Up);

    /// <summary>The spit's program. Facing is irrelevant and fixed so equal programs compare equal.</summary>
    internal static BotwoonSegmentProgram Spit => new(BotwoonSegmentProgramKind.Spit, BotwoonFacing.Up);

    /// <summary>
    /// Decodes the selector the AI keeps in the segment's direction word: an even byte offset 0 through
    /// 62 into <c>BotwoonsBodyTail_InstListPointers</c> ($86:E9F1). Word index bit 4 selects the tail,
    /// bit 3 the hidden half of a region, and bits 0-2 the facing octant.
    /// </summary>
    internal static BotwoonSegmentProgram FromSelector(int byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset is < 0 or >= 64)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset), byteOffset, "Botwoon segment selector must be even and zero through 62.");
        }
        int entry = byteOffset >> 1;
        if ((entry & 8) != 0)
            return Hidden;
        return new((entry & 16) != 0 ? BotwoonSegmentProgramKind.Tail : BotwoonSegmentProgramKind.Body,
            BotwoonAnimations.FacingOfOctant(entry & 7));
    }
}
