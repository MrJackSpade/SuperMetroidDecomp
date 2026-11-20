namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How one of Mother Brain's projectiles moves itself by a motion that reads nothing it keeps (#1843, #2554); a projectile
/// keeping what its motion reads moves by one of <see cref="MotherBrainKeptMotion"/>.
/// </summary>
internal enum MotherBrainProjectileMotion
{
    /// <summary>Flies a shard of Mother Brain's glass under gravity, trailing sparkles.</summary>
    GlassShardFlying,

    /// <summary>Drops released drool, accelerating, until it reaches the arena floor.</summary>
    DroolFalling,

    /// <summary>
    /// Holds the death subtitle still at its fixed place in the room, which every pass writes again; like every
    /// projectile it is drawn where that place lies on screen (#1851).
    /// </summary>
    SubtitleHeld,
}
