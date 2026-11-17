namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="CacatacAnimations"/>.
/// </summary>
internal enum CacatacSegment
{
    /// <summary>The upside up idle animation.</summary>
    UpsideUpIdle,
    /// <summary>The upside up attack animation.</summary>
    UpsideUpAttack,
    /// <summary>The upside-down idle's start: its restore-patrol cue.</summary>
    UpsideDownIdle,
    /// <summary>The upside-down idle's poses.</summary>
    UpsideDownIdlePoses,
    /// <summary>The upside down attack animation.</summary>
    UpsideDownAttack,
}
