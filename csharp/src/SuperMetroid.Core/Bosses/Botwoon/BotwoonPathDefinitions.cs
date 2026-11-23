using static SuperMetroid.Core.Bosses.Botwoon.BotwoonPathStep;
using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's 22 authored paths through his room, one step per entry (#1868): sixteen in the open, from each hole to
/// each other hole and back into the same hole, and six inside the wall, one between each pair of holes, walked
/// forward from the first hole of the pair and backward from the second.
/// </summary>
internal static partial class BotwoonPathDefinitions
{
    /// <summary>In the open from the left hole to the bottom hole.</summary>
    private static readonly BotwoonPathStep[] OpenLeftToBottom =
    [
            Right, Right, Up, .. Repeated(Right, 5), Up, .. Repeated(Right, 5), Up, .. Repeated(Right, 5), Up, .. Repeated(Right, 4), UpRight, .. Repeated(Right, 5), Up,
            .. Repeated(Right, 13), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 3), Down, Right, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, DownRight, Right, Down, Right, DownRight, Right, Down, Right, Down, Right, Right, Down,
            Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down, Right, Right, Down, .. Repeated(Right, 3), Down, Right, Right, Down, Right, Right, Down,
            .. Repeated(Right, 3), Down, Right, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 13), Up, .. Repeated(Right, 3), Up, Right, Right, Up, Right, Right, Up, Right, UpRight,
            Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, UpRight, .. Repeated(Up, 4), Right, .. Repeated(Up, 5),
            Left, .. Repeated(Up, 5), Left, Up, Up, Left, Up, Up, Left, Up, UpLeft, Up, Left, Up, Left, Up, Left, Up,
            Left, UpLeft, Left, Up, Left, Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 10), Down, .. Repeated(Left, 3), Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down,
            Left, Down, DownLeft, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, Left, .. Repeated(Down, 3), Left, Down, Down, Left, .. Repeated(Down, 3), Left, Down, Down, Left, .. Repeated(Down, 3), Left, Down,
            Down, Left, .. Repeated(Down, 3), Left, Down, Down, Left, Down, Down, Left, Down, Down, Left, Down, Left, Down,
            Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down,
            Down, Left, Down, Left, Down, Down, DownLeft, .. Repeated(Down, 3), Left, .. Repeated(Down, 6),
    ];

    /// <summary>In the open from the left hole to the top hole.</summary>
    private static readonly BotwoonPathStep[] OpenLeftToTop =
    [
            Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, DownRight, Right,
            Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down, Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 4), Right, .. Repeated(Down, 13), Left,
            .. Repeated(Down, 4), Left, .. Repeated(Down, 3), Left, Down, Down, Left, Down, Down, Left, Down, Down, Left,
            Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down,
            DownLeft, Down, Left, Down, Down, Left, Down, Down, Left, .. Repeated(Down, 4), Left, .. Repeated(Down, 12), Right, Down, Down, Right, Down, Down, Right, Down,
            Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 7), Down, .. Repeated(Right, 18), Up, .. Repeated(Right, 7), Up, .. Repeated(Right, 5), Up, .. Repeated(Right, 4), UpRight, .. Repeated(Right, 3), Up, .. Repeated(Right, 3), Up, Right, Right, Up, Right, Right, Up, Right, Right, UpRight, Right, Up, Right, Right, Up, Right,
            Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, UpRight, Right, Up, Right, Up,
            Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right,
            Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 11), Left, Up, Up,
            Left, Up, Up, Left, Up, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 5), Up, .. Repeated(Left, 4), Up, .. Repeated(Left, 5), Up,
            .. Repeated(Left, 5), Up, .. Repeated(Left, 5), Up, .. Repeated(Left, 4), Up, .. Repeated(Left, 4), Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, Up, Left, Left, Up, Left,
            Up, Left, Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 5), Right,
            .. Repeated(Up, 4), Right, Up, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right,
            Up, .. Repeated(Right, 3), Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 6), Up,
            .. Repeated(Right, 10), Up, .. Repeated(Right, 11),
    ];

    /// <summary>In the open from the left hole to the right hole.</summary>
    private static readonly BotwoonPathStep[] OpenLeftToRight =
    [
            Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down,
            DownRight, Down, Down, DownRight, .. Repeated(Down, 3), Right, .. Repeated(Down, 4), DownRight, .. Repeated(Down, 5), Right, .. Repeated(Down, 8), Left, .. Repeated(Down, 3), Left, Down,
            Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down,
            Down, Left, Down, Down, DownLeft, .. Repeated(Down, 3), Left, .. Repeated(Down, 12), Right, .. Repeated(Down, 3), DownRight, Down, Down, Right, Down, Down, Right, Down,
            Down, Right, Down, DownRight, DownRight, Down, Right, Down, Right, Down, Right, Down, Right, DownRight,
            Right, Down, Right, Right, DownRight, .. Repeated(Right, 9),
            UpRight, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            .. Repeated(UpRight, 4), Up, Right, .. Repeated(UpRight, 5), Up, Right, Up,
            Right, Up, UpRight, Up, UpRight, .. Repeated(Up, 4), Right, .. Repeated(Up, 3), UpLeft, .. Repeated(Up, 13), Right, .. Repeated(Up, 4), Right, Up, Up, UpRight, Up, Up, Right, Up, Up, Right, Up, Up,
            Right, Up, UpRight, Up, Right, Up, UpRight, Right, Up, Right, Up, Right, Up, Right, Right, Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 16), Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            DownRight, Down, Right, Down, Down, Right, Down, Down, DownRight, Down, Down, DownRight, .. Repeated(Down, 9), Left, Down, Down, DownLeft, Down, Left, Down, DownLeft, DownLeft, Left,
            Down, Left, Left, Down, Left, Left, DownLeft, .. Repeated(Left, 7), Up, Left, Left, Up,
            Left, Up, Left, UpLeft, Up, Left, Up, UpLeft, Up, UpLeft, Up, Up, UpLeft, .. Repeated(Up, 9), Right, Up, Up, Right, Up, Up, Right, UpRight, Right, Up, Right, Up, Right, Right, UpRight, Right, Right,
            Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 5), Up, .. Repeated(Right, 6), UpRight, .. Repeated(Right, 16), Down, Right, Right, DownRight, Right, Right, Down, .. Repeated(Right, 14), Up, .. Repeated(Right, 5),
            UpRight, .. Repeated(Right, 11), Down, .. Repeated(Right, 3), DownRight, Right, Right, Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Down, Right,
            Down, Right,
    ];

    /// <summary>In the open from the left hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenLeftBackIntoItself =
    [
            .. Repeated(Right, 3), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 3), DownRight, Right,
            Right, Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, .. Repeated(DownRight, 3),
            Down, Right, Down, Right, Down, DownRight, Down, Right, Down, Down, Right, Down, Down, Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 4), Right, .. Repeated(Down, 5), Right,
            .. Repeated(Down, 5), DownRight, .. Repeated(Down, 4), Right, .. Repeated(Down, 4), Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 3), Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down,
            Right, Down, Down, Right, Down, Right, Down, DownRight, Down, Right, Down, DownRight, DownRight, Down,
            Right, Down, DownRight, Down, Right, Down, Right, Down, .. Repeated(DownRight, 5), Right, Down, Right, Right, Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 18), Up, .. Repeated(Right, 4), UpRight, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right,
            .. Repeated(UpRight, 3), Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, UpRight, .. Repeated(Up, 3),
            Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 3), UpRight, Up, Up, Right, Up, Up, Right, Up, UpRight, Up, Right, Up,
            .. Repeated(UpRight, 4), Right, Up, Right, Right, Up, .. Repeated(Right, 7), Down, .. Repeated(Right, 3), Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Right,
            Down, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, DownRight, Down,
            Right, Down, Down, DownRight, .. Repeated(Down, 8), DownLeft, Down, Down, Left,
            Down, Down, Left, Down, DownLeft, DownLeft, Down, Left, DownLeft, Left, Down, Left, DownLeft, Left,
            DownLeft, .. Repeated(Left, 3), Down, .. Repeated(Left, 20), Down, .. Repeated(Left, 7), Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up,
            Left, UpLeft, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, UpLeft, Left, UpLeft, Left, Up,
            Left, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Up,
            Left, Left, Up, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up,
            Left, Left, Up, Left, Left, UpLeft, .. Repeated(Left, 3), Up, .. Repeated(Left, 6), UpLeft, .. Repeated(Left, 7), UpLeft, .. Repeated(Left, 5), Up, .. Repeated(Left, 4), Up,
            .. Repeated(Left, 3), Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, Up,
            UpLeft, Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, UpLeft, .. Repeated(Up, 3), Left, .. Repeated(Up, 9), Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up,
    ];

    /// <summary>In the open from the bottom hole to the left hole.</summary>
    private static readonly BotwoonPathStep[] OpenBottomToLeft =
    [
            .. Repeated(Right, 4), Up, .. Repeated(Right, 7), Up, .. Repeated(Right, 7), Up, .. Repeated(Right, 7), Up, .. Repeated(Right, 6), Up, .. Repeated(Right, 6), Up, .. Repeated(Right, 4), Up,
            .. Repeated(Right, 4), Up, .. Repeated(Right, 4), Up, Right, Right, Up, .. Repeated(Right, 3), Up,
            Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right,
            Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up,
            Up, Right, Up, UpRight, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 3), Right, Up, Up,
            Right, .. Repeated(Up, 3), Right, Up, Up, Right, .. Repeated(Up, 4), Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 4), Right,
            .. Repeated(Up, 6), Right, .. Repeated(Up, 18), Left,
            .. Repeated(Up, 5), Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up,
            Left, Up, Left, Up, Up, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left,
            Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 5), Up, .. Repeated(Left, 14), Down, .. Repeated(Left, 5), Down, .. Repeated(Left, 4),
            Down, .. Repeated(Left, 3), Down, Left, Left, Down, .. Repeated(Left, 3), Down, Left, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, DownLeft, DownLeft, Down, Left, Down, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left,
            Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left,
            Down, Left, Left, DownLeft, Left, Left, Down, .. Repeated(Left, 3), DownLeft, .. Repeated(Left, 3), Down, .. Repeated(Left, 5), Down, .. Repeated(Left, 6), Down, .. Repeated(Left, 7), Down, .. Repeated(Left, 7), DownLeft, .. Repeated(Left, 8), Down, .. Repeated(Left, 5),
    ];

    /// <summary>In the open from the bottom hole to the top hole.</summary>
    private static readonly BotwoonPathStep[] OpenBottomToTop =
    [
            UpRight, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 5), Right,
            .. Repeated(Up, 13), Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 3), Left, Up, Up,
            Left, Up, Left, Up, UpLeft, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, .. Repeated(Left, 12), Down, Left, Left, DownLeft, Left, Down, Left, Left,
            Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, .. Repeated(Down, 3),
            Left, .. Repeated(Down, 10), Right, Down, Down, DownRight, Down,
            DownRight, Down, Right, Down, Right, Down, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right,
            Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 10), Up,
            .. Repeated(Right, 3), Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            UpRight, Up, Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 4),
            Right, .. Repeated(Up, 4), Right, .. Repeated(Up, 5), Right, .. Repeated(Up, 7), Right, .. Repeated(Up, 8), Left, .. Repeated(Up, 5), Left, .. Repeated(Up, 3), Left, Up, Up, UpLeft, Up, Left, Up, Up, Left,
            Up, Left, Up, UpLeft, Up, Left, Up, Left, Left, Up, Left, Up, Left, Up, Left, Left, Up, Left, Left, UpLeft,
            Left, Left, UpLeft, .. Repeated(Left, 10), Down, Left, Left, Down,
            Left, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down,
            Down, Left, .. Repeated(Down, 4), Left, .. Repeated(Down, 8), Right, .. Repeated(Down, 3), Right, Down, Down, Right, Down, Down, Right, Down, DownRight, Down, Right, Down, Right, Down, Right,
            Down, Right, DownRight, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right,
            DownRight, .. Repeated(Right, 3), Down, .. Repeated(Right, 17), Up, .. Repeated(Right, 3), UpRight, Right, Right, Up, .. Repeated(Right, 3), Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right,
            Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, UpRight, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Right, Up, UpRight, Up, Right, Up, Up, UpRight,
            .. Repeated(Up, 3), Right, .. Repeated(Up, 8), Left, .. Repeated(Up, 3), Left, Up, Up, Left, Up, Left, Left, Up,
            .. Repeated(Left, 6), Down, .. Repeated(Left, 3), Down, Left, Left, Down, Left, Down, Left, Left,
            Down, DownLeft, Down, Left, Down, Down, Left, Down, Down, Left, Down, Down, DownLeft, Down,
    ];

    /// <summary>In the open from the bottom hole to the right hole.</summary>
    private static readonly BotwoonPathStep[] OpenBottomToRight =
    [
            Right, Up, Right, Up, .. Repeated(UpRight, 4), Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up,
            Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 4), Right, .. Repeated(Up, 6), Right, .. Repeated(Up, 15), Left, .. Repeated(Up, 7), Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 3), Left, Up, Up, Left, Up, Left, Up, Left, Up, Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 3), DownLeft, .. Repeated(Left, 3), Down, Left, Left, DownLeft, Left, Down, Left,
            Down, Left, DownLeft, Down, Left, Down, Left, Down, Down, Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 6), Left, .. Repeated(Down, 13), Left,
            .. Repeated(Down, 16), Right, .. Repeated(Down, 5), Right, .. Repeated(Down, 3), Right, Down, Down, Right, Down, Down, Right, Down,
            DownRight, Down, Right, Down, Right, Down, Right, Down, Down, Right, DownRight, Right, Down, Right, Down,
            Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, .. Repeated(DownRight, 3), Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 3), Down,
            .. Repeated(Right, 15), Up,
            .. Repeated(Right, 4), Up, .. Repeated(Right, 3), Up, Right, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 4), Right, .. Repeated(Up, 8), Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), Left,
            Up, UpLeft, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, .. Repeated(Up, 5),
            Left, .. Repeated(Up, 3), Right, .. Repeated(Up, 5), Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 11), Down,
            .. Repeated(Right, 4), Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, DownRight,
            Down, Right, Down, Right, Down, Down, Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 23), Left,
            .. Repeated(Down, 7), Left, .. Repeated(Down, 3), Left, Down, Down, Left, Down, Left, Down,
            Left, Down, .. Repeated(Left, 6), Up, Left, Left,
    ];

    /// <summary>In the open from the bottom hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenBottomBackIntoItself =
    [
            Left, Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, Left, UpLeft, Left, UpLeft, Left, UpLeft, UpLeft,
            Left, Up, Left, Up, Left, .. Repeated(UpLeft, 3), Up, Left, Up, UpLeft, Up, Up, UpLeft, .. Repeated(Up, 17), Left, .. Repeated(Up, 5), Left, Up, Up, Left, Up, Up, Left,
            Up, Up, Left, Up, .. Repeated(UpLeft, 3), Left, Up, Left, Left, Up, .. Repeated(Left, 8), Up, .. Repeated(Left, 6), Up, Left, Left, UpLeft, Left, Left, Up, Left, Up, Left,
            .. Repeated(UpLeft, 3), Up, Left, Up, Up, Left, .. Repeated(Up, 9), Right, .. Repeated(Up, 3),
            UpRight, Up, UpRight, Up, Right, Up, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Right, Up, Right, Right, UpRight, .. Repeated(Right, 3), Up, .. Repeated(Right, 41), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 8), Down, .. Repeated(Right, 4), UpRight, .. Repeated(Right, 6), UpRight, .. Repeated(Right, 3), Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 3), Up,
            .. Repeated(Right, 19), Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 3), Down, Right, Down,
            Right, DownRight, DownRight, Down, Right, Down, Down, DownRight, .. Repeated(Down, 12), DownLeft, Down, Down, DownLeft, Down, DownLeft, Down, Left, Down, DownLeft,
            DownLeft, Left, Down, Left, Down, Left, Left, Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 4), Down,
            .. Repeated(Left, 9), Down, .. Repeated(Left, 17), DownLeft, Left, Left, DownLeft, Left, Down, Left,
            Down, Left, Down, Down, Left, .. Repeated(Down, 3), DownLeft, .. Repeated(Down, 11), Right, .. Repeated(Down, 14),
            DownLeft, Down, Down, DownLeft, Down, Down, Left, Down, Down, DownLeft, Down, Left, Down, Down, Left, Down,
            Left, Down, Left, Down, Left, DownLeft, Left, Down, Left, DownLeft, Left, Down, Left,
    ];

    /// <summary>In the open from the top hole to the left hole.</summary>
    private static readonly BotwoonPathStep[] OpenTopToLeft =
    [
            .. Repeated(Down, 6), Left, .. Repeated(Down, 8), Left, .. Repeated(Down, 4), Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), Left, Down, Down,
            DownLeft, Down, Down, DownLeft, Down, Down, DownLeft, .. Repeated(Down, 3), DownLeft, .. Repeated(Down, 9), Right, .. Repeated(Down, 3), Right, Down, Down, Right, Down, Down, Right, Down,
            Down, Right, Down, Right, Down, Right, Down, DownRight, DownRight, Right, Down, Right, Down, Right,
            DownRight, Right, Down, Right, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 4), DownRight, .. Repeated(Right, 5), Down, .. Repeated(Right, 6), Down, .. Repeated(Right, 5), DownRight, .. Repeated(Right, 3),
            DownRight, Right, Right, Down, Right, Right, DownRight, Right, Down, Right, Right, Down, Right, Down, Right,
            Down, DownRight, DownRight, Down, DownRight, Down, DownRight, Down, DownRight, Down, Down, DownRight, .. Repeated(Down, 3), DownRight, .. Repeated(Down, 7), Left, Down, Down, Left, Down, DownLeft,
            DownLeft, Left, Down, Left, Left, DownLeft, .. Repeated(Left, 3), Down, .. Repeated(Left, 6),
            Down, .. Repeated(Left, 18), Up, .. Repeated(Left, 5), Up, Left, Left, UpLeft, Left, Left, Up, Left, Left, Up, Left,
            UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left,
            UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, UpLeft, Left, Up, Left, Up,
            Left, Up, Left, Up, Left, Up, Left, Up, .. Repeated(UpLeft, 3), Up, Left, Up, Left, Up, Left, Up, Left, Up,
            Left, Up, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, UpLeft, UpLeft, Left, Up, Left,
            UpLeft, Left, Up, Left, Left, Up, Left, Left, UpLeft, .. Repeated(Left, 3), Up, .. Repeated(Left, 6), DownLeft, Left, Left, Down, Left, DownLeft, Left, Down, Left, Down, Left, Down, DownLeft, Down, Left,
            Down, Down, DownLeft, .. Repeated(Down, 3), DownLeft, .. Repeated(Down, 3), DownRight, .. Repeated(Down, 4),
            Right, Down, Down, Right, Down, .. Repeated(DownRight, 3), Right, Down, Right, DownRight, Right,
            Right, Down, .. Repeated(Right, 9), Up, Right, Right, Up, Right,
            Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 16), Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 3), Left, Up, Up, Left, Up, Up,
            Left, Up, UpLeft, UpLeft, Up, Left, Up, Left, .. Repeated(UpLeft, 3), Left, Up, Left, UpLeft, Left, Up,
            Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, .. Repeated(Left, 3), Up, Left, Left, Up, .. Repeated(Left, 3), Up, Left, Left, Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left,
            Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up,
            UpLeft, UpLeft, Up, Left, Up, Up, Left, Up, UpLeft, Up, Left,
    ];

    /// <summary>In the open from the top hole to the bottom hole.</summary>
    private static readonly BotwoonPathStep[] OpenTopToBottom =
    [
            .. Repeated(Down, 3), Left, .. Repeated(Down, 4), Left, .. Repeated(Down, 3), Left, Down, Down, Left, Down, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 4),
            DownLeft, .. Repeated(Left, 5), DownLeft, .. Repeated(Left, 11), Down, .. Repeated(Left, 30), Up, .. Repeated(Left, 8), Down, .. Repeated(Left, 5), Down, Left, Left, Down, Left, Left,
            Down, Left, Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, .. Repeated(Down, 4), Left, .. Repeated(Down, 4), Left, .. Repeated(Down, 4), DownLeft, .. Repeated(Down, 10), Right, .. Repeated(Down, 3), Right, Down, Down, Right, Down, Right, Down, Down,
            Right, Down, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 17), Up, .. Repeated(Right, 7), Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 5), Up, .. Repeated(Right, 9), DownRight, Right, Right, Down, Right, Right, Down, Right,
            Down, Right, Down, Right, Down, Right,
    ];

    /// <summary>In the open from the top hole to the right hole.</summary>
    private static readonly BotwoonPathStep[] OpenTopToRight =
    [
            Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, DownRight, Right, Down, Down,
            Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down,
            Right, Down, Right, Down, Down, Right, Down, Down, Right, .. Repeated(Down, 3), Right, Down, Down, Right, .. Repeated(Down, 4), Right, .. Repeated(Down, 5), Right, .. Repeated(Down, 15), Left, .. Repeated(Down, 5), DownLeft, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), Left, Down, Down, Left, Down, Down, Left, Down, Down,
            Left, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Left, Down, .. Repeated(Left, 3), DownLeft, .. Repeated(Left, 4),
            Down, .. Repeated(Left, 7), Down, .. Repeated(Left, 12), DownLeft, .. Repeated(Left, 22), Up, .. Repeated(Left, 6), Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, Left, Up, Left, Left, Up, Left, Up, Left, Left,
            Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left, Up, Up,
            Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 9), Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up, Up,
            Right, Up, Up, Right, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right,
            Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up, .. Repeated(Right, 3), Up, .. Repeated(Right, 5), Up, .. Repeated(Right, 14), Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 4), Down, Right, Right, DownRight, Right, Right, Down, Right, Right, Down, Right, Right, Down,
            Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right,
            Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right,
            Down, Right, Down, Down, Right, Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down, Right,
            Down, Down, Right, Down, Down, DownRight, Down, Down, Right, .. Repeated(Down, 3), Right, Down, Down, DownRight,
            Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Right, Down, Right, Down, DownRight, Right,
            Down, Right, Down, Right, DownRight, Right, DownRight, .. Repeated(Right, 3), Down, .. Repeated(Right, 8), Up, Right, Right, UpRight, Right, Up, Right, Right, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
    ];

    /// <summary>In the open from the top hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenTopBackIntoItself =
    [
            .. Repeated(Right, 4), Down, .. Repeated(Right, 7), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 3),
            Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Down, Right, DownRight, DownRight, Down,
            Right, Down, Right, Down, Down, Right, Down, Down, Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 23), Right, Down, Down, Right, Down, Right, Down, Right, Down, DownRight, DownRight, Down, Right, Down,
            Right, Down, Right, Down, Right, Down, Down, Right, .. Repeated(Down, 3), DownRight, Down, Down, Left, .. Repeated(Down, 4), Left, Down, Down, Left, Down, Left, Down, DownLeft, Left, Down, Left, Down, Left, Left,
            Down, .. Repeated(Left, 4), Down, .. Repeated(Left, 12),
            Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, Left, Up, Left, Up, Left, Up, Up, Left, Up, UpLeft, Up, Up,
            Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), UpLeft, .. Repeated(Up, 4), Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 4),
            Left, .. Repeated(Up, 3), Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Left, Up, Left, Up,
            Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 12), Down, .. Repeated(Left, 3), Down, Left, Left, Down, Left, Left, Down,
            Left, Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 5), Left, .. Repeated(Down, 7), Left, Down, Down, Left, Down, Down,
            Left, Down, .. Repeated(Left, 3), Down, Left, UpLeft, .. Repeated(Left, 4), Up, Left, Left, Up, Left, Left,
            Up, Left, Up, Left, Up, Left, Up, Left, Up, Up, Left, Up, Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 15), Right, .. Repeated(Up, 6), Right, .. Repeated(Up, 3),
            Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, Right, Up,
            UpRight, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right, Up,
            .. Repeated(Right, 3), Up, .. Repeated(Right, 16), Down, .. Repeated(Right, 3),
    ];

    /// <summary>In the open from the right hole to the left hole.</summary>
    private static readonly BotwoonPathStep[] OpenRightToLeft =
    [
            Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Left, Up, Left, Up,
            Left, Left, Up, Left, Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up, Left, Left, Up, Left, Up,
            Left, Left, Up, Left, Up, Left, Up, Left, Left, Up, Left, Up, Left, Up, Left, Up, Left, Up, Left, Left, Up,
            Left, Up, Left, Left, Up, .. Repeated(Left, 3), UpLeft, .. Repeated(Left, 14), Down, .. Repeated(Left, 5), Down, Left, Left, Down, Left, Left, Down,
            Left, Down, Left, DownLeft, Down, Left, Down, Left, Down, Down, Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), Left, Down, Down, Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 3), DownLeft, .. Repeated(Down, 7), Left, .. Repeated(Down, 6),
            Right, .. Repeated(Down, 13), Right, .. Repeated(Down, 10), Right, .. Repeated(Down, 8),
            Right, .. Repeated(Down, 5), Right, .. Repeated(Down, 4), Right, .. Repeated(Down, 3), Right, Down,
            Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Right, Right, Down, Right, DownRight, Right,
            Right, Down, .. Repeated(Right, 10), Up, .. Repeated(Right, 3),
            Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Up, Right, Up,
            Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, .. Repeated(Up, 4), Right, .. Repeated(Up, 6), UpRight,
            Up, Left, .. Repeated(Up, 6), UpLeft, Up, Up, Left, Up, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left,
            Up, Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Left, Up, Left, Left, Up,
            .. Repeated(Left, 3), Up, .. Repeated(Left, 4), Up, .. Repeated(Left, 6), Up, .. Repeated(Left, 16), Down, .. Repeated(Left, 4),
            Down, .. Repeated(Left, 3), Down, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left,
            Down, Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Left, Down,
            Left, Left, Down, Left, Left, Down, Left, Left, Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 10), Up, Left, Left, Up, Left, UpLeft, Left, Up, Left, Up,
            Up, Left, Up, Left, Up, Up, Left, Up, Up, Left, .. Repeated(Up, 3), UpLeft, .. Repeated(Up, 8), UpRight,
            .. Repeated(Up, 3), Right, Up, Up, Right, .. Repeated(Up, 3), Right, Up, Up, Right, .. Repeated(Up, 4), Right, .. Repeated(Up, 12), Left, .. Repeated(Up, 3), Left, Up, Up, Left, Up, Left, Up, Up, Left, Up, Left,
            Left, Up, Left, Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 7),
    ];

    /// <summary>In the open from the right hole to the bottom hole.</summary>
    private static readonly BotwoonPathStep[] OpenRightToBottom =
    [
            Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 4), Up, .. Repeated(Left, 4), Up, .. Repeated(Left, 4), Up, .. Repeated(Left, 3), Up, Left,
            Left, Up, Left, Left, Up, Left, Up, Left, .. Repeated(UpLeft, 3), Up, Left, Up, Left, Up, UpLeft, Up, Left,
            Up, Up, Left, Up, Up, Left, Up, Up, Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 5), Left, .. Repeated(Up, 4), UpRight, .. Repeated(Up, 3), Right, Up, Up, Right, Up, Right, Right, Up, Right, Right, Up, .. Repeated(Right, 9), Down, Right, Right, Down, Right, Right, Down, Right, Down,
            Down, Right, Down, Down, Right, .. Repeated(Down, 4), Right, .. Repeated(Down, 20), Left, .. Repeated(Down, 3), Left,
            Down, Down, Left, Down, Down, Left, Down, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left,
            Down, Left, Left, Down, Left, Left, Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 3), DownLeft, .. Repeated(Left, 4), Down, .. Repeated(Left, 6), Down, .. Repeated(Left, 6), DownLeft,
            .. Repeated(Left, 8), Down, .. Repeated(Left, 17), Up, .. Repeated(Left, 3), Up, Left, Left, Up, Left, UpLeft,
            Left, Up, Left, Up, Left, Up, Left, Up, UpLeft, Up, Left, Up, Left, Up, Up, Left, Up, Left, Up, Up, Left,
            Up, UpLeft, Up, Left, Up, Up, Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 10), Right, Up, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up, .. Repeated(Right, 3), Up, .. Repeated(Right, 6), Down, Right, Right, Down, Right, Right, Down,
            Right, Down, Right, Down, Right, Down, DownRight, Down, Right, Down, Down, Right, Down, DownRight, Down,
            Down, Right, Down, Down, DownRight, .. Repeated(Down, 3), Right, .. Repeated(Down, 9), Right, .. Repeated(Down, 13), Right, .. Repeated(Down, 6), Right, .. Repeated(Down, 6), Right, .. Repeated(Down, 6), Right, .. Repeated(Down, 18), Left, .. Repeated(Down, 3), Left, Down, Down, Left, Left, Down, .. Repeated(Left, 5),
            Up, .. Repeated(Left, 3), Up, Left, Left, UpLeft, Left, UpLeft, Left, Up, Left, Left, Up, Left, Left, Up, Left,
    ];

    /// <summary>In the open from the right hole to the top hole.</summary>
    private static readonly BotwoonPathStep[] OpenRightToTop =
    [
            Left, Down, Left, Down, Left, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left,
            Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, Left, Down, Left, Down, Left, DownLeft, Left,
            Down, Left, Left, Down, Left, Down, Left, Left, Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 4), Down, .. Repeated(Left, 19), Up, .. Repeated(Left, 5), Up, .. Repeated(Left, 3), Up, Left, Left, Up,
            Left, Left, Up, Left, Up, Left, Up, Left, UpLeft, Left, Up, Up, Left, Up, Left, Up, Left, Up, UpLeft, Up,
            Left, Up, Up, Left, Up, Up, Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 15), Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up, Up, Right, Up, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, UpRight, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Up,
            Right, Right, Up, Right, Right, Up, Right, Right, UpRight, .. Repeated(Right, 3), Up, .. Repeated(Right, 11), Down, .. Repeated(Right, 3), Down, Right, Right, Down,
            Right, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            Down, Right, Down, DownRight, Down, Down, Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 14), Left, .. Repeated(Down, 4), Left, Down, Down, Left,
            Down, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down, Left, Left, Down, Left, Down, Left,
            Left, Down, .. Repeated(Left, 5), Down, .. Repeated(Left, 3), Up, .. Repeated(Left, 5), Up,
            Left, Left, Up, Left, Left, Up, Left, UpLeft, UpLeft, Up, Left, Up, Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 11), Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up, Right, Up,
            UpRight, UpRight, Right, Up, Right, UpRight, .. Repeated(Right, 5), Down, Right, Right, Down,
            Right, DownRight, Right, Down, Right, Down, Down, Right, Down, Right, .. Repeated(Down, 6),
            Left, Down, Down, Left, DownLeft, Left, Down, Left, Left, Down, .. Repeated(Left, 5), Up, Left,
            Left, Up, Up, Left, Up, Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 5), Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 4), Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), Left, .. Repeated(Up, 3), Left, Up, Up, Left, Up, Up, UpLeft, Up, Up, Left,
            Up,
    ];

    /// <summary>In the open from the right hole back into it.</summary>
    private static readonly BotwoonPathStep[] OpenRightBackIntoItself =
    [
            Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, Up, Left, Left, Up, Left, Up,
            Left, Up, Left, Left, Up, Left, Up, Left, UpLeft, Left, Up, Left, UpLeft, Left, Up, Left, Left, Up, Left,
            Left, Up, Left, Left, UpLeft, .. Repeated(Left, 3), Up, .. Repeated(Left, 5), UpLeft, .. Repeated(Left, 7), Up, .. Repeated(Left, 12), Down,
            .. Repeated(Left, 5), DownLeft, .. Repeated(Left, 5), Down, .. Repeated(Left, 6), Down, .. Repeated(Left, 3), DownLeft, Left, Left, DownLeft, Left, DownLeft, Left, Down, Left, Left, Down,
            Left, Left, DownLeft, .. Repeated(Left, 3), Down, .. Repeated(Left, 17), DownLeft, .. Repeated(Left, 4), Down, .. Repeated(Left, 3), Down,
            Left, Left, Down, Left, Left, Down, Left, Left, Down, Left, Down, Left, DownLeft, Left, Down, Left, Down,
            DownLeft, DownLeft, Down, DownLeft, .. Repeated(Down, 9), DownRight, Down,
            Down, Right, Down, DownRight, DownRight, Down, Right, DownRight, Right, Down, Right, Right, Down, Right,
            Right, Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 16), Down, .. Repeated(Right, 6),
            Down, .. Repeated(Right, 3), Down, Right, Right, Down, Right, DownRight, Right, Down, Right, DownRight, Right,
            Down, Right, Down, Right, Down, Right, Down, Down, Right, Down, Right, Down, Down, Right, Down, Down, Right,
            .. Repeated(Down, 4), Right, .. Repeated(Down, 10), Left, .. Repeated(Down, 3), DownLeft, Down, Down, Left, Down, Down, Left, Down, Left, Down, .. Repeated(DownLeft, 3), Left,
            Down, Left, Down, Left, Left, Down, Left, Left, Down, .. Repeated(Left, 3), DownLeft, .. Repeated(Left, 9), UpLeft, .. Repeated(Left, 3), Up, Left, Left, UpLeft, Left, Up, Left, .. Repeated(UpLeft, 3), Up, Left, Up, UpLeft, Up, UpLeft, Up, Up, Left, .. Repeated(Up, 14), Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up, UpRight, Up, Right, Up, Right, Up, UpRight, Up,
            Right, Up, Right, UpRight, UpRight, Right, Up, Right, UpRight, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Right, Up, Right, Right, UpRight, Right, Right, UpRight, .. Repeated(Right, 3), Up, .. Repeated(Right, 4), UpRight, .. Repeated(Right, 5), UpRight, .. Repeated(Right, 8), Up, .. Repeated(Right, 10), Up, .. Repeated(Right, 16), DownRight,
            .. Repeated(Right, 6), Down, .. Repeated(Right, 7), Down,
            .. Repeated(Right, 10), UpRight, .. Repeated(Right, 3), UpRight,
            Right, Right, Up, Right, Right, UpRight, Right, Up, Right, UpRight, UpRight, Right, Up, UpRight, Up, Right,
            Up, Up, Right, .. Repeated(Up, 4), Right, .. Repeated(Up, 4),
    ];

    /// <summary>Inside the wall between the left and bottom holes, forward from the left hole.</summary>
    private static readonly BotwoonPathStep[] WallLeftBottom =
    [
            Right, Down, .. Repeated(Right, 3), Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Down, Down, Right, Down, DownRight, Down, Down, Right, Down, Down, Right, Down,
            Down, Right, Down, Down, Right, Down, Right, Down, Right, Right, Down, .. Repeated(Right, 4), Down,
            .. Repeated(Right, 5), Down, Right, Right, Down, Right, DownRight, Down, Right, .. Repeated(Down, 3), Right, .. Repeated(Down, 6), Left, Down, Down, Left, Down, Down, Left, Left, Down, Left,
            Down, .. Repeated(Left, 3), Down, .. Repeated(Left, 3), Down, Left, Left, Down, Left, Left, Down, Left, Down, Left,
            Down, Left, Down, Down, Left, Down, Down, Left, .. Repeated(Down, 9), Right,
            Down, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, Right, Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 3), Up, .. Repeated(Right, 5), Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Right, Up,
            Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, .. Repeated(Right, 4), Down, Right,
            Down, Right, Down, Right, Down, Right, Down,
    ];

    /// <summary>Inside the wall between the left and top holes, forward from the left hole.</summary>
    private static readonly BotwoonPathStep[] WallLeftTop =
    [
            Right, Up, .. Repeated(Right, 3), Up, .. Repeated(Right, 3), Up, Right, Right, UpRight, .. Repeated(Right, 3), Up,
            .. Repeated(Right, 5), Up, .. Repeated(Right, 6), Down, .. Repeated(Right, 3),
            Down, Right, Right, Down, Right, Right, Down, Right, Down, Right, Down, Right, Down, Down, Right, Down,
            Down, Right, .. Repeated(Down, 7), Left, .. Repeated(Down, 4), Left, .. Repeated(Down, 7), Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Down, Right, Right, Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 6), Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 3), Up, Right, UpRight, Right, Up,
            Right, UpRight, Right, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up, Up, Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 15), Left, .. Repeated(Up, 6), Left, .. Repeated(Up, 5), Right, .. Repeated(Up, 3), Right, Up, Up, Right, Up, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up,
            .. Repeated(Right, 3), Up, .. Repeated(Right, 4), Down, .. Repeated(Right, 4), Down, Right, Right,
            DownRight, Right, Right, Down, Right,
    ];

    /// <summary>Inside the wall between the left and right holes, forward from the left hole.</summary>
    private static readonly BotwoonPathStep[] WallLeftRight =
    [
            Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 3), Down, Right, Right, Down, Right, Right, Down, Right,
            Right, Down, Right, Right, Down, Right, DownRight, Right, Down, Right, Down, Right, Right, Down, Right,
            Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, Right,
            Right, Down, .. Repeated(Right, 3), Down, Right, DownRight, Right, Down, Right, Right, Down, Right, Down,
            Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Down, Right, Right,
            Down, Right, Down, Right, Down, Right, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down,
            Right, Right, Down, Right, Right, Down, Right, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down,
            Right, Right, Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 15), Up, .. Repeated(Right, 8), Up, .. Repeated(Right, 21), Down, .. Repeated(Right, 5), Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 4), Down, .. Repeated(Right, 3),
            Down, .. Repeated(Right, 4), DownRight, .. Repeated(Right, 11), Up, .. Repeated(Right, 3),
    ];

    /// <summary>Inside the wall between the bottom and top holes, forward from the bottom hole.</summary>
    private static readonly BotwoonPathStep[] WallBottomTop =
    [
            Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right,
            .. Repeated(UpRight, 4), Up, Right, Up, Right, Up, Right, Up, Up, Right, Up, UpRight, Up,
            UpRight, Up, Up, Right, .. Repeated(Up, 10), Left, Up, Up, Left, Up, Up, Left, Up, Left,
            Up, Left, Up, Left, Up, Left, Up, Left, Left, Up, .. Repeated(Left, 3), Up, .. Repeated(Left, 5), Up,
            .. Repeated(Left, 3), Up, Left, Left, Up, Left, UpLeft, Up, Left, Up, UpLeft, .. Repeated(Up, 3), Left, .. Repeated(Up, 6), Right, .. Repeated(Up, 3), Right, Up, UpRight, Right, Up, Right, Right, Up, .. Repeated(Right, 5), DownRight, .. Repeated(Right, 11), Up, .. Repeated(Right, 3), Up, Right, Up, Right, UpRight, Right, Up, Right, Up, Right, Up, Up, Right, Up, Right, Up,
            Right, Up, Up, Right, Up, Right, Up,
    ];

    /// <summary>Inside the wall between the bottom and right holes, forward from the bottom hole.</summary>
    private static readonly BotwoonPathStep[] WallBottomRight =
    [
            Right, Up, Right, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up, Right, Right,
            Up, Right, Up, Right, Right, Up, Right, Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
            Right, Up, Right, Up, Up, Right, .. Repeated(Up, 3), Right, .. Repeated(Up, 9), Left, .. Repeated(Up, 4), Right, .. Repeated(Up, 4), Right, Up, Up, Right, Up, Right, Right, Up, Right, Right, Up, .. Repeated(Right, 4), Up, .. Repeated(Right, 14), Down, Right, Right, Down, Right, Right, Down, Right, DownRight, Down, Right, Down, Right, Down, Down,
            Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Down, Right, Down, Right, Down,
            DownRight, Right, Down, Right, Down, Right, Right, Down, Right, Right, Down, .. Repeated(Right, 4),
            Down, .. Repeated(Right, 6), Up, .. Repeated(Right, 3), Up, Right, Right, Up, Right, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up,
    ];

    /// <summary>Inside the wall between the top and right holes, forward from the top hole.</summary>
    private static readonly BotwoonPathStep[] WallTopRight =
    [
            Down, Left, Down, Left, Down, Left, Down, Down, Left, Down, Left, Down, Down, Left, Down, Down, Left, Down,
            Down, Left, .. Repeated(Down, 3), Left, .. Repeated(Down, 12),
            Right, .. Repeated(Down, 3), Right, Down, Right, Down, Down, Right, Down, Right, Down, Right, Down, Right, Down,
            Right, Right, Down, Right, Right, Down, .. Repeated(Right, 3), Down, .. Repeated(Right, 9), Up, .. Repeated(Right, 3), Up, Right, Right, Up, Right, Up, Right, Up, Right, Up, Right,
            Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Up, Right, Right, Up, Right, Right, Up,
            .. Repeated(Right, 4), Up, .. Repeated(Right, 4), Down, Right, Right, .. Repeated(Down, 9), Left, .. Repeated(Down, 13), DownRight, Down, Down, Right, Down, Right, Down, DownRight, Down, Right, Down, Right, Right, Down,
            Right, Down, Right, Right, Down, Right, Right, Down, .. Repeated(Right, 5), Up, Right, Right,
            Up, Up, Right, Up, Up, Right, .. Repeated(Up, 3), UpRight, Up, Up,
    ];

    /// <summary>
    /// Every path Botwoon walks between his holes, through the open room and inside the wall.
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
