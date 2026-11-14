using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One Draygon picture its typed animations show (#1747): the arms, body, eye and tail poses of each facing, named for the
/// installed frame <see cref="DraygonFrameDefinitions"/> shows for each. Its values are arbitrary.
/// </summary>
public enum DraygonFrame
{
    /// <summary>Arms facing left idle first.</summary>
    ArmsFacingLeftIdleFirst,
    /// <summary>Arms facing left idle second.</summary>
    ArmsFacingLeftIdleSecond,
    /// <summary>Arms facing left idle third.</summary>
    ArmsFacingLeftIdleThird,
    /// <summary>Arms facing left idle fourth.</summary>
    ArmsFacingLeftIdleFourth,
    /// <summary>Arms facing left idle fifth.</summary>
    ArmsFacingLeftIdleFifth,
    /// <summary>Arms facing left idle sixth.</summary>
    ArmsFacingLeftIdleSixth,
    /// <summary>Arms facing left dying first.</summary>
    ArmsFacingLeftDyingFirst,
    /// <summary>Arms facing left dying second.</summary>
    ArmsFacingLeftDyingSecond,
    /// <summary>Arms facing left dying third.</summary>
    ArmsFacingLeftDyingThird,
    /// <summary>Arms facing left dying fourth.</summary>
    ArmsFacingLeftDyingFourth,
    /// <summary>Body facing left fire goop first.</summary>
    BodyFacingLeftFireGoopFirst,
    /// <summary>Body facing left fire goop second.</summary>
    BodyFacingLeftFireGoopSecond,
    /// <summary>Body facing left fire goop third.</summary>
    BodyFacingLeftFireGoopThird,
    /// <summary>Body facing left fire goop fourth.</summary>
    BodyFacingLeftFireGoopFourth,
    /// <summary>Eye facing left idle first.</summary>
    EyeFacingLeftIdleFirst,
    /// <summary>Eye facing left idle second.</summary>
    EyeFacingLeftIdleSecond,
    /// <summary>Eye facing left idle third.</summary>
    EyeFacingLeftIdleThird,
    /// <summary>Eye facing left dying loop fifth.</summary>
    EyeFacingLeftDyingLoopFifth,
    /// <summary>Eye facing left idle fourth.</summary>
    EyeFacingLeftIdleFourth,
    /// <summary>Eye facing left dying loop third.</summary>
    EyeFacingLeftDyingLoopThird,
    /// <summary>Eye facing left idle fifth.</summary>
    EyeFacingLeftIdleFifth,
    /// <summary>Eye facing left idle sixth.</summary>
    EyeFacingLeftIdleSixth,
    /// <summary>Body facing left reset.</summary>
    BodyFacingLeftReset,
    /// <summary>Arms facing left near swoop apex first.</summary>
    ArmsFacingLeftNearSwoopApexFirst,
    /// <summary>Arms facing left near swoop apex second.</summary>
    ArmsFacingLeftNearSwoopApexSecond,
    /// <summary>Arms facing left near swoop apex third.</summary>
    ArmsFacingLeftNearSwoopApexThird,
    /// <summary>Arms facing left near swoop apex fourth.</summary>
    ArmsFacingLeftNearSwoopApexFourth,
    /// <summary>Tail facing left idle first.</summary>
    TailFacingLeftIdleFirst,
    /// <summary>Tail facing left idle second.</summary>
    TailFacingLeftIdleSecond,
    /// <summary>Tail facing left idle third.</summary>
    TailFacingLeftIdleThird,
    /// <summary>Tail facing left idle fourth.</summary>
    TailFacingLeftIdleFourth,
    /// <summary>Tail facing left idle fifth.</summary>
    TailFacingLeftIdleFifth,
    /// <summary>Tail facing left idle sixth.</summary>
    TailFacingLeftIdleSixth,
    /// <summary>Tail facing left idle seventh.</summary>
    TailFacingLeftIdleSeventh,
    /// <summary>Tail facing left initial fake whip second.</summary>
    TailFacingLeftInitialFakeWhipSecond,
    /// <summary>Tail facing left initial fake whip third.</summary>
    TailFacingLeftInitialFakeWhipThird,
    /// <summary>Tail facing left initial fake whip fourth.</summary>
    TailFacingLeftInitialFakeWhipFourth,
    /// <summary>Tail facing left initial fake whip fifth.</summary>
    TailFacingLeftInitialFakeWhipFifth,
    /// <summary>Tail facing left initial fake whip sixth.</summary>
    TailFacingLeftInitialFakeWhipSixth,
    /// <summary>Tail facing left initial fake whip seventh.</summary>
    TailFacingLeftInitialFakeWhipSeventh,
    /// <summary>Tail facing left initial fake whip eighth.</summary>
    TailFacingLeftInitialFakeWhipEighth,
    /// <summary>Arms facing right idle first.</summary>
    ArmsFacingRightIdleFirst,
    /// <summary>Arms facing right idle second.</summary>
    ArmsFacingRightIdleSecond,
    /// <summary>Arms facing right idle third.</summary>
    ArmsFacingRightIdleThird,
    /// <summary>Arms facing right idle fourth.</summary>
    ArmsFacingRightIdleFourth,
    /// <summary>Arms facing right idle fifth.</summary>
    ArmsFacingRightIdleFifth,
    /// <summary>Arms facing right idle sixth.</summary>
    ArmsFacingRightIdleSixth,
    /// <summary>Arms facing right dying first.</summary>
    ArmsFacingRightDyingFirst,
    /// <summary>Arms facing right dying second.</summary>
    ArmsFacingRightDyingSecond,
    /// <summary>Arms facing right dying third.</summary>
    ArmsFacingRightDyingThird,
    /// <summary>Arms facing right dying fourth.</summary>
    ArmsFacingRightDyingFourth,
    /// <summary>Body facing right fire goop first.</summary>
    BodyFacingRightFireGoopFirst,
    /// <summary>Body facing right fire goop second.</summary>
    BodyFacingRightFireGoopSecond,
    /// <summary>Body facing right fire goop third.</summary>
    BodyFacingRightFireGoopThird,
    /// <summary>Body facing right fire goop fourth.</summary>
    BodyFacingRightFireGoopFourth,
    /// <summary>Eye facing right idle first.</summary>
    EyeFacingRightIdleFirst,
    /// <summary>Eye facing right idle second.</summary>
    EyeFacingRightIdleSecond,
    /// <summary>Eye facing right idle third.</summary>
    EyeFacingRightIdleThird,
    /// <summary>Eye facing right dying loop fifth.</summary>
    EyeFacingRightDyingLoopFifth,
    /// <summary>Eye facing right idle fourth.</summary>
    EyeFacingRightIdleFourth,
    /// <summary>Eye facing right dying loop third.</summary>
    EyeFacingRightDyingLoopThird,
    /// <summary>Eye facing right idle fifth.</summary>
    EyeFacingRightIdleFifth,
    /// <summary>Eye facing right idle sixth.</summary>
    EyeFacingRightIdleSixth,
    /// <summary>Body facing right reset.</summary>
    BodyFacingRightReset,
    /// <summary>Arms facing right near swoop apex first.</summary>
    ArmsFacingRightNearSwoopApexFirst,
    /// <summary>Arms facing right near swoop apex second.</summary>
    ArmsFacingRightNearSwoopApexSecond,
    /// <summary>Arms facing right near swoop apex third.</summary>
    ArmsFacingRightNearSwoopApexThird,
    /// <summary>Arms facing right near swoop apex fourth.</summary>
    ArmsFacingRightNearSwoopApexFourth,
    /// <summary>Tail facing right idle first.</summary>
    TailFacingRightIdleFirst,
    /// <summary>Tail facing right idle second.</summary>
    TailFacingRightIdleSecond,
    /// <summary>Tail facing right idle third.</summary>
    TailFacingRightIdleThird,
    /// <summary>Tail facing right idle fourth.</summary>
    TailFacingRightIdleFourth,
    /// <summary>Tail facing right idle fifth.</summary>
    TailFacingRightIdleFifth,
    /// <summary>Tail facing right idle sixth.</summary>
    TailFacingRightIdleSixth,
    /// <summary>Tail facing right idle seventh.</summary>
    TailFacingRightIdleSeventh,
    /// <summary>Tail facing right final whips loop second.</summary>
    TailFacingRightFinalWhipsLoopSecond,
    /// <summary>Tail facing right final whips loop third.</summary>
    TailFacingRightFinalWhipsLoopThird,
    /// <summary>Tail facing right final whips loop fourth.</summary>
    TailFacingRightFinalWhipsLoopFourth,
    /// <summary>Tail facing right final whips loop fifth.</summary>
    TailFacingRightFinalWhipsLoopFifth,
    /// <summary>Tail facing right final whips loop sixth.</summary>
    TailFacingRightFinalWhipsLoopSixth,
    /// <summary>Tail facing right final whips loop seventh.</summary>
    TailFacingRightFinalWhipsLoopSeventh,
    /// <summary>Tail facing right final whips loop eighth.</summary>
    TailFacingRightFinalWhipsLoopEighth,
}
