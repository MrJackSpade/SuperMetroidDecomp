namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How one of Mother Brain's projectiles, or one of her room's, moves itself (#1843).
/// </summary>
internal enum MotherBrainProjectileMotion
{
    /// <summary>Turns a Mother Brain room turret, fires it, or removes it once the room asks.</summary>
    TurretAiming,

    /// <summary>Flies a shard of Mother Brain's glass under gravity, trailing sparkles.</summary>
    GlassShardFlying,

    /// <summary>Raises dust once, then drops a ceiling tube, accelerating.</summary>
    TubeFalling,

    /// <summary>Keeps drool hanging from Mother Brain's mouth while its animation shows it hanging.</summary>
    DroolHanging,

    /// <summary>Drops released drool, accelerating, until it reaches the arena floor.</summary>
    DroolFalling,

    /// <summary>Holds an onion ring at the mouth for its delay, then flies it with its own collision and arena cull.</summary>
    OnionRingFlying,

    /// <summary>Drops a Mother Brain bomb under gravity, bouncing in stages and testing Samus's bombs.</summary>
    BombBouncing,

    /// <summary>Contracts the rainbow beam's charge around the moving brain.</summary>
    RainbowBeamCharging,

    /// <summary>Keeps a rainbow-beam impact at its offset from Samus as she moves.</summary>
    RainbowImpactTracking,

    /// <summary>Keeps a death explosion at its offset from Mother Brain's body each pass.</summary>
    DeathExplosionTracking,

    /// <summary>Drags and drops a door fragment under gravity for its fixed lifetime.</summary>
    DoorFragmentFalling,

    /// <summary>
    /// Holds the death subtitle still at its fixed place in the room, which every pass writes again; like every
    /// projectile it is drawn where that place lies on screen (#1851).
    /// </summary>
    SubtitleHeld,
}
