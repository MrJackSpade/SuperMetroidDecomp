namespace SuperMetroid.Core.Game;

/// <summary>Compiled ceiling-break projectile placement data.</summary>
public static class KraidCeilingRockPositions
{
    /// <summary>$A7:ACC5 first callback word, .spawnPLM4 at $AD03, adjacent to the nine X words.</summary>
    private const ushort FollowingCallback = 0xad03;

    /// <summary>
    /// $A7:ACB3 nine X words, followed by ACC5's AD03 callback pointer.
    /// Native authored selectors are even byte offsets. Retaining the following
    /// word also preserves the existing reader's overlapping odd-offset result;
    /// this does not define native execution of a malformed callback pointer.
    /// </summary>
    /// <remarks>
    /// Alternate gaps at block columns2..6 and10..13, taking each gap's right
    /// then left remaining edge inward. Convert to rock centers with16*column+8.
    /// Independently verified for #1165 against all nineteen original byte windows;
    /// offsets0..18 are accepted, and all other int inputs throw IndexOutOfRangeException.
    /// </remarks>
    public static ushort AtByteOffset(int offset)
    {
        if ((uint)offset > 18) throw new IndexOutOfRangeException();
        int index = offset >> 1;
        return (offset & 1) == 0 ? Word(index)
            : unchecked((ushort)((Word(index) >> 8) | (Word(index + 1) << 8)));
    }

    // Alternate the left (columns2..6) and right (10..13) ceiling gaps.
    // Within each gap, take rightmost then leftmost remaining blocks inward.
    // Native rocks originate at the selected block center, eight pixels in.
    /// <summary>Resolves a packed table slot to its rock-center X coordinate or adjacent callback word.</summary>
    /// <param name="index">Zero-based word slot in the compiled placement data; slot nine is the following callback.</param>
    /// <returns>The selected rock's horizontal center in pixels, or the retained callback pointer for slot nine.</returns>
    private static ushort Word(int index)
    {
        if (index == 9) return FollowingCallback;
        bool rightGap = (index & 1) != 0;
        int withinGap = index >> 1;
        int inset = withinGap >> 1;
        int column = (withinGap & 1) == 0
            ? (rightGap ? 13 : 6) - inset
            : (rightGap ? 10 : 2) + inset;
        return (ushort)(column * 16 + 8);
    }
}
