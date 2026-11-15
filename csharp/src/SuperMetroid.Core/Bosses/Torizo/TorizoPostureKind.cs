namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which posture a <see cref="TorizoCue.Posture"/> cue is: standing a Torizo up or sitting it down.</summary>
internal enum TorizoPostureKind
{
    /// <summary>Stands the Torizo up, moving it by the cue's displacement.</summary>
    StandUp,
    /// <summary>Sits the Torizo down, moving it back by the cue's displacement.</summary>
    SitDown,
}
