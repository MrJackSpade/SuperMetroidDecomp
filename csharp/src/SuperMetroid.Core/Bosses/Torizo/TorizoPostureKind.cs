namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which posture a <see cref="TorizoCue.Posture"/> cue is. Stands Bomb Torizo up or sits it down, moving it by a movement-table entry.</summary>
internal enum TorizoPostureKind
{
    /// <summary>Applies a standing-up movement step.</summary>
    StandUp,
    /// <summary>Applies a sitting-down movement step.</summary>
    SitDown,
}
