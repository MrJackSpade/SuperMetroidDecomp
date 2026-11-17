namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="EscapeEtecoonAnimations"/>.
/// </summary>
internal enum EscapeEtecoonSegment
{
    /// <summary>The leftward run's low-tide check, its entry cue.</summary>
    RunningLeftLowTide,
    /// <summary>The leftward run's four strides at low tide.</summary>
    RunningLeftLowTideStride,
    /// <summary>The running left high tide animation.</summary>
    RunningLeftHighTide,
    /// <summary>The rightward run's low-tide check, its entry cue.</summary>
    RunningRightLowTide,
    /// <summary>The rightward run's four strides at low tide.</summary>
    RunningRightLowTideStride,
    /// <summary>The running right high tide animation.</summary>
    RunningRightHighTide,
    /// <summary>The escape run's start, its entry cue.</summary>
    RunningForEscape,
    /// <summary>The escape run's four strides.</summary>
    RunningForEscapeStride,
    /// <summary>The stationary animation.</summary>
    Stationary,
    /// <summary>The thanks to Samus, its entry cue.</summary>
    ExpressGratitudeThenEscape,
    /// <summary>A thanking Etecoon's four steps to the left, each moving it three pixels.</summary>
    ThankingSteps,
    /// <summary>A thanking Etecoon's look back to the right and turn, before it runs for the escape.</summary>
    ThankingLook,
}
