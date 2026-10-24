namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Whether a non-gameplay frame runs the per-line effect phase before drawing its random number; which door-transition
/// frames run it is compatibility behaviour that keeps the random sequence the cartridge's.
/// </summary>
public enum PerLineEffectProcessing
{
    /// <summary>The per-line effects do not advance.</summary>
    Skipped,

    /// <summary>The per-line effects advance.</summary>
    Run,
}
