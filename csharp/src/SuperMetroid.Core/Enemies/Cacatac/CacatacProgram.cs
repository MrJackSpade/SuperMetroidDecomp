namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations as typed programs (#641), one per bank-$A2 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="CacatacAnimations"/>.
/// </summary>
internal enum CacatacProgram
{
    /// <summary><c>InstList_Cacatac_UpsideUp_Idling</c> at $A2:9E8A.</summary>
    UpsideUpIdle,
    /// <summary><c>InstList_Cacatac_UpsideUp_Attacking</c> at $A2:9EB0.</summary>
    UpsideUpAttack,
    /// <summary><c>InstList_Cacatac_UpsideDown_Idling_0</c> at $A2:9EDA.</summary>
    UpsideDownIdle,
    /// <summary><c>InstList_Cacatac_UpsideDown_Idling_1</c> at $A2:9EDC.</summary>
    UpsideDownIdleLoop,
    /// <summary><c>InstList_Cacatac_UpsideDown_Attacking</c> at $A2:9F00.</summary>
    UpsideDownAttack,
}
