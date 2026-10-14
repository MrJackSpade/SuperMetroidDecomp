namespace SuperMetroid.Core.Game;

/// <summary>
/// Draygon's body, eye, tail and arms programs, in the order the cartridge authors them in bank $A5. The four parts
/// run programs of one set: a body cue starts all four, and the arms' dying program runs on into the body's.
/// </summary>
public enum DraygonProgram
{
    /// <summary><c>InstList_Draygon_Sleep</c> (<c>$A5:97B9</c>).</summary>
    Sleep,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Reset</c> (<c>$A5:97BB</c>).</summary>
    BodyFacingLeftReset,
    /// <summary><c>InstList_DraygonBody_FacingRight_Reset</c> (<c>$A5:97D1</c>).</summary>
    BodyFacingRightReset,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Idle_0</c> (<c>$A5:97E7</c>).</summary>
    ArmsFacingLeftIdle,
    /// <summary><c>InstList_DraygonArms_FacingLeft_NearSwoopApex</c> (<c>$A5:9813</c>).</summary>
    ArmsFacingLeftNearSwoopApex,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Grab</c> (<c>$A5:9845</c>).</summary>
    ArmsFacingLeftGrab,
    /// <summary><c>InstList_DraygonArms_FacingLeft_Dying</c> (<c>$A5:9867</c>).</summary>
    ArmsFacingLeftDying,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Idle</c> (<c>$A5:9889</c>).</summary>
    BodyFacingLeftIdle,
    /// <summary><c>InstList_DraygonBody_Dying_0</c> (<c>$A5:989B</c>).</summary>
    BodyDying,
    /// <summary><c>InstList_DraygonBody_Dying_1</c> (<c>$A5:98A5</c>).</summary>
    BodyDyingExplosions,
    /// <summary><c>InstList_DraygonBody_Dying_2</c> (<c>$A5:98BF</c>).</summary>
    BodyDyingExplosionsLoop,
    /// <summary><c>InstList_Draygon_Delete</c> (<c>$A5:98ED</c>).</summary>
    Delete,
    /// <summary><c>InstList_DraygonBody_FacingLeft_FireGoop</c> (<c>$A5:98FE</c>).</summary>
    BodyFacingLeftFireGoop,
    /// <summary><c>InstList_DraygonBody_FacingLeft_Roar</c> (<c>$A5:9922</c>).</summary>
    BodyFacingLeftRoar,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Idle</c> (<c>$A5:9944</c>).</summary>
    EyeFacingLeftIdle,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_0</c> (<c>$A5:997A</c>).</summary>
    EyeFacingLeftDying,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dying_1</c> (<c>$A5:997E</c>).</summary>
    EyeFacingLeftDyingLoop,
    /// <summary><c>InstList_DraygonEye_FacingLeft_Dead</c> (<c>$A5:999C</c>).</summary>
    EyeFacingLeftDead,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingLeft</c> (<c>$A5:99AE</c>).</summary>
    EyeFacingLeftLookingLeft,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingRight</c> (<c>$A5:99B4</c>).</summary>
    EyeFacingLeftLookingRight,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingUp</c> (<c>$A5:99BA</c>).</summary>
    EyeFacingLeftLookingUp,
    /// <summary><c>InstList_DraygonEye_FacingLeft_LookingDown</c> (<c>$A5:99C0</c>).</summary>
    EyeFacingLeftLookingDown,
    /// <summary><c>InstList_DraygonTail_FacingLeft_Idle_0</c> (<c>$A5:99C6</c>).</summary>
    TailFacingLeftIdle,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FakeTailWhip</c> (<c>$A5:99FC</c>).</summary>
    TailFacingLeftInitialFakeWhip,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_0</c> (<c>$A5:9A68</c>).</summary>
    TailFacingLeftFinalWhips,
    /// <summary><c>InstList_DraygonTail_FacingLeft_FinalTailWhips_1</c> (<c>$A5:9A6C</c>).</summary>
    TailFacingLeftFinalWhipsLoop,
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailWhip</c> (<c>$A5:9AE8</c>).</summary>
    TailFacingLeftWhip,
    /// <summary><c>InstList_DraygonTail_FacingLeft_TailFlail</c> (<c>$A5:9B5A</c>).</summary>
    TailFacingLeftFlail,
    /// <summary><c>InstList_DraygonArms_FacingRight_Idle_0</c> (<c>$A5:9BDA</c>).</summary>
    ArmsFacingRightIdle,
    /// <summary><c>InstList_DraygonArms_FacingRight_NearSwoopApex</c> (<c>$A5:9C06</c>).</summary>
    ArmsFacingRightNearSwoopApex,
    /// <summary><c>InstList_DraygonArms_FacingRight_Grab</c> (<c>$A5:9C38</c>).</summary>
    ArmsFacingRightGrab,
    /// <summary><c>InstList_DraygonArms_FacingRight_Dying_0</c> (<c>$A5:9C5A</c>).</summary>
    ArmsFacingRightDying,
    /// <summary><c>InstList_DraygonBody_FacingRight_Idle</c> (<c>$A5:9C7E</c>).</summary>
    BodyFacingRightIdle,
    /// <summary><c>InstList_DraygonBody_FacingRight_FireGoop</c> (<c>$A5:9C90</c>).</summary>
    BodyFacingRightFireGoop,
    /// <summary><c>InstList_DraygonBody_FacingRight_Roar</c> (<c>$A5:9CB4</c>).</summary>
    BodyFacingRightRoar,
    /// <summary><c>InstList_DraygonEye_FacingRight_Idle</c> (<c>$A5:9CD6</c>).</summary>
    EyeFacingRightIdle,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_0</c> (<c>$A5:9D1C</c>).</summary>
    EyeFacingRightDying,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dying_1</c> (<c>$A5:9D20</c>).</summary>
    EyeFacingRightDyingLoop,
    /// <summary><c>InstList_DraygonEye_FacingRight_Dead</c> (<c>$A5:9D3E</c>).</summary>
    EyeFacingRightDead,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingRight</c> (<c>$A5:9D50</c>).</summary>
    EyeFacingRightLookingRight,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingLeft</c> (<c>$A5:9D56</c>).</summary>
    EyeFacingRightLookingLeft,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingUp</c> (<c>$A5:9D5C</c>).</summary>
    EyeFacingRightLookingUp,
    /// <summary><c>InstList_DraygonEye_FacingRight_LookingDown</c> (<c>$A5:9D62</c>).</summary>
    EyeFacingRightLookingDown,
    /// <summary><c>InstList_DraygonTail_FacingRight_Idle_0</c> (<c>$A5:9D68</c>).</summary>
    TailFacingRightIdle,
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_0</c> (<c>$A5:9E21</c>).</summary>
    TailFacingRightFinalWhips,
    /// <summary><c>InstList_DraygonTail_FacingRight_FinalTailWhips_1</c> (<c>$A5:9E25</c>).</summary>
    TailFacingRightFinalWhipsLoop,
    /// <summary><c>InstList_DraygonTail_FacingRight_TailWhip_0</c> (<c>$A5:9EA1</c>).</summary>
    TailFacingRightWhip,
    /// <summary><c>InstList_DraygonTail_FacingRight_TailFlail_0</c> (<c>$A5:9F15</c>).</summary>
    TailFacingRightFlail,
}
