using static SuperMetroid.Core.Samus.Projectiles.SamusBombSpreadLaunchDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The fuse and launch of each bomb of a Bomb Spread.</summary>
internal static class SamusBombSpreadLaunches
{
    /// <summary>
    /// The bomb spread's five symmetric launches. Fuse length increases
    /// ten frames per step from center; outward X speed grows by half a pixel/frame, leftward for the two left places.
    /// Y rises two-and-a-half at center, one beside it, and zero at the edges.
    /// </summary>
    /// <param name="place">The bomb's place, zero through four.</param>
    internal static BombSpreadLaunchDefinition ForPlace(int place)
    {
        if ((uint)place >= MorphBallBombDefinitions.PlaceCount) throw new ArgumentOutOfRangeException(nameof(place));
        int radius = Math.Abs(place - 2);
        return new(100 + radius * 10,
            BombSpreadHorizontalVelocity.Of(place < 2, radius / 2, radius % 2 * 128),
            radius == 0 ? CentreRise : new UnsignedSixteenSixteen(2 - radius, 0));
    }
}
