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
    /// The walks from the starting destination, by random choice: each of the first four takes one value of
    /// <see cref="KraidRandomDefinitions.MovementChoice"/> and the last the other four, so it is taken half the time.
    /// </summary>
    internal static readonly KraidMovementRow Starting = new(StartingX, new(
        KraidRandomDefinitions.MovementChoice,
        new(1, new(LeftX, MediumPause)),
        new(1, new(RightmostX, LongPause)),
        new(1, new(LeftmostX, ShortPause)),
        new(1, new(MiddleX, ShortPause)),
        new(4, new(RightX, ShortPause))));

    /// <summary>
    /// Each destination's walks, by random choice, in the order he looks them up: each of the first four takes one value of
    /// <see cref="KraidRandomDefinitions.MovementChoice"/> and the last the other four.
    /// </summary>
    internal static readonly ImmutableArray<KraidMovementRow> Rows =
    [
        new(LeftmostX, new(
            KraidRandomDefinitions.MovementChoice,
            new(1, new(LeftX, MediumPause)),
            new(1, new(StartingX, LongPause)),
            new(1, new(RightmostX, LongPause)),
            new(1, new(MiddleX, ShortPause)),
            new(4, new(RightX, ShortPause)))),
        new(LeftX, new(
            KraidRandomDefinitions.MovementChoice,
            new(1, new(RightmostX, LongPause)),
            new(1, new(RightmostX, LongPause)),
            new(1, new(LeftmostX, ShortPause)),
            new(1, new(RightX, ShortPause)),
            new(4, new(RightX, ShortPause)))),
        new(MiddleX, new(
            KraidRandomDefinitions.MovementChoice,
            new(1, new(LeftX, MediumPause)),
            new(1, new(StartingX, LongPause)),
            new(1, new(RightmostX, LongPause)),
            new(1, new(LeftmostX, ShortPause)),
            new(4, new(RightX, ShortPause)))),
        Starting,
        new(RightX, new(
            KraidRandomDefinitions.MovementChoice,
            new(1, new(LeftX, MediumPause)),
            new(1, new(RightmostX, LongPause)),
            new(1, new(RightX, LongPause)),
            new(1, new(LeftmostX, ShortPause)),
            new(4, new(MiddleX, ShortPause)))),
        new(RightmostX, new(
            KraidRandomDefinitions.MovementChoice,
            new(1, new(LeftX, MediumPause)),
            new(1, new(StartingX, LongPause)),
            new(1, new(LeftmostX, ShortPause)),
            new(1, new(MiddleX, ShortPause)),
            new(4, new(RightX, ShortPause)))),
    ];
}
