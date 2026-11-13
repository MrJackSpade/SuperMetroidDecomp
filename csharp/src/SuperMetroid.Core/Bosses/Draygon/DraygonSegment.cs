namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's body, eye, tail and arms animations, in authored order. The four parts
/// run animations of one set: a body cue starts all four, and the arms' dying animation runs on into the body's.
/// </summary>
public enum DraygonSegment
{
    /// <summary>Draygon animation: sleep.</summary>
    Sleep,
    /// <summary>Draygon body animation: facing left, reset.</summary>
    BodyFacingLeftReset,
    /// <summary>Draygon body animation: facing right, reset.</summary>
    BodyFacingRightReset,
    /// <summary>Draygon arms animation: facing left, idle, part 0.</summary>
    ArmsFacingLeftIdle,
    /// <summary>Draygon arms animation: facing left, near swoop apex.</summary>
    ArmsFacingLeftNearSwoopApex,
    /// <summary>Draygon arms animation: facing left, grab.</summary>
    ArmsFacingLeftGrab,
    /// <summary>Draygon arms animation: facing left, dying.</summary>
    ArmsFacingLeftDying,
    /// <summary>Draygon body animation: facing left, idle.</summary>
    BodyFacingLeftIdle,
    /// <summary>Draygon body animation: dying, part 0.</summary>
    BodyDying,
    /// <summary>Draygon body animation: dying, part 1.</summary>
    BodyDyingExplosions,
    /// <summary>Draygon body animation: dying, part 2.</summary>
    BodyDyingExplosionsLoop,
    /// <summary>Draygon animation: delete.</summary>
    Delete,
    /// <summary>Draygon body animation: facing left, fire goop.</summary>
    BodyFacingLeftFireGoop,
    /// <summary>Draygon body animation: facing left, roar.</summary>
    BodyFacingLeftRoar,
    /// <summary>Draygon eye animation: facing left, idle.</summary>
    EyeFacingLeftIdle,
    /// <summary>Draygon eye animation: facing left, dying, part 0.</summary>
    EyeFacingLeftDying,
    /// <summary>Draygon eye animation: facing left, dying, part 1.</summary>
    EyeFacingLeftDyingLoop,
    /// <summary>Draygon eye animation: facing left, dead.</summary>
    EyeFacingLeftDead,
    /// <summary>Draygon eye animation: facing left, looking left.</summary>
    EyeFacingLeftLookingLeft,
    /// <summary>Draygon eye animation: facing left, looking right.</summary>
    EyeFacingLeftLookingRight,
    /// <summary>Draygon eye animation: facing left, looking up.</summary>
    EyeFacingLeftLookingUp,
    /// <summary>Draygon eye animation: facing left, looking down.</summary>
    EyeFacingLeftLookingDown,
    /// <summary>Draygon tail animation: facing left, idle, part 0.</summary>
    TailFacingLeftIdle,
    /// <summary>Draygon tail animation: facing left, fake tail whip.</summary>
    TailFacingLeftInitialFakeWhip,
    /// <summary>Draygon tail animation: facing left, final tail whips, part 0.</summary>
    TailFacingLeftFinalWhips,
    /// <summary>Draygon tail animation: facing left, final tail whips, part 1.</summary>
    TailFacingLeftFinalWhipsLoop,
    /// <summary>Draygon tail animation: facing left, tail whip.</summary>
    TailFacingLeftWhip,
    /// <summary>Draygon tail animation: facing left, tail flail.</summary>
    TailFacingLeftFlail,
    /// <summary>Draygon arms animation: facing right, idle, part 0.</summary>
    ArmsFacingRightIdle,
    /// <summary>Draygon arms animation: facing right, near swoop apex.</summary>
    ArmsFacingRightNearSwoopApex,
    /// <summary>Draygon arms animation: facing right, grab.</summary>
    ArmsFacingRightGrab,
    /// <summary>Draygon arms animation: facing right, dying, part 0.</summary>
    ArmsFacingRightDying,
    /// <summary>Draygon body animation: facing right, idle.</summary>
    BodyFacingRightIdle,
    /// <summary>Draygon body animation: facing right, fire goop.</summary>
    BodyFacingRightFireGoop,
    /// <summary>Draygon body animation: facing right, roar.</summary>
    BodyFacingRightRoar,
    /// <summary>Draygon eye animation: facing right, idle.</summary>
    EyeFacingRightIdle,
    /// <summary>Draygon eye animation: facing right, dying, part 0.</summary>
    EyeFacingRightDying,
    /// <summary>Draygon eye animation: facing right, dying, part 1.</summary>
    EyeFacingRightDyingLoop,
    /// <summary>Draygon eye animation: facing right, dead.</summary>
    EyeFacingRightDead,
    /// <summary>Draygon eye animation: facing right, looking right.</summary>
    EyeFacingRightLookingRight,
    /// <summary>Draygon eye animation: facing right, looking left.</summary>
    EyeFacingRightLookingLeft,
    /// <summary>Draygon eye animation: facing right, looking up.</summary>
    EyeFacingRightLookingUp,
    /// <summary>Draygon eye animation: facing right, looking down.</summary>
    EyeFacingRightLookingDown,
    /// <summary>Draygon tail animation: facing right, idle, part 0.</summary>
    TailFacingRightIdle,
    /// <summary>Draygon tail animation: facing right, final tail whips, part 0.</summary>
    TailFacingRightFinalWhips,
    /// <summary>Draygon tail animation: facing right, final tail whips, part 1.</summary>
    TailFacingRightFinalWhipsLoop,
    /// <summary>Draygon tail animation: facing right, tail whip, part 0.</summary>
    TailFacingRightWhip,
    /// <summary>Draygon tail animation: facing right, tail flail, part 0.</summary>
    TailFacingRightFlail,
}
