namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="KiHunterAnimations"/>.
/// </summary>
internal enum KiHunterProgram
{
    /// <summary><c>InstList_Kihunter_Idling_FacingLeft</c> at $A8:E9FA.</summary>
    FlyingLeft,
    /// <summary><c>InstList_Kihunter_Swiping_FacingLeft</c> at $A8:EA08.</summary>
    SwoopLeft,
    /// <summary><c>InstList_Kihunter_Idling_FacingRight</c> at $A8:EA24.</summary>
    FlyingRight,
    /// <summary><c>InstList_Kihunter_Swiping_FacingRight</c> at $A8:EA32.</summary>
    SwoopRight,
    /// <summary><c>InstList_KihunterWings_FacingLeft</c> at $A8:EA4E.</summary>
    WingsLeft,
    /// <summary><c>InstList_KihunterWings_FacingRight</c> at $A8:EA5E.</summary>
    WingsRight,
    /// <summary><c>InstList_KihunterWings_Falling</c> at $A8:EA7E.</summary>
    DetachedWings,
    /// <summary><c>InstList_Kihunter_Hop_FacingLeft</c> at $A8:EA8A.</summary>
    JumpLeft,
    /// <summary><c>InstList_Kihunter_Hop_FacingRight</c> at $A8:EAA6.</summary>
    JumpRight,
    /// <summary><c>InstList_Kihunter_LandedFromHop_FacingLeft</c> at $A8:EAC2.</summary>
    LandLeft,
    /// <summary><c>InstList_Kihunter_LandedFromHop_FacingRight</c> at $A8:EADA.</summary>
    LandRight,
    /// <summary><c>InstList_Kihunter_AcidSpitAttack_FacingLeft</c> at $A8:EAF2.</summary>
    SpitLeft,
    /// <summary><c>InstList_Kihunter_AcidSpitAttack_FacingRight</c> at $A8:EB10.</summary>
    SpitRight,
}
