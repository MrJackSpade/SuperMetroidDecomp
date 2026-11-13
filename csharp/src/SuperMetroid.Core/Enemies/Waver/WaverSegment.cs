namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="WaverAnimations"/>.
/// </summary>
internal enum WaverSegment
{
    /// <summary>The steady facing left animation.</summary>
    SteadyFacingLeft,
    /// <summary>The steady facing right animation.</summary>
    SteadyFacingRight,
    /// <summary>The spinning facing left animation.</summary>
    SpinningFacingLeft,
    /// <summary>The spinning facing right animation.</summary>
    SpinningFacingRight,
}
