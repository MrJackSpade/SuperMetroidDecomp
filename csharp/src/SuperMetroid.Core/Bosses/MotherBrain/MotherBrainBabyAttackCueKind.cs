namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What a cue only her volley at the arrived Baby Metroid raises does (#2554): it acts on the Baby its playback carries and
/// the late fight it belongs to, or continues the play in that volley's animations.
/// </summary>
internal enum MotherBrainBabyAttackCueKind
{
    /// <summary>Counts an attack on the Baby Metroid.</summary>
    CountBabyMetroidAttack,
    /// <summary>Aims the onion rings at the Baby Metroid.</summary>
    AimOnionRingsAtBabyMetroid,
    /// <summary>Queues the attack cry unless the Baby Metroid attack has run its course.</summary>
    QueueBabyMetroidAttackSound,
    /// <summary>Usually holds the phase-three neutral pose another round.</summary>
    MaybeHoldNeutralPhase3,
}
