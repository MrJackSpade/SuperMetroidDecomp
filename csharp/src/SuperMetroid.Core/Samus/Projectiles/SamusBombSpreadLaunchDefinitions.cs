namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Fixed Bomb Spread mechanics, not editable explosion artwork or animation.</summary>
internal static partial class SamusBombSpreadLaunchDefinitions
{
    /// <summary>The leftmost bomb: the longest fuse, 120 frames, one pixel per frame to the left and no rise.</summary>
    internal static BombSpreadLaunchDefinition FarLeft { get; } = new(120,
        SignedSixteenSixteen.FromWholePixels(-1),
        SignedSixteenSixteen.Zero);

    /// <summary>The bomb left of centre: a 110-frame fuse, half a pixel per frame to the left and a rise of one pixel per frame.</summary>
    internal static BombSpreadLaunchDefinition NearLeft { get; } = new(110,
        SignedSixteenSixteen.FromPixelFraction(-1, 2),
        SignedSixteenSixteen.FromWholePixels(-1));

    /// <summary>
    /// The centre bomb: the shortest fuse, 100 frames, no sideways travel and a rise of one and a half pixels per frame.
    /// </summary>
    internal static BombSpreadLaunchDefinition Centre { get; } = new(100,
        SignedSixteenSixteen.Zero,
        SignedSixteenSixteen.FromPixelFraction(-3, 2));

    /// <summary>The bomb right of centre: a 110-frame fuse, half a pixel per frame to the right and a rise of one pixel per frame.</summary>
    internal static BombSpreadLaunchDefinition NearRight { get; } = new(110,
        SignedSixteenSixteen.FromPixelFraction(1, 2),
        SignedSixteenSixteen.FromWholePixels(-1));

    /// <summary>The rightmost bomb: the longest fuse, 120 frames, one pixel per frame to the right and no rise.</summary>
    internal static BombSpreadLaunchDefinition FarRight { get; } = new(120,
        SignedSixteenSixteen.FromWholePixels(1),
        SignedSixteenSixteen.Zero);

    /// <summary>The spread's five launches from its leftmost place to its rightmost.</summary>
    internal static IReadOnlyList<BombSpreadLaunchDefinition> LeftToRight { get; } =
        [FarLeft, NearLeft, Centre, NearRight, FarRight];
}
