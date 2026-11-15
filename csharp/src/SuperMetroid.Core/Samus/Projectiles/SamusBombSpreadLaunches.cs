namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The fuse and launch of each bomb of a Bomb Spread.</summary>
internal static class SamusBombSpreadLaunches
{
    /// <summary>The launch authored for the bomb at <paramref name="place"/>, counted from the spread's leftmost bomb.</summary>
    /// <param name="place">The bomb's place, zero (leftmost) through four (rightmost).</param>
    internal static BombSpreadLaunchDefinition ForPlace(int place)
    {
        if ((uint)place >= MorphBallBombDefinitions.PlaceCount) throw new ArgumentOutOfRangeException(nameof(place));
        return SamusBombSpreadLaunchDefinitions.LeftToRight[place];
    }
}
