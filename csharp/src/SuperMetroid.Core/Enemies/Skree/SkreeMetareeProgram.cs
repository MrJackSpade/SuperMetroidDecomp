namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's idling, wind-up, diving and stop animations as typed programs (#641), one per bank-$A3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="SkreeMetareeAnimations"/>.
/// </summary>
internal enum SkreeMetareeProgram
{
    /// <summary><c>InstList_Metaree_Idling</c> at $A3:8910.</summary>
    MetareeIdling,
    /// <summary><c>InstList_Metaree_PrepareToLaunchAttack</c> at $A3:8924.</summary>
    MetareePreparingAttack,
    /// <summary><c>InstList_Metaree_LaunchedAttack_0</c> at $A3:8930.</summary>
    MetareeDiving,
    /// <summary>The loop at $A3:8932.</summary>
    MetareeDivingLoop,
    /// <summary><c>UNUSED_InstList_Metaree_StopAnimating_A38946</c> at $A3:8946.</summary>
    MetareeStopAnimating,
    /// <summary><c>InstList_Skree_Idling</c> at $A3:C65E.</summary>
    SkreeIdling,
    /// <summary><c>InstList_Skree_PrepareToLaunchAttack</c> at $A3:C672.</summary>
    SkreePreparingAttack,
    /// <summary><c>InstList_Skree_LaunchedAttack_0</c> at $A3:C67E.</summary>
    SkreeDiving,
    /// <summary>The loop at $A3:C680.</summary>
    SkreeDivingLoop,
    /// <summary><c>UNUSED_InstList_Skree_StopAnimating_A3C694</c> at $A3:C694.</summary>
    SkreeStopAnimating,
}
