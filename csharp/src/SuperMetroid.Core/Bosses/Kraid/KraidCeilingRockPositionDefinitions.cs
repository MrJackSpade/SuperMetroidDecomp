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
    /// $A7:ACB3 nine X words, followed by the callback word read as <see cref="FollowingWordX"/>. The rocks alternate
    /// between the gaps at block columns 2..6 and 10..13, taking each gap's right then left remaining edge inward, at
    /// each block's center (16 * column + 8). Native authored selectors are even byte offsets; retaining the following
    /// word also preserves the existing reader's overlapping odd-offset result. This does not define native execution
    /// of a malformed callback pointer.
    /// </summary>
    public static readonly short[] Words = [104, 216, 40, 168, 88, 200, 56, 184, 72, FollowingWordX];

    /// <summary>The largest byte offset the reader accepts: the odd offset straddling the last rock and the following word.</summary>
    public const int LastByteOffset = 18;
}
