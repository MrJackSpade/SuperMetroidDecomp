using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Where Kraid's grown body walks between attacks and how long he thinks after each walk (#2300): from each of his
/// destinations, the walk each random choice sends him on and the pause that follows it.
/// </summary>
internal static class KraidMovementDefinitions
{
    /// <summary>The leftmost destination.</summary>
    private const int LeftmostX = 208;

    /// <summary>The left destination.</summary>
    private const int LeftX = 240;

    /// <summary>The middle destination.</summary>
    private const int MiddleX = 320;

    /// <summary>The destination he walks to first once grown, and the one he chooses from wherever he stands off his destinations.</summary>
    internal const int StartingX = 352;

    /// <summary>The right destination.</summary>
    private const int RightX = 368;

    /// <summary>The rightmost destination.</summary>
    private const int RightmostX = 384;

    /// <summary>The long pause after a walk, in updates.</summary>
    private const int LongPause = 344;

    /// <summary>The medium pause after a walk, in updates.</summary>
    private const int MediumPause = 256;

    /// <summary>The short pause after a walk, in updates.</summary>
    private const int ShortPause = 44;

    /// <summary>
    /// The walks from the starting destination, by random choice; the last also serves every choice past it, so it is taken
    /// half the time.
    /// </summary>
    internal static readonly KraidMovementRow Starting = new(StartingX,
        [new(LeftX, MediumPause), new(RightmostX, LongPause), new(LeftmostX, ShortPause), new(MiddleX, ShortPause), new(RightX, ShortPause)]);

    /// <summary>Each destination's walks, by random choice, the last serving every choice past it, in the order he looks them up.</summary>
    internal static readonly ImmutableArray<KraidMovementRow> Rows =
    [
        new(LeftmostX, [new(LeftX, MediumPause), new(StartingX, LongPause), new(RightmostX, LongPause), new(MiddleX, ShortPause), new(RightX, ShortPause)]),
        new(LeftX, [new(RightmostX, LongPause), new(RightmostX, LongPause), new(LeftmostX, ShortPause), new(RightX, ShortPause), new(RightX, ShortPause)]),
        new(MiddleX, [new(LeftX, MediumPause), new(StartingX, LongPause), new(RightmostX, LongPause), new(LeftmostX, ShortPause), new(RightX, ShortPause)]),
        Starting,
        new(RightX, [new(LeftX, MediumPause), new(RightmostX, LongPause), new(RightX, LongPause), new(LeftmostX, ShortPause), new(MiddleX, ShortPause)]),
        new(RightmostX, [new(LeftX, MediumPause), new(StartingX, LongPause), new(LeftmostX, ShortPause), new(MiddleX, ShortPause), new(RightX, ShortPause)]),
    ];
}
