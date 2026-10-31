namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's body, eye, tail and arms programs, in the order the cartridge authors them. The four parts
/// run programs of one set: a body cue starts all four, and the arms' dying program runs on into the body's.
/// </summary>
public enum DraygonProgram
{
    /// <summary><c>InstList_Draygon_Sleep</c>.</summary>
    Sleep,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Reset</c>.</summary>
    BodyFacingLeftReset,
    /// <summary><c>InstList_DraygonBody_FacingRight_Reset</c>.</summary>
    BodyFacingRightReset,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Idle_0</c>.</summary>
    ArmsFacingLeftIdle,
    /// <summary><c>InstList_DraygonArms_FacingLeft_NearSwoopApex</c>.</summary>
    ArmsFacingLeftNearSwoopApex,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Grab</c>.</summary>
    ArmsFacingLeftGrab,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Dying</c>.</summary>
    ArmsFacingLeftDying,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Idle</c>.</summary>
    BodyFacingLeftIdle,
    /// <summary><c>InstList_DraygonBody_Dying_0</c>.</summary>
    BodyDying,
    /// <summary><c>InstList_DraygonBody_Dying_1</c>.</summary>
    BodyDyingExplosions,
    /// <summary><c>InstList_DraygonBody_Dying_2</c>.</summary>
    BodyDyingExplosionsLoop,
    /// <summary><c>InstList_Draygon_Delete</c>.</summary>
    Delete,
    /// <summary><c>InstList_DraygonBody_FacingLeft_FireGoop</c>.</summary>
    BodyFacingLeftFireGoop,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Roar</c>.</summary>
    BodyFacingLeftRoar,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Idle</c>.</summary>
    EyeFacingLeftIdle,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_0</c>.</summary>
    EyeFacingLeftDying,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_1</c>.</summary>
    EyeFacingLeftDyingLoop,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dead</c>.</summary>
    EyeFacingLeftDead,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingLeft</c>.</summary>
    EyeFacingLeftLookingLeft,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingRight</c>.</summary>
    EyeFacingLeftLookingRight,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingUp</c>.</summary>
    EyeFacingLeftLookingUp,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingDown</c>.</summary>
    EyeFacingLeftLookingDown,
    /// <summary><c>InstList_DraygonTail_FacingLeft_Idle_0</c>.</summary>
    TailFacingLeftIdle,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FakeTailWhip</c>.</summary>
    TailFacingLeftInitialFakeWhip,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_0</c>.</summary>
    TailFacingLeftFinalWhips,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_1</c>.</summary>
    TailFacingLeftFinalWhipsLoop,
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailWhip</c>.</summary>
    TailFacingLeftWhip,
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailFlail</c>.</summary>
    TailFacingLeftFlail,
    /// <summary><c>InstList_DraygonArms_FacingRight_Idle_0</c>.</summary>
    ArmsFacingRightIdle,
    /// <summary><c>InstList_DraygonArms_FacingRight_NearSwoopApex</c>.</summary>
    ArmsFacingRightNearSwoopApex,
    /// <summary><c>InstList_DraygonArms_FacingRight_Grab</c>.</summary>
    ArmsFacingRightGrab,
    /// <summary><c>InstList_DraygonArms_FacingRight_Dying_0</c>.</summary>
    ArmsFacingRightDying,
    /// <summary><c>InstList_DraygonBody_FacingRight_Idle</c>.</summary>
    BodyFacingRightIdle,
    /// <summary><c>InstList_DraygonBody_FacingRight_FireGoop</c>.</summary>
    BodyFacingRightFireGoop,
    /// <summary><c>InstList_DraygonBody_FacingRight_Roar</c>.</summary>
    BodyFacingRightRoar,
    /// <summary><c>InstList_DraygonEye_FacingRight_Idle</c>.</summary>
    EyeFacingRightIdle,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_0</c>.</summary>
    EyeFacingRightDying,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_1</c>.</summary>
    EyeFacingRightDyingLoop,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dead</c>.</summary>
    EyeFacingRightDead,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingRight</c>.</summary>
    EyeFacingRightLookingRight,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingLeft</c>.</summary>
    EyeFacingRightLookingLeft,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingUp</c>.</summary>
    EyeFacingRightLookingUp,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingDown</c>.</summary>
    EyeFacingRightLookingDown,
    /// <summary><c>InstList_DraygonTail_FacingRight_Idle_0</c>.</summary>
    TailFacingRightIdle,
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_0</c>.</summary>
    TailFacingRightFinalWhips,
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_1</c>.</summary>
    TailFacingRightFinalWhipsLoop,
    /// <summary><c>InstList_DraygonTail_FacingRight_TailWhip_0</c>.</summary>
    TailFacingRightWhip,
    /// <summary><c>InstList_DraygonTail_FacingRight_TailFlail_0</c>.</summary>
    TailFacingRightFlail,
}
