namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle programs (#641), one per bank-$86 list a spawn, branch or family routine enters, in authored order.
/// Generated from the verified native lists; see <see cref="MotherBrainEffectAnimations"/>.
/// </summary>
internal enum MotherBrainEffectProgram
{
    /// <summary><c>InstList_EnemyProjectile_MotherBrainsOnionRings</c> at $86:C432.</summary>
    OnionRing,
    /// <summary><c>InstList_EnemyProjectile_Hit_MotherBrainsOnionRings</c> at $86:C464.</summary>
    OnionRingHit,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainBomb</c> at $86:C76E.</summary>
    Bomb,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainRainbowBeam_Charging</c> at $86:C829.</summary>
    RainbowBeamCharging,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainsDrool</c> at $86:C8B0.</summary>
    Drool,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainsDrool_HitFloor</c> at $86:C8E1.</summary>
    DroolHitFloor,
    /// <summary><c>InstList_EnemyProj_MotherBrainExplodedEscapeDoorParticle</c> at $86:CA22.</summary>
    EscapeDoorParticle,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainPurpleBreath_Big</c> at $86:CAA4.</summary>
    PurpleBreathBig,
    /// <summary><c>InstList_EnemyProjectile_MotherBrainPurpleBreath_Small</c> at $86:CAC8.</summary>
    PurpleBreathSmall,
    /// <summary><c>InstList_EnemyProjectile_TimeBombSetJapanText</c> at $86:CB0D.</summary>
    EscapeSubtitle,
}
