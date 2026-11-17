namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="WaverAnimations"/>.
/// </summary>
internal enum WaverSegment
{
    /// <summary>Flying level facing left, held.</summary>
    SteadyFacingLeft,
    /// <summary>Flying level facing right, held.</summary>
    SteadyFacingRight,
    /// <summary>
    /// Rolling over facing left: edge-on, tilted, its underside and turning back, before the spin ends.
    /// </summary>
    SpinningFacingLeft,
    /// <summary>
    /// Rolling over facing right: edge-on, tilted, its underside and turning back, before the spin ends.
    /// </summary>
    SpinningFacingRight,
}
