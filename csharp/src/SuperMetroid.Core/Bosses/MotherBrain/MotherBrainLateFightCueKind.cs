namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What a cue only her late fight's volley at Samus raises does (#2554): it acts on the late fight its playback carries, or
/// continues the play in that volley's animations.
/// </summary>
internal enum MotherBrainLateFightCueKind
{
    /// <summary>Resets the count of attacks on the Baby Metroid.</summary>
    ResetBabyMetroidAttacks,
    /// <summary>Queues the attack cry unless the Baby Metroid attack has run its course.</summary>
    QueueBabyMetroidAttackSound,
    /// <summary>Usually holds the phase-three neutral pose another round.</summary>
    MaybeHoldNeutralPhase3,
}
