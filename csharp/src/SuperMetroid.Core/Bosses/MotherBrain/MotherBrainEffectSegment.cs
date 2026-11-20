namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle animations (#641).
/// Generated from the animations; see <see cref="MotherBrainEffectAnimations"/>.
/// </summary>
internal enum MotherBrainEffectSegment
{
    /// <summary>Mother Brain's onion rings in flight.</summary>
    OnionRing,
    /// <summary>An onion ring bursting where it hits.</summary>
    OnionRingHit,
    /// <summary>One pulse of Mother Brain's bomb, its glow running from red through orange and yellow round it and back to dim red.</summary>
    Bomb,
    /// <summary>The charge of Mother Brain's rainbow beam: spread sparks closing into streaks that gather with fireballs, then gone.</summary>
    RainbowBeamCharging,
    /// <summary>Mother Brain's drool falling.</summary>
    Drool,
    /// <summary>Mother Brain's drool splashing on the floor.</summary>
    DroolHitFloor,
    /// <summary>A fragment of the escape door Mother Brain blew apart.</summary>
    EscapeDoorParticle,
    /// <summary>A large puff of Mother Brain's purple breath, stopping where it is and growing into a plume that disperses and fades.</summary>
    PurpleBreathBig,
    /// <summary>A small puff of Mother Brain's purple breath, stopping where it is and swelling into a cloud that disperses and fades.</summary>
    PurpleBreathSmall,
    /// <summary>The Japanese subtitle that the time bomb is set, shown as the escape begins.</summary>
    EscapeSubtitle,
}
