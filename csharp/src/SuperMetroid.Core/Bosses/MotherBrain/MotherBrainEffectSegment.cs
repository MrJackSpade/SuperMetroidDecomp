namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Generated from the animations; see <see cref="MotherBrainEffectAnimations"/>.
/// </summary>
internal enum MotherBrainEffectSegment
{
    /// <summary>Mother Brain's onion rings in flight.</summary>
    OnionRing,
    /// <summary>An onion ring bursting where it hits.</summary>
    OnionRingHit,
    /// <summary>Mother Brain's bomb.</summary>
    Bomb,
    /// <summary>Mother Brain's rainbow beam charging.</summary>
    RainbowBeamCharging,
    /// <summary>Mother Brain's drool falling.</summary>
    Drool,
    /// <summary>Mother Brain's drool splashing on the floor.</summary>
    DroolHitFloor,
    /// <summary>A fragment of the escape door Mother Brain blew apart.</summary>
    EscapeDoorParticle,
    /// <summary>Mother Brain's big purple breath.</summary>
    PurpleBreathBig,
    /// <summary>Mother Brain's small purple breath.</summary>
    PurpleBreathSmall,
    /// <summary>The Japanese subtitle that the time bomb is set, shown as the escape begins.</summary>
    EscapeSubtitle,
}
