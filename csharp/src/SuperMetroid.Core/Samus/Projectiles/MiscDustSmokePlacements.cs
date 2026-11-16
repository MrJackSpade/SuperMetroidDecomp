namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where a puff of smoke lands around the point it is spawned at (#2044): a point, or a box centred on it that one
/// random sample scatters the puff across, one part of the sample across the box and another down it.
/// </summary>
internal static class MiscDustSmokePlacements
{
    /// <summary>The puff appears at the point itself: its fields read no bits, so each has one outcome, zero.</summary>
    internal static MiscDustPlacementDefinition Point { get; } = new(new(0, 0), new(8, 0), default);

    /// <summary>The puff lands anywhere in a centred 8x8-pixel square.</summary>
    internal static MiscDustPlacementDefinition CentredEightPixelSquare { get; } = new(new(0, 3), new(8, 3), new(-4, -4));

    /// <summary>The puff lands anywhere in a centred 16x16-pixel square.</summary>
    internal static MiscDustPlacementDefinition CentredSixteenPixelSquare { get; } = new(new(0, 4), new(8, 4), new(-8, -8));

    /// <summary>The puff lands anywhere in a centred box 16 pixels wide and 32 tall.</summary>
    internal static MiscDustPlacementDefinition CentredSixteenByThirtyTwoBox { get; } = new(new(0, 4), new(8, 5), new(-8, -16));

    /// <summary>The puff lands anywhere in a centred box 32 pixels wide and 64 tall.</summary>
    internal static MiscDustPlacementDefinition CentredThirtyTwoBySixtyFourBox { get; } = new(new(0, 5), new(8, 6), new(-16, -32));
}
