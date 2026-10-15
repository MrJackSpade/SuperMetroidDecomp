namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Sidehopper's hop, idle and corpse animations and the dead Zoomer, Ripper and Skree corpse poses as typed programs (#641), one per bank-$A9 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="DeadTourianAnimations"/>.
/// </summary>
internal enum DeadTourianProgram
{
    /// <summary><c>InstList_CorpseSidehopper_Alive_Hopping</c> at $A9:ECAC.</summary>
    SidehopperHopping,
    /// <summary><c>InstList_CorpseSidehopper_Alive_Idle</c> at $A9:ECE3.</summary>
    SidehopperIdle,
    /// <summary><c>InstList_CorpseSidehopper_Alive_Corpse</c> at $A9:ECE9.</summary>
    SidehopperCorpse,
    /// <summary><c>InstList_CorpseSidehopper_Alive_Dead</c> at $A9:ECEF.</summary>
    SidehopperInitiallyDead,
    /// <summary><c>InstList_CorpseZoomer_Param1_0</c> at $A9:ECF5.</summary>
    ZoomerCorpse0,
    /// <summary>The native list at $A9:ECFB.</summary>
    ZoomerCorpse1,
    /// <summary>The native list at $A9:ED01.</summary>
    ZoomerCorpse2,
    /// <summary>The native list at $A9:ED07.</summary>
    RipperCorpse0,
    /// <summary>The native list at $A9:ED0D.</summary>
    RipperCorpse1,
    /// <summary>The native list at $A9:ED13.</summary>
    SkreeCorpse0,
    /// <summary>The native list at $A9:ED19.</summary>
    SkreeCorpse1,
    /// <summary>The native list at $A9:ED1F.</summary>
    SkreeCorpse2,
}
