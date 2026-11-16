namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What a signalling cue of Crocomire's body animations does.</summary>
internal enum CrocomireBodyCueKind
{
    /// <summary>Runs the fight AI's current phase, which may start another animation.</summary>
    FightAi,
    /// <summary>Starts the projectile attack on a one-in-four random draw.</summary>
    MaybeStartProjectileAttack,
    /// <summary>Queues Crocomire's cry.</summary>
    Cry,
    /// <summary>Queues the big explosion sound.</summary>
    BigExplosionSound,
    /// <summary>Queues the skeleton-collapse sound.</summary>
    SkeletonCollapseSound,
    /// <summary>Shakes the screen and queues its sound.</summary>
    ShakeScreen,
    /// <summary>Steps four pixels toward the spike wall, backing off when the wall stops him, and otherwise spawns foot dust.</summary>
    StepTowardSpikeWall,
}
