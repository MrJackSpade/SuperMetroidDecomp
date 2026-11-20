namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What a cue only her head's own brain animations raise does (#2554): a chance to play on at another of those animations.
/// </summary>
internal enum MotherBrainHeadChanceCueKind
{
    /// <summary>Usually holds the phase-two neutral pose another round.</summary>
    MaybeHoldNeutralPhase2,
    /// <summary>Usually holds the phase-three neutral pose another round.</summary>
    MaybeHoldNeutralPhase3,
    /// <summary>Rarely bursts into another round of dying drool.</summary>
    MaybeDroolAgain,
}
