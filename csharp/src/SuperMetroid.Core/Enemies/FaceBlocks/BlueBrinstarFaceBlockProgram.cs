namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The Blue Brinstar face block's neutral and turning animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="BlueBrinstarFaceBlockAnimations"/>.
/// </summary>
internal enum BlueBrinstarFaceBlockProgram
{
    /// <summary>The native list at $A8:E80C.</summary>
    SamusLeft,
    /// <summary>The native list at $A8:E81A.</summary>
    SamusRight,
    /// <summary>The native list at $A8:E828.</summary>
    Initial,
}
