namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="MotherBrainEffectAnimations"/>.
/// </summary>
internal enum MotherBrainEffectProgram
{
    /// <summary><c>InstList_EnemyProjectile_MotherBrainsOnionRings</c>.</summary>
    OnionRing,
    /// <summary><c>InstList_EnemyProjectile_Hit_MotherBrainsOnionRings</c>.</summary>
    OnionRingHit,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainBomb</c>.</summary>
    Bomb,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainRainbowBeam_Charging</c>.</summary>
    RainbowBeamCharging,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainsDrool</c>.</summary>
    Drool,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainsDrool_HitFloor</c>.</summary>
    DroolHitFloor,
    /// <summary><c>InstList_EnemyProj_MotherBrainExplodedEscapeDoorParticle</c>.</summary>
    EscapeDoorParticle,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainPurpleBreath_Big</c>.</summary>
    PurpleBreathBig,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainPurpleBreath_Small</c>.</summary>
    PurpleBreathSmall,
    /// <summary><c>InstList_EnemyProjectile_TimeBombSetJapanText</c>.</summary>
    EscapeSubtitle,
}
