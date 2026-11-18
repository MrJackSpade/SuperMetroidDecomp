namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The fuse and launch of each bomb of a Bomb Spread.</summary>
internal static class SamusBombSpreadLaunches
{
    /// <summary>The launch authored for the bomb at <paramref name="place"/>, counted from the spread's leftmost bomb.</summary>
    /// <param name="place">The bomb's place, zero (leftmost) through four (rightmost).</param>
    internal static BombSpreadLaunchDefinition ForPlace(int place)
    {
        return SamusBombSpreadLaunchDefinitions.LeftToRight[Ensure.BetweenInclusive(0, MorphBallBombDefinitions.PlaceCount - 1, place)];
    }

    /// <summary>
    /// The flight the bomb at <paramref name="place"/> launches on after Down was held <paramref name="heldDownUpdates"/>
    /// updates: its authored velocities, its rise faster by one whole pixel per update for each
    /// <see cref="MorphBallBombDefinitions.DownChargeUpdatesPerLaunchPixel"/> updates held, its fraction unchanged. The
    /// centre bomb rises at 1.5 pixels per update unheld and at 4.5 after the full hold.
    /// </summary>
    /// <param name="place">The bomb's place, zero (leftmost) through four (rightmost).</param>
    /// <param name="heldDownUpdates">
    /// The updates Down was held, zero through <see cref="MorphBallBombDefinitions.DownChargeTimeoutUpdates"/>.
    /// </param>
    internal static BombSpreadFlight Launch(int place, int heldDownUpdates)
    {
        BombSpreadLaunchDefinition launch = ForPlace(place);
        int bonusPixels = Ensure.BetweenInclusive(0, MorphBallBombDefinitions.DownChargeTimeoutUpdates, heldDownUpdates) /
            MorphBallBombDefinitions.DownChargeUpdatesPerLaunchPixel;
        return new BombSpreadFlight(launch.HorizontalVelocity,
            launch.VerticalVelocity - SignedSixteenSixteen.FromWholePixels(bonusPixels));
    }
}
