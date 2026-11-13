namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle programs (#641), one per list a spawn, branch or family step enters, in authored order.
/// Generated from the verified lists; see <see cref="MotherBrainEffectAnimations"/>.
/// </summary>
internal enum MotherBrainEffectProgram
{
    /// <summary>Enemy projectile animation script: Mother brains onion rings.</summary>
    OnionRing,
    /// <summary>Enemy projectile animation script: hit, Mother brains onion rings.</summary>
    OnionRingHit,
    /// <summary>Enemy projectile animation script: Mother Brain Bomb.</summary>
    Bomb,
    /// <summary>Enemy projectile animation script: Mother Brain rainbow beam, charging.</summary>
    RainbowBeamCharging,
    /// <summary>Enemy projectile animation script: Mother brains drool.</summary>
    Drool,
    /// <summary>Enemy projectile animation script: Mother brains drool, hit floor.</summary>
    DroolHitFloor,
    /// <summary>Enemy proj animation script: Mother Brain exploded escape door particle.</summary>
    EscapeDoorParticle,
    /// <summary>Enemy projectile animation script: Mother Brain purple breath, big.</summary>
    PurpleBreathBig,
    /// <summary>Enemy projectile animation script: Mother Brain purple breath, small.</summary>
    PurpleBreathSmall,
    /// <summary>Enemy projectile animation script: time Bomb set japan text.</summary>
    EscapeSubtitle,
}
