namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="WaverAnimations"/>.
/// </summary>
internal enum WaverProgram
{
    /// <summary>The native list at $A3:86A7.</summary>
    SteadyFacingLeft,
    /// <summary>The native list at $A3:86AD.</summary>
    SteadyFacingRight,
    /// <summary>The native list at $A3:86B3.</summary>
    SpinningFacingLeft,
    /// <summary>The native list at $A3:86C7.</summary>
    SpinningFacingRight,
}
