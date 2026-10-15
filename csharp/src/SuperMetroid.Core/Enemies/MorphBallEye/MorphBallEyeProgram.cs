namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's tracking, eyelid and mount animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="MorphBallEyeAnimations"/>.
/// </summary>
internal enum MorphBallEyeProgram
{
    /// <summary><c>InstList_Eye_Active</c> at $A8:8FAC.</summary>
    Active,
    /// <summary><c>InstList_Eye_FacingRight_Deactivating</c> at $A8:8FF0.</summary>
    FacingRightDeactivating,
    /// <summary><c>InstList_Eye_FacingRight_Closed</c> at $A8:8FFC.</summary>
    FacingRightClosed,
    /// <summary><c>InstList_Eye_FacingLeft_Deactivating</c> at $A8:9002.</summary>
    FacingLeftDeactivating,
    /// <summary><c>InstList_Eye_FacingLeft_Closed</c> at $A8:900E.</summary>
    FacingLeftClosed,
    /// <summary><c>InstList_Eye_FacingRight_Activating</c> at $A8:9014.</summary>
    FacingRightActivating,
    /// <summary><c>InstList_Eye_FacingLeft_Activating</c> at $A8:9026.</summary>
    FacingLeftActivating,
    /// <summary><c>InstList_Eye_Mount_FacingRight</c> at $A8:9038.</summary>
    MountFacingRight,
    /// <summary><c>InstList_Eye_Mount_FacingDown</c> at $A8:903E.</summary>
    MountFacingDown,
    /// <summary><c>InstList_Eye_Mount_FacingLeft</c> at $A8:9044.</summary>
    MountFacingLeft,
    /// <summary><c>InstList_Eye_Mount_FacingUp</c> at $A8:904A.</summary>
    MountFacingUp,
}
