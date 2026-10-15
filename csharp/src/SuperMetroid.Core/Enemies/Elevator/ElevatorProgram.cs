namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator's two-frame animation loop as a typed program (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="ElevatorAnimations"/>.
/// </summary>
internal enum ElevatorProgram
{
    /// <summary><c>InstList_Elevator</c> at $A3:94D6.</summary>
    Loop,
}
