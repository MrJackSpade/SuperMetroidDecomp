namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Fixed Bomb Spread mechanics, not editable explosion artwork or animation.</summary>
internal static class SamusBombSpreadLaunchDefinitions
{
    /// <summary>The leftmost bomb: the longest fuse, 120 frames, one pixel per frame to the left and no rise.</summary>
    internal static BombSpreadLaunchDefinition FarLeft { get; } = new(120,
        BombSpreadHorizontalVelocity.Leftward(wholePixels: 1, fractionSteps: 0),
        UnsignedSixteenSixteen.FromSixteenthsOfPixel(0));

    /// <summary>The bomb left of centre: a 110-frame fuse, half a pixel per frame to the left and a one-pixel rise.</summary>
    internal static BombSpreadLaunchDefinition NearLeft { get; } = new(110,
        BombSpreadHorizontalVelocity.Leftward(wholePixels: 0, fractionSteps: 128),
        UnsignedSixteenSixteen.FromSixteenthsOfPixel(16));

    /// <summary>
    /// The centre bomb: the shortest fuse, 100 frames, no sideways travel (held as rightward at no speed) and a
    /// two-and-a-half-pixel rise.
    /// </summary>
    internal static BombSpreadLaunchDefinition Centre { get; } = new(100,
        BombSpreadHorizontalVelocity.Rightward(wholePixels: 0, fractionSteps: 0),
        UnsignedSixteenSixteen.FromSixteenthsOfPixel(40));

    /// <summary>The bomb right of centre: a 110-frame fuse, half a pixel per frame to the right and a one-pixel rise.</summary>
    internal static BombSpreadLaunchDefinition NearRight { get; } = new(110,
        BombSpreadHorizontalVelocity.Rightward(wholePixels: 0, fractionSteps: 128),
        UnsignedSixteenSixteen.FromSixteenthsOfPixel(16));

    /// <summary>The rightmost bomb: the longest fuse, 120 frames, one pixel per frame to the right and no rise.</summary>
    internal static BombSpreadLaunchDefinition FarRight { get; } = new(120,
        BombSpreadHorizontalVelocity.Rightward(wholePixels: 1, fractionSteps: 0),
        UnsignedSixteenSixteen.FromSixteenthsOfPixel(0));

    /// <summary>The spread's five launches from its leftmost place to its rightmost.</summary>
    internal static IReadOnlyList<BombSpreadLaunchDefinition> LeftToRight { get; } =
        [FarLeft, NearLeft, Centre, NearRight, FarRight];
}
