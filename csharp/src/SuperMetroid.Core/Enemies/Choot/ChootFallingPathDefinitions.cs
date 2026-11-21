namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The paths a Choot follows down from its jump's apex, one sample after another.</summary>
internal static partial class ChootFallingPathDefinitions
{
    /// <summary>The normal path's swing out to the right, from the falling origin to just before its rightmost sample.</summary>
    private static readonly ChootFallSample[] NormalSwingOut =
    [
        At(new(0, 0)), At(new(1, 1)), At(new(2, 1)), At(new(3, 2)), At(new(4, 2)), At(new(5, 2)), At(new(6, 3)),
        At(new(7, 3)), At(new(8, 3)), At(new(9, 3)), At(new(10, 3)), At(new(11, 3)), At(new(12, 3), updates: 2),
        At(new(13, 3), updates: 3), At(new(14, 3), updates: 3), At(new(15, 3), updates: 2), At(new(16, 3), updates: 4),
    ];

    /// <summary>The normal path's swing back across to the left, from its rightmost sample to just before its leftmost.</summary>
    private static readonly ChootFallSample[] NormalSwingBack =
    [
        At(new(16, 5)), At(new(15, 6)), At(new(14, 8)), At(new(12, 9)), At(new(11, 10)), At(new(10, 12)), At(new(8, 13)),
        At(new(7, 14)), At(new(6, 14)), At(new(4, 15)), At(new(3, 16)), At(new(2, 16)), At(new(1, 17)), At(new(0, 17)),
        At(new(-1, 17)), At(new(-2, 18)), At(new(-4, 18), updates: 2), At(new(-5, 18)), At(new(-6, 18)), At(new(-7, 18)),
        At(new(-8, 18)), At(new(-9, 18), updates: 3), At(new(-10, 18), updates: 2), At(new(-11, 18), updates: 2),
        At(new(-12, 18), updates: 3),
    ];

    /// <summary>The normal path's final drop, from its leftmost sample to the loop's end.</summary>
    private static readonly ChootFallSample[] NormalDrop =
    [
        At(new(-12, 20)), At(new(-11, 22)), At(new(-10, 23)), At(new(-9, 25)), At(new(-7, 26)), At(new(-6, 27)),
        At(new(-5, 28)), At(new(-3, 29)), At(new(-2, 30)), At(new(-1, 31)),
    ];

    /// <summary>
    /// The normal falling pattern: a swing out to the right, back across to the left and a drop, holding its rightmost and
    /// leftmost samples for the updates each lists. The loop's Y distance is separate from the path.
    /// </summary>
    private static readonly ChootFallSample[] Normal =
        [.. NormalSwingOut, At(new(17, 3), updates: 2), .. NormalSwingBack, At(new(-13, 18), updates: 3), .. NormalDrop];

    /// <summary>The normal path holding its rightmost and leftmost samples longer.</summary>
    private static readonly ChootFallSample[] Slow =
        [.. NormalSwingOut, At(new(17, 3), updates: 12), .. NormalSwingBack, At(new(-13, 18), updates: 13), .. NormalDrop];

    /// <summary>The normal path holding its rightmost and leftmost samples longer still.</summary>
    private static readonly ChootFallSample[] VerySlow =
        [.. NormalSwingOut, At(new(17, 3), updates: 32), .. NormalSwingBack, At(new(-13, 18), updates: 33), .. NormalDrop];

    /// <summary>The wide falling pattern, each sample held until the Choot moves on.</summary>
    private static readonly ChootFallSample[] Wide =
    [
        At(new(0, 0)), At(new(3, 0)), At(new(5, 1)), At(new(7, 1)), At(new(9, 2)), At(new(11, 2)), At(new(13, 2)),
        At(new(14, 2)), At(new(16, 3)), At(new(18, 3)), At(new(19, 3)), At(new(21, 3)), At(new(22, 3)), At(new(23, 3)),
        At(new(24, 3)), At(new(25, 3), updates: 2), At(new(26, 3)), At(new(27, 3)), At(new(28, 3)), At(new(29, 3)),
        At(new(30, 3), updates: 2), At(new(31, 3), updates: 2), At(new(32, 3), updates: 3), At(new(30, 4)), At(new(28, 6)),
        At(new(26, 7)), At(new(23, 8)), At(new(21, 10)), At(new(19, 11)), At(new(16, 12)), At(new(14, 12)), At(new(11, 13)),
        At(new(9, 14)), At(new(7, 14)), At(new(5, 15)), At(new(2, 15)), At(new(0, 16)), At(new(-2, 16)), At(new(-4, 16)),
        At(new(-6, 16)), At(new(-7, 17)), At(new(-9, 17)), At(new(-11, 17)), At(new(-12, 17)), At(new(-14, 17)),
        At(new(-15, 17)), At(new(-16, 17), updates: 2), At(new(-17, 17)), At(new(-18, 17)), At(new(-19, 17)),
        At(new(-20, 17)), At(new(-21, 17)), At(new(-22, 17), updates: 2), At(new(-23, 17)), At(new(-24, 17), updates: 2),
        At(new(-22, 19)), At(new(-20, 20)), At(new(-18, 22)), At(new(-16, 23)), At(new(-14, 24)), At(new(-11, 25)),
        At(new(-9, 26)), At(new(-6, 27)), At(new(-4, 28)), At(new(-2, 29)),
    ];

    /// <summary>The very wide falling pattern, each sample held until the Choot moves on.</summary>
    private static readonly ChootFallSample[] VeryWide =
    [
        At(new(0, 0)), At(new(3, 1)), At(new(6, 1)), At(new(9, 2)), At(new(12, 2)), At(new(15, 3)), At(new(17, 3)),
        At(new(20, 3)), At(new(22, 3)), At(new(24, 3)), At(new(26, 3)), At(new(28, 4)), At(new(30, 4)), At(new(32, 4)),
        At(new(34, 4)), At(new(35, 4)), At(new(36, 4)), At(new(37, 4)), At(new(38, 4)), At(new(40, 4)), At(new(41, 4)),
        At(new(42, 4), updates: 2), At(new(43, 4)), At(new(44, 3)), At(new(45, 3), updates: 3), At(new(42, 5)),
        At(new(39, 7)), At(new(36, 9)), At(new(33, 10)), At(new(30, 11)), At(new(26, 13)), At(new(23, 14)), At(new(20, 15)),
        At(new(16, 15)), At(new(13, 16)), At(new(10, 17)), At(new(6, 17)), At(new(3, 18)), At(new(0, 18)), At(new(-3, 19)),
        At(new(-5, 19)), At(new(-8, 19)), At(new(-10, 19)), At(new(-13, 19)), At(new(-15, 20)), At(new(-17, 20)),
        At(new(-19, 20)), At(new(-21, 20)), At(new(-23, 20)), At(new(-24, 20)), At(new(-25, 20)), At(new(-27, 20)),
        At(new(-28, 20)), At(new(-29, 20)), At(new(-30, 20)), At(new(-31, 19)), At(new(-32, 19)), At(new(-33, 19)),
        At(new(-34, 19), updates: 2), At(new(-31, 21)), At(new(-28, 23)), At(new(-25, 24)), At(new(-22, 26)),
        At(new(-18, 27)), At(new(-15, 28)), At(new(-12, 29)), At(new(-8, 30)), At(new(-5, 31)), At(new(-2, 32)),
    ];

    /// <summary>
    /// Each falling path: its samples, the update after its last ending one loop of the path, and its loop origin's advance.
    /// Slow variants repeat normal-path plateau frames and use the same vertical advance.
    /// </summary>
    internal static readonly IdentityTable<ChootFallingPath, ChootFallingPathDefinition> Paths = new(new Dictionary<ChootFallingPath, ChootFallingPathDefinition>
    {
        [ChootFallingPath.Normal] = new(Normal, ChootPatternDefinitions.NormalLoopYDistance),
        [ChootFallingPath.Wide] = new(Wide, ChootPatternDefinitions.WideLoopYDistance),
        [ChootFallingPath.VeryWide] = new(VeryWide, ChootPatternDefinitions.VeryWideLoopYDistance),
        [ChootFallingPath.Slow] = new(Slow, ChootPatternDefinitions.NormalLoopYDistance),
        [ChootFallingPath.VerySlow] = new(VerySlow, ChootPatternDefinitions.NormalLoopYDistance),
    });

    /// <summary>A sample at <paramref name="offset"/> from the falling origin, held <paramref name="updates"/> updates.</summary>
    private static ChootFallSample At(Offset16<RoomPixels> offset, int updates = 1) => new(offset, updates);
}
