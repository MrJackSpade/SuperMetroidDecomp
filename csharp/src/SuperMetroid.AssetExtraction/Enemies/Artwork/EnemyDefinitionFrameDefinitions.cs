using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The frames each enemy definition can show (#1392): the frames its behaviour's code reaches, which its pre-coloured
/// sheets hold. Generated from the enemy behaviours by tools/frame-definitions-generator; regenerate rather than edit.
/// </summary>
[ArchitectureReview("large", "Generated data: the two thousand frames the enemy definitions show; it holds no logic, and splitting it by family would only scatter one table the sheet composer reads as a whole.")]
internal static class EnemyDefinitionFrameDefinitions
{
    /// <summary>The frames <c>Areas/Ceres/CeresDoorBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] CeresDoor =
    [
        EnemyFrameId.CeresDoorRotatingOverlay,
        EnemyFrameId.CeresDoorLeftHold,
        EnemyFrameId.CeresDoorCloseFacingLeftFourth,
        EnemyFrameId.CeresDoorCloseFacingLeftThird,
        EnemyFrameId.CeresDoorCloseFacingLeftSecond,
        EnemyFrameId.CeresDoorCloseFacingLeftFirst,
        EnemyFrameId.CeresDoorRightHold,
        EnemyFrameId.CeresDoorRidleyRoomFacingRightFourth,
        EnemyFrameId.CeresDoorRidleyRoomFacingRightThird,
        EnemyFrameId.CeresDoorRidleyRoomFacingRightSecond,
        EnemyFrameId.CeresDoorRidleyRoomFacingRightFirst,
        EnemyFrameId.CeresDoorInitial,
        EnemyFrameId.CeresDoorTurningFloorLeftWall,
        EnemyFrameId.CeresDoorTurningFloorRightWall,
    ];

    /// <summary>The frames <c>Areas/Ceres/CeresSteamBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] CeresSteam =
    [
        EnemyFrameId.CeresSteamUp,
        EnemyFrameId.CeresSteamUpLoopTwoSecond,
        EnemyFrameId.CeresSteamUpLoopTwoThird,
        EnemyFrameId.CeresSteamUpLoopTwoFourth,
        EnemyFrameId.CeresSteamUpLoopTwoFifth,
        EnemyFrameId.CeresSteamUpLoopTwoSixth,
        EnemyFrameId.CeresSteamUpLoopTwoSeventh,
        EnemyFrameId.CeresSteamLeft,
        EnemyFrameId.CeresSteamLeftLoopTwoSecond,
        EnemyFrameId.CeresSteamLeftLoopTwoThird,
        EnemyFrameId.CeresSteamLeftLoopTwoFourth,
        EnemyFrameId.CeresSteamLeftLoopTwoFifth,
        EnemyFrameId.CeresSteamLeftLoopTwoSixth,
        EnemyFrameId.CeresSteamLeftLoopTwoSeventh,
        EnemyFrameId.CeresSteamDown,
        EnemyFrameId.CeresSteamDownLoopTwoSecond,
        EnemyFrameId.CeresSteamDownLoopTwoThird,
        EnemyFrameId.CeresSteamDownLoopTwoFourth,
        EnemyFrameId.CeresSteamDownLoopTwoFifth,
        EnemyFrameId.CeresSteamDownLoopTwoSixth,
        EnemyFrameId.CeresSteamDownLoopTwoSeventh,
        EnemyFrameId.CeresSteamRight,
        EnemyFrameId.CeresSteamRightLoopTwoSecond,
        EnemyFrameId.CeresSteamRightLoopTwoThird,
        EnemyFrameId.CeresSteamRightLoopTwoFourth,
        EnemyFrameId.CeresSteamRightLoopTwoFifth,
        EnemyFrameId.CeresSteamRightLoopTwoSixth,
        EnemyFrameId.CeresSteamRightLoopTwoSeventh,
    ];

    /// <summary>The frames <c>Areas/Tourian/TourianEntranceStatueBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] TourianEntranceStatue = [];

    /// <summary>The frames <c>Bosses/Botwoon/BotwoonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Botwoon =
    [
        EnemyFrameId.BotwoonHeadClosedUpLeft,
        EnemyFrameId.BotwoonHeadClosedLeft,
        EnemyFrameId.BotwoonHeadClosedDownLeft,
        EnemyFrameId.BotwoonHeadClosedDown,
        EnemyFrameId.BotwoonHeadClosedDownRight,
        EnemyFrameId.BotwoonHeadClosedRight,
        EnemyFrameId.BotwoonHeadClosedUpRight,
        EnemyFrameId.BotwoonHeadClosedUp,
        EnemyFrameId.BotwoonHeadOpenUpLeft,
        EnemyFrameId.BotwoonHeadOpenLeft,
        EnemyFrameId.BotwoonHeadOpenDownLeft,
        EnemyFrameId.BotwoonHeadOpenDown,
        EnemyFrameId.BotwoonHeadOpenDownRight,
        EnemyFrameId.BotwoonHeadOpenRight,
        EnemyFrameId.BotwoonHeadOpenUpRight,
        EnemyFrameId.BotwoonHeadOpenUp,
    ];

    /// <summary>The frames <c>Bosses/Crocomire/CrocomireBehavior.cs</c> shows for <c>Crocomire</c>.</summary>
    private static readonly EnemyFrameId[] CrocomireFrames =
    [
        EnemyFrameId.CrocomireTongueFightFirst,
        EnemyFrameId.CrocomireTongueFightSecond,
        EnemyFrameId.CrocomireTongueFightThird,
        EnemyFrameId.CrocomireTongueFightFourth,
        EnemyFrameId.CrocomireTongueMeltingPoseFirst,
        EnemyFrameId.CrocomireTongueMeltingPoseSecond,
        EnemyFrameId.CrocomireTongueMeltingPoseThird,
        EnemyFrameId.CrocomireTongueMeltingPoseFourth,
        EnemyFrameId.CrocomireTongueMeltingPoseFifth,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFirst,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSecond,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopThird,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFourth,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopFifth,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSixth,
        EnemyFrameId.CrocomireBodyPowerBombReactionMouthNotOpenLoopSeventh,
        EnemyFrameId.CrocomireBodySteppingBackFifth,
        EnemyFrameId.CrocomireBodySteppingBackFourth,
        EnemyFrameId.CrocomireBodySteppingBackThird,
        EnemyFrameId.CrocomireBodySteppingBackSecond,
        EnemyFrameId.CrocomireBodySteppingBackFirst,
        EnemyFrameId.CrocomireBodyInitial,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSixth,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFifth,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFourth,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageThird,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageSecond,
        EnemyFrameId.CrocomireBodyWaitForFirstSecondDamageFirst,
        EnemyFrameId.CrocomireBodyProjectileAttackFirst,
        EnemyFrameId.CrocomireBodyProjectileAttackSecond,
        EnemyFrameId.CrocomireBodyProjectileAttackThird,
        EnemyFrameId.CrocomireBodyProjectileAttackFourth,
        EnemyFrameId.CrocomireBodyProjectileAttackFifth,
        EnemyFrameId.CrocomireBodyProjectileAttackSixth,
        EnemyFrameId.CrocomireBodyProjectileAttackSeventh,
        EnemyFrameId.CrocomireBodyProjectileAttackEighth,
        EnemyFrameId.CrocomireBodyStepForwardAfterDelay,
        EnemyFrameId.CrocomireBodyStepForwardFourth,
        EnemyFrameId.CrocomireBodyStepForwardFifth,
        EnemyFrameId.CrocomireBodyStepForwardSixth,
        EnemyFrameId.CrocomireBodyStepForwardSeventh,
        EnemyFrameId.CrocomireBodyStepForwardEighth,
        EnemyFrameId.CrocomireBodyStepForwardNinth,
        EnemyFrameId.CrocomireBodyStepForwardTenth,
        EnemyFrameId.CrocomireBodyStepForwardEleventh,
        EnemyFrameId.CrocomireBodyStepForwardTwelfth,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFirst,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopSecond,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopThird,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFourth,
        EnemyFrameId.CrocomireBodyProjectileAttackLoopFifth,
        EnemyFrameId.CrocomireBodyMeltingOneTopRow,
        EnemyFrameId.CrocomireBodyMeltingOneTopTwoRows,
        EnemyFrameId.CrocomireBodyMeltingOneTopThreeRows,
        EnemyFrameId.CrocomireBodyMeltingOneTopFourRows,
        EnemyFrameId.CrocomireBodyMeltingTwoTopRow,
        EnemyFrameId.CrocomireBodyMeltingTwoTopTwoRows,
        EnemyFrameId.CrocomireBodyMeltingTwoTopThreeRows,
        EnemyFrameId.CrocomireBodyMeltingTwoTopFourRows,
        EnemyFrameId.CrocomireBodySkeletonFallingFirst,
        EnemyFrameId.CrocomireBodySkeletonFallingSecond,
        EnemyFrameId.CrocomireBodySkeletonFallingThird,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFirst,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSecond,
        EnemyFrameId.CrocomireBodySkeletonFallsApartThird,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFourth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFifth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSixth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSeventh,
        EnemyFrameId.CrocomireBodySkeletonFallsApartEighth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartNinth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartTenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartEleventh,
        EnemyFrameId.CrocomireBodySkeletonFallsApartTwelfth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartThirteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFourteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartFifteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSixteenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartSeventeenth,
        EnemyFrameId.CrocomireBodySkeletonFallsApartEighteenth,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFirst,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSecond,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverThird,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFourth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverFifth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSixth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverSeventh,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverEighth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverNinth,
        EnemyFrameId.CrocomireBodySkeletonFlowingDownRiverTenth,
    ];

    /// <summary>The frames <c>Bosses/Crocomire/CrocomireBehavior.cs</c> shows for <c>CrocomireTongue</c>.</summary>
    private static readonly EnemyFrameId[] CrocomireTongueFrames =
    [
        EnemyFrameId.CrocomireTongueFightFirst,
        EnemyFrameId.CrocomireTongueFightSecond,
        EnemyFrameId.CrocomireTongueFightThird,
        EnemyFrameId.CrocomireTongueFightFourth,
        EnemyFrameId.CrocomireTongueMeltingPoseFirst,
        EnemyFrameId.CrocomireTongueMeltingPoseSecond,
        EnemyFrameId.CrocomireTongueMeltingPoseThird,
        EnemyFrameId.CrocomireTongueMeltingPoseFourth,
        EnemyFrameId.CrocomireTongueMeltingPoseFifth,
    ];

    /// <summary>The frames <c>Bosses/Draygon/DraygonBehavior.cs</c> shows for <c>DraygonArms</c>.</summary>
    private static readonly EnemyFrameId[] DraygonArmsFrames =
    [
        EnemyFrameId.DraygonArmsFacingLeftIdleFirst,
        EnemyFrameId.DraygonArmsFacingLeftIdleSecond,
        EnemyFrameId.DraygonArmsFacingLeftIdleThird,
        EnemyFrameId.DraygonArmsFacingLeftIdleFourth,
        EnemyFrameId.DraygonArmsFacingLeftIdleFifth,
        EnemyFrameId.DraygonArmsFacingLeftIdleSixth,
        EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexFirst,
        EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexSecond,
        EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexThird,
        EnemyFrameId.DraygonArmsFacingLeftNearSwoopApexFourth,
        EnemyFrameId.DraygonArmsFacingRightIdleFirst,
        EnemyFrameId.DraygonArmsFacingRightIdleSecond,
        EnemyFrameId.DraygonArmsFacingRightIdleThird,
        EnemyFrameId.DraygonArmsFacingRightIdleFourth,
        EnemyFrameId.DraygonArmsFacingRightIdleFifth,
        EnemyFrameId.DraygonArmsFacingRightIdleSixth,
        EnemyFrameId.DraygonArmsFacingRightNearSwoopApexFirst,
        EnemyFrameId.DraygonArmsFacingRightNearSwoopApexSecond,
        EnemyFrameId.DraygonArmsFacingRightNearSwoopApexThird,
        EnemyFrameId.DraygonArmsFacingRightNearSwoopApexFourth,
    ];

    /// <summary>The frames <c>Bosses/Draygon/DraygonBehavior.cs</c> shows for <c>DraygonTail</c>.</summary>
    private static readonly EnemyFrameId[] DraygonTailFrames =
    [
        EnemyFrameId.DraygonTailFacingLeftIdleFirst,
        EnemyFrameId.DraygonTailFacingLeftIdleSecond,
        EnemyFrameId.DraygonTailFacingLeftIdleThird,
        EnemyFrameId.DraygonTailFacingLeftIdleFourth,
        EnemyFrameId.DraygonTailFacingLeftIdleFifth,
        EnemyFrameId.DraygonTailFacingLeftIdleSixth,
        EnemyFrameId.DraygonTailFacingLeftIdleSeventh,
        EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipSecond,
        EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipThird,
        EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipFourth,
        EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipFifth,
        EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipSixth,
        EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipSeventh,
        EnemyFrameId.DraygonTailFacingLeftInitialFakeWhipEighth,
        EnemyFrameId.DraygonTailFacingRightIdleFirst,
        EnemyFrameId.DraygonTailFacingRightIdleSecond,
        EnemyFrameId.DraygonTailFacingRightIdleThird,
        EnemyFrameId.DraygonTailFacingRightIdleFourth,
        EnemyFrameId.DraygonTailFacingRightIdleFifth,
        EnemyFrameId.DraygonTailFacingRightIdleSixth,
        EnemyFrameId.DraygonTailFacingRightIdleSeventh,
        EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopSecond,
        EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopThird,
        EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopFourth,
        EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopFifth,
        EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopSixth,
        EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopSeventh,
        EnemyFrameId.DraygonTailFacingRightFinalWhipsLoopEighth,
    ];

    /// <summary>The frames <c>Bosses/Kraid/FakeKraidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] FakeKraid =
    [
        EnemyFrameId.FakeKraidStepForwardsFacingLeftFirst,
        EnemyFrameId.FakeKraidStepForwardsFacingLeftSecond,
        EnemyFrameId.FakeKraidStepForwardsFacingLeftThird,
        EnemyFrameId.FakeKraidStepForwardsFacingLeftFourth,
        EnemyFrameId.FakeKraidFireSpitFacingLeftFirst,
        EnemyFrameId.FakeKraidFireSpitFacingLeftSecond,
        EnemyFrameId.FakeKraidFireSpitFacingLeftThird,
        EnemyFrameId.FakeKraidStepForwardsFacingRightFirst,
        EnemyFrameId.FakeKraidStepForwardsFacingRightSecond,
        EnemyFrameId.FakeKraidStepForwardsFacingRightThird,
        EnemyFrameId.FakeKraidStepForwardsFacingRightFourth,
        EnemyFrameId.FakeKraidFireSpitFacingRightFirst,
        EnemyFrameId.FakeKraidFireSpitFacingRightSecond,
        EnemyFrameId.FakeKraidFireSpitFacingRightThird,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidArm</c>.</summary>
    private static readonly EnemyFrameId[] KraidArmFrames =
    [
        EnemyFrameId.KraidArmNormalFirst,
        EnemyFrameId.KraidArmNormalSecond,
        EnemyFrameId.KraidArmNormalThird,
        EnemyFrameId.KraidArmNormalFourth,
        EnemyFrameId.KraidArmNormalFifth,
        EnemyFrameId.KraidArmNormalSixth,
        EnemyFrameId.KraidArmNormalSeventh,
        EnemyFrameId.KraidArmNormalEighth,
        EnemyFrameId.KraidArmNormalNinth,
        EnemyFrameId.KraidArmNormalTenth,
        EnemyFrameId.KraidArmRisingOrSinkingFirst,
        EnemyFrameId.KraidArmRisingOrSinkingSecond,
        EnemyFrameId.KraidArmRisingOrSinkingThird,
        EnemyFrameId.KraidArmRisingOrSinkingFourth,
        EnemyFrameId.KraidArmRisingOrSinkingFifth,
        EnemyFrameId.KraidArmRisingOrSinkingSixth,
        EnemyFrameId.KraidArmRisingOrSinkingSeventh,
        EnemyFrameId.KraidArmRisingOrSinkingEighth,
        EnemyFrameId.KraidArmRisingOrSinkingNinth,
        EnemyFrameId.KraidArmRisingOrSinkingTenth,
        EnemyFrameId.KraidArmDyingOrPreparingToLungeSecond,
        EnemyFrameId.KraidArmDyingOrPreparingToLungeThird,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidFoot</c>.</summary>
    private static readonly EnemyFrameId[] KraidFootFrames =
    [
        EnemyFrameId.KraidFootNeutral,
        EnemyFrameId.KraidFootWalkingForwardSecond,
        EnemyFrameId.KraidFootWalkingForwardThird,
        EnemyFrameId.KraidFootWalkingForwardFourth,
        EnemyFrameId.KraidFootWalkingForwardFifth,
        EnemyFrameId.KraidFootWalkingForwardSixth,
        EnemyFrameId.KraidFootWalkingForwardSeventh,
        EnemyFrameId.KraidFootWalkingForwardEighth,
        EnemyFrameId.KraidFootWalkingForwardNinth,
        EnemyFrameId.KraidFootWalkingForwardTenth,
        EnemyFrameId.KraidFootWalkingForwardEleventh,
        EnemyFrameId.KraidFootWalkingForwardTwelfth,
        EnemyFrameId.KraidFootWalkingForwardThirteenth,
        EnemyFrameId.KraidFootWalkingForwardFourteenth,
        EnemyFrameId.KraidFootWalkingForwardFifteenth,
        EnemyFrameId.KraidFootWalkingForwardSixteenth,
        EnemyFrameId.KraidFootWalkingForwardSeventeenth,
        EnemyFrameId.KraidFootWalkingForwardEighteenth,
        EnemyFrameId.KraidFootWalkingForwardNineteenth,
        EnemyFrameId.KraidFootWalkingForwardTwentieth,
        EnemyFrameId.KraidFootWalkingBackwardThirteenth,
        EnemyFrameId.KraidFootWalkingForwardTwentyFirst,
        EnemyFrameId.KraidFootWalkingForwardTwentySecond,
        EnemyFrameId.KraidFootWalkingForwardTwentyThird,
        EnemyFrameId.KraidFootWalkingForwardTwentyFourth,
        EnemyFrameId.KraidFootWalkingForwardTwentyFifth,
        EnemyFrameId.KraidFootWalkingForwardTwentySixth,
        EnemyFrameId.KraidFootWalkingForwardTwentySeventh,
        EnemyFrameId.KraidFootWalkingForwardTwentyEighth,
        EnemyFrameId.KraidFootWalkingForwardTwentyNinth,
        EnemyFrameId.KraidFootWalkingForwardThirtieth,
        EnemyFrameId.KraidFootWalkingForwardThirtyFirst,
        EnemyFrameId.KraidFootWalkingBackwardSecond,
        EnemyFrameId.KraidFootWalkingBackwardFirst,
        EnemyFrameId.KraidFootStanding,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidLintBottom</c>, <c>KraidLintMiddle</c>, <c>KraidLintTop</c>.</summary>
    private static readonly EnemyFrameId[] KraidLintBottomFrames =
    [
        EnemyFrameId.KraidLintInitial,
        EnemyFrameId.KraidLintPostGrowth,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidNail</c>, <c>KraidNailBad</c>.</summary>
    private static readonly EnemyFrameId[] KraidNailFrames =
    [
        EnemyFrameId.KraidNailLoopFirst,
        EnemyFrameId.KraidNailLoopSecond,
        EnemyFrameId.KraidNailLoopThird,
        EnemyFrameId.KraidNailLoopFourth,
        EnemyFrameId.KraidNailLoopFifth,
        EnemyFrameId.KraidNailLoopSixth,
        EnemyFrameId.KraidNailLoopSeventh,
        EnemyFrameId.KraidNailLoopEighth,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/BabyMetroidCutsceneBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] BabyMetroidCutscene =
    [
        EnemyFrameId.BabyMetroidInitialThird,
        EnemyFrameId.BabyMetroidInitialSecond,
        EnemyFrameId.BabyMetroidInitialFirst,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/MotherBrainBehavior.cs</c> shows for <c>MotherBrainBody</c>.</summary>
    private static readonly EnemyFrameId[] MotherBrainBodyFrames =
    [
        EnemyFrameId.MotherBrainBodyStanding,
        EnemyFrameId.MotherBrainBodyWalkingFirst,
        EnemyFrameId.MotherBrainBodyWalkingSecond,
        EnemyFrameId.MotherBrainBodyWalkingThird,
        EnemyFrameId.MotherBrainBodyWalkingFourth,
        EnemyFrameId.MotherBrainBodyWalkingFifth,
        EnemyFrameId.MotherBrainBodyWalkingSixth,
        EnemyFrameId.MotherBrainBodyWalkingSeventh,
        EnemyFrameId.MotherBrainBodyWalkingEighth,
        EnemyFrameId.MotherBrainBodyCrouched,
        EnemyFrameId.MotherBrainBodyUncrouching,
        EnemyFrameId.MotherBrainBodyLeaningDown,
        EnemyFrameId.MotherBrainBodyInitialDummy,
        EnemyFrameId.MotherBrainBodyHandBeamSecond,
        EnemyFrameId.MotherBrainBodyHandBeamThird,
        EnemyFrameId.MotherBrainBodyHandBeamFourth,
        EnemyFrameId.MotherBrainBodyHandBeamFifth,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/MotherBrainBehavior.cs</c> shows for <c>MotherBrainHead</c>.</summary>
    private static readonly EnemyFrameId[] MotherBrainHeadFrames =
    [
        EnemyFrameId.MotherBrainHeadInitial,
        EnemyFrameId.MotherBrainHeadNeutralPhaseTwoSecond,
        EnemyFrameId.MotherBrainHeadStretchingPhaseTwoFirst,
        EnemyFrameId.MotherBrainHeadStretchingPhaseTwoSecond,
        EnemyFrameId.MotherBrainHeadStretchingPhaseTwoThird,
        EnemyFrameId.MotherBrainNeckJoint,
        EnemyFrameId.MotherBrainHeadDecapitatedLoop,
        EnemyFrameId.MotherBrainHeadDecapitatedSecond,
        EnemyFrameId.MotherBrainHeadStretchingPhaseThreeFirst,
        EnemyFrameId.MotherBrainHeadStretchingPhaseThreeSecond,
        EnemyFrameId.MotherBrainHeadStretchingPhaseThreeThird,
        EnemyFrameId.MotherBrainHeadCorpseFourth,
        EnemyFrameId.MotherBrainHeadCorpseLoop,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/MotherBrainBehavior.cs</c> shows for <c>MotherBrainTubes</c>.</summary>
    private static readonly EnemyFrameId[] MotherBrainTubesFrames =
    [
        EnemyFrameId.MotherBrainTubeFirst,
        EnemyFrameId.MotherBrainTubeSecond,
        EnemyFrameId.MotherBrainTubeThird,
        EnemyFrameId.MotherBrainTubeFourth,
        EnemyFrameId.MotherBrainTubeFifth,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/RinkaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Rinka =
    [
        EnemyFrameId.RinkaOrdinary,
        EnemyFrameId.RinkaOrdinaryLoopSecond,
        EnemyFrameId.RinkaOrdinaryLoopThird,
        EnemyFrameId.RinkaOrdinaryLoopFourth,
        EnemyFrameId.RinkaOrdinaryLoopFifth,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/ShitroidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Shitroid =
    [
        EnemyFrameId.BabyMetroidInitialThird,
        EnemyFrameId.BabyMetroidInitialSecond,
        EnemyFrameId.BabyMetroidInitialFirst,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/ZebetiteBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Zebetite =
    [
        EnemyFrameId.ZebetiteFirst,
        EnemyFrameId.ZebetiteSecond,
        EnemyFrameId.ZebetiteThird,
        EnemyFrameId.ZebetiteFourth,
        EnemyFrameId.ZebetiteFifth,
        EnemyFrameId.ZebetiteSixth,
        EnemyFrameId.ZebetiteSeventh,
        EnemyFrameId.ZebetiteEighth,
        EnemyFrameId.ZebetiteNinth,
        EnemyFrameId.ZebetiteTenth,
    ];

    /// <summary>The frames <c>Bosses/Phantoon/PhantoonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Phantoon =
    [
        EnemyFrameId.PhantoonInvulnerableBody,
        EnemyFrameId.PhantoonFullHitboxBody,
        EnemyFrameId.PhantoonEyeHitboxBody,
        EnemyFrameId.PhantoonEyeClosed,
        EnemyFrameId.PhantoonEyeOpenFirst,
        EnemyFrameId.PhantoonEyeOpenSecond,
        EnemyFrameId.PhantoonEyeOpenThird,
        EnemyFrameId.PhantoonEyeballCentered,
        EnemyFrameId.PhantoonEyeLookingUp,
        EnemyFrameId.PhantoonEyeLookingDown,
        EnemyFrameId.PhantoonEyeLookingLeft,
        EnemyFrameId.PhantoonEyeLookingRight,
        EnemyFrameId.PhantoonEyeLookingDownLeft,
        EnemyFrameId.PhantoonEyeLookingDownRight,
        EnemyFrameId.PhantoonEyeLookingUpLeft,
        EnemyFrameId.PhantoonEyeLookingUpRight,
        EnemyFrameId.PhantoonTentaclesFirst,
        EnemyFrameId.PhantoonTentaclesSecond,
        EnemyFrameId.PhantoonTentaclesThird,
        EnemyFrameId.PhantoonMouthInitial,
        EnemyFrameId.PhantoonMouthSpawnFlameSecond,
        EnemyFrameId.PhantoonMouthSpawnFlameFirst,
    ];

    /// <summary>The frames <c>Bosses/Ridley/RidleyEntityFactory.cs</c> shows for <c>Ridley</c>.</summary>
    private static readonly EnemyFrameId[] RidleyFrames =
    [
        EnemyFrameId.RidleyTailSegmentSmall,
        EnemyFrameId.RidleyTailSegmentMedium,
        EnemyFrameId.RidleyTailSegmentLarge,
        EnemyFrameId.RidleyBreakupTailTipThirteenth,
        EnemyFrameId.RidleyBreakupTailTipTwelfth,
        EnemyFrameId.RidleyBreakupTailTipEleventh,
        EnemyFrameId.RidleyBreakupTailTipTenth,
        EnemyFrameId.RidleyBreakupTailTipNinth,
        EnemyFrameId.RidleyBreakupTailTipEighth,
        EnemyFrameId.RidleyBreakupTailTipSeventh,
        EnemyFrameId.RidleyBreakupTailTipSixth,
        EnemyFrameId.RidleyBreakupTailTipFifth,
        EnemyFrameId.RidleyBreakupTailTipFourth,
        EnemyFrameId.RidleyBreakupTailTipThird,
        EnemyFrameId.RidleyBreakupTailTipSecond,
        EnemyFrameId.RidleyBreakupTailTipFirst,
        EnemyFrameId.RidleyBreakupTailTipSixteenth,
        EnemyFrameId.RidleyBreakupTailTipFifteenth,
        EnemyFrameId.RidleyBreakupTailTipFourteenth,
        EnemyFrameId.RidleyWingFacingLeftFirst,
        EnemyFrameId.RidleyWingFacingLeftSecond,
        EnemyFrameId.RidleyWingFacingLeftThird,
        EnemyFrameId.RidleyWingFacingLeftFourth,
        EnemyFrameId.RidleyWingFacingLeftFifth,
        EnemyFrameId.RidleyWingFacingLeftSixth,
        EnemyFrameId.RidleyWingFacingRightFirst,
        EnemyFrameId.RidleyWingFacingRightSecond,
        EnemyFrameId.RidleyWingFacingRightThird,
        EnemyFrameId.RidleyWingFacingRightFourth,
        EnemyFrameId.RidleyWingFacingRightFifth,
        EnemyFrameId.RidleyWingFacingRightSixth,
        EnemyFrameId.RidleyBodyLeft,
        EnemyFrameId.RidleyBodyRight,
        EnemyFrameId.RidleyBodyLeftMouthHalfOpen,
        EnemyFrameId.RidleyBodyLeftMouthOpen,
        EnemyFrameId.RidleyBodyRightMouthHalfOpen,
        EnemyFrameId.RidleyBodyRightMouthOpen,
        EnemyFrameId.RidleyBodyLeftLegsHalfExtended,
        EnemyFrameId.RidleyBodyLeftLegsExtended,
        EnemyFrameId.RidleyBodyRightLegsHalfExtended,
        EnemyFrameId.RidleyBodyRightLegsExtended,
        EnemyFrameId.RidleyBodyForward,
    ];

    /// <summary>The frames <c>Bosses/Ridley/RidleyEntityFactory.cs</c> shows for <c>RidleyCeres</c>.</summary>
    private static readonly EnemyFrameId[] RidleyCeresFrames =
    [
        EnemyFrameId.RidleyTailSegmentSmall,
        EnemyFrameId.RidleyTailSegmentMedium,
        EnemyFrameId.RidleyTailSegmentLarge,
        EnemyFrameId.RidleyBreakupTailTipThirteenth,
        EnemyFrameId.RidleyBreakupTailTipTwelfth,
        EnemyFrameId.RidleyBreakupTailTipEleventh,
        EnemyFrameId.RidleyBreakupTailTipTenth,
        EnemyFrameId.RidleyBreakupTailTipNinth,
        EnemyFrameId.RidleyBreakupTailTipEighth,
        EnemyFrameId.RidleyBreakupTailTipSeventh,
        EnemyFrameId.RidleyBreakupTailTipSixth,
        EnemyFrameId.RidleyBreakupTailTipFifth,
        EnemyFrameId.RidleyBreakupTailTipFourth,
        EnemyFrameId.RidleyBreakupTailTipThird,
        EnemyFrameId.RidleyBreakupTailTipSecond,
        EnemyFrameId.RidleyBreakupTailTipFirst,
        EnemyFrameId.RidleyBreakupTailTipSixteenth,
        EnemyFrameId.RidleyBreakupTailTipFifteenth,
        EnemyFrameId.RidleyBreakupTailTipFourteenth,
        EnemyFrameId.RidleyWingFacingLeftFirst,
        EnemyFrameId.RidleyWingFacingLeftSecond,
        EnemyFrameId.RidleyWingFacingLeftThird,
        EnemyFrameId.RidleyWingFacingLeftFourth,
        EnemyFrameId.RidleyWingFacingLeftFifth,
        EnemyFrameId.RidleyWingFacingLeftSixth,
        EnemyFrameId.RidleyWingFacingRightFirst,
        EnemyFrameId.RidleyWingFacingRightSecond,
        EnemyFrameId.RidleyWingFacingRightThird,
        EnemyFrameId.RidleyWingFacingRightFourth,
        EnemyFrameId.RidleyWingFacingRightFifth,
        EnemyFrameId.RidleyWingFacingRightSixth,
        EnemyFrameId.RidleyBodyLeft,
        EnemyFrameId.RidleyBodyRight,
        EnemyFrameId.RidleyBodyLeftMouthHalfOpen,
        EnemyFrameId.RidleyBodyLeftMouthOpen,
        EnemyFrameId.RidleyBodyRightMouthHalfOpen,
        EnemyFrameId.RidleyBodyRightMouthOpen,
        EnemyFrameId.RidleyBodyLeftLegsHalfExtended,
        EnemyFrameId.RidleyBodyLeftLegsExtended,
        EnemyFrameId.RidleyBodyRightLegsHalfExtended,
        EnemyFrameId.RidleyBodyRightLegsExtended,
    ];

    /// <summary>The frames <c>Bosses/Ridley/RidleyEntityFactory.cs</c> shows for <c>RidleyExplosion</c>.</summary>
    private static readonly EnemyFrameId[] RidleyExplosionFrames =
    [
        EnemyFrameId.RidleyTailSegmentSmall,
        EnemyFrameId.RidleyTailSegmentMedium,
        EnemyFrameId.RidleyTailSegmentLarge,
        EnemyFrameId.RidleyBreakupTailTipThirteenth,
        EnemyFrameId.RidleyBreakupTailTipTwelfth,
        EnemyFrameId.RidleyBreakupTailTipEleventh,
        EnemyFrameId.RidleyBreakupTailTipTenth,
        EnemyFrameId.RidleyBreakupTailTipNinth,
        EnemyFrameId.RidleyBreakupTailTipEighth,
        EnemyFrameId.RidleyBreakupTailTipSeventh,
        EnemyFrameId.RidleyBreakupTailTipSixth,
        EnemyFrameId.RidleyBreakupTailTipFifth,
        EnemyFrameId.RidleyBreakupTailTipFourth,
        EnemyFrameId.RidleyBreakupTailTipThird,
        EnemyFrameId.RidleyBreakupTailTipSecond,
        EnemyFrameId.RidleyBreakupTailTipFirst,
        EnemyFrameId.RidleyBreakupTailTipSixteenth,
        EnemyFrameId.RidleyBreakupTailTipFifteenth,
        EnemyFrameId.RidleyBreakupTailTipFourteenth,
        EnemyFrameId.RidleyWingFacingLeftFirst,
        EnemyFrameId.RidleyWingFacingLeftSecond,
        EnemyFrameId.RidleyWingFacingLeftThird,
        EnemyFrameId.RidleyWingFacingLeftFourth,
        EnemyFrameId.RidleyWingFacingLeftFifth,
        EnemyFrameId.RidleyWingFacingLeftSixth,
        EnemyFrameId.RidleyWingFacingRightFirst,
        EnemyFrameId.RidleyWingFacingRightSecond,
        EnemyFrameId.RidleyWingFacingRightThird,
        EnemyFrameId.RidleyWingFacingRightFourth,
        EnemyFrameId.RidleyWingFacingRightFifth,
        EnemyFrameId.RidleyWingFacingRightSixth,
        EnemyFrameId.RidleyBreakupLegsLeft,
        EnemyFrameId.RidleyBreakupLegsRight,
        EnemyFrameId.RidleyBreakupTorsoLeft,
        EnemyFrameId.RidleyBreakupTorsoRight,
        EnemyFrameId.RidleyBreakupHeadLeft,
        EnemyFrameId.RidleyBreakupHeadRight,
        EnemyFrameId.RidleyBreakupClawLeft,
        EnemyFrameId.RidleyBreakupClawRight,
    ];

    /// <summary>The frames <c>Bosses/SporeSpawn/SporeSpawnBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] SporeSpawn =
    [
        EnemyFrameId.SporeSpawnInitialDead,
        EnemyFrameId.SporeSpawnInitialAlive,
        EnemyFrameId.SporeSpawnOpeningFirst,
        EnemyFrameId.SporeSpawnOpeningSecond,
        EnemyFrameId.SporeSpawnOpeningThird,
        EnemyFrameId.SporeSpawnOpeningFourth,
        EnemyFrameId.SporeSpawnOpeningFifth,
        EnemyFrameId.SporeSpawnDeath,
        EnemyFrameId.SporeSpawnOpeningSeventh,
        EnemyFrameId.SporeSpawnOpenPulseFirst,
        EnemyFrameId.SporeSpawnOpenPulseSecond,
        EnemyFrameId.SporeSpawnOpenPulseThird,
    ];

    /// <summary>The frames <c>Bosses/Torizo/BombTorizoBehavior.cs</c> shows for <c>BombTorizo</c>.</summary>
    private static readonly EnemyFrameId[] BombTorizoFrames =
    [
        EnemyFrameId.BombTorizoDormant,
        EnemyFrameId.GoldenTorizoInitial,
        EnemyFrameId.BombTorizoInitialFourth,
        EnemyFrameId.BombTorizoInitialThird,
        EnemyFrameId.BombTorizoInitialResumed,
        EnemyFrameId.BombTorizoInitialSixth,
        EnemyFrameId.BombTorizoInitialSeventh,
        EnemyFrameId.BombTorizoInitialEighth,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth,
        EnemyFrameId.TorizoFacingLeftTurningLeft,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth,
        EnemyFrameId.TorizoFacingRightJumpingForwardsFirst,
        EnemyFrameId.TorizoFacingRightJumpingForwardsSecond,
        EnemyFrameId.TorizoFacingRightJumpingForwardsThird,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdSecond,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdThird,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFourth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFifth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdSixth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSecond,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneThird,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFourth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFifth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSixth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEighth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneNinth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneTenth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEleventh,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSecond,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneThird,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFourth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFifth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSixth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEighth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneNinth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneTenth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEleventh,
        EnemyFrameId.TorizoFallingLeft,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdSecond,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdThird,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFourth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFifth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdSixth,
        EnemyFrameId.TorizoFacingLeftJumpingForwardsFirst,
        EnemyFrameId.TorizoFacingLeftJumpingForwardsThird,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdSecond,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdThird,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFourth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFifth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdSixth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSecond,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdThird,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFourth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFifth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSixth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSecond,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneThird,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFourth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFifth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSixth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEighth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneNinth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneTenth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEleventh,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSecond,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneThird,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFourth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFifth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSixth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEighth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneNinth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneTenth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEleventh,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFirst,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSecond,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedThird,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFourth,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFifth,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSixth,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSeventh,
    ];

    /// <summary>The frames <c>Bosses/Torizo/BombTorizoBehavior.cs</c> shows for <c>GoldenTorizo</c>.</summary>
    private static readonly EnemyFrameId[] GoldenTorizoFrames =
    [
        EnemyFrameId.GoldenTorizoInitial,
        EnemyFrameId.BombTorizoInitialFourth,
        EnemyFrameId.BombTorizoInitialThird,
        EnemyFrameId.BombTorizoInitialResumed,
        EnemyFrameId.BombTorizoInitialSixth,
        EnemyFrameId.BombTorizoInitialSeventh,
        EnemyFrameId.BombTorizoInitialEighth,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth,
        EnemyFrameId.TorizoFacingLeftTurningLeft,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth,
        EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth,
        EnemyFrameId.TorizoFacingRightJumpingForwardsFirst,
        EnemyFrameId.TorizoFacingRightJumpingForwardsSecond,
        EnemyFrameId.TorizoFacingRightJumpingForwardsThird,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFirst,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdSecond,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdThird,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFourth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdFifth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsRightFootFwdSixth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFirst,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSecond,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneThird,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFourth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneFifth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSixth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEighth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneNinth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneTenth,
        EnemyFrameId.TorizoFacingRightSonicBoomsLeftFootForwardOneEleventh,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSecond,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneThird,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFourth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneFifth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSixth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEighth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneNinth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneTenth,
        EnemyFrameId.TorizoFacingRightSonicBoomsRightFootForwardOneEleventh,
        EnemyFrameId.TorizoFallingLeft,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdSecond,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdThird,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFourth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdFifth,
        EnemyFrameId.TorizoFacingRightSpewingChozoOrbsLeftFootFwdSixth,
        EnemyFrameId.TorizoFacingLeftJumpingForwardsFirst,
        EnemyFrameId.TorizoFacingLeftJumpingForwardsThird,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFirst,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdSecond,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdThird,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFourth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdFifth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsRightFootFwdSixth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFirst,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSecond,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdThird,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFourth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdFifth,
        EnemyFrameId.TorizoFacingLeftSpewingChozoOrbsLeftFootFwdSixth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSecond,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneThird,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFourth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneFifth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSixth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEighth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneNinth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneTenth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsRightFootForwardOneEleventh,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSecond,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneThird,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFourth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneFifth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSixth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneSeventh,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEighth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneNinth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneTenth,
        EnemyFrameId.TorizoFacingLeftSonicBoomsLeftFootForwardOneEleventh,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFirst,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSecond,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedThird,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFourth,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedFifth,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSixth,
        EnemyFrameId.TorizoFacingLeftFacelessWalkingLeftLegMovingTrailingResumedSeventh,
    ];

    /// <summary>The frames <c>Enemies/Alcoon/AlcoonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Alcoon =
    [
        EnemyFrameId.AlcoonWalkingLeftLoopFirst,
        EnemyFrameId.AlcoonWalkingLeftLoopSecond,
        EnemyFrameId.AlcoonWalkingLeftLoopThird,
        EnemyFrameId.AlcoonWalkingLeftLoopFourth,
        EnemyFrameId.AlcoonFireLeftFirst,
        EnemyFrameId.AlcoonFireLeftSecond,
        EnemyFrameId.AlcoonFireLeftThird,
        EnemyFrameId.AlcoonFireLeftFourth,
        EnemyFrameId.AlcoonLeftAirUp,
        EnemyFrameId.AlcoonWalkingRightLoopFirst,
        EnemyFrameId.AlcoonWalkingRightLoopSecond,
        EnemyFrameId.AlcoonWalkingRightLoopThird,
        EnemyFrameId.AlcoonWalkingRightLoopFourth,
        EnemyFrameId.AlcoonFireRightFirst,
        EnemyFrameId.AlcoonFireRightSecond,
        EnemyFrameId.AlcoonFireRightThird,
        EnemyFrameId.AlcoonFireRightFourth,
        EnemyFrameId.AlcoonRightAirUp,
    ];

    /// <summary>The frames <c>Enemies/Animals/DachoraBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Dachora =
    [
        EnemyFrameId.DachoraRunningLeftFirst,
        EnemyFrameId.DachoraRunningLeftSecond,
        EnemyFrameId.DachoraRunningLeftThird,
        EnemyFrameId.DachoraRunningLeftFourth,
        EnemyFrameId.DachoraRunningLeftFifth,
        EnemyFrameId.DachoraRunningLeftSixth,
        EnemyFrameId.DachoraIdleLeftFirst,
        EnemyFrameId.DachoraIdleLeftSecond,
        EnemyFrameId.DachoraIdleLeftThird,
        EnemyFrameId.DachoraIdleLeftFourth,
        EnemyFrameId.DachoraIdleLeftFifth,
        EnemyFrameId.DachoraBlinkLeftSecond,
        EnemyFrameId.DachoraBlinkLeftThird,
        EnemyFrameId.DachoraEchoLeft,
        EnemyFrameId.DachoraRunningRightFirst,
        EnemyFrameId.DachoraRunningRightSecond,
        EnemyFrameId.DachoraRunningRightThird,
        EnemyFrameId.DachoraRunningRightFourth,
        EnemyFrameId.DachoraRunningRightFifth,
        EnemyFrameId.DachoraRunningRightSixth,
        EnemyFrameId.DachoraIdleRightFirst,
        EnemyFrameId.DachoraIdleRightSecond,
        EnemyFrameId.DachoraIdleRightThird,
        EnemyFrameId.DachoraIdleRightFourth,
        EnemyFrameId.DachoraIdleRightFifth,
        EnemyFrameId.DachoraBlinkRightSecond,
        EnemyFrameId.DachoraBlinkRightThird,
        EnemyFrameId.DachoraChargeRight,
        EnemyFrameId.DachoraEchoRight,
    ];

    /// <summary>The frames <c>Enemies/Animals/EscapeAnimalsBehavior.cs</c> shows for <c>EtecoonEscape</c>.</summary>
    private static readonly EnemyFrameId[] EtecoonEscapeFrames =
    [
        EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopFirst,
        EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopSecond,
        EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopThird,
        EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopFirst,
        EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopSecond,
        EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopThird,
        EnemyFrameId.EscapeEtecoonStationaryFirst,
        EnemyFrameId.EscapeEtecoonStationarySecond,
        EnemyFrameId.EscapeEtecoonStationaryThird,
        EnemyFrameId.EscapeEtecoonStationaryFourth,
    ];

    /// <summary>The frames <c>Enemies/Animals/EscapeAnimalsBehavior.cs</c> shows for <c>DachoraEscape</c>.</summary>
    private static readonly EnemyFrameId[] DachoraEscapeFrames =
    [
        EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFirst,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftSecond,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftThird,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFourth,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftFifth,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideLeftSixth,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFirst,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSecond,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideRightThird,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFourth,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideRightFifth,
        EnemyFrameId.EscapeDachoraRunningAroundLowTideRightSixth,
    ];

    /// <summary>The frames <c>Enemies/Animals/EtecoonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Etecoon =
    [
        EnemyFrameId.EtecoonLookRightAtSamusAndRunLeft,
        EnemyFrameId.EtecoonBeginRunningLeft,
        EnemyFrameId.EtecoonRunningLeftFirst,
        EnemyFrameId.EtecoonRunningLeftSecond,
        EnemyFrameId.EtecoonRunningLeftThird,
        EnemyFrameId.EtecoonWallJumpLeft,
        EnemyFrameId.EtecoonWallJumpLeftLoopFirst,
        EnemyFrameId.EtecoonWallJumpLeftLoopSecond,
        EnemyFrameId.EtecoonWallJumpLeftLoopThird,
        EnemyFrameId.EtecoonWallJumpLeftLoopFourth,
        EnemyFrameId.EtecoonHoppingFacingLeft,
        EnemyFrameId.EtecoonContinueHoppingFacingLeftFirst,
        EnemyFrameId.EtecoonContinueHoppingFacingLeftSecond,
        EnemyFrameId.EtecoonHitCeilingFirst,
        EnemyFrameId.EtecoonWallJumpLeftEligible,
        EnemyFrameId.EtecoonLookLeftAtSamusAndRunRight,
        EnemyFrameId.EtecoonBeginRunningRight,
        EnemyFrameId.EtecoonRunningRightFirst,
        EnemyFrameId.EtecoonRunningRightSecond,
        EnemyFrameId.EtecoonRunningRightThird,
        EnemyFrameId.EtecoonWallJumpRight,
        EnemyFrameId.EtecoonJumpingRightFirst,
        EnemyFrameId.EtecoonJumpingRightSecond,
        EnemyFrameId.EtecoonJumpingRightThird,
        EnemyFrameId.EtecoonJumpingRightFourth,
        EnemyFrameId.EtecoonHoppingFacingRight,
        EnemyFrameId.EtecoonContinueHoppingFacingRightFirst,
        EnemyFrameId.EtecoonContinueHoppingFacingRightSecond,
        EnemyFrameId.EtecoonContinueHoppingFacingRightThird,
        EnemyFrameId.EtecoonWallJumpRightEligible,
        EnemyFrameId.EtecoonFlexingLoopFifth,
    ];

    /// <summary>The frames <c>Enemies/Atomic/AtomicBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Atomic =
    [
        EnemyFrameId.AtomicUpRightFirst,
        EnemyFrameId.AtomicUpRightSecond,
        EnemyFrameId.AtomicUpRightThird,
        EnemyFrameId.AtomicUpRightFourth,
        EnemyFrameId.AtomicUpRightFifth,
        EnemyFrameId.AtomicUpRightSixth,
        EnemyFrameId.AtomicUpLeftFirst,
        EnemyFrameId.AtomicUpLeftSecond,
        EnemyFrameId.AtomicUpLeftThird,
        EnemyFrameId.AtomicUpLeftFourth,
        EnemyFrameId.AtomicUpLeftFifth,
        EnemyFrameId.AtomicUpLeftSixth,
    ];

    /// <summary>The frames <c>Enemies/Beetom/BeetomBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Beetom =
    [
        EnemyFrameId.BeetomCrawlingLeftLoopFirst,
        EnemyFrameId.BeetomCrawlingLeftLoopSecond,
        EnemyFrameId.BeetomCrawlingLeftLoopThird,
        EnemyFrameId.BeetomHopLeftFirst,
        EnemyFrameId.BeetomHopLeftSecond,
        EnemyFrameId.BeetomDrainingLeftFirst,
        EnemyFrameId.BeetomDrainingLeftSecond,
        EnemyFrameId.BeetomDrainingLeftThird,
        EnemyFrameId.BeetomDrainingLeftLoopFirst,
        EnemyFrameId.BeetomDrainingLeftLoopSecond,
        EnemyFrameId.BeetomDrainingLeftLoopThird,
        EnemyFrameId.BeetomCrawlingRightLoopFirst,
        EnemyFrameId.BeetomCrawlingRightLoopSecond,
        EnemyFrameId.BeetomCrawlingRightLoopThird,
        EnemyFrameId.BeetomHopRightFirst,
        EnemyFrameId.BeetomHopRightSecond,
        EnemyFrameId.BeetomDrainingRightFirst,
        EnemyFrameId.BeetomDrainingRightSecond,
        EnemyFrameId.BeetomDrainingRightThird,
        EnemyFrameId.BeetomDrainingRightLoopFirst,
        EnemyFrameId.BeetomDrainingRightLoopSecond,
        EnemyFrameId.BeetomDrainingRightLoopThird,
    ];

    /// <summary>The frames <c>Enemies/Boulder/BoulderBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Boulder =
    [
        EnemyFrameId.BoulderLeftFirst,
        EnemyFrameId.BoulderLeftSecond,
        EnemyFrameId.BoulderLeftThird,
        EnemyFrameId.BoulderLeftFourth,
        EnemyFrameId.BoulderLeftFifth,
        EnemyFrameId.BoulderLeftSixth,
        EnemyFrameId.BoulderLeftSeventh,
        EnemyFrameId.BoulderLeftEighth,
    ];

    /// <summary>The frames <c>Enemies/Boyon/BoyonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Boyon =
    [
        EnemyFrameId.BoyonIdleLoopFirst,
        EnemyFrameId.BoyonIdleLoopSecond,
        EnemyFrameId.BoyonIdleLoopThird,
        EnemyFrameId.BoyonBouncingLoopFirst,
        EnemyFrameId.BoyonBouncingLoopSecond,
        EnemyFrameId.BoyonBouncingLoopThird,
        EnemyFrameId.BoyonBouncingLoopFourth,
    ];

    /// <summary>The frames <c>Enemies/Bull/BullBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Bull =
    [
        EnemyFrameId.BullNormalFirst,
        EnemyFrameId.BullNormalSecond,
        EnemyFrameId.BullNormalThird,
    ];

    /// <summary>The frames <c>Enemies/Cacatac/CacatacBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Cacatac =
    [
        EnemyFrameId.CacatacUpsideUpIdleFirst,
        EnemyFrameId.CacatacUpsideUpIdleSecond,
        EnemyFrameId.CacatacUpsideUpIdleThird,
        EnemyFrameId.CacatacUpsideUpIdleFourth,
        EnemyFrameId.CacatacUpsideUpIdleFifth,
        EnemyFrameId.CacatacUpsideUpIdleSixth,
        EnemyFrameId.CacatacUpsideUpIdleSeventh,
        EnemyFrameId.CacatacUpsideUpIdleEighth,
        EnemyFrameId.CacatacUpsideUpAttackSecond,
        EnemyFrameId.CacatacUpsideUpAttackThird,
        EnemyFrameId.CacatacUpsideDownIdleLoopFirst,
        EnemyFrameId.CacatacUpsideDownIdleLoopSecond,
        EnemyFrameId.CacatacUpsideDownIdleLoopThird,
        EnemyFrameId.CacatacUpsideDownIdleLoopFourth,
        EnemyFrameId.CacatacUpsideDownIdleLoopFifth,
        EnemyFrameId.CacatacUpsideDownIdleLoopSixth,
        EnemyFrameId.CacatacUpsideDownIdleLoopSeventh,
        EnemyFrameId.CacatacUpsideDownIdleLoopEighth,
        EnemyFrameId.CacatacUpsideDownAttackSecond,
        EnemyFrameId.CacatacUpsideDownAttackThird,
    ];

    /// <summary>The frames <c>Enemies/Choot/ChootBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Choot =
    [
        EnemyFrameId.ChootIdle,
        EnemyFrameId.ChootJump,
        EnemyFrameId.ChootJumpApex,
        EnemyFrameId.ChootFallEnd,
    ];

    /// <summary>The frames <c>Enemies/Chozo/ChozoStatueBehavior.cs</c> shows for <c>Chozo</c>.</summary>
    private static readonly EnemyFrameId[] ChozoFrames =
    [
        EnemyFrameId.ChozoStatueLowerNorfairInitial,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedSecond,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedThird,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedFourth,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedFifth,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedSixth,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedSeventh,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedEighth,
        EnemyFrameId.ChozoStatueLowerNorfairActivatedNinth,
        EnemyFrameId.ChozoStatueWreckedShipInitial,
        EnemyFrameId.ChozoStatueWreckedShipActivatedSecond,
        EnemyFrameId.ChozoStatueWreckedShipActivatedThird,
        EnemyFrameId.ChozoStatueWreckedShipActivatedFourth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedFifth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedSixth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedSeventh,
        EnemyFrameId.ChozoStatueWreckedShipActivatedEighth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedNinth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoFirst,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoSecond,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoThird,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoFourth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoFifth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoSixth,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoSeventh,
        EnemyFrameId.ChozoStatueWreckedShipActivatedLoopTwoEighth,
    ];

    /// <summary>The frames <c>Enemies/Chozo/ChozoStatueBehavior.cs</c> shows for <c>NoobTubeCrack</c>.</summary>
    private static readonly EnemyFrameId[] NoobTubeCrackFrames = [];

    /// <summary>The frames <c>Enemies/Corpses/DeadSidehopperBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] DeadSidehopper =
    [
        EnemyFrameId.DeadSidehopperHopBody,
        EnemyFrameId.DeadSidehopperHopAlt,
        EnemyFrameId.DeadSidehopperIdle,
        EnemyFrameId.DeadSidehopperCorpse,
        EnemyFrameId.DeadSidehopperInitiallyDead,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTorizoBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] DeadTorizo =
    [
        EnemyFrameId.DeadTorizoCorpseHook,
        EnemyFrameId.DeadTorizoStationary,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTourianBehavior.cs</c> shows for <c>CorpseRipper</c>.</summary>
    private static readonly EnemyFrameId[] CorpseRipperFrames =
    [
        EnemyFrameId.DeadRipperCorpseFirst,
        EnemyFrameId.DeadRipperCorpseSecond,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTourianBehavior.cs</c> shows for <c>CorpseSkree</c>.</summary>
    private static readonly EnemyFrameId[] CorpseSkreeFrames =
    [
        EnemyFrameId.DeadSkreeCorpseFirst,
        EnemyFrameId.DeadSkreeCorpseSecond,
        EnemyFrameId.DeadSkreeCorpseThird,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTourianBehavior.cs</c> shows for <c>CorpseZoomer</c>.</summary>
    private static readonly EnemyFrameId[] CorpseZoomerFrames =
    [
        EnemyFrameId.DeadZoomerCorpseFirst,
        EnemyFrameId.DeadZoomerCorpseSecond,
        EnemyFrameId.DeadZoomerCorpseThird,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Sciser</c>.</summary>
    private static readonly EnemyFrameId[] SciserFrames =
    [
        EnemyFrameId.SciserUpsideRightLoopFirst,
        EnemyFrameId.SciserUpsideRightLoopSecond,
        EnemyFrameId.SciserUpsideRightLoopThird,
        EnemyFrameId.SciserUpsideLeftLoopFirst,
        EnemyFrameId.SciserUpsideLeftLoopSecond,
        EnemyFrameId.SciserUpsideLeftLoopThird,
        EnemyFrameId.SciserUpsideDownLoopFirst,
        EnemyFrameId.SciserUpsideDownLoopSecond,
        EnemyFrameId.SciserUpsideDownLoopThird,
        EnemyFrameId.SciserUpsideUpLoopFirst,
        EnemyFrameId.SciserUpsideUpLoopSecond,
        EnemyFrameId.SciserUpsideUpLoopThird,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Zero</c>.</summary>
    private static readonly EnemyFrameId[] ZeroFrames =
    [
        EnemyFrameId.ZeroUpsideRightLoopFirst,
        EnemyFrameId.ZeroUpsideRightLoopSecond,
        EnemyFrameId.ZeroUpsideRightLoopThird,
        EnemyFrameId.ZeroUpsideRightLoopFourth,
        EnemyFrameId.ZeroUpsideLeftLoopFirst,
        EnemyFrameId.ZeroUpsideLeftLoopSecond,
        EnemyFrameId.ZeroUpsideLeftLoopThird,
        EnemyFrameId.ZeroUpsideLeftLoopFourth,
        EnemyFrameId.ZeroUpsideDownLoopFirst,
        EnemyFrameId.ZeroUpsideDownLoopSecond,
        EnemyFrameId.ZeroUpsideDownLoopThird,
        EnemyFrameId.ZeroUpsideDownLoopFourth,
        EnemyFrameId.ZeroUpsideUpLoopFirst,
        EnemyFrameId.ZeroUpsideUpLoopSecond,
        EnemyFrameId.ZeroUpsideUpLoopThird,
        EnemyFrameId.ZeroUpsideUpLoopFourth,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Viola</c>.</summary>
    private static readonly EnemyFrameId[] ViolaFrames =
    [
        EnemyFrameId.ViolaNormalFirst,
        EnemyFrameId.ViolaNormalSecond,
        EnemyFrameId.ViolaNormalThird,
        EnemyFrameId.ViolaNormalFourth,
        EnemyFrameId.ViolaNormalFifth,
        EnemyFrameId.ViolaNormalSixth,
        EnemyFrameId.ViolaNormalSeventh,
        EnemyFrameId.ViolaNormalEighth,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Zeela</c>, <c>Sova</c>, <c>Zoomer</c>, <c>MZoomer</c>, <c>HZoomer</c>.</summary>
    private static readonly EnemyFrameId[] ZeelaFrames =
    [
        EnemyFrameId.HZoomerUpsideRightLoopFirst,
        EnemyFrameId.HZoomerUpsideRightLoopSecond,
        EnemyFrameId.HZoomerUpsideRightLoopThird,
        EnemyFrameId.HZoomerUpsideRightLoopFourth,
        EnemyFrameId.HZoomerUpsideRightLoopFifth,
        EnemyFrameId.HZoomerUpsideLeftLoopFirst,
        EnemyFrameId.HZoomerUpsideLeftLoopSecond,
        EnemyFrameId.HZoomerUpsideLeftLoopThird,
        EnemyFrameId.HZoomerUpsideLeftLoopFourth,
        EnemyFrameId.HZoomerUpsideLeftLoopFifth,
        EnemyFrameId.HZoomerUpsideDownLoopFirst,
        EnemyFrameId.HZoomerUpsideDownLoopSecond,
        EnemyFrameId.HZoomerUpsideDownLoopThird,
        EnemyFrameId.HZoomerUpsideDownLoopFourth,
        EnemyFrameId.HZoomerUpsideDownLoopFifth,
        EnemyFrameId.HZoomerUpsideUpLoopFirst,
        EnemyFrameId.HZoomerUpsideUpLoopSecond,
        EnemyFrameId.HZoomerUpsideUpLoopThird,
        EnemyFrameId.HZoomerUpsideUpLoopFourth,
        EnemyFrameId.HZoomerUpsideUpLoopFifth,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/MultiviolaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Multiviola =
    [
        EnemyFrameId.MultiviolaFlyingFirst,
        EnemyFrameId.MultiviolaFlyingSecond,
        EnemyFrameId.MultiviolaFlyingThird,
        EnemyFrameId.MultiviolaFlyingFourth,
        EnemyFrameId.MultiviolaFlyingFifth,
        EnemyFrameId.MultiviolaFlyingSixth,
        EnemyFrameId.MultiviolaFlyingSeventh,
        EnemyFrameId.MultiviolaFlyingEighth,
    ];

    /// <summary>The frames <c>Enemies/Dragon/DragonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Dragon =
    [
        EnemyFrameId.DragonBodyIdleLeft,
        EnemyFrameId.DragonWingsFacingLeftFirst,
        EnemyFrameId.DragonWingsFacingLeftSecond,
        EnemyFrameId.DragonBodyIdleRight,
        EnemyFrameId.DragonWingsFacingRightFirst,
        EnemyFrameId.DragonWingsFacingRightSecond,
        EnemyFrameId.DragonAttackingFacingLeftFirst,
        EnemyFrameId.DragonAttackingFacingLeftSecond,
        EnemyFrameId.DragonAttackingFacingLeftThird,
        EnemyFrameId.DragonAttackingFacingRightFirst,
        EnemyFrameId.DragonAttackingFacingRightSecond,
        EnemyFrameId.DragonAttackingFacingRightThird,
    ];

    /// <summary>The frames <c>Enemies/Elevator/ElevatorBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Elevator =
    [
        EnemyFrameId.ElevatorLoopFirst,
        EnemyFrameId.ElevatorLoopSecond,
    ];

    /// <summary>The frames <c>Enemies/Evir/EvirBehavior.cs</c> shows for <c>Evir</c>.</summary>
    private static readonly EnemyFrameId[] EvirFrames =
    [
        EnemyFrameId.EvirBodyFacingLeftFirst,
        EnemyFrameId.EvirBodyFacingLeftSecond,
        EnemyFrameId.EvirBodyFacingLeftThird,
        EnemyFrameId.EvirBodyFacingLeftFourth,
        EnemyFrameId.EvirBodyFacingLeftFifth,
        EnemyFrameId.EvirBodyFacingLeftSixth,
        EnemyFrameId.EvirArmsFacingLeftFirst,
        EnemyFrameId.EvirArmsFacingLeftSecond,
        EnemyFrameId.EvirArmsFacingLeftThird,
        EnemyFrameId.EvirArmsFacingLeftFourth,
        EnemyFrameId.EvirArmsFacingLeftFifth,
        EnemyFrameId.EvirBodyFacingRightFirst,
        EnemyFrameId.EvirBodyFacingRightSecond,
        EnemyFrameId.EvirBodyFacingRightThird,
        EnemyFrameId.EvirBodyFacingRightFourth,
        EnemyFrameId.EvirBodyFacingRightFifth,
        EnemyFrameId.EvirBodyFacingRightSixth,
        EnemyFrameId.EvirArmsFacingRightFirst,
        EnemyFrameId.EvirArmsFacingRightSecond,
        EnemyFrameId.EvirArmsFacingRightThird,
        EnemyFrameId.EvirArmsFacingRightFourth,
        EnemyFrameId.EvirArmsFacingRightFifth,
    ];

    /// <summary>The frames <c>Enemies/Evir/EvirBehavior.cs</c> shows for <c>EvirProjectile</c>.</summary>
    private static readonly EnemyFrameId[] EvirProjectileFrames =
    [
        EnemyFrameId.EvirProjectileFlight,
        EnemyFrameId.EvirProjectileRegenerating,
    ];

    /// <summary>The frames <c>Enemies/FaceBlocks/BlueBrinstarFaceBlockBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] BlueBrinstarFaceBlock =
    [
        EnemyFrameId.FaceBlockNeutral,
        EnemyFrameId.BlueBrinstarFaceBlockSamusLeftSecond,
        EnemyFrameId.BlueBrinstarFaceBlockSamusLeftThird,
        EnemyFrameId.BlueBrinstarFaceBlockSamusRightSecond,
        EnemyFrameId.BlueBrinstarFaceBlockSamusRightThird,
    ];

    /// <summary>The frames <c>Enemies/Fireflea/FirefleaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Fireflea =
    [
        EnemyFrameId.FirefleaLoopFirst,
        EnemyFrameId.FirefleaLoopSecond,
        EnemyFrameId.FirefleaLoopThird,
        EnemyFrameId.FirefleaLoopFourth,
        EnemyFrameId.FirefleaLoopFifth,
        EnemyFrameId.FirefleaLoopSixth,
        EnemyFrameId.FirefleaLoopSeventh,
        EnemyFrameId.FirefleaLoopEighth,
        EnemyFrameId.FirefleaLoopNinth,
        EnemyFrameId.FirefleaLoopTenth,
        EnemyFrameId.FirefleaLoopEleventh,
        EnemyFrameId.FirefleaLoopTwelfth,
        EnemyFrameId.FirefleaLoopThirteenth,
        EnemyFrameId.FirefleaLoopFourteenth,
        EnemyFrameId.FirefleaLoopFifteenth,
        EnemyFrameId.FirefleaLoopSixteenth,
        EnemyFrameId.FirefleaLoopSeventeenth,
        EnemyFrameId.FirefleaLoopEighteenth,
        EnemyFrameId.FirefleaLoopNineteenth,
        EnemyFrameId.FirefleaLoopTwentieth,
        EnemyFrameId.FirefleaLoopTwentyFirst,
    ];

    /// <summary>The frames <c>Enemies/Flies/FliesBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Flies =
    [
        EnemyFrameId.FlyFlightFirst,
        EnemyFrameId.FlyFlightSecond,
        EnemyFrameId.FlyFlightThird,
        EnemyFrameId.FlyFlightFourth,
    ];

    /// <summary>The frames <c>Enemies/FuneNamihe/FuneNamiheBehavior.cs</c> shows for <c>Fune</c>.</summary>
    private static readonly EnemyFrameId[] FuneFrames =
    [
        EnemyFrameId.FuneLeftIdle,
        EnemyFrameId.FuneActiveLeftSecond,
        EnemyFrameId.FuneActiveLeftThird,
        EnemyFrameId.FuneActiveLeftFourth,
        EnemyFrameId.FuneActiveLeftFifth,
        EnemyFrameId.FuneRightIdle,
        EnemyFrameId.FuneActiveRightSecond,
        EnemyFrameId.FuneActiveRightThird,
        EnemyFrameId.FuneActiveRightFourth,
        EnemyFrameId.FuneActiveRightFifth,
    ];

    /// <summary>The frames <c>Enemies/FuneNamihe/FuneNamiheBehavior.cs</c> shows for <c>Namihe</c>.</summary>
    private static readonly EnemyFrameId[] NamiheFrames =
    [
        EnemyFrameId.NamiheLeftIdle,
        EnemyFrameId.NamiheActiveLeftSecond,
        EnemyFrameId.NamiheActiveLeftThird,
        EnemyFrameId.NamiheActiveLeftFourth,
        EnemyFrameId.NamiheActiveLeftFifth,
        EnemyFrameId.NamiheActiveLeftSixth,
        EnemyFrameId.NamiheRightIdle,
        EnemyFrameId.NamiheActiveRightSecond,
        EnemyFrameId.NamiheActiveRightThird,
        EnemyFrameId.NamiheActiveRightFourth,
        EnemyFrameId.NamiheActiveRightFifth,
        EnemyFrameId.NamiheActiveRightSixth,
    ];

    /// <summary>The frames <c>Enemies/Gunship/GunshipBehavior.cs</c> shows for <c>ShipBottomEntrance</c>.</summary>
    private static readonly EnemyFrameId[] ShipBottomEntranceFrames =
    [
        EnemyFrameId.GunshipEntrancePadOpeningFirst,
        EnemyFrameId.GunshipEntrancePadOpeningSecond,
        EnemyFrameId.GunshipEntrancePadOpeningThird,
        EnemyFrameId.GunshipEntrancePadOpeningFourth,
        EnemyFrameId.GunshipEntrancePadOpeningFifth,
        EnemyFrameId.GunshipEntrancePadOpeningSixth,
        EnemyFrameId.GunshipEntrancePadOpeningSeventh,
        EnemyFrameId.GunshipEntrancePadOpeningEighth,
        EnemyFrameId.GunshipEntrancePadOpeningNinth,
        EnemyFrameId.GunshipEntrancePadOpeningTenth,
        EnemyFrameId.GunshipEntrancePadOpen,
        EnemyFrameId.GunshipBottomHull,
    ];

    /// <summary>The frames <c>Enemies/Gunship/GunshipBehavior.cs</c> shows for <c>ShipTop</c>.</summary>
    private static readonly EnemyFrameId[] ShipTopFrames =
    [
        EnemyFrameId.GunshipTopHull,
    ];

    /// <summary>The frames <c>Enemies/Hibashi/HibashiBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Hibashi =
    [
        EnemyFrameId.HibashiGraphicsFirst,
        EnemyFrameId.HibashiGraphicsSecond,
        EnemyFrameId.HibashiGraphicsThird,
        EnemyFrameId.HibashiGraphicsFourth,
        EnemyFrameId.HibashiGraphicsFifth,
        EnemyFrameId.HibashiGraphicsSixth,
        EnemyFrameId.HibashiGraphicsSeventh,
        EnemyFrameId.HibashiGraphicsEighth,
        EnemyFrameId.HibashiGraphicsNinth,
        EnemyFrameId.HibashiGraphicsTenth,
        EnemyFrameId.HibashiGraphicsEleventh,
        EnemyFrameId.HibashiGraphicsTwelfth,
        EnemyFrameId.HibashiGraphicsThirteenth,
        EnemyFrameId.HibashiGraphicsFourteenth,
        EnemyFrameId.HibashiGraphicsFifteenth,
        EnemyFrameId.HibashiGraphicsSixteenth,
        EnemyFrameId.HibashiGraphicsSeventeenth,
        EnemyFrameId.HibashiGraphicsEighteenth,
        EnemyFrameId.HibashiGraphicsNineteenth,
        EnemyFrameId.HibashiGraphicsTwentieth,
        EnemyFrameId.HibashiGraphicsTwentyFirst,
        EnemyFrameId.HibashiGraphicsTwentySecond,
        EnemyFrameId.HibashiGraphicsTwentyThird,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>Dessgeega</c>.</summary>
    private static readonly EnemyFrameId[] DessgeegaFrames =
    [
        EnemyFrameId.DessgeegaJumpFloor,
        EnemyFrameId.DessgeegaFloorLandedFirst,
        EnemyFrameId.DessgeegaFloorLandedSecond,
        EnemyFrameId.DessgeegaJumpCeiling,
        EnemyFrameId.DessgeegaCeilingLandedFirst,
        EnemyFrameId.DessgeegaCeilingLandedSecond,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>DessgeegaLarge</c>.</summary>
    private static readonly EnemyFrameId[] DessgeegaLargeFrames =
    [
        EnemyFrameId.LargeDessgeegaJumpFloor,
        EnemyFrameId.LargeDessgeegaFloorLandedFirst,
        EnemyFrameId.LargeDessgeegaFloorLandedSecond,
        EnemyFrameId.LargeDessgeegaJumpCeiling,
        EnemyFrameId.LargeDessgeegaCeilingLandedFirst,
        EnemyFrameId.LargeDessgeegaCeilingLandedSecond,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>Sidehopper</c>.</summary>
    private static readonly EnemyFrameId[] SidehopperFrames =
    [
        EnemyFrameId.SidehopperJumpFloor,
        EnemyFrameId.SidehopperFloorLandedFirst,
        EnemyFrameId.SidehopperFloorLandedSecond,
        EnemyFrameId.SidehopperJumpCeiling,
        EnemyFrameId.SidehopperCeilingLandedFirst,
        EnemyFrameId.SidehopperCeilingLandedSecond,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>SidehopperLarge</c>, <c>SidehopperTourian</c>.</summary>
    private static readonly EnemyFrameId[] SidehopperLargeFrames =
    [
        EnemyFrameId.LargeSidehopperJumpFloor,
        EnemyFrameId.LargeSidehopperFloorLandedFirst,
        EnemyFrameId.LargeSidehopperFloorLandedSecond,
        EnemyFrameId.LargeSidehopperJumpCeiling,
        EnemyFrameId.LargeSidehopperCeilingLandedFirst,
        EnemyFrameId.LargeSidehopperCeilingLandedSecond,
    ];

    /// <summary>The frames <c>Enemies/Inert/InertEnemyFactory.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] InertEnemy = [];

    /// <summary>The frames <c>Enemies/Kago/KagoBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Kago =
    [
        EnemyFrameId.KagoSlowFirst,
        EnemyFrameId.KagoSlowSecond,
        EnemyFrameId.KagoSlowThird,
    ];

    /// <summary>The frames <c>Enemies/KiHunters/KiHunterBehavior.cs</c> shows for <c>KihunterGreen</c>, <c>KihunterRed</c>, <c>KihunterYellow</c>.</summary>
    private static readonly EnemyFrameId[] KihunterGreenFrames =
    [
        EnemyFrameId.KiHunterFlyingLeftFirst,
        EnemyFrameId.KiHunterFlyingLeftSecond,
        EnemyFrameId.KiHunterFlyingLeftThird,
        EnemyFrameId.KiHunterFlyingRightFirst,
        EnemyFrameId.KiHunterFlyingRightSecond,
        EnemyFrameId.KiHunterFlyingRightThird,
        EnemyFrameId.KiHunterSwoopLeftThird,
        EnemyFrameId.KiHunterSwoopLeftFirst,
        EnemyFrameId.KiHunterSwoopLeftSecond,
        EnemyFrameId.KiHunterSwoopLeftFourth,
        EnemyFrameId.KiHunterSwoopLeftFifth,
        EnemyFrameId.KiHunterSwoopRightThird,
        EnemyFrameId.KiHunterSwoopRightFirst,
        EnemyFrameId.KiHunterSwoopRightSecond,
        EnemyFrameId.KiHunterSwoopRightFourth,
        EnemyFrameId.KiHunterSwoopRightFifth,
        EnemyFrameId.KiHunterJumpLeftFirst,
        EnemyFrameId.KiHunterJumpLeftSecond,
        EnemyFrameId.KiHunterJumpLeftThird,
        EnemyFrameId.KiHunterJumpLeftFourth,
        EnemyFrameId.KiHunterJumpRightFirst,
        EnemyFrameId.KiHunterJumpRightSecond,
        EnemyFrameId.KiHunterJumpRightThird,
        EnemyFrameId.KiHunterJumpRightFourth,
        EnemyFrameId.KiHunterSpitLeftFirst,
        EnemyFrameId.KiHunterSpitLeftSecond,
        EnemyFrameId.KiHunterSpitLeftThird,
        EnemyFrameId.KiHunterSpitLeftFourth,
        EnemyFrameId.KiHunterSpitLeftFifth,
        EnemyFrameId.KiHunterSpitRightFirst,
        EnemyFrameId.KiHunterSpitRightSecond,
        EnemyFrameId.KiHunterSpitRightThird,
        EnemyFrameId.KiHunterSpitRightFourth,
        EnemyFrameId.KiHunterSpitRightFifth,
    ];

    /// <summary>The frames <c>Enemies/KiHunters/KiHunterBehavior.cs</c> shows for <c>KihunterGreenWings</c>, <c>KihunterRedWings</c>, <c>KihunterYellowWings</c>.</summary>
    private static readonly EnemyFrameId[] KihunterGreenWingsFrames =
    [
        EnemyFrameId.KiHunterWingsLeftFirst,
        EnemyFrameId.KiHunterWingsLeftSecond,
        EnemyFrameId.KiHunterWingsLeftThird,
        EnemyFrameId.KiHunterWingsRightFirst,
        EnemyFrameId.KiHunterWingsRightSecond,
        EnemyFrameId.KiHunterWingsRightThird,
        EnemyFrameId.KiHunterDetachedWings,
    ];

    /// <summary>The frames <c>Enemies/Kzan/KzanBehavior.cs</c> shows for <c>KzanBottom</c>.</summary>
    private static readonly EnemyFrameId[] KzanBottomFrames = [];

    /// <summary>The frames <c>Enemies/Kzan/KzanBehavior.cs</c> shows for <c>KzanTop</c>.</summary>
    private static readonly EnemyFrameId[] KzanTopFrames =
    [
        EnemyFrameId.KzanIdle,
    ];

    /// <summary>The frames <c>Enemies/LavaJumpers/NorfairLavaJumpingEnemyBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NorfairLavaJumpingEnemy =
    [
        EnemyFrameId.NorfairLavaJumperHidden,
        EnemyFrameId.NorfairLavaJumperJumpSecond,
        EnemyFrameId.NorfairLavaJumperJumpThird,
        EnemyFrameId.NorfairLavaJumperJumpFourth,
        EnemyFrameId.NorfairLavaJumperJumpFifth,
        EnemyFrameId.NorfairLavaJumperJumpSixth,
        EnemyFrameId.NorfairLavaJumperJumpSeventh,
        EnemyFrameId.NorfairLavaJumperFollowerFirst,
        EnemyFrameId.NorfairLavaJumperFollowerLoopFirst,
        EnemyFrameId.NorfairLavaJumperFollowerLoopThird,
    ];

    /// <summary>The frames <c>Enemies/Magdollite/MagdolliteBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Magdollite =
    [
        EnemyFrameId.MagdolliteLeftIdleFirst,
        EnemyFrameId.MagdolliteLeftIdleSecond,
        EnemyFrameId.MagdolliteLeftIdleThird,
        EnemyFrameId.MagdolliteLeftThrowFirst,
        EnemyFrameId.MagdolliteLeftThrowSecond,
        EnemyFrameId.MagdolliteLeftThrowThird,
        EnemyFrameId.MagdolliteLeftThrowFourth,
        EnemyFrameId.MagdollitePillarCap,
        EnemyFrameId.MagdolliteLeftRaisePillar,
        EnemyFrameId.MagdolliteLeftSubmergeSecond,
        EnemyFrameId.MagdolliteLeftSubmergeThird,
        EnemyFrameId.MagdolliteRightIdleFirst,
        EnemyFrameId.MagdolliteRightIdleSecond,
        EnemyFrameId.MagdolliteRightIdleThird,
        EnemyFrameId.MagdolliteRightThrowFirst,
        EnemyFrameId.MagdolliteRightThrowSecond,
        EnemyFrameId.MagdolliteRightThrowThird,
        EnemyFrameId.MagdolliteRightThrowFourth,
        EnemyFrameId.MagdolliteRightRaisePillar,
        EnemyFrameId.MagdolliteRightSubmergeSecond,
        EnemyFrameId.MagdolliteRightSubmergeThird,
        EnemyFrameId.MagdollitePillarPhaseFirst,
        EnemyFrameId.MagdollitePillarPhaseSecond,
        EnemyFrameId.MagdollitePillarPhaseThird,
        EnemyFrameId.MagdollitePillarPhaseFourth,
        EnemyFrameId.MagdollitePillarPhaseFifth,
        EnemyFrameId.MagdollitePillarPhaseSixth,
        EnemyFrameId.MagdollitePillarPhaseSeventh,
        EnemyFrameId.MagdollitePillarPhaseEighth,
    ];

    /// <summary>The frames <c>Enemies/MamaTurtles/MamaTurtleBehavior.cs</c> shows for <c>BabyTurtle</c>.</summary>
    private static readonly EnemyFrameId[] BabyTurtleFrames =
    [
        EnemyFrameId.TurtleBabyCrawlingLeftFirst,
        EnemyFrameId.TurtleBabyCrawlingLeftSecond,
        EnemyFrameId.TurtleBabyCrawlingLeftThird,
        EnemyFrameId.TurtleBabyCrawlingLeftFourth,
        EnemyFrameId.TurtleBabyCrawlingLeftFifth,
        EnemyFrameId.TurtleBabyCrawlingLeftSixth,
        EnemyFrameId.TurtleBabyCrawlingLeftSeventh,
        EnemyFrameId.TurtleBabySpinningFirst,
        EnemyFrameId.TurtleBabySpinningSecond,
        EnemyFrameId.TurtleBabySpinningThird,
        EnemyFrameId.TurtleBabySpinningFourth,
        EnemyFrameId.TurtleBabyCrawlingRightFirst,
        EnemyFrameId.TurtleBabyCrawlingRightSecond,
        EnemyFrameId.TurtleBabyCrawlingRightThird,
        EnemyFrameId.TurtleBabyCrawlingRightFourth,
        EnemyFrameId.TurtleBabyCrawlingRightFifth,
        EnemyFrameId.TurtleBabyCrawlingRightSixth,
        EnemyFrameId.TurtleBabyCrawlingRightSeventh,
        EnemyFrameId.TurtleBabyHidingRightThird,
    ];

    /// <summary>The frames <c>Enemies/MamaTurtles/MamaTurtleBehavior.cs</c> shows for <c>MamaTurtle</c>.</summary>
    private static readonly EnemyFrameId[] MamaTurtleFrames =
    [
        EnemyFrameId.TurtleMamaSpinningFirst,
        EnemyFrameId.TurtleMamaSpinningSecond,
        EnemyFrameId.TurtleMamaSpinningThird,
        EnemyFrameId.TurtleMamaEnterShellLeftFirst,
        EnemyFrameId.TurtleMamaEnterShellLeftSecond,
        EnemyFrameId.TurtleMamaEnterShellLeftThird,
        EnemyFrameId.TurtleMamaEnterShellRightFirst,
        EnemyFrameId.TurtleMamaEnterShellRightSecond,
        EnemyFrameId.TurtleMamaEnterShellRightThird,
        EnemyFrameId.TurtleMamaEnterShellRightFourth,
    ];

    /// <summary>The frames <c>Enemies/MaridiaSnails/MaridiaLargeSnailBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] MaridiaLargeSnail =
    [
        EnemyFrameId.OumFacingLeftIdle,
        EnemyFrameId.OumFacingLeftAttackingSecond,
        EnemyFrameId.OumFacingLeftAttackingThird,
        EnemyFrameId.OumFacingLeftAttackingFourth,
        EnemyFrameId.OumFacingLeftAttackingFifth,
        EnemyFrameId.OumFacingLeftAttackingSixth,
        EnemyFrameId.OumFacingLeftAttackingSeventh,
        EnemyFrameId.OumFacingLeftAttackingEighth,
        EnemyFrameId.OumFacingLeftRollingForwardsSecond,
        EnemyFrameId.OumFacingLeftRollingForwardsThird,
        EnemyFrameId.OumFacingLeftRollingForwardsFourth,
        EnemyFrameId.OumFacingLeftRollingForwardsFifth,
        EnemyFrameId.OumFacingLeftRollingForwardsSixth,
        EnemyFrameId.OumFacingLeftRollingForwardsSeventh,
        EnemyFrameId.OumFacingLeftRollingForwardsEighth,
        EnemyFrameId.OumFacingRightIdle,
        EnemyFrameId.OumFacingRightAttackingSecond,
        EnemyFrameId.OumFacingRightAttackingThird,
        EnemyFrameId.OumFacingRightAttackingFourth,
        EnemyFrameId.OumFacingRightAttackingFifth,
        EnemyFrameId.OumFacingRightAttackingSixth,
        EnemyFrameId.OumFacingRightAttackingSeventh,
        EnemyFrameId.OumFacingRightAttackingEighth,
        EnemyFrameId.OumFacingRightRollingForwardsSecond,
        EnemyFrameId.OumFacingRightRollingForwardsThird,
        EnemyFrameId.OumFacingRightRollingForwardsFourth,
        EnemyFrameId.OumFacingRightRollingForwardsFifth,
        EnemyFrameId.OumFacingRightRollingForwardsSixth,
        EnemyFrameId.OumFacingRightRollingForwardsSeventh,
        EnemyFrameId.OumFacingRightRollingForwardsEighth,
    ];

    /// <summary>The frames <c>Enemies/Metaree/MetareeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Metaree =
    [
        EnemyFrameId.MetareeIdlingFirst,
        EnemyFrameId.MetareeIdlingSecond,
        EnemyFrameId.MetareeIdlingThird,
        EnemyFrameId.MetareeIdlingFourth,
        EnemyFrameId.MetareePreparingAttackSecond,
    ];

    /// <summary>The frames <c>Enemies/Metroids/MetroidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Metroid =
    [
        EnemyFrameId.MetroidChasingSamusFirst,
        EnemyFrameId.MetroidChasingSamusSecond,
        EnemyFrameId.MetroidChasingSamusThird,
        EnemyFrameId.MetroidChasingSamusFourth,
    ];

    /// <summary>The frames <c>Enemies/Metroids/MochtroidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Mochtroid =
    [
        EnemyFrameId.MochtroidFreeFlightFirst,
        EnemyFrameId.MochtroidFreeFlightSecond,
        EnemyFrameId.MochtroidFreeFlightThird,
        EnemyFrameId.MochtroidAttachedFirst,
        EnemyFrameId.MochtroidAttachedSecond,
        EnemyFrameId.MochtroidAttachedThird,
    ];

    /// <summary>The frames <c>Enemies/MorphBallEye/MorphBallEyeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] MorphBallEye =
    [
        EnemyFrameId.MorphBallEyeActiveFirst,
        EnemyFrameId.MorphBallEyeActiveSecond,
        EnemyFrameId.MorphBallEyeActiveThird,
        EnemyFrameId.MorphBallEyeActiveFourth,
        EnemyFrameId.MorphBallEyeActiveFifth,
        EnemyFrameId.MorphBallEyeActiveSixth,
        EnemyFrameId.MorphBallEyeActiveSeventh,
        EnemyFrameId.MorphBallEyeActiveEighth,
        EnemyFrameId.MorphBallEyeActiveNinth,
        EnemyFrameId.MorphBallEyeActiveTenth,
        EnemyFrameId.MorphBallEyeActiveEleventh,
        EnemyFrameId.MorphBallEyeActiveTwelfth,
        EnemyFrameId.MorphBallEyeActiveThirteenth,
        EnemyFrameId.MorphBallEyeActiveFourteenth,
        EnemyFrameId.MorphEyeRightTransition,
        EnemyFrameId.MorphEyeRightClosed,
        EnemyFrameId.MorphEyeLeftTransition,
        EnemyFrameId.MorphEyeLeftClosed,
        EnemyFrameId.MorphEyeMountRight,
        EnemyFrameId.MorphEyeMountDown,
        EnemyFrameId.MorphEyeMountLeft,
        EnemyFrameId.MorphEyeMountUp,
    ];

    /// <summary>The frames <c>Enemies/Nuclear/NuclearWaffleBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NuclearWaffle =
    [
        EnemyFrameId.NuclearWaffleBodyLoopFirst,
        EnemyFrameId.NuclearWaffleBodyLoopSecond,
        EnemyFrameId.NuclearWaffleBodyLoopThird,
        EnemyFrameId.NuclearWaffleBodyLoopFourth,
        EnemyFrameId.NuclearWaffleBodyLoopFifth,
        EnemyFrameId.NuclearWaffleBodyLoopSixth,
        EnemyFrameId.NuclearWaffleBodyLoopSeventh,
        EnemyFrameId.NuclearWaffleBodyLoopEighth,
    ];

    /// <summary>The frames <c>Enemies/Owtch/OwtchBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Owtch =
    [
        EnemyFrameId.OwtchMovingLeftLoopFirst,
        EnemyFrameId.OwtchMovingLeftLoopSecond,
        EnemyFrameId.OwtchMovingLeftLoopThird,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Gamet</c>.</summary>
    private static readonly EnemyFrameId[] GametFrames =
    [
        EnemyFrameId.GametRisingLeftFirst,
        EnemyFrameId.GametRisingLeftSecond,
        EnemyFrameId.GametRisingLeftThird,
        EnemyFrameId.GametRisingLeftFourth,
        EnemyFrameId.GametRisingLeftFifth,
        EnemyFrameId.GametRisingRightFirst,
        EnemyFrameId.GametRisingRightSecond,
        EnemyFrameId.GametRisingRightThird,
        EnemyFrameId.GametRisingRightFourth,
        EnemyFrameId.GametRisingRightFifth,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Geega</c>.</summary>
    private static readonly EnemyFrameId[] GeegaFrames =
    [
        EnemyFrameId.GeegaFlyingLeftFirst,
        EnemyFrameId.GeegaFlyingLeftSecond,
        EnemyFrameId.GeegaFlyingLeftThird,
        EnemyFrameId.GeegaArcingLeftFirst,
        EnemyFrameId.GeegaArcingLeftSecond,
        EnemyFrameId.GeegaArcingLeftThird,
        EnemyFrameId.GeegaFlyingRightFirst,
        EnemyFrameId.GeegaFlyingRightSecond,
        EnemyFrameId.GeegaFlyingRightThird,
        EnemyFrameId.GeegaArcingRightFirst,
        EnemyFrameId.GeegaArcingRightSecond,
        EnemyFrameId.GeegaArcingRightThird,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Zeb</c>.</summary>
    private static readonly EnemyFrameId[] ZebFrames =
    [
        EnemyFrameId.ZebRisingLeftFirst,
        EnemyFrameId.ZebRisingLeftSecond,
        EnemyFrameId.ZebRisingLeftThird,
        EnemyFrameId.ZebRisingLeftFourth,
        EnemyFrameId.ZebRisingLeftFifth,
        EnemyFrameId.ZebRisingRightFirst,
        EnemyFrameId.ZebRisingRightSecond,
        EnemyFrameId.ZebRisingRightThird,
        EnemyFrameId.ZebRisingRightFourth,
        EnemyFrameId.ZebRisingRightFifth,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Zebbo</c>.</summary>
    private static readonly EnemyFrameId[] ZebboFrames =
    [
        EnemyFrameId.ZebboShootingLeftFirst,
        EnemyFrameId.ZebboShootingLeftSecond,
        EnemyFrameId.ZebboShootingLeftThird,
        EnemyFrameId.ZebboRisingLeftFirst,
        EnemyFrameId.ZebboRisingLeftSecond,
        EnemyFrameId.ZebboRisingLeftThird,
        EnemyFrameId.ZebboShootingRightFirst,
        EnemyFrameId.ZebboShootingRightSecond,
        EnemyFrameId.ZebboShootingRightThird,
        EnemyFrameId.ZebboRisingRightFirst,
        EnemyFrameId.ZebboRisingRightSecond,
        EnemyFrameId.ZebboRisingRightThird,
    ];

    /// <summary>The frames <c>Enemies/Platforms/PlatformsBehavior.cs</c> shows for <c>Kamer2</c>.</summary>
    private static readonly EnemyFrameId[] Kamer2Frames =
    [
        EnemyFrameId.KamerMovingLeftLoopFirst,
        EnemyFrameId.KamerMovingLeftLoopSecond,
        EnemyFrameId.KamerMovingLeftLoopThird,
        EnemyFrameId.KamerMovingLeftLoopFourth,
    ];

    /// <summary>The frames <c>Enemies/Platforms/PlatformsBehavior.cs</c> shows for <c>Tripper</c>.</summary>
    private static readonly EnemyFrameId[] TripperFrames =
    [
        EnemyFrameId.TripperMovingLeftLoopFirst,
        EnemyFrameId.TripperMovingLeftLoopSecond,
        EnemyFrameId.TripperMovingLeftLoopThird,
        EnemyFrameId.TripperMovingRightLoopFirst,
        EnemyFrameId.TripperMovingRightLoopSecond,
        EnemyFrameId.TripperMovingRightLoopThird,
        EnemyFrameId.TripperStillMovingLeftLoopFirst,
        EnemyFrameId.TripperStillMovingLeftLoopSecond,
        EnemyFrameId.TripperStillMovingLeftLoopThird,
        EnemyFrameId.TripperStillMovingRightLoopFirst,
        EnemyFrameId.TripperStillMovingRightLoopSecond,
        EnemyFrameId.TripperStillMovingRightLoopThird,
        EnemyFrameId.TripperFrozenMovingLeft,
        EnemyFrameId.TripperFrozenMovingRight,
    ];

    /// <summary>The frames <c>Enemies/Polyp/PolypBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Polyp =
    [
        EnemyFrameId.PolypStationary,
    ];

    /// <summary>The frames <c>Enemies/Powamp/PowampBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Powamp =
    [
        EnemyFrameId.PowampBodyFastFirst,
        EnemyFrameId.PowampBodyFastSecond,
        EnemyFrameId.PowampBodyFastThird,
        EnemyFrameId.PowampBalloonInflateFirst,
        EnemyFrameId.PowampBalloonInflateSecond,
        EnemyFrameId.PowampBalloonInflateThird,
    ];

    /// <summary>The frames <c>Enemies/Puyo/PuyoBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Puyo =
    [
        EnemyFrameId.PuyoGroundedFastFirst,
        EnemyFrameId.PuyoGroundedFastSecond,
        EnemyFrameId.PuyoGroundedFastThird,
        EnemyFrameId.PuyoAirborneFirst,
        EnemyFrameId.PuyoAirborneSecond,
        EnemyFrameId.PuyoAirborneThird,
        EnemyFrameId.PuyoAirborneFourth,
        EnemyFrameId.PuyoAirborneFifth,
    ];

    /// <summary>The frames <c>Enemies/Rios/LowerNorfairRioBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] LowerNorfairRio =
    [
        EnemyFrameId.LowerNorfairRioIdleLoopFirst,
        EnemyFrameId.LowerNorfairRioIdleLoopSecond,
        EnemyFrameId.LowerNorfairRioIdleLoopThird,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopSecond,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopThird,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopFourth,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopFifth,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopSixth,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopSeventh,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopEighth,
        EnemyFrameId.LowerNorfairRioPrepareToSwoopNinth,
        EnemyFrameId.LowerNorfairRioAscendingPartOneThird,
        EnemyFrameId.LowerNorfairRioAscendingPartTwoLoopSecond,
        EnemyFrameId.LowerNorfairRioAscendingPartTwoLoopThird,
        EnemyFrameId.LowerNorfairRioAscendingPartTwoLoopFirst,
        EnemyFrameId.LowerNorfairRioFlamesFirst,
        EnemyFrameId.LowerNorfairRioFlamesSecond,
        EnemyFrameId.LowerNorfairRioFlamesThird,
    ];

    /// <summary>The frames <c>Enemies/Rios/NorfairRioBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NorfairRio =
    [
        EnemyFrameId.NorfairRioIdleSecond,
        EnemyFrameId.NorfairRioIdleFirst,
        EnemyFrameId.NorfairRioIdleThird,
        EnemyFrameId.NorfairRioFlamesAscendingFirst,
        EnemyFrameId.NorfairRioFlamesAscendingSecond,
        EnemyFrameId.NorfairRioFlamesAscendingThird,
        EnemyFrameId.NorfairRioDescendingFirst,
        EnemyFrameId.NorfairRioDescendingSecond,
        EnemyFrameId.NorfairRioDescendingThird,
        EnemyFrameId.NorfairRioStartDescendingSecond,
        EnemyFrameId.NorfairRioStartDescendingThird,
        EnemyFrameId.NorfairRioStartDescendingFourth,
        EnemyFrameId.NorfairRioStartDescendingFifth,
        EnemyFrameId.NorfairRioStartDescendingSixth,
        EnemyFrameId.NorfairRioAscendingFirst,
        EnemyFrameId.NorfairRioAscendingSecond,
        EnemyFrameId.NorfairRioAscendingThird,
        EnemyFrameId.NorfairRioFlamesDescendingFirst,
        EnemyFrameId.NorfairRioFlamesDescendingSecond,
        EnemyFrameId.NorfairRioFlamesDescendingThird,
    ];

    /// <summary>The frames <c>Enemies/Rios/RioBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Rio =
    [
        EnemyFrameId.RioIdleFirst,
        EnemyFrameId.RioIdleSecond,
        EnemyFrameId.RioPostSwoopIdleThird,
        EnemyFrameId.RioPostSwoopIdleFourth,
        EnemyFrameId.RioSwoopingPartOneThird,
        EnemyFrameId.RioSwoopingPartOneFourth,
        EnemyFrameId.RioSwoopingPartOneFifth,
        EnemyFrameId.RioSwoopingPartTwoFirst,
    ];

    /// <summary>The frames <c>Enemies/Ripper/RipperBehavior.cs</c> shows for <c>GRipper</c>, <c>Ripper2</c>.</summary>
    private static readonly EnemyFrameId[] GRipperFrames =
    [
        EnemyFrameId.GRipperMovingLeftFirst,
        EnemyFrameId.GRipperMovingLeftSecond,
        EnemyFrameId.GRipperMovingLeftThird,
        EnemyFrameId.GRipperMovingRightFirst,
        EnemyFrameId.GRipperMovingRightSecond,
        EnemyFrameId.GRipperMovingRightThird,
        EnemyFrameId.RipperSharedFrozenLeft,
        EnemyFrameId.RipperSharedFrozenRight,
    ];

    /// <summary>The frames <c>Enemies/Ripper/RipperBehavior.cs</c> shows for <c>Ripper</c>.</summary>
    private static readonly EnemyFrameId[] RipperFrames =
    [
        EnemyFrameId.RipperMovingLeftFirst,
        EnemyFrameId.RipperMovingLeftSecond,
        EnemyFrameId.RipperMovingLeftThird,
        EnemyFrameId.RipperMovingRightFirst,
        EnemyFrameId.RipperMovingRightSecond,
        EnemyFrameId.RipperMovingRightThird,
    ];

    /// <summary>The frames <c>Enemies/Sbug/SbugBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Sbug =
    [
        EnemyFrameId.SbugRightFirst,
        EnemyFrameId.SbugRightSecond,
        EnemyFrameId.SbugRightThird,
        EnemyFrameId.SbugUpRightFirst,
        EnemyFrameId.SbugUpRightSecond,
        EnemyFrameId.SbugUpRightThird,
        EnemyFrameId.SbugUpFirst,
        EnemyFrameId.SbugUpSecond,
        EnemyFrameId.SbugUpThird,
        EnemyFrameId.SbugUpLeftFirst,
        EnemyFrameId.SbugUpLeftSecond,
        EnemyFrameId.SbugUpLeftThird,
        EnemyFrameId.SbugLeftFirst,
        EnemyFrameId.SbugLeftSecond,
        EnemyFrameId.SbugLeftThird,
        EnemyFrameId.SbugDownLeftFirst,
        EnemyFrameId.SbugDownLeftSecond,
        EnemyFrameId.SbugDownLeftThird,
        EnemyFrameId.SbugDownFirst,
        EnemyFrameId.SbugDownSecond,
        EnemyFrameId.SbugDownThird,
        EnemyFrameId.SbugDownRightFirst,
        EnemyFrameId.SbugDownRightSecond,
        EnemyFrameId.SbugDownRightThird,
    ];

    /// <summary>The frames <c>Enemies/Shaktool/ShaktoolBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Shaktool =
    [
        EnemyFrameId.ShaktoolSawHandFinalPieceFirst,
        EnemyFrameId.ShaktoolSawHandFinalPieceSecond,
        EnemyFrameId.ShaktoolSawHandFinalPieceThird,
        EnemyFrameId.ShaktoolArm,
        EnemyFrameId.ShaktoolHeadLeft,
        EnemyFrameId.ShaktoolHeadUpLeft,
        EnemyFrameId.ShaktoolHeadUp,
        EnemyFrameId.ShaktoolHeadUpRight,
        EnemyFrameId.ShaktoolHeadRight,
        EnemyFrameId.ShaktoolHeadDownRight,
        EnemyFrameId.ShaktoolHeadDown,
        EnemyFrameId.ShaktoolHeadDownLeft,
        EnemyFrameId.ShaktoolSawHandPrimaryPieceFirst,
        EnemyFrameId.ShaktoolSawHandPrimaryPieceSecond,
        EnemyFrameId.ShaktoolSawHandPrimaryPieceThird,
    ];

    /// <summary>The frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> shows for <c>Kamer</c>.</summary>
    private static readonly EnemyFrameId[] KamerFrames =
    [
        EnemyFrameId.KamerPlatformFirst,
        EnemyFrameId.KamerPlatformSecond,
        EnemyFrameId.KamerPlatformThird,
        EnemyFrameId.KamerPlatformFourth,
    ];

    /// <summary>The frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> shows for <c>ShutterDestroyable</c>, <c>ShutterShootable</c>.</summary>
    private static readonly EnemyFrameId[] ShutterDestroyableFrames =
    [
        EnemyFrameId.ShutterFortyPixels,
    ];

    /// <summary>The frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> shows for <c>ShutterGrowing</c>.</summary>
    private static readonly EnemyFrameId[] ShutterGrowingFrames =
    [
        EnemyFrameId.ShutterGrowingTenPixels,
        EnemyFrameId.ShutterGrowingTwentyPixels,
        EnemyFrameId.ShutterGrowingThirtyPixels,
        EnemyFrameId.ShutterFortyPixels,
    ];

    /// <summary>The frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> shows for <c>ShutterHorizShootable</c>.</summary>
    private static readonly EnemyFrameId[] ShutterHorizShootableFrames =
    [
        EnemyFrameId.ShutterHorizontal,
    ];

    /// <summary>The frames <c>Enemies/Skree/SkreeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Skree =
    [
        EnemyFrameId.SkreeIdlingFirst,
        EnemyFrameId.SkreeIdlingSecond,
        EnemyFrameId.SkreeIdlingThird,
        EnemyFrameId.SkreeIdlingFourth,
        EnemyFrameId.SkreePreparingAttackSecond,
    ];

    /// <summary>The frames <c>Enemies/Skultera/SkulteraBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Skultera =
    [
        EnemyFrameId.SkulteraSwimmingLeftLoopFirst,
        EnemyFrameId.SkulteraSwimmingLeftLoopSecond,
        EnemyFrameId.SkulteraSwimmingLeftLoopThird,
        EnemyFrameId.SkulteraTurningRightFirst,
        EnemyFrameId.SkulteraTurningRightSecond,
        EnemyFrameId.SkulteraTurningRightThird,
        EnemyFrameId.SkulteraTurningRightFourth,
        EnemyFrameId.SkulteraTurningRightFifth,
        EnemyFrameId.SkulteraTurningRightSixth,
        EnemyFrameId.SkulteraTurningRightSeventh,
        EnemyFrameId.SkulteraTurningRightEighth,
        EnemyFrameId.SkulteraSwimmingRightLoopFirst,
        EnemyFrameId.SkulteraSwimmingRightLoopSecond,
        EnemyFrameId.SkulteraSwimmingRightLoopThird,
        EnemyFrameId.SkulteraTurningLeftFirst,
        EnemyFrameId.SkulteraTurningLeftSecond,
        EnemyFrameId.SkulteraTurningLeftThird,
        EnemyFrameId.SkulteraTurningLeftFourth,
        EnemyFrameId.SkulteraTurningLeftFifth,
        EnemyFrameId.SkulteraTurningLeftSixth,
        EnemyFrameId.SkulteraTurningLeftSeventh,
        EnemyFrameId.SkulteraTurningLeftEighth,
    ];

    /// <summary>The frames <c>Enemies/SpacePirates/NinjaSpacePirateBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NinjaSpacePirate =
    [
        EnemyFrameId.NinjaPirateClawAttackLeftFirst,
        EnemyFrameId.NinjaPirateClawAttackLeftSecond,
        EnemyFrameId.NinjaPirateClawAttackLeftThird,
        EnemyFrameId.NinjaPirateClawAttackLeftFourth,
        EnemyFrameId.NinjaPirateClawAttackLeftFifth,
        EnemyFrameId.NinjaPirateClawAttackLeftSixth,
        EnemyFrameId.NinjaPirateClawAttackLeftSeventh,
        EnemyFrameId.NinjaPirateClawAttackLeftEighth,
        EnemyFrameId.NinjaPirateClawAttackLeftNinth,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopFirst,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopSecond,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopThird,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopFourth,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopFifth,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopSixth,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopSeventh,
        EnemyFrameId.NinjaPirateSpinJumpLeftLoopEighth,
        EnemyFrameId.NinjaPirateActiveFacingLeftLoopFirst,
        EnemyFrameId.NinjaPirateActiveFacingLeftLoopSecond,
        EnemyFrameId.NinjaPirateActiveFacingLeftLoopThird,
        EnemyFrameId.NinjaPirateFlinchFacingLeft,
        EnemyFrameId.NinjaPirateDivekickLeftJump,
        EnemyFrameId.NinjaPirateDivekickLeftJumpLoop,
        EnemyFrameId.NinjaPirateDivekickLeftDive,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopFirst,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopSecond,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopThird,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopFourth,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopFifth,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopSixth,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopSeventh,
        EnemyFrameId.NinjaPirateWalkToLeftPostLoopEighth,
        EnemyFrameId.NinjaPirateInitialFacingLeftLoopFirst,
        EnemyFrameId.NinjaPirateInitialFacingLeftLoopSecond,
        EnemyFrameId.NinjaPirateInitialFacingLeftLoopThird,
        EnemyFrameId.NinjaPirateLandFacingLeftThird,
        EnemyFrameId.NinjaPirateLandFacingLeftFourth,
        EnemyFrameId.NinjaPirateLandFacingLeftLoopFirst,
        EnemyFrameId.NinjaPirateLandFacingLeftLoopSecond,
        EnemyFrameId.NinjaPirateLandFacingLeftLoopThird,
        EnemyFrameId.NinjaPirateKickFacingLeftFirst,
        EnemyFrameId.NinjaPirateKickFacingLeftSecond,
        EnemyFrameId.NinjaPirateKickFacingLeftThird,
        EnemyFrameId.NinjaPirateClawAttackRightFirst,
        EnemyFrameId.NinjaPirateClawAttackRightSecond,
        EnemyFrameId.NinjaPirateClawAttackRightThird,
        EnemyFrameId.NinjaPirateClawAttackRightFourth,
        EnemyFrameId.NinjaPirateClawAttackRightFifth,
        EnemyFrameId.NinjaPirateClawAttackRightSixth,
        EnemyFrameId.NinjaPirateClawAttackRightSeventh,
        EnemyFrameId.NinjaPirateClawAttackRightEighth,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopFirst,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopSecond,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopThird,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopFourth,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopFifth,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopSixth,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopSeventh,
        EnemyFrameId.NinjaPirateSpinJumpRightLoopEighth,
        EnemyFrameId.NinjaPirateFlinchFacingRight,
        EnemyFrameId.NinjaPirateDivekickRightJump,
        EnemyFrameId.NinjaPirateDivekickRightJumpLoop,
        EnemyFrameId.NinjaPirateDivekickRightDive,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopFirst,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopSecond,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopThird,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopFourth,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopFifth,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopSixth,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopSeventh,
        EnemyFrameId.NinjaPirateWalkToRightPostLoopEighth,
        EnemyFrameId.NinjaPirateInitialFacingRightLoopFirst,
        EnemyFrameId.NinjaPirateInitialFacingRightLoopThird,
        EnemyFrameId.NinjaPirateKickFacingRightFirst,
        EnemyFrameId.NinjaPirateKickFacingRightSecond,
        EnemyFrameId.NinjaPirateKickFacingRightThird,
    ];

    /// <summary>The frames <c>Enemies/SpacePirates/WalkingSpacePirateBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] WalkingSpacePirate =
    [
        EnemyFrameId.WalkingPirateFlinchLeft,
        EnemyFrameId.WalkingPirateFlinchRight,
        EnemyFrameId.WalkingPirateWalkingLeftLoopFirst,
        EnemyFrameId.WalkingPirateWalkingLeftLoopSecond,
        EnemyFrameId.WalkingPirateWalkingLeftLoopThird,
        EnemyFrameId.WalkingPirateWalkingLeftLoopFourth,
        EnemyFrameId.WalkingPirateWalkingLeftLoopFifth,
        EnemyFrameId.WalkingPirateWalkingLeftLoopSixth,
        EnemyFrameId.WalkingPirateWalkingLeftLoopSeventh,
        EnemyFrameId.WalkingPirateWalkingLeftLoopEighth,
        EnemyFrameId.WalkingPirateFireLasersLeftFirst,
        EnemyFrameId.WalkingPirateFireLasersLeftSecond,
        EnemyFrameId.WalkingPirateFireLasersLeftThird,
        EnemyFrameId.WalkingPirateFireLasersLeftFourth,
        EnemyFrameId.WalkingPirateFireLasersLeftFifth,
        EnemyFrameId.WalkingPirateFireLasersLeftSixth,
        EnemyFrameId.WalkingPirateLookingFacingLeftFirst,
        EnemyFrameId.WalkingPirateLookingFacingLeftSecond,
        EnemyFrameId.WalkingPirateLookingFacingLeftThird,
        EnemyFrameId.WalkingPirateLookShared,
        EnemyFrameId.WalkingPirateWalkingRightLoopFirst,
        EnemyFrameId.WalkingPirateWalkingRightLoopSecond,
        EnemyFrameId.WalkingPirateWalkingRightLoopThird,
        EnemyFrameId.WalkingPirateWalkingRightLoopFourth,
        EnemyFrameId.WalkingPirateWalkingRightLoopFifth,
        EnemyFrameId.WalkingPirateWalkingRightLoopSixth,
        EnemyFrameId.WalkingPirateWalkingRightLoopSeventh,
        EnemyFrameId.WalkingPirateWalkingRightLoopEighth,
        EnemyFrameId.WalkingPirateFireLasersRightFirst,
        EnemyFrameId.WalkingPirateFireLasersRightSecond,
        EnemyFrameId.WalkingPirateFireLasersRightThird,
        EnemyFrameId.WalkingPirateFireLasersRightFourth,
        EnemyFrameId.WalkingPirateFireLasersRightFifth,
        EnemyFrameId.WalkingPirateFireLasersRightSixth,
        EnemyFrameId.WalkingPirateLookingFacingRightFirst,
        EnemyFrameId.WalkingPirateLookingFacingRightSecond,
        EnemyFrameId.WalkingPirateLookingFacingRightThird,
    ];

    /// <summary>The frames <c>Enemies/SpacePirates/WallSpacePirateBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] WallSpacePirate =
    [
        EnemyFrameId.WallPirateFireAndJumpLeftFirst,
        EnemyFrameId.WallPirateFireAndJumpLeftSecond,
        EnemyFrameId.WallPirateFireAndJumpLeftThird,
        EnemyFrameId.WallPirateFireAndJumpLeftFourth,
        EnemyFrameId.WallPirateMovingUpLeftWallLoopFirst,
        EnemyFrameId.WallPirateMovingUpLeftWallLoopSecond,
        EnemyFrameId.WallPirateMovingUpLeftWallLoopThird,
        EnemyFrameId.WallPirateMovingUpLeftWallLoopFourth,
        EnemyFrameId.WallPirateMovingUpLeftWallLoopFifth,
        EnemyFrameId.WallPirateFireAndJumpRightFirst,
        EnemyFrameId.WallPirateFireAndJumpRightSecond,
        EnemyFrameId.WallPirateFireAndJumpRightThird,
        EnemyFrameId.WallPirateFireAndJumpRightFourth,
        EnemyFrameId.WallPirateMovingDownRightWallLoopFirst,
        EnemyFrameId.WallPirateMovingDownRightWallLoopSecond,
        EnemyFrameId.WallPirateMovingDownRightWallLoopThird,
        EnemyFrameId.WallPirateMovingDownRightWallLoopFourth,
        EnemyFrameId.WallPirateMovingDownRightWallLoopFifth,
    ];

    /// <summary>The frames <c>Enemies/Sparks/SparkBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Spark =
    [
        EnemyFrameId.SparkFlickerOnFirst,
        EnemyFrameId.SparkFlickerOnThird,
        EnemyFrameId.SparkFlickerOnFourth,
        EnemyFrameId.SparkFlickerOnFifth,
        EnemyFrameId.SparkActiveFirst,
        EnemyFrameId.SparkActiveSecond,
        EnemyFrameId.SparkActiveThird,
        EnemyFrameId.SparkActiveFourth,
    ];

    /// <summary>The frames <c>Enemies/Stoke/StokeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Stoke =
    [
        EnemyFrameId.StokeMovingLeftLoopFirst,
        EnemyFrameId.StokeMovingLeftLoopSecond,
        EnemyFrameId.StokeMovingLeftLoopThird,
        EnemyFrameId.StokeMovingLeftLoopFourth,
        EnemyFrameId.StokeAttackLeft,
        EnemyFrameId.StokeMovingRightLoopFirst,
        EnemyFrameId.StokeMovingRightLoopSecond,
        EnemyFrameId.StokeMovingRightLoopThird,
        EnemyFrameId.StokeMovingRightLoopFourth,
        EnemyFrameId.StokeAttackRight,
    ];

    /// <summary>The frames <c>Enemies/Waver/WaverBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Waver =
    [
        EnemyFrameId.WaverSteadyLeft,
        EnemyFrameId.WaverSteadyRight,
        EnemyFrameId.WaverSpinningFacingLeftFirst,
        EnemyFrameId.WaverSpinningFacingLeftSecond,
        EnemyFrameId.WaverSpinningFacingLeftThird,
        EnemyFrameId.WaverSpinningFacingLeftFourth,
        EnemyFrameId.WaverSpinningFacingRightFirst,
        EnemyFrameId.WaverSpinningFacingRightSecond,
        EnemyFrameId.WaverSpinningFacingRightThird,
        EnemyFrameId.WaverSpinningFacingRightFourth,
    ];

    /// <summary>The frames <c>Enemies/WorkRobots/WorkRobotBehavior.cs</c> shows for <c>Robot</c>.</summary>
    private static readonly EnemyFrameId[] RobotFrames =
    [
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopTwelfth,
        EnemyFrameId.WorkRobotInitial,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopSecond,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopThird,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopFourth,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopFifth,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopSixth,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopSeventh,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopEighth,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopNinth,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopTenth,
        EnemyFrameId.WorkRobotFacingLeftWalkingForwardsLoopEleventh,
        EnemyFrameId.WorkRobotApproachingFallRightFirst,
        EnemyFrameId.WorkRobotApproachingFallRightSecond,
        EnemyFrameId.WorkRobotApproachingFallRightThird,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopFourth,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopFifth,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopSixth,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopSeventh,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopEighth,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopNinth,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopTenth,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopEleventh,
        EnemyFrameId.WorkRobotFacingRightWalkingForwardsLoopTwelfth,
        EnemyFrameId.WorkRobotUnpoweredNeutral,
        EnemyFrameId.WorkRobotUnpoweredLeft,
        EnemyFrameId.WorkRobotUnpoweredRight,
    ];

    /// <summary>The frames <c>Enemies/WorkRobots/WorkRobotBehavior.cs</c> shows for <c>RobotNoPower</c>.</summary>
    private static readonly EnemyFrameId[] RobotNoPowerFrames =
    [
        EnemyFrameId.WorkRobotUnpoweredNeutral,
        EnemyFrameId.WorkRobotUnpoweredLeft,
        EnemyFrameId.WorkRobotUnpoweredRight,
    ];

    /// <summary>The frames <c>Enemies/WreckedShip/WreckedShipGhostBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] WreckedShipGhost =
    [
        EnemyFrameId.WreckedShipGhostFloatingFirst,
        EnemyFrameId.WreckedShipGhostFloatingSecond,
        EnemyFrameId.WreckedShipGhostFloatingThird,
    ];

    /// <summary>The frames <c>Enemies/YappingMaws/YappingMawBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] YappingMaw =
    [
        EnemyFrameId.YappingMawAttackingUpFirst,
        EnemyFrameId.YappingMawAttackingUpSecond,
        EnemyFrameId.YappingMawAttackingUpThird,
        EnemyFrameId.YappingMawAttackingUpRightFirst,
        EnemyFrameId.YappingMawAttackingUpRightSecond,
        EnemyFrameId.YappingMawAttackingUpRightThird,
        EnemyFrameId.YappingMawAttackingRightFirst,
        EnemyFrameId.YappingMawAttackingRightSecond,
        EnemyFrameId.YappingMawAttackingRightThird,
        EnemyFrameId.YappingMawAttackingDownRightFirst,
        EnemyFrameId.YappingMawAttackingDownRightSecond,
        EnemyFrameId.YappingMawAttackingDownRightThird,
        EnemyFrameId.YappingMawAttackingDownFirst,
        EnemyFrameId.YappingMawAttackingDownSecond,
        EnemyFrameId.YappingMawAttackingDownThird,
        EnemyFrameId.YappingMawAttackingDownLeftFirst,
        EnemyFrameId.YappingMawAttackingDownLeftSecond,
        EnemyFrameId.YappingMawAttackingDownLeftThird,
        EnemyFrameId.YappingMawAttackingLeftFirst,
        EnemyFrameId.YappingMawAttackingLeftSecond,
        EnemyFrameId.YappingMawAttackingLeftThird,
        EnemyFrameId.YappingMawAttackingUpLeftFirst,
        EnemyFrameId.YappingMawAttackingUpLeftSecond,
        EnemyFrameId.YappingMawAttackingUpLeftThird,
    ];

    /// <summary>The frames <c>Enemies/Yard/YardEnemyBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] YardEnemy =
    [
        EnemyFrameId.YardOutsideTurnUpsideRightMovingUpFirst,
        EnemyFrameId.YardOutsideTurnUpsideRightMovingUpSecond,
        EnemyFrameId.YardOutsideTurnUpsideRightMovingUpThird,
        EnemyFrameId.YardCrawlingUpsideUpMovingLeftFirst,
        EnemyFrameId.YardCrawlingUpsideUpMovingLeftSecond,
        EnemyFrameId.YardCrawlingUpsideUpMovingLeftThird,
        EnemyFrameId.YardOutsideTurnUpsideUpMovingLeftFirst,
        EnemyFrameId.YardOutsideTurnUpsideUpMovingLeftSecond,
        EnemyFrameId.YardOutsideTurnUpsideUpMovingLeftThird,
        EnemyFrameId.YardCrawlingUpsideLeftMovingDownFirst,
        EnemyFrameId.YardCrawlingUpsideLeftMovingDownSecond,
        EnemyFrameId.YardCrawlingUpsideLeftMovingDownThird,
        EnemyFrameId.YardOutsideTurnUpsideLeftMovingDownFirst,
        EnemyFrameId.YardOutsideTurnUpsideLeftMovingDownSecond,
        EnemyFrameId.YardOutsideTurnUpsideLeftMovingDownThird,
        EnemyFrameId.YardCrawlingUpsideDownMovingRightFirst,
        EnemyFrameId.YardCrawlingUpsideDownMovingRightSecond,
        EnemyFrameId.YardCrawlingUpsideDownMovingRightThird,
        EnemyFrameId.YardOutsideTurnUpsideDownMovingRightFirst,
        EnemyFrameId.YardOutsideTurnUpsideDownMovingRightSecond,
        EnemyFrameId.YardOutsideTurnUpsideDownMovingRightThird,
        EnemyFrameId.YardCrawlingUpsideRightMovingUpFirst,
        EnemyFrameId.YardCrawlingUpsideRightMovingUpSecond,
        EnemyFrameId.YardCrawlingUpsideRightMovingUpThird,
        EnemyFrameId.YardOutsideTurnUpsideLeftMovingUpFirst,
        EnemyFrameId.YardOutsideTurnUpsideLeftMovingUpSecond,
        EnemyFrameId.YardOutsideTurnUpsideLeftMovingUpThird,
        EnemyFrameId.YardCrawlingUpsideUpMovingRightFirst,
        EnemyFrameId.YardCrawlingUpsideUpMovingRightSecond,
        EnemyFrameId.YardCrawlingUpsideUpMovingRightThird,
        EnemyFrameId.YardOutsideTurnUpsideUpMovingRightFirst,
        EnemyFrameId.YardOutsideTurnUpsideUpMovingRightSecond,
        EnemyFrameId.YardOutsideTurnUpsideUpMovingRightThird,
        EnemyFrameId.YardCrawlingUpsideRightMovingDownFirst,
        EnemyFrameId.YardCrawlingUpsideRightMovingDownSecond,
        EnemyFrameId.YardCrawlingUpsideRightMovingDownThird,
        EnemyFrameId.YardOutsideTurnUpsideRightMovingDownFirst,
        EnemyFrameId.YardOutsideTurnUpsideRightMovingDownSecond,
        EnemyFrameId.YardOutsideTurnUpsideRightMovingDownThird,
        EnemyFrameId.YardCrawlingUpsideDownMovingLeftFirst,
        EnemyFrameId.YardCrawlingUpsideDownMovingLeftSecond,
        EnemyFrameId.YardCrawlingUpsideDownMovingLeftThird,
        EnemyFrameId.YardOutsideTurnUpsideDownMovingLeftFirst,
        EnemyFrameId.YardOutsideTurnUpsideDownMovingLeftSecond,
        EnemyFrameId.YardOutsideTurnUpsideDownMovingLeftThird,
        EnemyFrameId.YardCrawlingUpsideLeftMovingUpFirst,
        EnemyFrameId.YardCrawlingUpsideLeftMovingUpSecond,
        EnemyFrameId.YardCrawlingUpsideLeftMovingUpThird,
        EnemyFrameId.YardInsideTurnUpsideUpMovingLeftFirst,
        EnemyFrameId.YardInsideTurnUpsideUpMovingLeftSecond,
        EnemyFrameId.YardInsideTurnUpsideUpMovingLeftThird,
        EnemyFrameId.YardInsideTurnUpsideRightMovingUpFirst,
        EnemyFrameId.YardInsideTurnUpsideRightMovingUpSecond,
        EnemyFrameId.YardInsideTurnUpsideRightMovingUpThird,
        EnemyFrameId.YardInsideTurnUpsideDownMovingRightFirst,
        EnemyFrameId.YardInsideTurnUpsideDownMovingRightSecond,
        EnemyFrameId.YardInsideTurnUpsideDownMovingRightThird,
        EnemyFrameId.YardInsideTurnUpsideLeftMovingDownFirst,
        EnemyFrameId.YardInsideTurnUpsideLeftMovingDownSecond,
        EnemyFrameId.YardInsideTurnUpsideLeftMovingDownThird,
        EnemyFrameId.YardInsideTurnUpsideUpMovingRightFirst,
        EnemyFrameId.YardInsideTurnUpsideUpMovingRightSecond,
        EnemyFrameId.YardInsideTurnUpsideUpMovingRightThird,
        EnemyFrameId.YardInsideTurnUpsideLeftMovingUpFirst,
        EnemyFrameId.YardInsideTurnUpsideLeftMovingUpSecond,
        EnemyFrameId.YardInsideTurnUpsideLeftMovingUpThird,
        EnemyFrameId.YardInsideTurnUpsideDownMovingLeftFirst,
        EnemyFrameId.YardInsideTurnUpsideDownMovingLeftSecond,
        EnemyFrameId.YardInsideTurnUpsideDownMovingLeftThird,
        EnemyFrameId.YardInsideTurnUpsideRightMovingDownFirst,
        EnemyFrameId.YardInsideTurnUpsideRightMovingDownSecond,
        EnemyFrameId.YardInsideTurnUpsideRightMovingDownThird,
        EnemyFrameId.YardHidingUpsideUpMovingLeftFirst,
        EnemyFrameId.YardHidingUpsideUpMovingLeftSecond,
        EnemyFrameId.YardHiddenUpsideUpMovingLeftFirst,
        EnemyFrameId.YardHiddenUpsideUpMovingLeftSecond,
        EnemyFrameId.YardHidingUpsideDownMovingLeftFirst,
        EnemyFrameId.YardHidingUpsideDownMovingLeftSecond,
        EnemyFrameId.YardHidingUpsideDownMovingLeftThird,
        EnemyFrameId.YardHidingUpsideDownMovingLeftFourth,
        EnemyFrameId.YardHidingUpsideDownMovingRightFirst,
        EnemyFrameId.YardHidingUpsideDownMovingRightSecond,
        EnemyFrameId.YardHidingUpsideDownMovingRightThird,
        EnemyFrameId.YardHidingUpsideDownMovingRightFourth,
        EnemyFrameId.YardHidingUpsideUpMovingRightFirst,
        EnemyFrameId.YardHidingUpsideUpMovingRightSecond,
        EnemyFrameId.YardHiddenUpsideUpMovingRightFirst,
        EnemyFrameId.YardHiddenUpsideUpMovingRightSecond,
        EnemyFrameId.YardHidingUpsideRightMovingUpFirst,
        EnemyFrameId.YardHidingUpsideRightMovingUpSecond,
        EnemyFrameId.YardHidingUpsideRightMovingUpThird,
        EnemyFrameId.YardHidingUpsideRightMovingUpFourth,
        EnemyFrameId.YardHidingUpsideLeftMovingUpFirst,
        EnemyFrameId.YardHidingUpsideLeftMovingUpSecond,
        EnemyFrameId.YardHidingUpsideLeftMovingUpThird,
        EnemyFrameId.YardHidingUpsideLeftMovingUpFourth,
        EnemyFrameId.YardHidingUpsideLeftMovingDownFirst,
        EnemyFrameId.YardHidingUpsideLeftMovingDownSecond,
        EnemyFrameId.YardHidingUpsideLeftMovingDownThird,
        EnemyFrameId.YardHidingUpsideLeftMovingDownFourth,
        EnemyFrameId.YardHidingUpsideRightMovingDownFirst,
        EnemyFrameId.YardHidingUpsideRightMovingDownSecond,
        EnemyFrameId.YardHidingUpsideRightMovingDownThird,
        EnemyFrameId.YardHidingUpsideRightMovingDownFourth,
    ];

    /// <summary>The frames <c>Enemies/Zoa/ZoaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Zoa =
    [
        EnemyFrameId.ZoaFacingLeftShootingFirst,
        EnemyFrameId.ZoaFacingLeftShootingSecond,
        EnemyFrameId.ZoaFacingLeftShootingThird,
        EnemyFrameId.ZoaFacingLeftRisingFirst,
        EnemyFrameId.ZoaFacingLeftRisingSecond,
        EnemyFrameId.ZoaFacingLeftRisingThird,
        EnemyFrameId.ZoaFacingRightShootingFirst,
        EnemyFrameId.ZoaFacingRightShootingSecond,
        EnemyFrameId.ZoaFacingRightShootingThird,
        EnemyFrameId.ZoaFacingRightRisingFirst,
        EnemyFrameId.ZoaFacingRightRisingSecond,
        EnemyFrameId.ZoaFacingRightRisingThird,
    ];

    /// <summary>The frames each enemy definition can show.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>> Frames { get; } =
        new Dictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>>
        {
            [EnemyDefinitionId.CeresDoor] = CeresDoor,
            [EnemyDefinitionId.Steam] = CeresSteam,
            [EnemyDefinitionId.TourianStatue] = TourianEntranceStatue,
            [EnemyDefinitionId.Botwoon] = Botwoon,
            [EnemyDefinitionId.Crocomire] = CrocomireFrames,
            [EnemyDefinitionId.CrocomireTongue] = CrocomireTongueFrames,
            [EnemyDefinitionId.DraygonArms] = DraygonArmsFrames,
            [EnemyDefinitionId.DraygonTail] = DraygonTailFrames,
            [EnemyDefinitionId.MiniKraid] = FakeKraid,
            [EnemyDefinitionId.KraidArm] = KraidArmFrames,
            [EnemyDefinitionId.KraidFoot] = KraidFootFrames,
            [EnemyDefinitionId.KraidLintBottom] = KraidLintBottomFrames,
            [EnemyDefinitionId.KraidLintMiddle] = KraidLintBottomFrames,
            [EnemyDefinitionId.KraidLintTop] = KraidLintBottomFrames,
            [EnemyDefinitionId.KraidNail] = KraidNailFrames,
            [EnemyDefinitionId.KraidNailBad] = KraidNailFrames,
            [EnemyDefinitionId.BabyMetroidCutscene] = BabyMetroidCutscene,
            [EnemyDefinitionId.MotherBrainBody] = MotherBrainBodyFrames,
            [EnemyDefinitionId.MotherBrainHead] = MotherBrainHeadFrames,
            [EnemyDefinitionId.MotherBrainTubes] = MotherBrainTubesFrames,
            [EnemyDefinitionId.Rinka] = Rinka,
            [EnemyDefinitionId.BabyMetroid] = Shitroid,
            [EnemyDefinitionId.Zebetite] = Zebetite,
            [EnemyDefinitionId.PhantoonBody] = Phantoon,
            [EnemyDefinitionId.PhantoonEye] = Phantoon,
            [EnemyDefinitionId.PhantoonMouth] = Phantoon,
            [EnemyDefinitionId.PhantoonTentacles] = Phantoon,
            [EnemyDefinitionId.Ridley] = RidleyFrames,
            [EnemyDefinitionId.RidleyCeres] = RidleyCeresFrames,
            [EnemyDefinitionId.RidleyExplosion] = RidleyExplosionFrames,
            [EnemyDefinitionId.SporeSpawn] = SporeSpawn,
            [EnemyDefinitionId.BombTorizo] = BombTorizoFrames,
            [EnemyDefinitionId.GoldenTorizo] = GoldenTorizoFrames,
            [EnemyDefinitionId.Alcoon] = Alcoon,
            [EnemyDefinitionId.Dachora] = Dachora,
            [EnemyDefinitionId.EtecoonEscape] = EtecoonEscapeFrames,
            [EnemyDefinitionId.DachoraEscape] = DachoraEscapeFrames,
            [EnemyDefinitionId.Etecoon] = Etecoon,
            [EnemyDefinitionId.Atomic] = Atomic,
            [EnemyDefinitionId.Beetom] = Beetom,
            [EnemyDefinitionId.Boulder] = Boulder,
            [EnemyDefinitionId.Boyon] = Boyon,
            [EnemyDefinitionId.Bull] = Bull,
            [EnemyDefinitionId.Cacatac] = Cacatac,
            [EnemyDefinitionId.Choot] = Choot,
            [EnemyDefinitionId.Chozo] = ChozoFrames,
            [EnemyDefinitionId.NoobTubeCrack] = NoobTubeCrackFrames,
            [EnemyDefinitionId.CorpseSidehopper] = DeadSidehopper,
            [EnemyDefinitionId.CorpseTorizo] = DeadTorizo,
            [EnemyDefinitionId.CorpseRipper] = CorpseRipperFrames,
            [EnemyDefinitionId.CorpseSkree] = CorpseSkreeFrames,
            [EnemyDefinitionId.CorpseZoomer] = CorpseZoomerFrames,
            [EnemyDefinitionId.Sciser] = SciserFrames,
            [EnemyDefinitionId.Zero] = ZeroFrames,
            [EnemyDefinitionId.Viola] = ViolaFrames,
            [EnemyDefinitionId.Zeela] = ZeelaFrames,
            [EnemyDefinitionId.Sova] = ZeelaFrames,
            [EnemyDefinitionId.Zoomer] = ZeelaFrames,
            [EnemyDefinitionId.MZoomer] = ZeelaFrames,
            [EnemyDefinitionId.HZoomer] = ZeelaFrames,
            [EnemyDefinitionId.Multiviola] = Multiviola,
            [EnemyDefinitionId.Dragon] = Dragon,
            [EnemyDefinitionId.Elevator] = Elevator,
            [EnemyDefinitionId.Evir] = EvirFrames,
            [EnemyDefinitionId.EvirProjectile] = EvirProjectileFrames,
            [EnemyDefinitionId.FaceBlock] = BlueBrinstarFaceBlock,
            [EnemyDefinitionId.Fireflea] = Fireflea,
            [EnemyDefinitionId.Mella] = Flies,
            [EnemyDefinitionId.Mellow] = Flies,
            [EnemyDefinitionId.Menu] = Flies,
            [EnemyDefinitionId.Fune] = FuneFrames,
            [EnemyDefinitionId.Namihe] = NamiheFrames,
            [EnemyDefinitionId.ShipBottomEntrance] = ShipBottomEntranceFrames,
            [EnemyDefinitionId.ShipTop] = ShipTopFrames,
            [EnemyDefinitionId.Hibashi] = Hibashi,
            [EnemyDefinitionId.Dessgeega] = DessgeegaFrames,
            [EnemyDefinitionId.DessgeegaLarge] = DessgeegaLargeFrames,
            [EnemyDefinitionId.Sidehopper] = SidehopperFrames,
            [EnemyDefinitionId.SidehopperLarge] = SidehopperLargeFrames,
            [EnemyDefinitionId.SidehopperTourian] = SidehopperLargeFrames,
            [EnemyDefinitionId.Respawn] = InertEnemy,
            [EnemyDefinitionId.TourianStatueGhost] = InertEnemy,
            [EnemyDefinitionId.Kago] = Kago,
            [EnemyDefinitionId.KihunterGreen] = KihunterGreenFrames,
            [EnemyDefinitionId.KihunterGreenWings] = KihunterGreenWingsFrames,
            [EnemyDefinitionId.KihunterRed] = KihunterGreenFrames,
            [EnemyDefinitionId.KihunterRedWings] = KihunterGreenWingsFrames,
            [EnemyDefinitionId.KihunterYellow] = KihunterGreenFrames,
            [EnemyDefinitionId.KihunterYellowWings] = KihunterGreenWingsFrames,
            [EnemyDefinitionId.KzanBottom] = KzanBottomFrames,
            [EnemyDefinitionId.KzanTop] = KzanTopFrames,
            [EnemyDefinitionId.Squeept] = NorfairLavaJumpingEnemy,
            [EnemyDefinitionId.Magdollite] = Magdollite,
            [EnemyDefinitionId.BabyTurtle] = BabyTurtleFrames,
            [EnemyDefinitionId.MamaTurtle] = MamaTurtleFrames,
            [EnemyDefinitionId.Oum] = MaridiaLargeSnail,
            [EnemyDefinitionId.Metaree] = Metaree,
            [EnemyDefinitionId.Metroid] = Metroid,
            [EnemyDefinitionId.Mochtroid] = Mochtroid,
            [EnemyDefinitionId.Eye] = MorphBallEye,
            [EnemyDefinitionId.Puromi] = NuclearWaffle,
            [EnemyDefinitionId.Owtch] = Owtch,
            [EnemyDefinitionId.Gamet] = GametFrames,
            [EnemyDefinitionId.Geega] = GeegaFrames,
            [EnemyDefinitionId.Zeb] = ZebFrames,
            [EnemyDefinitionId.Zebbo] = ZebboFrames,
            [EnemyDefinitionId.Kamer2] = Kamer2Frames,
            [EnemyDefinitionId.Tripper] = TripperFrames,
            [EnemyDefinitionId.LavaRocks] = Polyp,
            [EnemyDefinitionId.Powamp] = Powamp,
            [EnemyDefinitionId.Puyo] = Puyo,
            [EnemyDefinitionId.Holtz] = LowerNorfairRio,
            [EnemyDefinitionId.Geruta] = NorfairRio,
            [EnemyDefinitionId.Rio] = Rio,
            [EnemyDefinitionId.GRipper] = GRipperFrames,
            [EnemyDefinitionId.Ripper] = RipperFrames,
            [EnemyDefinitionId.Ripper2] = GRipperFrames,
            [EnemyDefinitionId.Sbug] = Sbug,
            [EnemyDefinitionId.Sbug2] = Sbug,
            [EnemyDefinitionId.Shaktool] = Shaktool,
            [EnemyDefinitionId.Kamer] = KamerFrames,
            [EnemyDefinitionId.ShutterDestroyable] = ShutterDestroyableFrames,
            [EnemyDefinitionId.ShutterGrowing] = ShutterGrowingFrames,
            [EnemyDefinitionId.ShutterHorizShootable] = ShutterHorizShootableFrames,
            [EnemyDefinitionId.ShutterShootable] = ShutterDestroyableFrames,
            [EnemyDefinitionId.Skree] = Skree,
            [EnemyDefinitionId.Skultera] = Skultera,
            [EnemyDefinitionId.PirateGoldNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateGreenNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateGreyNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateMagentaNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateRedNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateSilverNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateGoldWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateGreenWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateGreyWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateMagentaWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateRedWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateSilverWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateGoldWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateGreenWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateGreyWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateMagentaWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateRedWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateSilverWall] = WallSpacePirate,
            [EnemyDefinitionId.Spark] = Spark,
            [EnemyDefinitionId.Stoke] = Stoke,
            [EnemyDefinitionId.Waver] = Waver,
            [EnemyDefinitionId.Robot] = RobotFrames,
            [EnemyDefinitionId.RobotNoPower] = RobotNoPowerFrames,
            [EnemyDefinitionId.Coven] = WreckedShipGhost,
            [EnemyDefinitionId.YappingMaw] = YappingMaw,
            [EnemyDefinitionId.Yard] = YardEnemy,
            [EnemyDefinitionId.Zoa] = Zoa,
        };
}
