namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="KagoAnimations"/>.
/// </summary>
internal enum KagoProgram
{
    /// <summary><c>InstList_Kago_Initial_SlowAnimation</c> at $A8:AB1E.</summary>
    Slow,
    /// <summary><c>InstList_Kago_TakenHit_FastAnimation</c> at $A8:AB32.</summary>
    Fast,
}
