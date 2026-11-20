namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// How a casual flame of Phantoon's that has landed moves itself (#1843, #2554).
/// </summary>
internal enum PhantoonLandedFlameMotion
{
    /// <summary>Holds a landed casual flame for its impact pause.</summary>
    CasualLanding,

    /// <summary>Bounces a casual flame twice, each bounce lower.</summary>
    CasualBouncing,
}
