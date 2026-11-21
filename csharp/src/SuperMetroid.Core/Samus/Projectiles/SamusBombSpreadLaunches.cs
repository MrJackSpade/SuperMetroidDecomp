namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The fuse and launch of each bomb of a Bomb Spread.</summary>
internal static partial class SamusBombSpreadLaunches
{
    /// <summary>
    /// The flight a bomb authored to launch on <paramref name="launch"/> launches on after Down was held <paramref name="heldDownUpdates"/>
    /// updates: its authored velocities, its rise faster by one whole pixel per update for each
    /// <see cref="MorphBallBombDefinitions.DownChargeUpdatesPerLaunchPixel"/> updates held, its fraction unchanged. The
    /// centre bomb rises slowest unheld and fastest after the full hold.
    /// </summary>
    /// <param name="launch">The bomb's authored launch.</param>
    /// <param name="heldDownUpdates">
    /// The updates Down was held, zero through <see cref="MorphBallBombDefinitions.DownChargeTimeoutUpdates"/>.
    /// </param>
    internal static BombSpreadFlight Launch(BombSpreadLaunchDefinition launch, int heldDownUpdates)
    {
        int bonusPixels = Ensure.BetweenInclusive(0, MorphBallBombDefinitions.DownChargeTimeoutUpdates, heldDownUpdates) /
            MorphBallBombDefinitions.DownChargeUpdatesPerLaunchPixel;
        return new BombSpreadFlight(launch.HorizontalVelocity,
            launch.VerticalVelocity - SignedSixteenSixteen.FromWholePixels(bonusPixels));
    }
}
