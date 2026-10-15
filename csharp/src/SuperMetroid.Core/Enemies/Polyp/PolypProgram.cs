namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's stationary pose as a typed program (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="PolypAnimations"/>.
/// </summary>
internal enum PolypProgram
{
    /// <summary><c>InstList_Polyp</c> at $A2:B51A.</summary>
    Stationary,
}
