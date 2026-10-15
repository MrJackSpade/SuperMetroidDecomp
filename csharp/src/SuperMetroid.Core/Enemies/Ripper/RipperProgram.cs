namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The GRipper, Ripper II and Ripper flying loops as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="RipperAnimations"/>.
/// </summary>
internal enum RipperProgram
{
    /// <summary><c>InstList_GRipper_MovingLeft</c> at $A2:E19B.</summary>
    GRipperMovingLeft,
    /// <summary><c>InstList_GRipper_MovingRight</c> at $A2:E1AF.</summary>
    GRipperMovingRight,
    /// <summary><c>InstList_Ripper2_MovingRight</c> at $A2:E2E0.</summary>
    Ripper2MovingRight,
    /// <summary><c>InstList_Ripper2_MovingLeft</c> at $A2:E2F4.</summary>
    Ripper2MovingLeft,
    /// <summary><c>InstList_Ripper_MovingRight</c> at $A2:E477.</summary>
    RipperMovingRight,
    /// <summary><c>InstList_Ripper_MovingLeft</c> at $A2:E48B.</summary>
    RipperMovingLeft,
}
