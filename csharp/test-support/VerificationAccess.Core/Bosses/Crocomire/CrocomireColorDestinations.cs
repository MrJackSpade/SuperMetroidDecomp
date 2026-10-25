namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Where Crocomire's colours land in the colour memory, which only verification reads.</summary>
internal static class CrocomireColorDestinations
{
    /// <summary>The first colour the body and head restoration writes.</summary>
    public const int FightBodyDestination = 112;
    /// <summary>The first colour the initial wall transfer writes.</summary>
    public const int InitialWallDestination = 160;
    /// <summary>The first colour the initial projectile transfer writes.</summary>
    public const int InitialProjectileDestination = 208;
    /// <summary>The first colour the skeleton arm's palette writes.</summary>
    public const int SkeletonArmDestination = 144;
    /// <summary>The first colour the wall spikes' palette writes.</summary>
    public const int WallSpikesDestination = 176;
}
