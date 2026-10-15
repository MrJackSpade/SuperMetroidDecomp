namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="AlcoonAnimations"/>.
/// </summary>
internal enum AlcoonProgram
{
    /// <summary><c>InstList_Alcoon_FacingLeft_Walking_0</c> at $A8:DBE7.</summary>
    WalkingLeft,
    /// <summary><c>InstList_Alcoon_FacingLeft_Walking_1</c> at $A8:DBE9.</summary>
    WalkingLeftLoop,
    /// <summary><c>InstList_Alcoon_FacingLeft_SpawnFireballs</c> at $A8:DC03.</summary>
    FireLeft,
    /// <summary><c>InstList_Alcoon_FacingLeft_Airborne_LookingUp</c> at $A8:DC4B.</summary>
    AirborneLeftLookingUp,
    /// <summary><c>InstList_Alcoon_FacingLeft_Airborne_LookingForward</c> at $A8:DC51.</summary>
    AirborneLeftLookingForward,
    /// <summary><c>InstList_Alcoon_FacingRight_Walking_0</c> at $A8:DC57.</summary>
    WalkingRight,
    /// <summary><c>InstList_Alcoon_FacingRight_Walking_1</c> at $A8:DC59.</summary>
    WalkingRightLoop,
    /// <summary><c>InstList_Alcoon_FacingRight_SpawnFireballs</c> at $A8:DC73.</summary>
    FireRight,
    /// <summary><c>InstList_Alcoon_FacingRight_Airborne_LookingUp</c> at $A8:DCBB.</summary>
    AirborneRightLookingUp,
    /// <summary><c>InstList_Alcoon_FacingRight_Airborne_LookingForward</c> at $A8:DCC1.</summary>
    AirborneRightLookingForward,
}
