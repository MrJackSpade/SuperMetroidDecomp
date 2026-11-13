namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Yapping Maw's bend corrects the offsets of its four links (#1620, was <c>YappingMawBehavior</c>'s local
/// "wrapped into next" sum): the eight offsets, link by link and X before Y, are corrected in one chain, and an offset
/// whose correction takes it past the end of the coordinate space, as it does when the offset and the correction both
/// point backward, or one does and their sum does not, moves the next offset in the chain one pixel further. The maw's head stands at its last
/// link's offset, so where the head reaches, and where it grabs Samus, follows the chain.
/// </summary>
/// <remarks>
/// Consulted once per offset on every update the maw extends or retracts, after the bend's corrections are formed and
/// before the head is placed at its last link. Evidence: the extension (<c>$A8:A445</c>) clears the carry once
/// (<c>$A8:A532</c>) before the eight additions of each link's correction to its offset (<c>$A8:A537</c>-<c>$A8:A58B</c>)
/// and never between them; a backward offset or correction is stored as its sixteen-bit complement, so the carry is
/// reached on most bends. The bend chain
/// check compares the chain with that chained addition over every offset and correction class.
/// </remarks>
[CompatibilityService("a Yapping Maw's bend corrects its link offsets in one chain, an offset passing the coordinate space's end moving the next one pixel further")]
internal struct YappingMawBendChain
{
    /// <summary>The coordinates one offset counts through before it continues from the other end.</summary>
    private const int CoordinateCount = 65536;

    /// <summary>Whether the previous offset passed the end of the coordinate space, moving the next one pixel further.</summary>
    private bool previousPassedEnd;

    /// <summary>
    /// The offset <paramref name="offset"/> becomes when moved by <paramref name="correction"/>, and by one pixel more
    /// when the previous offset of the chain passed the end of the coordinate space; records whether this one does.
    /// </summary>
    /// <param name="offset">The link's offset, as its coordinate word, 0 through 65535.</param>
    /// <param name="correction">The bend's correction, as its coordinate word, 0 through 65535.</param>
    /// <exception cref="ArgumentOutOfRangeException">An offset or correction lies outside the coordinate space.</exception>
    internal int Corrected(int offset, int correction)
    {
        int moved = Ensure.BetweenInclusive(0, CoordinateCount - 1, offset)
            + Ensure.BetweenInclusive(0, CoordinateCount - 1, correction)
            + (previousPassedEnd ? 1 : 0);
        previousPassedEnd = moved >= CoordinateCount;
        return CoordinateAxis.Wrap(moved);
    }
}
