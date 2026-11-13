namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="EscapeEtecoonAnimations"/>.
/// </summary>
internal enum EscapeEtecoonSegment
{
    /// <summary>The running left low tide animation.</summary>
    RunningLeftLowTide,
    /// <summary>The loop.</summary>
    RunningLeftLowTideLoop,
    /// <summary>The running left high tide animation.</summary>
    RunningLeftHighTide,
    /// <summary>The running right low tide animation.</summary>
    RunningRightLowTide,
    /// <summary>The loop.</summary>
    RunningRightLowTideLoop,
    /// <summary>The running right high tide animation.</summary>
    RunningRightHighTide,
    /// <summary>The running for escape animation.</summary>
    RunningForEscape,
    /// <summary>The loop.</summary>
    RunningForEscapeLoop,
    /// <summary>The stationary animation.</summary>
    Stationary,
    /// <summary>The express gratitude then escape animation.</summary>
    ExpressGratitudeThenEscape,
    /// <summary>The loop.</summary>
    ExpressGratitudeThenEscapeLoop,
}
