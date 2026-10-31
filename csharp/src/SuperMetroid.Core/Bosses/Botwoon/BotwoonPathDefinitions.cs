using static SuperMetroid.Core.Bosses.Botwoon.BotwoonPathStep;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's 22 authored paths through its room, one step per entry. Paths 0 through 15 run in the open, four from
/// each hole in hole order (left, bottom, top, right): to each other hole in turn, then back into the same hole.
/// Paths 16 through 21 run inside the wall, one for each pair of holes in the order left-bottom, left-top,
/// left-right, bottom-top, bottom-right and top-right, walked forward from the first hole of the pair and backward
/// from the second.
/// </summary>
internal static class BotwoonPathDefinitions
{
    /// <summary>Path 0: In the open from the left hole to the bottom hole.</summary>
    private static readonly BotwoonPathStep[] OpenLeftToBottom =
    [
            Right, Right, Up, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Up, Right,
            Right, Right, Right, Right, Up, Right, Right, Right, Right, UpRight, Right, Right, Right, Right, Right, Up,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Down, Right,
            Right, Right, Right, Right, Down, Right, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, DownRight, Right, Down, Right, DownRight, Right, Down, Right, Down, Right, Right, Down,
            Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right,
            Right, Right, Down, Right, Right, Down, Right, Right, Right, Down, Right, Right, Down, Right, Right, Down,
            Right, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Right, Down, Right, Right,
            Right, Down, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Up, Right, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, UpRight,
            Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, UpRight, Up, Up, Up, Up, Right, Up, Up, Up, Up, Up,
            Left, Up, Up, Up, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, UpLeft, Up, Left, Up, Left, Up, Left, Up,
            Left, UpLeft, Left, Up, Left, Left, Up, Left, Left, Left, Up, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Down, Left, Left, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down,
            Left, Down, DownLeft, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Left, Down,
            Down, Down, Left, Down, Down, Left, Down, Down, Down, Left, Down, Down, Left, Down, Down, Down, Left, Down,
            Down, Left, Down, Down, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Left, Down, Left, Down,
            Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down,
            Down, Left, Down, Left, Down, Down, DownLeft, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down,
    ];

    /// <summary>Path 1: In the open from the left hole to the top hole.</summary>
    private static readonly BotwoonPathStep[] OpenLeftToTop =
    [
            Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, DownRight, Right,
            Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down, Right, Down, Down, Down, Right, Down,
            Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Left,
            Down, Down, Down, Down, Left, Down, Down, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Left,
            Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down,
            DownLeft, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Down, Down, Left, Down, Down, Down,
            Down, Down, Down, Down, Down, Down, Down, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down,
            Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Right, Down, Right,
            Right, Right, Right, Down, Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right,
            Right, Right, Down, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Up, Right,
            Right, Right, Right, Right, Up, Right, Right, Right, Right, UpRight, Right, Right, Right, Up, Right, Right,
            Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, UpRight, Right, Up, Right, Right, Up, Right,
            Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, UpRight, Right, Up, Right, Up,
            Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right,
            Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up,
            Left, Up, Up, Left, Up, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left,
            Left, Up, Left, Left, Left, Left, Left, Up, Left, Left, Left, Left, Up, Left, Left, Left, Left, Left, Up,
            Left, Left, Left, Left, Left, Up, Left, Left, Left, Left, Left, Up, Left, Left, Left, Left, Up, Left, Left,
            Left, Left, Up, Left, Left, Left, Up, Left, Left, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left,
            Up, Left, Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Up, Up, Left, Up, Up, Up, Up, Up, Right,
            Up, Up, Up, Up, Right, Up, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Right, Right, Up, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Up,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right,
    ];

    /// <summary>Path 2: In the open from the left hole to the right hole.</summary>
    private static readonly BotwoonPathStep[] OpenLeftToRight =
    [
            Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down,
            DownRight, Down, Down, DownRight, Down, Down, Down, Right, Down, Down, Down, Down, DownRight, Down, Down,
            Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Left, Down,
            Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down,
            Down, Left, Down, Down, DownLeft, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Down,
            Down, Down, Down, Down, Right, Down, Down, Down, DownRight, Down, Down, Right, Down, Down, Right, Down,
            Down, Right, Down, DownRight, DownRight, Down, Right, Down, Right, Down, Right, Down, Right, DownRight,
            Right, Down, Right, Right, DownRight, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            UpRight, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            UpRight, UpRight, UpRight, UpRight, Up, Right, UpRight, UpRight, UpRight, UpRight, UpRight, Up, Right, Up,
            Right, Up, UpRight, Up, UpRight, Up, Up, Up, Up, Right, Up, Up, Up, UpLeft, Up, Up, Up, Up, Up, Up, Up, Up,
            Up, Up, Up, Up, Up, Right, Up, Up, Up, Up, Right, Up, Up, UpRight, Up, Up, Right, Up, Up, Right, Up, Up,
            Right, Up, UpRight, Up, Right, Up, UpRight, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right,
            Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            DownRight, Down, Right, Down, Down, Right, Down, Down, DownRight, Down, Down, DownRight, Down, Down, Down,
            Down, Down, Down, Down, Down, Down, Left, Down, Down, DownLeft, Down, Left, Down, DownLeft, DownLeft, Left,
            Down, Left, Left, Down, Left, Left, DownLeft, Left, Left, Left, Left, Left, Left, Left, Up, Left, Left, Up,
            Left, Up, Left, UpLeft, Up, Left, Up, UpLeft, Up, UpLeft, Up, Up, UpLeft, Up, Up, Up, Up, Up, Up, Up, Up,
            Up, Right, Up, Up, Right, Up, Up, Right, UpRight, Right, Up, Right, Up, Right, Right, UpRight, Right, Right,
            Up, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right,
            Right, Right, UpRight, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Down, Right, Right, DownRight, Right, Right, Down, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right,
            UpRight, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Down, Right, Right,
            Right, DownRight, Right, Right, Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Down, Right,
            Down, Right,
    ];

    /// <summary>Path 3: In the open from the left hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenLeftBackIntoItself =
    [
            Right, Right, Right, Down, Right, Right, Right, Right, Right, Down, Right, Right, Right, DownRight, Right,
            Right, Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, DownRight, DownRight, DownRight,
            Down, Right, Down, Right, Down, DownRight, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down,
            Down, Right, Down, Down, Down, Right, Down, Down, Down, Down, Right, Down, Down, Down, Down, Down, Right,
            Down, Down, Down, Down, Down, DownRight, Down, Down, Down, Down, Right, Down, Down, Down, Down, Right, Down,
            Down, Down, Right, Down, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down,
            Right, Down, Down, Right, Down, Right, Down, DownRight, Down, Right, Down, DownRight, DownRight, Down,
            Right, Down, DownRight, Down, Right, Down, Right, Down, DownRight, DownRight, DownRight, DownRight,
            DownRight, Right, Down, Right, Right, Down, Right, Right, Right, Right, Down, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right,
            Right, Right, Right, UpRight, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right,
            UpRight, UpRight, UpRight, Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, UpRight, Up, Up, Up,
            Right, Up, Up, Up, Right, Up, Up, Up, UpRight, Up, Up, Right, Up, Up, Right, Up, UpRight, Up, Right, Up,
            UpRight, UpRight, UpRight, UpRight, Right, Up, Right, Right, Up, Right, Right, Right, Right, Right, Right,
            Right, Down, Right, Right, Right, Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Right,
            Down, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, DownRight, Down,
            Right, Down, Down, DownRight, Down, Down, Down, Down, Down, Down, Down, Down, DownLeft, Down, Down, Left,
            Down, Down, Left, Down, DownLeft, DownLeft, Down, Left, DownLeft, Left, Down, Left, DownLeft, Left,
            DownLeft, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Up, Left,
            Left, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up,
            Left, UpLeft, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, UpLeft, Left, UpLeft, Left, Up,
            Left, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Up,
            Left, Left, Up, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up,
            Left, Left, Up, Left, Left, UpLeft, Left, Left, Left, Up, Left, Left, Left, Left, Left, Left, UpLeft, Left,
            Left, Left, Left, Left, Left, Left, UpLeft, Left, Left, Left, Left, Left, Up, Left, Left, Left, Left, Up,
            Left, Left, Left, Up, Left, Left, Left, Up, Left, Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, Up,
            UpLeft, Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, UpLeft, Up, Up, Up, Left, Up,
            Up, Up, Up, Up, Up, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Right, Up,
    ];

    /// <summary>Path 4: In the open from the bottom hole to the left hole.</summary>
    private static readonly BotwoonPathStep[] OpenBottomToLeft =
    [
            Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right,
            Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right,
            Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Up,
            Right, Right, Right, Right, Up, Right, Right, Right, Right, Up, Right, Right, Up, Right, Right, Right, Up,
            Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right,
            Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up,
            Up, Right, Up, UpRight, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Up, Right, Up, Up,
            Right, Up, Up, Up, Right, Up, Up, Right, Up, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Up, Right,
            Up, Up, Up, Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left,
            Up, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up,
            Left, Up, Left, Up, Up, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left,
            Left, Up, Left, Left, Left, Up, Left, Left, Left, Left, Left, Up, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left,
            Down, Left, Left, Left, Down, Left, Left, Down, Left, Left, Left, Down, Left, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, DownLeft, DownLeft, Down, Left, Down, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left,
            Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left,
            Down, Left, Left, DownLeft, Left, Left, Down, Left, Left, Left, DownLeft, Left, Left, Left, Down, Left,
            Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left,
            Left, Down, Left, Left, Left, Left, Left, Left, Left, DownLeft, Left, Left, Left, Left, Left, Left, Left,
            Left, Down, Left, Left, Left, Left, Left,
    ];

    /// <summary>Path 5: In the open from the bottom hole to the top hole.</summary>
    private static readonly BotwoonPathStep[] OpenBottomToTop =
    [
            UpRight, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Up, Up, Right,
            Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up,
            Left, Up, Left, Up, UpLeft, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, DownLeft, Left, Down, Left, Left,
            Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Down, Down,
            Left, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Right, Down, Down, DownRight, Down,
            DownRight, Down, Right, Down, Right, Down, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right,
            Down, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up,
            Right, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            UpRight, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Up,
            Right, Up, Up, Up, Up, Right, Up, Up, Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Right, Up, Up, Up, Up,
            Up, Up, Up, Up, Left, Up, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, UpLeft, Up, Left, Up, Up, Left,
            Up, Left, Up, UpLeft, Up, Left, Up, Left, Left, Up, Left, Up, Left, Up, Left, Left, Up, Left, Left, UpLeft,
            Left, Left, UpLeft, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down,
            Down, Left, Down, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Down, Right, Down, Down,
            Down, Right, Down, Down, Right, Down, Down, Right, Down, DownRight, Down, Right, Down, Right, Down, Right,
            Down, Right, DownRight, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right,
            DownRight, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, UpRight, Right, Right, Up, Right,
            Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right,
            Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, UpRight, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Right, Up, UpRight, Up, Right, Up, Up, UpRight,
            Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Left, Up, Left, Left, Up,
            Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Down, Left, Left, Down, Left, Down, Left, Left,
            Down, DownLeft, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, DownLeft, Down,
    ];

    /// <summary>Path 6: In the open from the bottom hole to the right hole.</summary>
    private static readonly BotwoonPathStep[] OpenBottomToRight =
    [
            Right, Up, Right, Up, UpRight, UpRight, UpRight, UpRight, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up,
            Right, Up, Up, Right, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Up, Right, Up, Up,
            Up, Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Up, Up,
            Up, Up, Left, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left,
            Left, Left, Up, Left, Left, Left, DownLeft, Left, Left, Left, Down, Left, Left, DownLeft, Left, Down, Left,
            Down, Left, DownLeft, Down, Left, Down, Left, Down, Down, Left, Down, Down, Down, Left, Down, Down, Down,
            Down, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Left,
            Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Right, Down,
            Down, Down, Down, Down, Right, Down, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down,
            DownRight, Down, Right, Down, Right, Down, Right, Down, Down, Right, DownRight, Right, Down, Right, Down,
            Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, DownRight,
            DownRight, DownRight, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Down, Right, Right, Down, Right, Right, Right, Down, Right, Right, Right, Down,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up,
            Right, Right, Right, Right, Up, Right, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up,
            Up, Right, Up, Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Left,
            Up, UpLeft, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Up, Up, Up,
            Left, Up, Up, Up, Right, Up, Up, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right,
            Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Down,
            Right, Right, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, DownRight,
            Down, Right, Down, Right, Down, Down, Right, Down, Down, Down, Right, Down, Down, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Left,
            Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Left, Down, Down, Left, Down, Left, Down,
            Left, Down, Left, Left, Left, Left, Left, Left, Up, Left, Left,
    ];

    /// <summary>Path 7: In the open from the bottom hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenBottomBackIntoItself =
    [
            Left, Up, Left, Left, Left, Up, Left, Left, Up, Left, Left, UpLeft, Left, UpLeft, Left, UpLeft, UpLeft,
            Left, Up, Left, Up, Left, UpLeft, UpLeft, UpLeft, Up, Left, Up, UpLeft, Up, Up, UpLeft, Up, Up, Up, Up, Up,
            Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Up, Up, Left, Up, Up, Left, Up, Up, Left,
            Up, Up, Left, Up, UpLeft, UpLeft, UpLeft, Left, Up, Left, Left, Up, Left, Left, Left, Left, Left, Left,
            Left, Left, Up, Left, Left, Left, Left, Left, Left, Up, Left, Left, UpLeft, Left, Left, Up, Left, Up, Left,
            UpLeft, UpLeft, UpLeft, Up, Left, Up, Up, Left, Up, Up, Up, Up, Up, Up, Up, Up, Up, Right, Up, Up, Up,
            UpRight, Up, UpRight, Up, Right, Up, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Right, Up, Right, Right, UpRight, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, Down, Right, Right, Right,
            Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, UpRight, Right, Right, Right, Right,
            Right, Right, UpRight, Right, Right, Right, Up, Right, Right, Right, Right, Up, Right, Right, Right, Up,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Down, Right, Right, Right, Right, Down, Right, Right, Right, Down, Right, Down,
            Right, DownRight, DownRight, Down, Right, Down, Down, DownRight, Down, Down, Down, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, DownLeft, Down, Down, DownLeft, Down, DownLeft, Down, Left, Down, DownLeft,
            DownLeft, Left, Down, Left, Down, Left, Left, Down, Left, Left, Left, Down, Left, Left, Left, Left, Down,
            Left, Left, Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Left, DownLeft, Left, Left, DownLeft, Left, Down, Left,
            Down, Left, Down, Down, Left, Down, Down, Down, DownLeft, Down, Down, Down, Down, Down, Down, Down, Down,
            Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down,
            DownLeft, Down, Down, DownLeft, Down, Down, Left, Down, Down, DownLeft, Down, Left, Down, Down, Left, Down,
            Left, Down, Left, Down, Left, DownLeft, Left, Down, Left, DownLeft, Left, Down, Left,
    ];

    /// <summary>Path 8: In the open from the top hole to the left hole.</summary>
    private static readonly BotwoonPathStep[] OpenTopToLeft =
    [
            Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down,
            Down, Down, Left, Down, Down, Down, Left, Down, Down, Down, Left, Down, Down, Down, Left, Down, Down,
            DownLeft, Down, Down, DownLeft, Down, Down, DownLeft, Down, Down, Down, DownLeft, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Right, Down, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down,
            Down, Right, Down, Right, Down, Right, Down, DownRight, DownRight, Right, Down, Right, Down, Right,
            DownRight, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Right, Down, Right, Right,
            Right, Right, Down, Right, Right, Right, Right, DownRight, Right, Right, Right, Right, Right, Down, Right,
            Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, DownRight, Right, Right, Right,
            DownRight, Right, Right, Down, Right, Right, DownRight, Right, Down, Right, Right, Down, Right, Down, Right,
            Down, DownRight, DownRight, Down, DownRight, Down, DownRight, Down, DownRight, Down, Down, DownRight, Down,
            Down, Down, DownRight, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Left, Down, DownLeft,
            DownLeft, Left, Down, Left, Left, DownLeft, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left,
            Down, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Up, Left, Left, Left, Left, Left, Up, Left, Left, UpLeft, Left, Left, Up, Left, Left, Up, Left,
            UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left,
            UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, UpLeft, Left, Up, Left, Up,
            Left, Up, Left, Up, Left, Up, Left, Up, UpLeft, UpLeft, UpLeft, Up, Left, Up, Left, Up, Left, Up, Left, Up,
            Left, Up, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, UpLeft, UpLeft, Left, Up, Left,
            UpLeft, Left, Up, Left, Left, Up, Left, Left, UpLeft, Left, Left, Left, Up, Left, Left, Left, Left, Left,
            Left, DownLeft, Left, Left, Down, Left, DownLeft, Left, Down, Left, Down, Left, Down, DownLeft, Down, Left,
            Down, Down, DownLeft, Down, Down, Down, DownLeft, Down, Down, Down, DownRight, Down, Down, Down, Down,
            Right, Down, Down, Right, Down, DownRight, DownRight, DownRight, Right, Down, Right, DownRight, Right,
            Right, Down, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Up, Right,
            Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up,
            Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Left, Up, Up,
            Left, Up, UpLeft, UpLeft, Up, Left, Up, Left, UpLeft, UpLeft, UpLeft, Left, Up, Left, UpLeft, Left, Up,
            Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Left, Up, Left, Left, Up, Left,
            Left, Left, Up, Left, Left, Up, Left, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left,
            Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up,
            UpLeft, UpLeft, Up, Left, Up, Up, Left, Up, UpLeft, Up, Left,
    ];

    /// <summary>Path 9: In the open from the top hole to the bottom hole.</summary>
    private static readonly BotwoonPathStep[] OpenTopToBottom =
    [
            Down, Down, Down, Left, Down, Down, Down, Down, Left, Down, Down, Down, Left, Down, Down, Left, Down, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Left, Left, Down, Left, Left, Left, Down, Left, Left, Left, Left,
            DownLeft, Left, Left, Left, Left, Left, DownLeft, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Up, Left, Left,
            Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Down, Left, Left, Down, Left, Left,
            Down, Left, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Down,
            Down, Left, Down, Down, Down, Down, Left, Down, Down, Down, Down, DownLeft, Down, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Right, Down, Down, Down, Right, Down, Down, Right, Down, Right, Down, Down,
            Right, Down, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Right,
            Right, Down, Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right,
            Right, Right, Up, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Up, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, DownRight, Right, Right, Down, Right, Right, Down, Right,
            Down, Right, Down, Right, Down, Right,
    ];

    /// <summary>Path 10: In the open from the top hole to the right hole.</summary>
    private static readonly BotwoonPathStep[] OpenTopToRight =
    [
            Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, DownRight, Right, Down, Down,
            Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down,
            Right, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Down, Right, Down, Down, Right, Down,
            Down, Down, Down, Right, Down, Down, Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Down, Down, DownLeft, Down, Down,
            Down, Left, Down, Down, Down, Left, Down, Down, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down,
            Left, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left, Left, DownLeft, Left, Left, Left, Left,
            Down, Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, DownLeft, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Left, Up, Left, Left, Left, Left, Left, Left, Up, Left,
            Left, Left, Up, Left, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Up, Left, Left,
            Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left, Up, Up,
            Left, Up, Up, Up, Left, Up, Up, Up, Up, Up, Up, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Right, Up, Up,
            Right, Up, Up, Right, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right,
            Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right,
            Right, Right, Up, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, Down, Right, Right,
            Right, Right, Down, Right, Right, DownRight, Right, Right, Down, Right, Right, Down, Right, Right, Down,
            Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right,
            Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right,
            Down, Right, Down, Down, Right, Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down, Right,
            Down, Down, Right, Down, Down, DownRight, Down, Down, Right, Down, Down, Down, Right, Down, Down, DownRight,
            Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Right, Down, Right, Down, DownRight, Right,
            Down, Right, Down, Right, DownRight, Right, DownRight, Right, Right, Right, Down, Right, Right, Right,
            Right, Right, Right, Right, Right, Up, Right, Right, UpRight, Right, Up, Right, Right, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
    ];

    /// <summary>Path 11: In the open from the top hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenTopBackIntoItself =
    [
            Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right, Right, Down, Right, Right,
            Right, Right, Right, Down, Right, Right, Right, Right, Down, Right, Right, Right, Down, Right, Right, Right,
            Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Down, Right, DownRight, DownRight, Down,
            Right, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Down, Right, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down,
            Down, Right, Down, Down, Right, Down, Right, Down, Right, Down, DownRight, DownRight, Down, Right, Down,
            Right, Down, Right, Down, Right, Down, Down, Right, Down, Down, Down, DownRight, Down, Down, Left, Down,
            Down, Down, Down, Left, Down, Down, Left, Down, Left, Down, DownLeft, Left, Down, Left, Down, Left, Left,
            Down, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Up, Left, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Up, Left, Up, Up, Left, Up, UpLeft, Up, Up,
            Left, Up, Up, Up, Left, Up, Up, Up, UpLeft, Up, Up, Up, Up, Left, Up, Up, Up, Up, Left, Up, Up, Up, Up,
            Left, Up, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Left, Up, Left, Up,
            Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Left, Up, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Down, Left, Left, Down, Left, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Down, Left, Down,
            Down, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Left, Down, Down,
            Left, Down, Left, Left, Left, Down, Left, UpLeft, Left, Left, Left, Left, Up, Left, Left, Up, Left, Left,
            Up, Left, Up, Left, Up, Left, Up, Left, Up, Up, Left, Up, Left, Up, Up, Up, Left, Up, Up, Up, Up, Left, Up,
            Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, Right, Up, Up, Up,
            Right, Up, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Right, Up,
            UpRight, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up,
            Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Down, Right, Right, Right,
    ];

    /// <summary>Path 12: In the open from the right hole to the left hole.</summary>
    private static readonly BotwoonPathStep[] OpenRightToLeft =
    [
            Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Up,
            Left, Left, Up, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up, Left, Left, Up, Left, Up,
            Left, Left, Up, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Left, Up,
            Left, Up, Left, Left, Up, Left, Left, Left, UpLeft, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Down, Left, Left, Down, Left, Left, Down,
            Left, Down, Left, DownLeft, Down, Left, Down, Left, Down, Down, Left, Down, Down, Down, Left, Down, Down,
            Down, Left, Down, Down, Left, Down, Down, Down, Left, Down, Down, Down, Left, Down, Down, Down, Left, Down,
            Down, Down, DownLeft, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down,
            Right, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Right, Down, Down,
            Down, Down, Down, Down, Down, Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down, Down,
            Right, Down, Down, Down, Down, Down, Right, Down, Down, Down, Down, Right, Down, Down, Down, Right, Down,
            Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Right, Right, Down, Right, DownRight, Right,
            Right, Down, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right,
            Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Up, Right, Up,
            Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, UpRight,
            Up, Left, Up, Up, Up, Up, Up, Up, UpLeft, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left,
            Up, Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up,
            Left, Left, Left, Up, Left, Left, Left, Left, Up, Left, Left, Left, Left, Left, Left, Up, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left,
            Down, Left, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left,
            Down, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left, Left, Down, Left, Left, Left, Down, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Up,
            Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Up, UpLeft, Up, Up, Up, Up, Up, Up, Up, Up, UpRight,
            Up, Up, Up, Right, Up, Up, Right, Up, Up, Up, Right, Up, Up, Right, Up, Up, Up, Up, Right, Up, Up, Up, Up,
            Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left, Up, Left,
            Left, Up, Left, Left, Up, Left, Left, Left, Up, Left, Left, Left, Left, Left, Left, Left,
    ];

    /// <summary>Path 13: In the open from the right hole to the bottom hole.</summary>
    private static readonly BotwoonPathStep[] OpenRightToBottom =
    [
            Left, Up, Left, Left, Left, Up, Left, Left, Left, Up, Left, Left, Left, Up, Left, Left, Left, Up, Left,
            Left, Left, Left, Up, Left, Left, Left, Left, Up, Left, Left, Left, Left, Up, Left, Left, Left, Up, Left,
            Left, Up, Left, Left, Up, Left, Up, Left, UpLeft, UpLeft, UpLeft, Up, Left, Up, Left, Up, UpLeft, Up, Left,
            Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Up, Up, Left, Up,
            Up, Up, Up, UpRight, Up, Up, Up, Right, Up, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Down,
            Down, Right, Down, Down, Right, Down, Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Left,
            Down, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left,
            Down, Left, Left, Down, Left, Left, Down, Left, Left, Left, Down, Left, Left, Left, DownLeft, Left, Left,
            Left, Left, Down, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, DownLeft,
            Left, Left, Left, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, Left, Up, Left, Left, Left, Up, Left, Left, Up, Left, UpLeft,
            Left, Up, Left, Up, Left, Up, Left, Up, UpLeft, Up, Left, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left,
            Up, UpLeft, Up, Left, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Up, Up, Up, Up, Up, Up,
            Up, Right, Up, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right,
            Right, Right, Up, Right, Right, Right, Right, Right, Right, Down, Right, Right, Down, Right, Right, Down,
            Right, Down, Right, Down, Right, Down, DownRight, Down, Right, Down, Down, Right, Down, DownRight, Down,
            Down, Right, Down, Down, DownRight, Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down, Down,
            Down, Right, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Right, Down,
            Down, Down, Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Right, Down, Down, Down, Down,
            Down, Down, Right, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down,
            Down, Down, Down, Left, Down, Down, Down, Left, Down, Down, Left, Left, Down, Left, Left, Left, Left, Left,
            Up, Left, Left, Left, Up, Left, Left, UpLeft, Left, UpLeft, Left, Up, Left, Left, Up, Left, Left, Up, Left,
    ];

    /// <summary>Path 14: In the open from the right hole to the top hole.</summary>
    private static readonly BotwoonPathStep[] OpenRightToTop =
    [
            Left, Down, Left, Down, Left, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left,
            Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, DownLeft, Left,
            Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Left, Left, Down, Left, Left, Left, Down, Left,
            Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Up, Left, Left, Left, Left, Left, Up, Left, Left, Left, Up, Left, Left, Up,
            Left, Left, Up, Left, Up, Left, Up, Left, UpLeft, Left, Up, Up, Left, Up, Left, Up, Left, Up, UpLeft, Up,
            Left, Up, Up, Left, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Up, Up, Up, Up, Up, Up,
            Up, Up, Up, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, UpRight, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up,
            Right, Right, Up, Right, Right, Up, Right, Right, UpRight, Right, Right, Right, Up, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Down, Right, Right, Right, Down, Right, Right, Down,
            Right, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            Down, Right, Down, DownRight, Down, Down, Right, Down, Down, Down, Right, Down, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Down, Left, Down, Down, Left,
            Down, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down, Left, Left, Down, Left, Down, Left,
            Left, Down, Left, Left, Left, Left, Left, Down, Left, Left, Left, Up, Left, Left, Left, Left, Left, Up,
            Left, Left, Up, Left, Left, Up, Left, UpLeft, UpLeft, Up, Left, Up, Left, Up, Up, Up, Left, Up, Up, Up, Up,
            Up, Up, Up, Up, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Right, Up, Right, Up,
            UpRight, UpRight, Right, Up, Right, UpRight, Right, Right, Right, Right, Right, Down, Right, Right, Down,
            Right, DownRight, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down, Down, Down, Down, Down,
            Left, Down, Down, Left, DownLeft, Left, Down, Left, Left, Down, Left, Left, Left, Left, Left, Up, Left,
            Left, Up, Up, Left, Up, Left, Up, Up, Up, Left, Up, Up, Up, Up, Up, Left, Up, Up, Up, Up, Left, Up, Up, Up,
            Up, Left, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Up, Left, Up, Up, Left, Up, Up, UpLeft, Up, Up, Left,
            Up,
    ];

    /// <summary>Path 15: In the open from the right hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenRightBackIntoItself =
    [
            Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Up,
            Left, Up, Left, Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up, Left,
            Left, Up, Left, Left, UpLeft, Left, Left, Left, Up, Left, Left, Left, Left, Left, UpLeft, Left, Left, Left,
            Left, Left, Left, Left, Up, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left, Down,
            Left, Left, Left, Left, Left, DownLeft, Left, Left, Left, Left, Left, Down, Left, Left, Left, Left, Left,
            Left, Down, Left, Left, Left, DownLeft, Left, Left, DownLeft, Left, DownLeft, Left, Down, Left, Left, Down,
            Left, Left, DownLeft, Left, Left, Left, Down, Left, Left, Left, Left, Left, Left, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, Left, Left, DownLeft, Left, Left, Left, Left, Down, Left, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Down, Left, DownLeft, Left, Down, Left, Down,
            DownLeft, DownLeft, Down, DownLeft, Down, Down, Down, Down, Down, Down, Down, Down, Down, DownRight, Down,
            Down, Right, Down, DownRight, DownRight, Down, Right, DownRight, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right,
            Down, Right, Right, Right, Down, Right, Right, Down, Right, DownRight, Right, Down, Right, DownRight, Right,
            Down, Right, Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down, Right, Down, Down, Right,
            Down, Down, Down, Down, Right, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down,
            Down, DownLeft, Down, Down, Left, Down, Down, Left, Down, Left, Down, DownLeft, DownLeft, DownLeft, Left,
            Down, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left, Left, DownLeft, Left, Left, Left, Left,
            Left, Left, Left, Left, Left, UpLeft, Left, Left, Left, Up, Left, Left, UpLeft, Left, Up, Left, UpLeft,
            UpLeft, UpLeft, Up, Left, Up, UpLeft, Up, UpLeft, Up, Up, Left, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up,
            Up, Up, Up, Right, Up, Up, Up, Right, Up, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, UpRight, Up,
            Right, Up, Right, UpRight, UpRight, Right, Up, Right, UpRight, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Right, Up, Right, Right, UpRight, Right, Right, UpRight, Right, Right, Right, Up, Right, Right,
            Right, Right, UpRight, Right, Right, Right, Right, Right, UpRight, Right, Right, Right, Right, Right, Right,
            Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, DownRight,
            Right, Right, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right, Right, Down,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, UpRight, Right, Right, Right, UpRight,
            Right, Right, Up, Right, Right, UpRight, Right, Up, Right, UpRight, UpRight, Right, Up, UpRight, Up, Right,
            Up, Up, Right, Up, Up, Up, Up, Right, Up, Up, Up, Up,
    ];

    /// <summary>Path 16: Inside the wall between the left and bottom holes, forward from the left hole.</summary>
    private static readonly BotwoonPathStep[] WallLeftBottom =
    [
            Right, Down, Right, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Down, Down, Right, Down, DownRight, Down, Down, Right, Down, Down, Right, Down,
            Down, Right, Down, Down, Right, Down, Right, Down, Right, Right, Down, Right, Right, Right, Right, Down,
            Right, Right, Right, Right, Right, Down, Right, Right, Down, Right, DownRight, Down, Right, Down, Down,
            Down, Right, Down, Down, Down, Down, Down, Down, Left, Down, Down, Left, Down, Down, Left, Left, Down, Left,
            Down, Left, Left, Left, Down, Left, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Down, Left,
            Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Down, Down, Right,
            Down, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, Right, Down, Right, Right, Right, Right, Right, Down, Right, Right, Right, Up, Right,
            Right, Right, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Right, Up,
            Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Right, Right, Down, Right,
            Down, Right, Down, Right, Down, Right, Down,
    ];

    /// <summary>Path 17: Inside the wall between the left and top holes, forward from the left hole.</summary>
    private static readonly BotwoonPathStep[] WallLeftTop =
    [
            Right, Up, Right, Right, Right, Up, Right, Right, Right, Up, Right, Right, UpRight, Right, Right, Right, Up,
            Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Down, Right, Right, Right,
            Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Down, Right, Down,
            Down, Right, Down, Down, Down, Down, Down, Down, Down, Left, Down, Down, Down, Down, Left, Down, Down, Down,
            Down, Down, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Down, Right, Right, Down, Right, Right, Right, Right, Down, Right, Right, Right,
            Right, Right, Right, Up, Right, Right, Right, Right, Up, Right, Right, Right, Up, Right, UpRight, Right, Up,
            Right, UpRight, Right, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, Right, Up, Up, Up, Right, Up,
            Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up, Up,
            Up, Right, Up, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up,
            Right, Right, Right, Up, Right, Right, Right, Right, Down, Right, Right, Right, Right, Down, Right, Right,
            DownRight, Right, Right, Down, Right,
    ];

    /// <summary>Path 18: Inside the wall between the left and right holes, forward from the left hole.</summary>
    private static readonly BotwoonPathStep[] WallLeftRight =
    [
            Down, Right, Right, Right, Down, Right, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Down, Right, Right, Down, Right,
            Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, Right, Right, Down, Right, DownRight, Right, Down, Right, Right, Down, Right, Down,
            Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down,
            Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Right, Down,
            Right, Right, Down, Right, Right, Right, Right, Down, Right, Right, Right, Right, Right, Down, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right, Right,
            Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Down, Right, Right,
            Right, Right, Right, Down, Right, Right, Right, Down, Right, Right, Right, Right, Down, Right, Right, Right,
            Down, Right, Right, Right, Right, DownRight, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Right, Up, Right, Right, Right,
    ];

    /// <summary>Path 19: Inside the wall between the bottom and top holes, forward from the bottom hole.</summary>
    private static readonly BotwoonPathStep[] WallBottomTop =
    [
            Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right,
            UpRight, UpRight, UpRight, UpRight, Up, Right, Up, Right, Up, Right, Up, Up, Right, Up, UpRight, Up,
            UpRight, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Left,
            Up, Left, Up, Left, Up, Left, Up, Left, Left, Up, Left, Left, Left, Up, Left, Left, Left, Left, Left, Up,
            Left, Left, Left, Up, Left, Left, Up, Left, UpLeft, Up, Left, Up, UpLeft, Up, Up, Up, Left, Up, Up, Up, Up,
            Up, Up, Right, Up, Up, Up, Right, Up, UpRight, Right, Up, Right, Right, Up, Right, Right, Right, Right,
            Right, DownRight, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Up, Right,
            Right, Right, Up, Right, Up, Right, UpRight, Right, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up,
            Right, Up, Up, Right, Up, Right, Up,
    ];

    /// <summary>Path 20: Inside the wall between the bottom and right holes, forward from the bottom hole.</summary>
    private static readonly BotwoonPathStep[] WallBottomRight =
    [
            Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Up, Up, Up, Right, Up, Up, Up, Up, Up, Up, Up, Up, Up, Left, Up, Up, Up,
            Up, Right, Up, Up, Up, Up, Right, Up, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right,
            Right, Right, Up, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right, Right,
            Right, Down, Right, Right, Down, Right, Right, Down, Right, DownRight, Down, Right, Down, Right, Down, Down,
            Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Right, Down,
            DownRight, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Right, Right,
            Down, Right, Right, Right, Right, Right, Right, Up, Right, Right, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
    ];

    /// <summary>Path 21: Inside the wall between the top and right holes, forward from the top hole.</summary>
    private static readonly BotwoonPathStep[] WallTopRight =
    [
            Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, Down,
            Down, Left, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down,
            Right, Down, Down, Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Right, Down, Right, Right, Right, Down, Right, Right, Right, Right, Right, Right,
            Right, Right, Right, Up, Right, Right, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up,
            Right, Right, Right, Right, Up, Right, Right, Right, Right, Down, Right, Right, Down, Down, Down, Down,
            Down, Down, Down, Down, Down, Left, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down, Down,
            Down, DownRight, Down, Down, Right, Down, Right, Down, DownRight, Down, Right, Down, Right, Right, Down,
            Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Right, Right, Right, Up, Right, Right,
            Up, Up, Right, Up, Up, Right, Up, Up, Up, UpRight, Up, Up,
    ];

    /// <summary>Every authored path, by path number.</summary>
    internal static readonly BotwoonPathStep[][] Paths =
    [
        OpenLeftToBottom,
        OpenLeftToTop,
        OpenLeftToRight,
        OpenLeftBackIntoItself,
        OpenBottomToLeft,
        OpenBottomToTop,
        OpenBottomToRight,
        OpenBottomBackIntoItself,
        OpenTopToLeft,
        OpenTopToBottom,
        OpenTopToRight,
        OpenTopBackIntoItself,
        OpenRightToLeft,
        OpenRightToBottom,
        OpenRightToTop,
        OpenRightBackIntoItself,
        WallLeftBottom,
        WallLeftTop,
        WallLeftRight,
        WallBottomTop,
        WallBottomRight,
        WallTopRight,
    ];
}
