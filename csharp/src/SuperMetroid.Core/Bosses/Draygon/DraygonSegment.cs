namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's body, eye, tail and arms animation segments, in authored order. The four parts
/// run animations of one set: a body cue starts all four, and the arms' dying animation runs on into the body's.
/// Each segment holds beats and is played whole; the eye's dying and the tail's final whips play their first segment four times (#1938, #2296).
/// </summary>
public enum DraygonSegment
{
    /// <summary>Draygon animation: a rest on the last frame.</summary>
    Rest,
    /// <summary>Draygon body animation: facing left, reset.</summary>
    BodyFacingLeftReset,
    /// <summary>Draygon body animation: facing right, reset.</summary>
    BodyFacingRightReset,
    /// <summary>Draygon arms animation: facing left, idle.</summary>
    ArmsFacingLeftIdle,
    /// <summary>Draygon arms animation: facing left, near swoop apex.</summary>
    ArmsFacingLeftNearSwoopApex,
    /// <summary>Draygon arms animation: facing left, grab.</summary>
    ArmsFacingLeftGrab,
    /// <summary>Draygon arms animation: facing left, dying.</summary>
    ArmsFacingLeftDying,
    /// <summary>Draygon body animation: facing left, idle.</summary>
    BodyFacingLeftIdle,
    /// <summary>Draygon body animation: dying, its cry as the body turns intangible.</summary>
    BodyDying,
    /// <summary>Draygon body animation: dying, one of the eight counted explosion bursts.</summary>
    BodyDyingExplosions,
    /// <summary>Draygon body animation: dying, the pause after the counted bursts and the paralysis of the tail and arms.</summary>
    BodyDyingParalysis,
    /// <summary>Draygon body animation: dying, the explosion burst that repeats for good once the tail and arms are paralysed.</summary>
    BodyDyingExplosionLoop,
    /// <summary>Draygon animation: delete.</summary>
    Delete,
    /// <summary>Draygon body animation: facing left, fire goop.</summary>
    BodyFacingLeftFireGoop,
    /// <summary>Draygon body animation: facing left, roar.</summary>
    BodyFacingLeftRoar,
    /// <summary>Draygon eye animation: facing left, idle.</summary>
    EyeFacingLeftIdle,
    /// <summary>Draygon eye animation: facing left, dying, its rolling, which the dying animation plays four times.</summary>
    EyeFacingLeftDying,
    /// <summary>Draygon eye animation: facing left, dying, the wide stare it settles into after its rolling.</summary>
    EyeFacingLeftDyingStare,
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
    /// <summary>Draygon tail animation: facing left, idle.</summary>
    TailFacingLeftIdle,
    /// <summary>Draygon tail animation: facing left, fake tail whip.</summary>
    TailFacingLeftInitialFakeWhip,
    /// <summary>Draygon tail animation: facing left, a final tail whip, which the final whips play four times.</summary>
    TailFacingLeftFinalWhips,
    /// <summary>Draygon tail animation: facing left, the release of Samus after the final whips.</summary>
    TailFacingLeftReleaseSamus,
    /// <summary>Draygon tail animation: facing left, tail whip.</summary>
    TailFacingLeftWhip,
    /// <summary>Draygon tail animation: facing left, tail flail.</summary>
    TailFacingLeftFlail,
    /// <summary>Draygon arms animation: facing right, idle.</summary>
    ArmsFacingRightIdle,
    /// <summary>Draygon arms animation: facing right, near swoop apex.</summary>
    ArmsFacingRightNearSwoopApex,
    /// <summary>Draygon arms animation: facing right, grab.</summary>
    ArmsFacingRightGrab,
    /// <summary>Draygon arms animation: facing right, dying.</summary>
    ArmsFacingRightDying,
    /// <summary>Draygon body animation: facing right, idle.</summary>
    BodyFacingRightIdle,
    /// <summary>Draygon body animation: facing right, fire goop.</summary>
    BodyFacingRightFireGoop,
    /// <summary>Draygon body animation: facing right, roar.</summary>
    BodyFacingRightRoar,
    /// <summary>Draygon eye animation: facing right, idle.</summary>
    EyeFacingRightIdle,
    /// <summary>Draygon eye animation: facing right, dying, its rolling, which the dying animation plays four times.</summary>
    EyeFacingRightDying,
    /// <summary>Draygon eye animation: facing right, dying, the wide stare it settles into after its rolling.</summary>
    EyeFacingRightDyingStare,
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
    /// <summary>Draygon tail animation: facing right, idle.</summary>
    TailFacingRightIdle,
    /// <summary>Draygon tail animation: facing right, a final tail whip, which the final whips play four times.</summary>
    TailFacingRightFinalWhips,
    /// <summary>Draygon tail animation: facing right, the release of Samus after the final whips.</summary>
    TailFacingRightReleaseSamus,
    /// <summary>Draygon tail animation: facing right, tail whip.</summary>
    TailFacingRightWhip,
    /// <summary>Draygon tail animation: facing right, tail flail.</summary>
    TailFacingRightFlail,
}
