using static SuperMetroid.Core.Bosses.Botwoon.BotwoonPathStep;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's 22 authored paths through his room, one step per entry (#1868): sixteen in the open, from each hole to
/// each other hole and back into the same hole, and six inside the wall, one between each pair of holes, walked
/// forward from the first hole of the pair and backward from the second.
/// </summary>
internal static class BotwoonPathDefinitions
{
    /// <summary>In the open from the left hole to the bottom hole.</summary>
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

    /// <summary>In the open from the left hole to the top hole.</summary>
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

    /// <summary>In the open from the left hole to the right hole.</summary>
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

    /// <summary>In the open from the left hole back into it.</summary>
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

    /// <summary>In the open from the bottom hole to the left hole.</summary>
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

    /// <summary>In the open from the bottom hole to the top hole.</summary>
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

    /// <summary>In the open from the bottom hole to the right hole.</summary>
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

    /// <summary>In the open from the bottom hole back into it.</summary>
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

    /// <summary>In the open from the top hole to the left hole.</summary>
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

    /// <summary>In the open from the top hole to the bottom hole.</summary>
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

    /// <summary>In the open from the top hole to the right hole.</summary>
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

    /// <summary>In the open from the top hole back into it.</summary>
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

    /// <summary>In the open from the right hole to the left hole.</summary>
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

    /// <summary>In the open from the right hole to the bottom hole.</summary>
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

    /// <summary>In the open from the right hole to the top hole.</summary>
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

    /// <summary>In the open from the right hole back into it.</summary>
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

    /// <summary>Inside the wall between the left and bottom holes, forward from the left hole.</summary>
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

    /// <summary>Inside the wall between the left and top holes, forward from the left hole.</summary>
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

    /// <summary>Inside the wall between the left and right holes, forward from the left hole.</summary>
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

    /// <summary>Inside the wall between the bottom and top holes, forward from the bottom hole.</summary>
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

    /// <summary>Inside the wall between the bottom and right holes, forward from the bottom hole.</summary>
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

    /// <summary>Inside the wall between the top and right holes, forward from the top hole.</summary>
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

    /// <summary>
    /// Every authored path, in a fixed order the checks compare them in.
    /// </summary>
    internal static readonly BotwoonPathDefinition[] Paths =
    [
        new(BotwoonHole.Left, BotwoonHole.Bottom, BotwoonPathRoute.Open, OpenLeftToBottom),
        new(BotwoonHole.Left, BotwoonHole.Top, BotwoonPathRoute.Open, OpenLeftToTop),
        new(BotwoonHole.Left, BotwoonHole.Right, BotwoonPathRoute.Open, OpenLeftToRight),
        new(BotwoonHole.Left, BotwoonHole.Left, BotwoonPathRoute.Open, OpenLeftBackIntoItself),
        new(BotwoonHole.Bottom, BotwoonHole.Left, BotwoonPathRoute.Open, OpenBottomToLeft),
        new(BotwoonHole.Bottom, BotwoonHole.Top, BotwoonPathRoute.Open, OpenBottomToTop),
        new(BotwoonHole.Bottom, BotwoonHole.Right, BotwoonPathRoute.Open, OpenBottomToRight),
        new(BotwoonHole.Bottom, BotwoonHole.Bottom, BotwoonPathRoute.Open, OpenBottomBackIntoItself),
        new(BotwoonHole.Top, BotwoonHole.Left, BotwoonPathRoute.Open, OpenTopToLeft),
        new(BotwoonHole.Top, BotwoonHole.Bottom, BotwoonPathRoute.Open, OpenTopToBottom),
        new(BotwoonHole.Top, BotwoonHole.Right, BotwoonPathRoute.Open, OpenTopToRight),
        new(BotwoonHole.Top, BotwoonHole.Top, BotwoonPathRoute.Open, OpenTopBackIntoItself),
        new(BotwoonHole.Right, BotwoonHole.Left, BotwoonPathRoute.Open, OpenRightToLeft),
        new(BotwoonHole.Right, BotwoonHole.Bottom, BotwoonPathRoute.Open, OpenRightToBottom),
        new(BotwoonHole.Right, BotwoonHole.Top, BotwoonPathRoute.Open, OpenRightToTop),
        new(BotwoonHole.Right, BotwoonHole.Right, BotwoonPathRoute.Open, OpenRightBackIntoItself),
        new(BotwoonHole.Left, BotwoonHole.Bottom, BotwoonPathRoute.InsideWall, WallLeftBottom),
        new(BotwoonHole.Left, BotwoonHole.Top, BotwoonPathRoute.InsideWall, WallLeftTop),
        new(BotwoonHole.Left, BotwoonHole.Right, BotwoonPathRoute.InsideWall, WallLeftRight),
        new(BotwoonHole.Bottom, BotwoonHole.Top, BotwoonPathRoute.InsideWall, WallBottomTop),
        new(BotwoonHole.Bottom, BotwoonHole.Right, BotwoonPathRoute.InsideWall, WallBottomRight),
        new(BotwoonHole.Top, BotwoonHole.Right, BotwoonPathRoute.InsideWall, WallTopRight),
    ];
}
