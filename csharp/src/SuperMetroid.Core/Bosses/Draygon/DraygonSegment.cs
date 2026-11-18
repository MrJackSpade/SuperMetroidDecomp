namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's body, eye, tail and arms animation segments, in authored order. The four parts
/// run animations of one set: a body cue starts all four, and his body's dying runs on into its cry and explosions.
/// Each segment holds beats and is played whole; the eye's dying plays its first segment four times, and the tail's final
/// whips play its whip four times (#1938, #2296, #2399).
/// </summary>
public enum DraygonSegment
{
    /// <summary>Nothing: the part keeps the picture it last showed, as the tail and arms do once his dying paralyses them.</summary>
    Rest,
    /// <summary>Starts his four parts' idles facing left, his eye tracking Samus, and shows his body with its jaw shut.</summary>
    BodyFacingLeftReset,
    /// <summary>Starts his four parts' idles facing right, his eye tracking Samus, and shows his body with its jaw shut.</summary>
    BodyFacingRightReset,
    /// <summary>One stroke of his arms facing left: forward, lowering, down, stretched down, raising and reaching, five updates each.</summary>
    ArmsFacingLeftIdle,
    /// <summary>His arms facing left spreading wide and closing into a tuck, which they hold for 64 updates near the top of his swoop.</summary>
    ArmsFacingLeftNearSwoopApex,
    /// <summary>His arms facing left spreading wide and closing round Samus, tucked for eight updates, then opening and closing again.</summary>
    ArmsFacingLeftGrab,
    /// <summary>
    /// His body facing left going limp as he dies, its claw raised, dropping, dropped and curled, five updates each; it runs on
    /// into his dying cry.
    /// </summary>
    BodyFacingLeftDying,
    /// <summary>His body facing left with its jaw shut, his eye tracking Samus.</summary>
    BodyFacingLeftIdle,
    /// <summary>His dying cry, as his body stops touching Samus.</summary>
    BodyDyingCry,
    /// <summary>One of the eight bursts of his dying: a wait of twelve updates, then explosions, a dust cloud and breath bubbles with their sound.</summary>
    BodyDyingExplosions,
    /// <summary>The pause after his dying's eight bursts, which stills his tail and arms.</summary>
    BodyDyingParalysis,
    /// <summary>The burst that repeats for good once his tail and arms are still: explosions, a dust cloud and breath bubbles every sixteen updates.</summary>
    BodyDyingExplosionLoop,
    /// <summary>Nothing: the part is removed.</summary>
    Delete,
    /// <summary>His body facing left opening its jaw wide to spit goop with its sound, then shutting it.</summary>
    BodyFacingLeftFireGoop,
    /// <summary>His body facing left roaring: its jaw opening wide and shutting again, six updates a picture.</summary>
    BodyFacingLeftRoar,
    /// <summary>His eye facing left shut, opening, staring, narrowing and shutting again, then tracking Samus.</summary>
    EyeFacingLeftIdle,
    /// <summary>His eye facing left rolling in its dying, open, narrowed, bloodshot and closing, which the dying plays four times.</summary>
    EyeFacingLeftDying,
    /// <summary>His eye facing left settling into a wide stare after its rolling, then half open.</summary>
    EyeFacingLeftDyingStare,
    /// <summary>His eye facing left closing slowly once he is dead: its wide, half-open and barely open pictures for 32 updates each, then shut.</summary>
    EyeFacingLeftDead,
    /// <summary>His eye facing left wide open as it looks left.</summary>
    EyeFacingLeftLookingLeft,
    /// <summary>His eye facing left turned back to look right, its bloodshot picture.</summary>
    EyeFacingLeftLookingRight,
    /// <summary>His eye facing left narrowed as it looks up.</summary>
    EyeFacingLeftLookingUp,
    /// <summary>His eye facing left half shut as it looks down.</summary>
    EyeFacingLeftLookingDown,
    /// <summary>His tail facing left swaying: its tip curled in, swinging out to its farthest and back again.</summary>
    TailFacingLeftIdle,
    /// <summary>
    /// His tail facing left winding up long, lashing out with his body's picture shifting and holding its recoil, without
    /// striking; it goes on into his idle tail.
    /// </summary>
    TailFacingLeftInitialFakeWhip,
    /// <summary>Releases Samus after his tail's final whips facing left.</summary>
    TailFacingLeftReleaseSamus,
    /// <summary>
    /// His tail facing left lashing out with his body's picture shifting, striking the held Samus with an explosion, and
    /// recoiling; the final whips play it four times.
    /// </summary>
    TailFacingLeftWhip,
    /// <summary>His tail facing left flailing: lashing out with an explosion and recoiling, his body's picture still and nothing struck.</summary>
    TailFacingLeftFlail,
    /// <summary>One stroke of his arms facing right: forward, lowering, down, stretched down, raising and reaching, five updates each.</summary>
    ArmsFacingRightIdle,
    /// <summary>His arms facing right spreading wide and closing into a tuck, which they hold for 64 updates near the top of his swoop.</summary>
    ArmsFacingRightNearSwoopApex,
    /// <summary>His arms facing right spreading wide and closing round Samus, tucked for eight updates, then opening and closing again.</summary>
    ArmsFacingRightGrab,
    /// <summary>
    /// His body facing right going limp as he dies, its claw raised, dropping, dropped and curled, five updates each; it runs
    /// on into his dying cry.
    /// </summary>
    BodyFacingRightDying,
    /// <summary>His body facing right with its jaw shut, his eye tracking Samus.</summary>
    BodyFacingRightIdle,
    /// <summary>His body facing right opening its jaw wide to spit goop with its sound, then shutting it.</summary>
    BodyFacingRightFireGoop,
    /// <summary>His body facing right roaring: its jaw opening wide and shutting again, six updates a picture.</summary>
    BodyFacingRightRoar,
    /// <summary>His eye facing right shut, opening, staring, narrowing and shutting again, then tracking Samus.</summary>
    EyeFacingRightIdle,
    /// <summary>His eye facing right rolling in its dying, open, narrowed, bloodshot and closing, which the dying plays four times.</summary>
    EyeFacingRightDying,
    /// <summary>His eye facing right settling into a wide stare after its rolling, then half open.</summary>
    EyeFacingRightDyingStare,
    /// <summary>His eye facing right closing slowly once he is dead: its wide, half-open and barely open pictures for 32 updates each, then shut.</summary>
    EyeFacingRightDead,
    /// <summary>His eye facing right wide open as it looks right.</summary>
    EyeFacingRightLookingRight,
    /// <summary>His eye facing right turned back to look left, its bloodshot picture.</summary>
    EyeFacingRightLookingLeft,
    /// <summary>His eye facing right narrowed as it looks up.</summary>
    EyeFacingRightLookingUp,
    /// <summary>His eye facing right half shut as it looks down.</summary>
    EyeFacingRightLookingDown,
    /// <summary>His tail facing right swaying: its tip curled in, swinging out to its farthest and back again.</summary>
    TailFacingRightIdle,
    /// <summary>Releases Samus after his tail's final whips facing right.</summary>
    TailFacingRightReleaseSamus,
    /// <summary>
    /// His tail facing right lashing out with his body's picture shifting, striking the held Samus with an explosion, and
    /// recoiling; the final whips play it four times.
    /// </summary>
    TailFacingRightWhip,
    /// <summary>His tail facing right flailing: lashing out with an explosion and recoiling, his body's picture still and nothing struck.</summary>
    TailFacingRightFlail,
}
