namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// Zoa's shooting and rising animations as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="ZoaAnimations"/>.
/// </summary>
internal enum ZoaProgram
{
    /// <summary>The native list at $A3:B3C1.</summary>
    FacingLeftShooting,
    /// <summary>The native list at $A3:B3D7.</summary>
    FacingLeftRising,
    /// <summary>The native list at $A3:B3E7.</summary>
    FacingRightShooting,
    /// <summary>The native list at $A3:B3FD.</summary>
    FacingRightRising,
}
