namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Generated from the verified animations; see <see cref="MotherBrainEffectAnimations"/>.
/// </summary>
internal enum MotherBrainEffectSegment
{
    /// <summary>Enemy projectile animation: Mother brains onion rings.</summary>
    OnionRing,
    /// <summary>Enemy projectile animation: hit, Mother brains onion rings.</summary>
    OnionRingHit,
    /// <summary>Enemy projectile animation: Mother Brain Bomb.</summary>
    Bomb,
    /// <summary>Enemy projectile animation: Mother Brain rainbow beam, charging.</summary>
    RainbowBeamCharging,
    /// <summary>Enemy projectile animation: Mother brains drool.</summary>
    Drool,
    /// <summary>Enemy projectile animation: Mother brains drool, hit floor.</summary>
    DroolHitFloor,
    /// <summary>Enemy proj animation: Mother Brain exploded escape door particle.</summary>
    EscapeDoorParticle,
    /// <summary>Enemy projectile animation: Mother Brain purple breath, big.</summary>
    PurpleBreathBig,
    /// <summary>Enemy projectile animation: Mother Brain purple breath, small.</summary>
    PurpleBreathSmall,
    /// <summary>Enemy projectile animation: time Bomb set japan text.</summary>
    EscapeSubtitle,
}
