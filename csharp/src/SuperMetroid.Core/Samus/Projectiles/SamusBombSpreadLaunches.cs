using static SuperMetroid.Core.Samus.Projectiles.SamusBombSpreadLaunchDefinitions;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The fuse and launch of each bomb of a Bomb Spread.</summary>
internal static class SamusBombSpreadLaunches
{
    /// <summary>
    /// $90:D8CF..D8F6 BombSpreadData: five symmetric launches. Fuse length increases
    /// ten frames per step from center; outward X speed grows by half a pixel/frame, leftward for the two left slots.
    /// Y rises two-and-a-half at center, one beside it, and zero at the edges.
    /// </summary>
    internal static BombSpreadLaunchDefinition ForSlot(int index)
    {
        if ((uint)index >= 5) throw new ArgumentOutOfRangeException(nameof(index));
        int radius = Math.Abs(index - 2);
        return new((ushort)(100 + radius * 10),
            BombSpreadHorizontalVelocity.Of(index < 2, radius / 2, radius % 2 * 128),
            radius == 0 ? CentreRise : new UnsignedSixteenSixteen(2 - radius, 0));
    }
}
