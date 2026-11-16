namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="MiscDustSmokePlacements"/> that only verification reads.</summary>
internal static class MiscDustSmokePlacementsConstants
{
    /// <summary>The puff lands anywhere in a centred 8x8-pixel square.</summary>
    internal static MiscDustPlacementDefinition CentredEightPixelSquare { get; } = new(new(0, 3), new(8, 3), new(-4, -4));
    /// <summary>The puff lands anywhere in a centred 16x16-pixel square.</summary>
    internal static MiscDustPlacementDefinition CentredSixteenPixelSquare { get; } = new(new(0, 4), new(8, 4), new(-8, -8));
    /// <summary>The puff lands anywhere in a centred box 32 pixels wide and 64 tall.</summary>
    internal static MiscDustPlacementDefinition CentredThirtyTwoBySixtyFourBox { get; } = new(new(0, 5), new(8, 6), new(-16, -32));
}
