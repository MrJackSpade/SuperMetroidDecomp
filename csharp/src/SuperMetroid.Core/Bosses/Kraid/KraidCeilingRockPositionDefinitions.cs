namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled ceiling-break projectile placement data.</summary>
public static class KraidCeilingRockPositionDefinitions
{
    /// <summary>
    /// The position the reader finds past the nine rock positions: the following callback word, read as an X coordinate
    /// far off the room's left (#1413). Kept because an odd offset reads its low byte.
    /// </summary>
    private const short FollowingWordX = -21245;

    /// <summary>
    /// $A7:ACB3 nine X words, followed by the callback word read as <see cref="FollowingWordX"/>.
    /// Native authored selectors are even byte offsets. Retaining the following
    /// word also preserves the existing reader's overlapping odd-offset result;
    /// this does not define native execution of a malformed callback pointer.
    /// </summary>
    /// <remarks>
    /// Alternate gaps at block columns2..6 and10..13, taking each gap's right
    /// then left remaining edge inward. Convert to rock centers with16*column+8.
    /// Independently verified for #1165 against all nineteen original byte windows;
    /// offsets0..18 are accepted, and all other int inputs throw ArgumentOutOfRangeException.
    /// </remarks>
    public static ushort AtByteOffset(int offset)
    {
        if ((uint)offset > 18) throw new ArgumentOutOfRangeException(nameof(offset));
        int index = offset >> 1;
        return (offset & 1) == 0 ? Word(index)
            : SnesLittleEndian.Word(SnesLittleEndian.HighByte(Word(index)), unchecked((byte)Word(index + 1)));
    }

    /// <summary>Alternates left and right ceiling gaps, taking outer edges inward and placing each rock at its block center.</summary>
    /// <remarks>Index nine deliberately returns the following callback word for the reader's overlapping odd-offset result.</remarks>
    private static ushort Word(int index)
    {
        if (index == 9) return unchecked((ushort)FollowingWordX);
        bool rightGap = (index & 1) != 0;
        int withinGap = index >> 1;
        int inset = withinGap >> 1;
        int column = (withinGap & 1) == 0
            ? (rightGap ? 13 : 6) - inset
            : (rightGap ? 10 : 2) + inset;
        return (ushort)(column * 16 + 8);
    }
}
