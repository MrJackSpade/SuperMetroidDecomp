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
    /// <summary>The frames <c>Areas/Ceres/CeresDoorFactory.cs</c> shows for <c>CeresDoor</c>.</summary>
    private static readonly EnemyFrameId[] CeresDoorFrames =
    [
        EnemyFrameId.CeresDoorFacingLeftClosed,
        EnemyFrameId.CeresDoorFacingLeftQuarterOpen,
        EnemyFrameId.CeresDoorFacingLeftHalfOpen,
        EnemyFrameId.CeresDoorFacingLeftThreeQuartersOpen,
        EnemyFrameId.CeresDoorFacingLeftOpen,
        EnemyFrameId.CeresDoorFacingRightClosed,
        EnemyFrameId.CeresDoorFacingRightQuarterOpen,
        EnemyFrameId.CeresDoorFacingRightHalfOpen,
        EnemyFrameId.CeresDoorFacingRightThreeQuartersOpen,
        EnemyFrameId.CeresDoorFacingRightOpen,
    ];

    /// <summary>The frames <c>Areas/Ceres/CeresDoorFactory.cs</c> shows for <c>CeresRidleyRoomDoor</c>.</summary>
    private static readonly EnemyFrameId[] CeresRidleyRoomDoorFrames =
    [
        EnemyFrameId.CeresDoorFacingRightClosed,
        EnemyFrameId.CeresDoorFacingRightQuarterOpen,
        EnemyFrameId.CeresDoorFacingRightHalfOpen,
        EnemyFrameId.CeresDoorFacingRightThreeQuartersOpen,
        EnemyFrameId.CeresDoorFacingRightOpen,
    ];

    /// <summary>The frames <c>Areas/Ceres/CeresDoorFactory.cs</c> shows for <c>CeresElevatorRoomWall</c>.</summary>
    private static readonly EnemyFrameId[] CeresElevatorRoomWallFrames =
    [
        EnemyFrameId.CeresDoorFacingLeftClosed,
        EnemyFrameId.CeresDoorFacingLeftQuarterOpen,
        EnemyFrameId.CeresDoorFacingLeftHalfOpen,
        EnemyFrameId.CeresDoorFacingLeftThreeQuartersOpen,
        EnemyFrameId.CeresDoorFacingLeftOpen,
    ];

    /// <summary>The frames <c>Areas/Ceres/CeresDoorFactory.cs</c> shows for <c>CeresGetawayChamberWall</c>.</summary>
    private static readonly EnemyFrameId[] CeresGetawayChamberWallFrames =
    [
        EnemyFrameId.CeresGetawayChamberLeftWall,
        EnemyFrameId.CeresGetawayChamberRightWall,
    ];

    /// <summary>The frames <c>Areas/Ceres/CeresElevatorDoorFactory.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] CeresElevatorDoor =
    [
        EnemyFrameId.CeresElevatorDoorBlocked,
    ];

    /// <summary>The frames <c>Areas/Ceres/CeresSteamFactory.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] CeresSteam =
    [
        EnemyFrameId.CeresSteamUp,
        EnemyFrameId.CeresSteamUpJetBursting,
        EnemyFrameId.CeresSteamUpJetLengthening,
        EnemyFrameId.CeresSteamUpJetFull,
        EnemyFrameId.CeresSteamUpPlumeDrifting,
        EnemyFrameId.CeresSteamUpPlumeScattering,
        EnemyFrameId.CeresSteamUpPlumeFading,
        EnemyFrameId.CeresSteamLeft,
        EnemyFrameId.CeresSteamLeftJetBursting,
        EnemyFrameId.CeresSteamLeftJetLengthening,
        EnemyFrameId.CeresSteamLeftJetFull,
        EnemyFrameId.CeresSteamLeftPlumeDrifting,
        EnemyFrameId.CeresSteamLeftPlumeScattering,
        EnemyFrameId.CeresSteamLeftPlumeFading,
        EnemyFrameId.CeresSteamDown,
        EnemyFrameId.CeresSteamDownJetBursting,
        EnemyFrameId.CeresSteamDownJetLengthening,
        EnemyFrameId.CeresSteamDownJetFull,
        EnemyFrameId.CeresSteamDownPlumeDrifting,
        EnemyFrameId.CeresSteamDownPlumeScattering,
        EnemyFrameId.CeresSteamDownPlumeFading,
        EnemyFrameId.CeresSteamRight,
        EnemyFrameId.CeresSteamRightJetBursting,
        EnemyFrameId.CeresSteamRightJetLengthening,
        EnemyFrameId.CeresSteamRightJetFull,
        EnemyFrameId.CeresSteamRightPlumeDrifting,
        EnemyFrameId.CeresSteamRightPlumeScattering,
        EnemyFrameId.CeresSteamRightPlumeFading,
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
        EnemyFrameId.CrocomireBodyNearFootForwardArmBent,
        EnemyFrameId.CrocomireBodyNearFootForwardArmSquare,
        EnemyFrameId.CrocomireBodyNearFootForwardArmReaching,
        EnemyFrameId.CrocomireBodyNearFootForwardArmSweeping,
        EnemyFrameId.CrocomireBodyNearFootForwardArmLifting,
        EnemyFrameId.CrocomireBodyNearFootForwardArmUpright,
        EnemyFrameId.CrocomireBodyFarFootForwardArmBent,
        EnemyFrameId.CrocomireBodyFarFootForwardArmSquare,
        EnemyFrameId.CrocomireBodyFarFootForwardArmReaching,
        EnemyFrameId.CrocomireBodyFarFootForwardArmSweeping,
        EnemyFrameId.CrocomireBodyFarFootForwardArmLifting,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUpright,
        EnemyFrameId.CrocomireBodyArmUprightMouthOpen,
        EnemyFrameId.CrocomireBodyArmUprightMouthOpenLess,
        EnemyFrameId.CrocomireBodyArmUprightMouthClosing,
        EnemyFrameId.CrocomireBodyArmUprightMouthHalfClosed,
        EnemyFrameId.CrocomireBodyArmUprightMouthMostlyClosed,
        EnemyFrameId.CrocomireBodyArmUprightMouthNearlyShut,
        EnemyFrameId.CrocomireBodyArmUprightMouthShut,
        EnemyFrameId.CrocomireBodySwingingArmBent,
        EnemyFrameId.CrocomireBodySwingingArmRaised,
        EnemyFrameId.CrocomireBodySwingingArmOutstretched,
        EnemyFrameId.CrocomireBodySwingingArmLowering,
        EnemyFrameId.CrocomireBodySwingingArmReturning,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShut,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthParted,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthGaping,
        EnemyFrameId.CrocomireBodyNearFootForwardArmBentMouthShut,
        EnemyFrameId.CrocomireBodyNearFootForwardArmSweepingMouthGaping,
        EnemyFrameId.CrocomireBodyNearFootForwardArmLiftingMouthGaping,
        EnemyFrameId.CrocomireBodyNearFootForwardArmUprightMouthGaping,
        EnemyFrameId.CrocomireBodyFarFootForwardArmBentMouthGaping,
        EnemyFrameId.CrocomireBodyFarFootForwardArmSquareMouthGaping,
        EnemyFrameId.CrocomireBodyFarFootForwardArmReachingMouthGaping,
        EnemyFrameId.CrocomireBodyFarFootForwardArmSweepingMouthGaping,
        EnemyFrameId.CrocomireBodyFarFootForwardArmLiftingMouthParted,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShutting,
        EnemyFrameId.CrocomireBodyLungingArmBent,
        EnemyFrameId.CrocomireBodyLungingArmRaised,
        EnemyFrameId.CrocomireBodyLungingArmOutstretched,
        EnemyFrameId.CrocomireBodyLungingArmLowering,
        EnemyFrameId.CrocomireBodyLungingArmReturning,
        EnemyFrameId.CrocomireBodyMeltingFleshTopRow,
        EnemyFrameId.CrocomireBodyMeltingFleshTopTwoRows,
        EnemyFrameId.CrocomireBodyMeltingFleshTopThreeRows,
        EnemyFrameId.CrocomireBodyMeltingFleshTopFourRows,
        EnemyFrameId.CrocomireBodyMeltingBareSkullTopRow,
        EnemyFrameId.CrocomireBodyMeltingBareSkullTopTwoRows,
        EnemyFrameId.CrocomireBodyMeltingBareSkullTopThreeRows,
        EnemyFrameId.CrocomireBodyMeltingBareSkullTopFourRows,
        EnemyFrameId.CrocomireBodySkeletonToppling,
        EnemyFrameId.CrocomireBodySkeletonLeaning,
        EnemyFrameId.CrocomireBodySkeletonTipping,
        EnemyFrameId.CrocomireBodySkeletonArmHeldLevel,
        EnemyFrameId.CrocomireBodySkeletonArmDropping,
        EnemyFrameId.CrocomireBodySkeletonArmLowered,
        EnemyFrameId.CrocomireBodySkeletonArmPlanted,
        EnemyFrameId.CrocomireBodySkeletonBodySettling,
        EnemyFrameId.CrocomireBodySkeletonArmSplaying,
        EnemyFrameId.CrocomireBodySkeletonArmFlat,
        EnemyFrameId.CrocomireBodySkeletonRibsSagging,
        EnemyFrameId.CrocomireBodySkeletonRibsSlumped,
        EnemyFrameId.CrocomireBodySkeletonArmFallenOff,
        EnemyFrameId.CrocomireBodySkeletonTailScattering,
        EnemyFrameId.CrocomireBodySkeletonSpineFallingApart,
        EnemyFrameId.CrocomireBodySkeletonRibsSplaying,
        EnemyFrameId.CrocomireBodySkeletonRibsFlattened,
        EnemyFrameId.CrocomireBodySkeletonBonesGathering,
        EnemyFrameId.CrocomireBodySkeletonBonesHuddled,
        EnemyFrameId.CrocomireBodySkeletonRibsBesideSkull,
        EnemyFrameId.CrocomireBodySkeletonLastBones,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBubblesForming,
        EnemyFrameId.CrocomireBubblesWelling,
        EnemyFrameId.CrocomireBubblesSpreading,
        EnemyFrameId.CrocomireBubblesBursting,
        EnemyFrameId.CrocomireBubblesSpraying,
        EnemyFrameId.CrocomireBubblesScattered,
        EnemyFrameId.CrocomireBubblesThinning,
        EnemyFrameId.CrocomireBubblesFading,
        EnemyFrameId.CrocomireBubblesVanishing,
        EnemyFrameId.CrocomireBubblesAdrift,
    ];

    /// <summary>The frames <c>Bosses/Crocomire/CrocomireBehavior.cs</c> shows for <c>CrocomireTongue</c>.</summary>
    private static readonly EnemyFrameId[] CrocomireTongueFrames =
    [
        EnemyFrameId.CrocomireTongueCurlingDown,
        EnemyFrameId.CrocomireTongueFlatRaised,
        EnemyFrameId.CrocomireTongueCurlingUp,
        EnemyFrameId.CrocomireTongueFlat,
        EnemyFrameId.CrocomireArmAloneBent,
        EnemyFrameId.CrocomireArmAloneRaised,
        EnemyFrameId.CrocomireArmAloneOutstretched,
        EnemyFrameId.CrocomireArmAloneLowering,
        EnemyFrameId.CrocomireArmAloneReturning,
    ];

    /// <summary>The frames <c>Bosses/Draygon/DraygonBehavior.cs</c> shows for <c>DraygonArms</c>.</summary>
    private static readonly EnemyFrameId[] DraygonArmsFrames =
    [
        EnemyFrameId.DraygonArmsFacingLeftForward,
        EnemyFrameId.DraygonArmsFacingLeftLowering,
        EnemyFrameId.DraygonArmsFacingLeftDown,
        EnemyFrameId.DraygonArmsFacingLeftDownStretched,
        EnemyFrameId.DraygonArmsFacingLeftRaising,
        EnemyFrameId.DraygonArmsFacingLeftReaching,
        EnemyFrameId.DraygonArmsFacingLeftSpreadWide,
        EnemyFrameId.DraygonArmsFacingLeftClosing,
        EnemyFrameId.DraygonArmsFacingLeftGathering,
        EnemyFrameId.DraygonArmsFacingLeftTucked,
        EnemyFrameId.DraygonArmsFacingRightForward,
        EnemyFrameId.DraygonArmsFacingRightLowering,
        EnemyFrameId.DraygonArmsFacingRightDown,
        EnemyFrameId.DraygonArmsFacingRightDownStretched,
        EnemyFrameId.DraygonArmsFacingRightRaising,
        EnemyFrameId.DraygonArmsFacingRightReaching,
        EnemyFrameId.DraygonArmsFacingRightSpreadWide,
        EnemyFrameId.DraygonArmsFacingRightClosing,
        EnemyFrameId.DraygonArmsFacingRightGathering,
        EnemyFrameId.DraygonArmsFacingRightTucked,
    ];

    /// <summary>The frames <c>Bosses/Draygon/DraygonBehavior.cs</c> shows for <c>DraygonTail</c>.</summary>
    private static readonly EnemyFrameId[] DraygonTailFrames =
    [
        EnemyFrameId.DraygonTailFacingLeftTipCurledIn,
        EnemyFrameId.DraygonTailFacingLeftTipSwingingOut,
        EnemyFrameId.DraygonTailFacingLeftTipOut,
        EnemyFrameId.DraygonTailFacingLeftTipFurtherOut,
        EnemyFrameId.DraygonTailFacingLeftTipFarthestOut,
        EnemyFrameId.DraygonTailFacingLeftTipReturning,
        EnemyFrameId.DraygonTailFacingLeftTipBack,
        EnemyFrameId.DraygonTailFacingLeftWhipCocking,
        EnemyFrameId.DraygonTailFacingLeftWhipRaising,
        EnemyFrameId.DraygonTailFacingLeftWhipHangingLong,
        EnemyFrameId.DraygonTailFacingLeftWhipSwinging,
        EnemyFrameId.DraygonTailFacingLeftWhipLashedOut,
        EnemyFrameId.DraygonTailFacingLeftWhipCurling,
        EnemyFrameId.DraygonTailFacingLeftWhipRecoiling,
        EnemyFrameId.DraygonTailFacingRightTipCurledIn,
        EnemyFrameId.DraygonTailFacingRightTipSwingingOut,
        EnemyFrameId.DraygonTailFacingRightTipOut,
        EnemyFrameId.DraygonTailFacingRightTipFurtherOut,
        EnemyFrameId.DraygonTailFacingRightTipFarthestOut,
        EnemyFrameId.DraygonTailFacingRightTipReturning,
        EnemyFrameId.DraygonTailFacingRightTipBack,
        EnemyFrameId.DraygonTailFacingRightWhipCocking,
        EnemyFrameId.DraygonTailFacingRightWhipRaising,
        EnemyFrameId.DraygonTailFacingRightWhipHangingLong,
        EnemyFrameId.DraygonTailFacingRightWhipSwinging,
        EnemyFrameId.DraygonTailFacingRightWhipLashedOut,
        EnemyFrameId.DraygonTailFacingRightWhipCurling,
        EnemyFrameId.DraygonTailFacingRightWhipRecoiling,
    ];

    /// <summary>The frames <c>Bosses/Kraid/FakeKraidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] FakeKraid =
    [
        EnemyFrameId.FakeKraidSteppingFacingLeftFeetTogether,
        EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootLifting,
        EnemyFrameId.FakeKraidSteppingFacingLeftFrontFootForward,
        EnemyFrameId.FakeKraidSteppingFacingLeftFeetPlanted,
        EnemyFrameId.FakeKraidFacingLeftMouthClosed,
        EnemyFrameId.FakeKraidFacingLeftMouthOpening,
        EnemyFrameId.FakeKraidFacingLeftMouthWide,
        EnemyFrameId.FakeKraidSteppingFacingRightFeetTogether,
        EnemyFrameId.FakeKraidSteppingFacingRightFrontFootLifting,
        EnemyFrameId.FakeKraidSteppingFacingRightFrontFootForward,
        EnemyFrameId.FakeKraidSteppingFacingRightFeetPlanted,
        EnemyFrameId.FakeKraidFacingRightMouthClosed,
        EnemyFrameId.FakeKraidFacingRightMouthOpening,
        EnemyFrameId.FakeKraidFacingRightMouthWide,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidArm</c>.</summary>
    private static readonly EnemyFrameId[] KraidArmFrames =
    [
        EnemyFrameId.KraidArmRaisedClawClosed,
        EnemyFrameId.KraidArmRaisedClawOpen,
        EnemyFrameId.KraidArmLoweringClawHooked,
        EnemyFrameId.KraidArmLevelClawHooked,
        EnemyFrameId.KraidArmLevelClawHangingDown,
        EnemyFrameId.KraidArmLevelClawCurlingUnder,
        EnemyFrameId.KraidArmLevelClawTucked,
        EnemyFrameId.KraidArmLevelClawTuckedTight,
        EnemyFrameId.KraidArmLevelClawFolded,
        EnemyFrameId.KraidArmLevelClawFoldedFlat,
        EnemyFrameId.KraidArmDroopingDiagonal,
        EnemyFrameId.KraidArmOutstretched,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidFoot</c>.</summary>
    private static readonly EnemyFrameId[] KraidFootFrames =
    [
        EnemyFrameId.KraidFootPlanted,
        EnemyFrameId.KraidFootLifting,
        EnemyFrameId.KraidFootRaised,
        EnemyFrameId.KraidFootSwinging,
        EnemyFrameId.KraidFootReaching,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidLintBottom</c>, <c>KraidLintMiddle</c>, <c>KraidLintTop</c>.</summary>
    private static readonly EnemyFrameId[] KraidLintBottomFrames =
    [
        EnemyFrameId.KraidLintBudding,
        EnemyFrameId.KraidLintGrown,
    ];

    /// <summary>The frames <c>Bosses/Kraid/KraidBehavior.cs</c> shows for <c>KraidFirstNail</c>, <c>KraidSecondNail</c>.</summary>
    private static readonly EnemyFrameId[] KraidFirstNailFrames =
    [
        EnemyFrameId.KraidNailTurned0,
        EnemyFrameId.KraidNailTurned45,
        EnemyFrameId.KraidNailTurned90,
        EnemyFrameId.KraidNailTurned135,
        EnemyFrameId.KraidNailTurned180,
        EnemyFrameId.KraidNailTurned225,
        EnemyFrameId.KraidNailTurned270,
        EnemyFrameId.KraidNailTurned315,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/BabyMetroidCutsceneBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] BabyMetroidCutscene =
    [
        EnemyFrameId.BabyMetroidNucleiBright,
        EnemyFrameId.BabyMetroidNucleiGlowing,
        EnemyFrameId.BabyMetroidNucleiDark,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/MotherBrainBehavior.cs</c> shows for <c>MotherBrainBody</c>.</summary>
    private static readonly EnemyFrameId[] MotherBrainBodyFrames =
    [
        EnemyFrameId.MotherBrainBodyStanding,
        EnemyFrameId.MotherBrainBodyWalkingNearLegLifting,
        EnemyFrameId.MotherBrainBodyWalkingNearLegReaching,
        EnemyFrameId.MotherBrainBodyWalkingNearLegPlanting,
        EnemyFrameId.MotherBrainBodyWalkingLegsApart,
        EnemyFrameId.MotherBrainBodyWalkingFarLegLifting,
        EnemyFrameId.MotherBrainBodyWalkingFarLegPassing,
        EnemyFrameId.MotherBrainBodyWalkingFarLegPlanting,
        EnemyFrameId.MotherBrainBodyWalkingLegsTogether,
        EnemyFrameId.MotherBrainBodyCrouched,
        EnemyFrameId.MotherBrainBodyUncrouching,
        EnemyFrameId.MotherBrainBodyLeaningDown,
        EnemyFrameId.MotherBrainBrainHitArea,
        EnemyFrameId.MotherBrainBodyHandCurling,
        EnemyFrameId.MotherBrainBodyArmDrawnBack,
        EnemyFrameId.MotherBrainBodyArmFlungOut,
        EnemyFrameId.MotherBrainBodyArmReturning,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/MotherBrainBehavior.cs</c> shows for <c>MotherBrainHead</c>.</summary>
    private static readonly EnemyFrameId[] MotherBrainHeadFrames =
    [
        EnemyFrameId.MotherBrainHeadEyeShut,
        EnemyFrameId.MotherBrainHeadEyeOpen,
        EnemyFrameId.MotherBrainHeadEyeGlaring,
        EnemyFrameId.MotherBrainHeadJawDropping,
        EnemyFrameId.MotherBrainHeadJawDropped,
        EnemyFrameId.MotherBrainNeckJoint,
        EnemyFrameId.MotherBrainDarkHeadEyeShut,
        EnemyFrameId.MotherBrainDarkHeadEyeOpen,
        EnemyFrameId.MotherBrainDarkHeadEyeGlaring,
        EnemyFrameId.MotherBrainDarkHeadJawDropping,
        EnemyFrameId.MotherBrainDarkHeadJawDropped,
        EnemyFrameId.MotherBrainHeadCorpseSlumping,
        EnemyFrameId.MotherBrainHeadCorpseSlumped,
        EnemyFrameId.MotherBrainBrainHitArea,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/MotherBrainBehavior.cs</c> shows for <c>MotherBrainTubes</c>.</summary>
    private static readonly EnemyFrameId[] MotherBrainTubesFrames =
    [
        EnemyFrameId.MotherBrainTubeBendRight,
        EnemyFrameId.MotherBrainTubeBendLeft,
        EnemyFrameId.MotherBrainTubeStraightLeft,
        EnemyFrameId.MotherBrainTubeStraightRight,
        EnemyFrameId.MotherBrainTubeColumn,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/RinkaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Rinka =
    [
        EnemyFrameId.RinkaWhiteHot,
        EnemyFrameId.RinkaYellow,
        EnemyFrameId.RinkaOrange,
        EnemyFrameId.RinkaRed,
        EnemyFrameId.RinkaDarkRed,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/BabyMetroidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] BabyMetroid =
    [
        EnemyFrameId.BabyMetroidNucleiBright,
        EnemyFrameId.BabyMetroidNucleiGlowing,
        EnemyFrameId.BabyMetroidNucleiDark,
    ];

    /// <summary>The frames <c>Bosses/MotherBrain/ZebetiteBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Zebetite =
    [
        EnemyFrameId.ZebetiteTallIntact,
        EnemyFrameId.ZebetiteTallDrained,
        EnemyFrameId.ZebetiteTallHalfDrained,
        EnemyFrameId.ZebetiteTallMostlyDrained,
        EnemyFrameId.ZebetiteTallNearlyDrained,
        EnemyFrameId.ZebetiteShortIntact,
        EnemyFrameId.ZebetiteShortDrained,
        EnemyFrameId.ZebetiteShortHalfDrained,
        EnemyFrameId.ZebetiteShortMostlyDrained,
        EnemyFrameId.ZebetiteShortNearlyDrained,
    ];

    /// <summary>The frames <c>Bosses/Phantoon/PhantoonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Phantoon =
    [
        EnemyFrameId.PhantoonInvulnerableBody,
        EnemyFrameId.PhantoonFullHitboxBody,
        EnemyFrameId.PhantoonEyeHitboxBody,
        EnemyFrameId.PhantoonEyeClosed,
        EnemyFrameId.PhantoonEyeHalfOpen,
        EnemyFrameId.PhantoonEyeballCentered,
        EnemyFrameId.PhantoonEyeLookingUp,
        EnemyFrameId.PhantoonEyeLookingDown,
        EnemyFrameId.PhantoonEyeLookingLeft,
        EnemyFrameId.PhantoonEyeLookingRight,
        EnemyFrameId.PhantoonEyeLookingDownLeft,
        EnemyFrameId.PhantoonEyeLookingDownRight,
        EnemyFrameId.PhantoonEyeLookingUpLeft,
        EnemyFrameId.PhantoonEyeLookingUpRight,
        EnemyFrameId.PhantoonTentaclesNarrow,
        EnemyFrameId.PhantoonTentaclesSpreading,
        EnemyFrameId.PhantoonTentaclesSpread,
        EnemyFrameId.PhantoonMouthOpen,
        EnemyFrameId.PhantoonMouthHalfOpen,
        EnemyFrameId.PhantoonMouthNarrow,
    ];

    /// <summary>The frames <c>Bosses/Ridley/RidleyEntityFactory.cs</c> shows for <c>Ridley</c>.</summary>
    private static readonly EnemyFrameId[] RidleyFrames =
    [
        EnemyFrameId.RidleyTailBaseLink,
        EnemyFrameId.RidleyTailMiddleLink,
        EnemyFrameId.RidleyTailOuterLink,
        EnemyFrameId.RidleyTailTipPointingToward270,
        EnemyFrameId.RidleyTailTipPointingToward292,
        EnemyFrameId.RidleyTailTipPointingToward315,
        EnemyFrameId.RidleyTailTipPointingToward337,
        EnemyFrameId.RidleyTailTipPointingToward0,
        EnemyFrameId.RidleyTailTipPointingToward22,
        EnemyFrameId.RidleyTailTipPointingToward45,
        EnemyFrameId.RidleyTailTipPointingToward67,
        EnemyFrameId.RidleyTailTipPointingToward90,
        EnemyFrameId.RidleyTailTipPointingToward112,
        EnemyFrameId.RidleyTailTipPointingToward135,
        EnemyFrameId.RidleyTailTipPointingToward157,
        EnemyFrameId.RidleyTailTipPointingToward180,
        EnemyFrameId.RidleyTailTipPointingToward202,
        EnemyFrameId.RidleyTailTipPointingToward225,
        EnemyFrameId.RidleyTailTipPointingToward247,
        EnemyFrameId.RidleyWingFacingLeftRaised,
        EnemyFrameId.RidleyWingFacingLeftLowering,
        EnemyFrameId.RidleyWingFacingLeftEdgeOn,
        EnemyFrameId.RidleyWingFacingLeftEdgeOnTilted,
        EnemyFrameId.RidleyWingFacingLeftDipping,
        EnemyFrameId.RidleyWingFacingLeftDown,
        EnemyFrameId.RidleyWingFacingRightRaised,
        EnemyFrameId.RidleyWingFacingRightLowering,
        EnemyFrameId.RidleyWingFacingRightEdgeOn,
        EnemyFrameId.RidleyWingFacingRightEdgeOnTilted,
        EnemyFrameId.RidleyWingFacingRightDipping,
        EnemyFrameId.RidleyWingFacingRightDown,
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
        EnemyFrameId.RidleyTailBaseLink,
        EnemyFrameId.RidleyTailMiddleLink,
        EnemyFrameId.RidleyTailOuterLink,
        EnemyFrameId.RidleyTailTipPointingToward270,
        EnemyFrameId.RidleyTailTipPointingToward292,
        EnemyFrameId.RidleyTailTipPointingToward315,
        EnemyFrameId.RidleyTailTipPointingToward337,
        EnemyFrameId.RidleyTailTipPointingToward0,
        EnemyFrameId.RidleyTailTipPointingToward22,
        EnemyFrameId.RidleyTailTipPointingToward45,
        EnemyFrameId.RidleyTailTipPointingToward67,
        EnemyFrameId.RidleyTailTipPointingToward90,
        EnemyFrameId.RidleyTailTipPointingToward112,
        EnemyFrameId.RidleyTailTipPointingToward135,
        EnemyFrameId.RidleyTailTipPointingToward157,
        EnemyFrameId.RidleyTailTipPointingToward180,
        EnemyFrameId.RidleyTailTipPointingToward202,
        EnemyFrameId.RidleyTailTipPointingToward225,
        EnemyFrameId.RidleyTailTipPointingToward247,
        EnemyFrameId.RidleyWingFacingLeftRaised,
        EnemyFrameId.RidleyWingFacingLeftLowering,
        EnemyFrameId.RidleyWingFacingLeftEdgeOn,
        EnemyFrameId.RidleyWingFacingLeftEdgeOnTilted,
        EnemyFrameId.RidleyWingFacingLeftDipping,
        EnemyFrameId.RidleyWingFacingLeftDown,
        EnemyFrameId.RidleyWingFacingRightRaised,
        EnemyFrameId.RidleyWingFacingRightLowering,
        EnemyFrameId.RidleyWingFacingRightEdgeOn,
        EnemyFrameId.RidleyWingFacingRightEdgeOnTilted,
        EnemyFrameId.RidleyWingFacingRightDipping,
        EnemyFrameId.RidleyWingFacingRightDown,
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

    /// <summary>The frames <c>Bosses/Ridley/RidleyEntityFactory.cs</c> shows for <c>RidleyBreakupPiece</c>.</summary>
    private static readonly EnemyFrameId[] RidleyBreakupPieceFrames =
    [
        EnemyFrameId.RidleyTailBaseLink,
        EnemyFrameId.RidleyTailMiddleLink,
        EnemyFrameId.RidleyTailOuterLink,
        EnemyFrameId.RidleyTailTipPointingToward270,
        EnemyFrameId.RidleyTailTipPointingToward292,
        EnemyFrameId.RidleyTailTipPointingToward315,
        EnemyFrameId.RidleyTailTipPointingToward337,
        EnemyFrameId.RidleyTailTipPointingToward0,
        EnemyFrameId.RidleyTailTipPointingToward22,
        EnemyFrameId.RidleyTailTipPointingToward45,
        EnemyFrameId.RidleyTailTipPointingToward67,
        EnemyFrameId.RidleyTailTipPointingToward90,
        EnemyFrameId.RidleyTailTipPointingToward112,
        EnemyFrameId.RidleyTailTipPointingToward135,
        EnemyFrameId.RidleyTailTipPointingToward157,
        EnemyFrameId.RidleyTailTipPointingToward180,
        EnemyFrameId.RidleyTailTipPointingToward202,
        EnemyFrameId.RidleyTailTipPointingToward225,
        EnemyFrameId.RidleyTailTipPointingToward247,
        EnemyFrameId.RidleyWingFacingLeftRaised,
        EnemyFrameId.RidleyWingFacingRightRaised,
        EnemyFrameId.RidleyBreakupLegsLeft,
        EnemyFrameId.RidleyBreakupLegsRight,
        EnemyFrameId.RidleyBreakupOpenHeadAndNeckLeft,
        EnemyFrameId.RidleyBreakupOpenHeadAndNeckRight,
        EnemyFrameId.RidleyBreakupTorsoLeft,
        EnemyFrameId.RidleyBreakupTorsoRight,
        EnemyFrameId.RidleyBreakupClawLeft,
        EnemyFrameId.RidleyBreakupClawRight,
    ];

    /// <summary>The frames <c>Bosses/SporeSpawn/SporeSpawnBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] SporeSpawn =
    [
        EnemyFrameId.SporeSpawnHusk,
        EnemyFrameId.SporeSpawnShut,
        EnemyFrameId.SporeSpawnCracked,
        EnemyFrameId.SporeSpawnAjar,
        EnemyFrameId.SporeSpawnParting,
        EnemyFrameId.SporeSpawnHalfOpen,
        EnemyFrameId.SporeSpawnWideOpening,
        EnemyFrameId.SporeSpawnNearlyOpen,
        EnemyFrameId.SporeSpawnOpen,
        EnemyFrameId.SporeSpawnOpenCoreGlowing,
        EnemyFrameId.SporeSpawnOpenCoreBright,
    ];

    /// <summary>The frames <c>Bosses/Torizo/TorizoBehavior.cs</c> shows for <c>BombTorizo</c>.</summary>
    private static readonly EnemyFrameId[] BombTorizoFrames =
    [
        EnemyFrameId.BombTorizoDormant,
        EnemyFrameId.TorizoFacingLeftHalfRisen,
        EnemyFrameId.TorizoFacingLeftCrouched,
        EnemyFrameId.TorizoFacingLeftRisingFromSeat,
        EnemyFrameId.TorizoFacingLeftSeated,
        EnemyFrameId.TorizoFacingLeftNearlyUpright,
        EnemyFrameId.TorizoFacingLeftUprightStepping,
        EnemyFrameId.TorizoFacingLeftUprightWideStance,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted,
        EnemyFrameId.TorizoFacingViewer,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegLifting,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegSwinging,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegReaching,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanting,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanted,
        EnemyFrameId.TorizoFacingRightWalkingRightLegLifting,
        EnemyFrameId.TorizoFacingRightWalkingRightLegSwinging,
        EnemyFrameId.TorizoFacingRightWalkingRightLegReaching,
        EnemyFrameId.TorizoFacingRightWalkingRightLegPlanting,
        EnemyFrameId.TorizoFacingRightWalkingRightLegPlanted,
        EnemyFrameId.TorizoFacingRightJumpCrouch,
        EnemyFrameId.TorizoFacingRightSpringing,
        EnemyFrameId.TorizoFacingRightLeaping,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmOverhead,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmOverhead,
        EnemyFrameId.TorizoFacingLeftSpringing,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingLeftJumpCrouch,
        EnemyFrameId.TorizoFacingLeftLeaping,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmOverhead,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmOverhead,
    ];

    /// <summary>The frames <c>Bosses/Torizo/TorizoBehavior.cs</c> shows for <c>GoldenTorizo</c>.</summary>
    private static readonly EnemyFrameId[] GoldenTorizoFrames =
    [
        EnemyFrameId.TorizoFacingLeftHalfRisen,
        EnemyFrameId.TorizoFacingLeftCrouched,
        EnemyFrameId.TorizoFacingLeftRisingFromSeat,
        EnemyFrameId.TorizoFacingLeftSeated,
        EnemyFrameId.TorizoFacingLeftNearlyUpright,
        EnemyFrameId.TorizoFacingLeftUprightStepping,
        EnemyFrameId.TorizoFacingLeftUprightWideStance,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting,
        EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting,
        EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted,
        EnemyFrameId.TorizoFacingViewer,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegLifting,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegSwinging,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegReaching,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanting,
        EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanted,
        EnemyFrameId.TorizoFacingRightWalkingRightLegLifting,
        EnemyFrameId.TorizoFacingRightWalkingRightLegSwinging,
        EnemyFrameId.TorizoFacingRightWalkingRightLegReaching,
        EnemyFrameId.TorizoFacingRightWalkingRightLegPlanting,
        EnemyFrameId.TorizoFacingRightWalkingRightLegPlanted,
        EnemyFrameId.TorizoFacingRightJumpCrouch,
        EnemyFrameId.TorizoFacingRightSpringing,
        EnemyFrameId.TorizoFacingRightLeaping,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardFarArmOverhead,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingRightRightFootForwardFarArmOverhead,
        EnemyFrameId.TorizoFacingLeftSpringing,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingLeftJumpCrouch,
        EnemyFrameId.TorizoFacingLeftLeaping,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadLowered,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardSpewingArmDrawnBack,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingLeftRightFootForwardFarArmOverhead,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmDown,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLifting,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmLevel,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardNearArmOverhead,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmDown,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLifting,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmLevel,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmRaised,
        EnemyFrameId.TorizoFacingLeftLeftFootForwardFarArmOverhead,
        EnemyFrameId.TorizoFacingRightSeated,
        EnemyFrameId.TorizoFacingRightRisingFromSeat,
        EnemyFrameId.TorizoFacingRightCrouched,
        EnemyFrameId.TorizoFacingRightHalfRisen,
        EnemyFrameId.TorizoFacingRightNearlyUpright,
        EnemyFrameId.TorizoFacingRightUprightStepping,
        EnemyFrameId.TorizoFacingRightUprightWideStance,
    ];

    /// <summary>The frames <c>Enemies/Alcoon/AlcoonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Alcoon =
    [
        EnemyFrameId.AlcoonWalkingLeftNearLegStepping,
        EnemyFrameId.AlcoonWalkingLeftNearLegPlanted,
        EnemyFrameId.AlcoonWalkingLeftFarLegStepping,
        EnemyFrameId.AlcoonWalkingLeftFarLegPlanted,
        EnemyFrameId.AlcoonFacingLeftMouthClosed,
        EnemyFrameId.AlcoonFacingLeftMouthOpening,
        EnemyFrameId.AlcoonFacingLeftThroatGlowing,
        EnemyFrameId.AlcoonFacingLeftMouthWide,
        EnemyFrameId.AlcoonLeapingFacingLeft,
        EnemyFrameId.AlcoonWalkingRightNearLegStepping,
        EnemyFrameId.AlcoonWalkingRightNearLegPlanted,
        EnemyFrameId.AlcoonWalkingRightFarLegStepping,
        EnemyFrameId.AlcoonWalkingRightFarLegPlanted,
        EnemyFrameId.AlcoonFacingRightMouthClosed,
        EnemyFrameId.AlcoonFacingRightMouthOpening,
        EnemyFrameId.AlcoonFacingRightThroatGlowing,
        EnemyFrameId.AlcoonFacingRightMouthWide,
        EnemyFrameId.AlcoonLeapingFacingRight,
    ];

    /// <summary>The frames <c>Enemies/Animals/DachoraBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Dachora =
    [
        EnemyFrameId.DachoraRunningLeftFarLegBack,
        EnemyFrameId.DachoraRunningLeftFarLegPassing,
        EnemyFrameId.DachoraRunningLeftFarLegReaching,
        EnemyFrameId.DachoraRunningLeftNearLegBack,
        EnemyFrameId.DachoraRunningLeftNearLegPassing,
        EnemyFrameId.DachoraRunningLeftNearLegReaching,
        EnemyFrameId.DachoraStandingFacingLeftHeadUp,
        EnemyFrameId.DachoraStandingFacingLeftNeckStretched,
        EnemyFrameId.DachoraStandingFacingLeftHeadLowered,
        EnemyFrameId.DachoraStandingFacingLeftHeadToGround,
        EnemyFrameId.DachoraStandingFacingLeftHeadRaising,
        EnemyFrameId.DachoraStandingFacingLeftEyeHalfShut,
        EnemyFrameId.DachoraStandingFacingLeftEyeShut,
        EnemyFrameId.DachoraStretchedUpFacingLeft,
        EnemyFrameId.DachoraRunningRightFarLegBack,
        EnemyFrameId.DachoraRunningRightFarLegPassing,
        EnemyFrameId.DachoraRunningRightFarLegReaching,
        EnemyFrameId.DachoraRunningRightNearLegBack,
        EnemyFrameId.DachoraRunningRightNearLegPassing,
        EnemyFrameId.DachoraRunningRightNearLegReaching,
        EnemyFrameId.DachoraStandingFacingRightHeadUp,
        EnemyFrameId.DachoraStandingFacingRightNeckStretched,
        EnemyFrameId.DachoraStandingFacingRightHeadLowered,
        EnemyFrameId.DachoraStandingFacingRightHeadToGround,
        EnemyFrameId.DachoraStandingFacingRightHeadRaising,
        EnemyFrameId.DachoraStandingFacingRightEyeHalfShut,
        EnemyFrameId.DachoraStandingFacingRightEyeShut,
        EnemyFrameId.DachoraCrouchedFacingRight,
        EnemyFrameId.DachoraStretchedUpFacingRight,
    ];

    /// <summary>The frames <c>Enemies/Animals/EscapeAnimalsBehavior.cs</c> shows for <c>EtecoonEscape</c>.</summary>
    private static readonly EnemyFrameId[] EtecoonEscapeFrames =
    [
        EnemyFrameId.EtecoonRunningLeftStretched,
        EnemyFrameId.EtecoonRunningLeftBunched,
        EnemyFrameId.EtecoonRunningLeftPushingOff,
        EnemyFrameId.EtecoonRunningRightStretched,
        EnemyFrameId.EtecoonRunningRightBunched,
        EnemyFrameId.EtecoonRunningRightPushingOff,
        EnemyFrameId.EscapeEtecoonLookingRight,
        EnemyFrameId.EscapeEtecoonTurningLeft,
        EnemyFrameId.EscapeEtecoonLookingLeft,
        EnemyFrameId.EscapeEtecoonTurningRight,
    ];

    /// <summary>The frames <c>Enemies/Animals/EscapeAnimalsBehavior.cs</c> shows for <c>DachoraEscape</c>.</summary>
    private static readonly EnemyFrameId[] DachoraEscapeFrames =
    [
        EnemyFrameId.DachoraRunningLeftFarLegBack,
        EnemyFrameId.DachoraRunningLeftFarLegPassing,
        EnemyFrameId.DachoraRunningLeftFarLegReaching,
        EnemyFrameId.DachoraRunningLeftNearLegBack,
        EnemyFrameId.DachoraRunningLeftNearLegPassing,
        EnemyFrameId.DachoraRunningLeftNearLegReaching,
        EnemyFrameId.DachoraRunningRightFarLegBack,
        EnemyFrameId.DachoraRunningRightFarLegPassing,
        EnemyFrameId.DachoraRunningRightFarLegReaching,
        EnemyFrameId.DachoraRunningRightNearLegBack,
        EnemyFrameId.DachoraRunningRightNearLegPassing,
        EnemyFrameId.DachoraRunningRightNearLegReaching,
    ];

    /// <summary>The frames <c>Enemies/Animals/EtecoonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Etecoon =
    [
        EnemyFrameId.EtecoonLookingRight,
        EnemyFrameId.EtecoonTurningLeft,
        EnemyFrameId.EtecoonRunningLeftStretched,
        EnemyFrameId.EtecoonRunningLeftBunched,
        EnemyFrameId.EtecoonRunningLeftPushingOff,
        EnemyFrameId.EtecoonLeapingLeft,
        EnemyFrameId.EtecoonSpinningLeftTurned0,
        EnemyFrameId.EtecoonSpinningLeftTurned90,
        EnemyFrameId.EtecoonSpinningLeftTurned180,
        EnemyFrameId.EtecoonSpinningLeftTurned270,
        EnemyFrameId.EtecoonHopCrouchFacingLeft,
        EnemyFrameId.EtecoonHopArmsRaisingFacingLeft,
        EnemyFrameId.EtecoonHopArmsUpFacingLeft,
        EnemyFrameId.EtecoonHopArmsFlungFacingLeft,
        EnemyFrameId.EtecoonClingingFacingLeft,
        EnemyFrameId.EtecoonLookingLeft,
        EnemyFrameId.EtecoonTurningRight,
        EnemyFrameId.EtecoonRunningRightStretched,
        EnemyFrameId.EtecoonRunningRightBunched,
        EnemyFrameId.EtecoonRunningRightPushingOff,
        EnemyFrameId.EtecoonLeapingRight,
        EnemyFrameId.EtecoonSpinningRightTurned0,
        EnemyFrameId.EtecoonSpinningRightTurned90,
        EnemyFrameId.EtecoonSpinningRightTurned180,
        EnemyFrameId.EtecoonSpinningRightTurned270,
        EnemyFrameId.EtecoonHopCrouchFacingRight,
        EnemyFrameId.EtecoonHopArmsRaisingFacingRight,
        EnemyFrameId.EtecoonHopArmsUpFacingRight,
        EnemyFrameId.EtecoonHopArmsFlungFacingRight,
        EnemyFrameId.EtecoonClingingFacingRight,
        EnemyFrameId.EtecoonFacingViewer,
    ];

    /// <summary>The frames <c>Enemies/Atomic/AtomicBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Atomic =
    [
        EnemyFrameId.AtomicFacingUpRightTurned0,
        EnemyFrameId.AtomicFacingUpRightTurned60,
        EnemyFrameId.AtomicFacingUpRightTurned120,
        EnemyFrameId.AtomicFacingUpRightTurned180,
        EnemyFrameId.AtomicFacingUpRightTurned240,
        EnemyFrameId.AtomicFacingUpRightTurned300,
        EnemyFrameId.AtomicFacingUpLeftTurned0,
        EnemyFrameId.AtomicFacingUpLeftTurned60,
        EnemyFrameId.AtomicFacingUpLeftTurned120,
        EnemyFrameId.AtomicFacingUpLeftTurned180,
        EnemyFrameId.AtomicFacingUpLeftTurned240,
        EnemyFrameId.AtomicFacingUpLeftTurned300,
    ];

    /// <summary>The frames <c>Enemies/Beetom/BeetomBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Beetom =
    [
        EnemyFrameId.BeetomCrawlingLeftLegsTucked,
        EnemyFrameId.BeetomCrawlingLeftLegsMid,
        EnemyFrameId.BeetomCrawlingLeftLegsOut,
        EnemyFrameId.BeetomHoppingLeftLegPushing,
        EnemyFrameId.BeetomHoppingLeftLegStretched,
        EnemyFrameId.BeetomLatchedFacingLeftGemGreenBright,
        EnemyFrameId.BeetomLatchedFacingLeftGemGreen,
        EnemyFrameId.BeetomLatchedFacingLeftGemGreenDim,
        EnemyFrameId.BeetomLatchedFacingLeftGemRedBright,
        EnemyFrameId.BeetomLatchedFacingLeftGemRed,
        EnemyFrameId.BeetomLatchedFacingLeftGemRedDim,
        EnemyFrameId.BeetomCrawlingRightLegsTucked,
        EnemyFrameId.BeetomCrawlingRightLegsMid,
        EnemyFrameId.BeetomCrawlingRightLegsOut,
        EnemyFrameId.BeetomHoppingRightLegPushing,
        EnemyFrameId.BeetomHoppingRightLegStretched,
        EnemyFrameId.BeetomLatchedFacingRightGemGreenBright,
        EnemyFrameId.BeetomLatchedFacingRightGemGreen,
        EnemyFrameId.BeetomLatchedFacingRightGemGreenDim,
        EnemyFrameId.BeetomLatchedFacingRightGemRedBright,
        EnemyFrameId.BeetomLatchedFacingRightGemRed,
        EnemyFrameId.BeetomLatchedFacingRightGemRedDim,
    ];

    /// <summary>The frames <c>Enemies/Boulder/BoulderBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Boulder =
    [
        EnemyFrameId.BoulderTurned0,
        EnemyFrameId.BoulderTurned45,
        EnemyFrameId.BoulderTurned90,
        EnemyFrameId.BoulderTurned135,
        EnemyFrameId.BoulderTurned180,
        EnemyFrameId.BoulderTurned225,
        EnemyFrameId.BoulderTurned270,
        EnemyFrameId.BoulderTurned315,
    ];

    /// <summary>The frames <c>Enemies/Boyon/BoyonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Boyon =
    [
        EnemyFrameId.BoyonRestingRound,
        EnemyFrameId.BoyonRestingGlinting,
        EnemyFrameId.BoyonRestingSquat,
        EnemyFrameId.BoyonBouncingBulging,
        EnemyFrameId.BoyonBouncingStarShaped,
        EnemyFrameId.BoyonBouncingStarSpread,
        EnemyFrameId.BoyonBouncingStarSpiked,
    ];

    /// <summary>The frames <c>Enemies/Bull/BullBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Bull =
    [
        EnemyFrameId.BullTentaclesUpright,
        EnemyFrameId.BullTentaclesTilted,
        EnemyFrameId.BullTentaclesTiltedFurther,
    ];

    /// <summary>The frames <c>Enemies/Cacatac/CacatacBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Cacatac =
    [
        EnemyFrameId.CacatacUprightSplayed,
        EnemyFrameId.CacatacUprightDrawingIn,
        EnemyFrameId.CacatacUprightDrawnIn,
        EnemyFrameId.CacatacUprightCrouching,
        EnemyFrameId.CacatacUprightGathered,
        EnemyFrameId.CacatacUprightGatheredTall,
        EnemyFrameId.CacatacUprightSpreading,
        EnemyFrameId.CacatacUprightStretched,
        EnemyFrameId.CacatacUprightPuffingUp,
        EnemyFrameId.CacatacUprightPuffedUp,
        EnemyFrameId.CacatacUpsideDownSplayed,
        EnemyFrameId.CacatacUpsideDownDrawingIn,
        EnemyFrameId.CacatacUpsideDownDrawnIn,
        EnemyFrameId.CacatacUpsideDownCrouching,
        EnemyFrameId.CacatacUpsideDownGathered,
        EnemyFrameId.CacatacUpsideDownGatheredTall,
        EnemyFrameId.CacatacUpsideDownSpreading,
        EnemyFrameId.CacatacUpsideDownStretched,
        EnemyFrameId.CacatacUpsideDownPuffingUp,
        EnemyFrameId.CacatacUpsideDownPuffedUp,
    ];

    /// <summary>The frames <c>Enemies/Choot/ChootBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Choot =
    [
        EnemyFrameId.ChootLyingFlat,
        EnemyFrameId.ChootSpringing,
        EnemyFrameId.ChootFolded,
        EnemyFrameId.ChootSpreadOut,
    ];

    /// <summary>The frames <c>Enemies/Chozo/ChozoStatueBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] ChozoStatue =
    [
        EnemyFrameId.ChozoStatueLowerNorfairHandsCupped,
        EnemyFrameId.ChozoStatueLowerNorfairHandReaching,
        EnemyFrameId.ChozoStatueLowerNorfairHandOutstretched,
        EnemyFrameId.ChozoStatueLowerNorfairHandOpen,
        EnemyFrameId.ChozoStatueLowerNorfairHandOpenEyeGlowing,
        EnemyFrameId.ChozoStatueLowerNorfairFingersCurling,
        EnemyFrameId.ChozoStatueLowerNorfairFingersCurled,
        EnemyFrameId.ChozoStatueLowerNorfairFingersClenched,
        EnemyFrameId.ChozoStatueWreckedShipHandsCupped,
        EnemyFrameId.ChozoStatueWreckedShipHandReaching,
        EnemyFrameId.ChozoStatueWreckedShipHandOutstretched,
        EnemyFrameId.ChozoStatueWreckedShipHandOpen,
        EnemyFrameId.ChozoStatueWreckedShipHandOpenEyeGlowing,
        EnemyFrameId.ChozoStatueWreckedShipFingersCurling,
        EnemyFrameId.ChozoStatueWreckedShipFingersCurled,
        EnemyFrameId.ChozoStatueWreckedShipFingersClenched,
        EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegLifting,
        EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegStriding,
        EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegPlanting,
        EnemyFrameId.ChozoStatueWreckedShipWalkingNearLegPlanted,
        EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegLifting,
        EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegStriding,
        EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegPlanting,
        EnemyFrameId.ChozoStatueWreckedShipWalkingFarLegPlanted,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadSidehopperBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] DeadSidehopper =
    [
        EnemyFrameId.LargeSidehopperLegsStraight,
        EnemyFrameId.LargeSidehopperLegsBent,
        EnemyFrameId.LargeSidehopperLegsCrouched,
        EnemyFrameId.DeadSidehopperSprawled,
        EnemyFrameId.DeadSidehopperSprawledLegsUp,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTorizoBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] DeadTorizo =
    [
        EnemyFrameId.DeadTorizoRubbleMound,
        EnemyFrameId.DeadTorizoStanding,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTourianBehavior.cs</c> shows for <c>CorpseRipper</c>.</summary>
    private static readonly EnemyFrameId[] CorpseRipperFrames =
    [
        EnemyFrameId.DeadRipperCorpseLevel,
        EnemyFrameId.DeadRipperCorpseTilted,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTourianBehavior.cs</c> shows for <c>CorpseSkree</c>.</summary>
    private static readonly EnemyFrameId[] CorpseSkreeFrames =
    [
        EnemyFrameId.DeadSkreeCorpse,
    ];

    /// <summary>The frames <c>Enemies/Corpses/DeadTourianBehavior.cs</c> shows for <c>CorpseZoomer</c>.</summary>
    private static readonly EnemyFrameId[] CorpseZoomerFrames =
    [
        EnemyFrameId.DeadZoomerCorpseOnItsBack,
        EnemyFrameId.DeadZoomerCorpseUpright,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Sciser</c>.</summary>
    private static readonly EnemyFrameId[] SciserFrames =
    [
        EnemyFrameId.SciserUpsideRightLegsTucked,
        EnemyFrameId.SciserUpsideRightLegsMid,
        EnemyFrameId.SciserUpsideRightLegsOut,
        EnemyFrameId.SciserUpsideLeftLegsTucked,
        EnemyFrameId.SciserUpsideLeftLegsMid,
        EnemyFrameId.SciserUpsideLeftLegsOut,
        EnemyFrameId.SciserUpsideDownLegsTucked,
        EnemyFrameId.SciserUpsideDownLegsMid,
        EnemyFrameId.SciserUpsideDownLegsOut,
        EnemyFrameId.SciserUpsideUpLegsTucked,
        EnemyFrameId.SciserUpsideUpLegsMid,
        EnemyFrameId.SciserUpsideUpLegsOut,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Zero</c>.</summary>
    private static readonly EnemyFrameId[] ZeroFrames =
    [
        EnemyFrameId.ZeroUpsideRightContracted,
        EnemyFrameId.ZeroUpsideRightStretching,
        EnemyFrameId.ZeroUpsideRightStretched,
        EnemyFrameId.ZeroUpsideRightFullyStretched,
        EnemyFrameId.ZeroUpsideLeftContracted,
        EnemyFrameId.ZeroUpsideLeftStretching,
        EnemyFrameId.ZeroUpsideLeftStretched,
        EnemyFrameId.ZeroUpsideLeftFullyStretched,
        EnemyFrameId.ZeroUpsideDownContracted,
        EnemyFrameId.ZeroUpsideDownStretching,
        EnemyFrameId.ZeroUpsideDownStretched,
        EnemyFrameId.ZeroUpsideDownFullyStretched,
        EnemyFrameId.ZeroUpsideUpContracted,
        EnemyFrameId.ZeroUpsideUpStretching,
        EnemyFrameId.ZeroUpsideUpStretched,
        EnemyFrameId.ZeroUpsideUpFullyStretched,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Viola</c>.</summary>
    private static readonly EnemyFrameId[] ViolaFrames =
    [
        EnemyFrameId.ViolaPlain,
        EnemyFrameId.ViolaFaceFaint,
        EnemyFrameId.ViolaFaceForming,
        EnemyFrameId.ViolaFaceDim,
        EnemyFrameId.ViolaFaceLit,
        EnemyFrameId.ViolaFaceBright,
        EnemyFrameId.ViolaFaceBrighter,
        EnemyFrameId.ViolaFaceBrightest,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/CrawlersBehavior.cs</c> shows for <c>Zeela</c>, <c>Sova</c>, <c>Zoomer</c>, <c>ArmoredZoomer</c>, <c>OrangeZoomer</c>.</summary>
    private static readonly EnemyFrameId[] ZeelaFrames =
    [
        EnemyFrameId.SpinyCrawlerUpsideRightSpinesTurned0,
        EnemyFrameId.SpinyCrawlerUpsideRightSpinesTurned12,
        EnemyFrameId.SpinyCrawlerUpsideRightSpinesTurned24,
        EnemyFrameId.SpinyCrawlerUpsideRightSpinesTurned36,
        EnemyFrameId.SpinyCrawlerUpsideRightSpinesTurned48,
        EnemyFrameId.SpinyCrawlerUpsideLeftSpinesTurned0,
        EnemyFrameId.SpinyCrawlerUpsideLeftSpinesTurned12,
        EnemyFrameId.SpinyCrawlerUpsideLeftSpinesTurned24,
        EnemyFrameId.SpinyCrawlerUpsideLeftSpinesTurned36,
        EnemyFrameId.SpinyCrawlerUpsideLeftSpinesTurned48,
        EnemyFrameId.SpinyCrawlerUpsideDownSpinesTurned0,
        EnemyFrameId.SpinyCrawlerUpsideDownSpinesTurned12,
        EnemyFrameId.SpinyCrawlerUpsideDownSpinesTurned24,
        EnemyFrameId.SpinyCrawlerUpsideDownSpinesTurned36,
        EnemyFrameId.SpinyCrawlerUpsideDownSpinesTurned48,
        EnemyFrameId.SpinyCrawlerUpsideUpSpinesTurned0,
        EnemyFrameId.SpinyCrawlerUpsideUpSpinesTurned12,
        EnemyFrameId.SpinyCrawlerUpsideUpSpinesTurned24,
        EnemyFrameId.SpinyCrawlerUpsideUpSpinesTurned36,
        EnemyFrameId.SpinyCrawlerUpsideUpSpinesTurned48,
    ];

    /// <summary>The frames <c>Enemies/Crawlers/MultiviolaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Multiviola =
    [
        EnemyFrameId.MultiviolaPlain,
        EnemyFrameId.MultiviolaFaceFaint,
        EnemyFrameId.MultiviolaFaceForming,
        EnemyFrameId.MultiviolaFaceDim,
        EnemyFrameId.MultiviolaFaceLit,
        EnemyFrameId.MultiviolaFaceBright,
        EnemyFrameId.MultiviolaFaceBrighter,
        EnemyFrameId.MultiviolaFaceBrightest,
    ];

    /// <summary>The frames <c>Enemies/Dragon/DragonBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Dragon =
    [
        EnemyFrameId.DragonBodyFacingLeft,
        EnemyFrameId.DragonWingFacingLeftRaised,
        EnemyFrameId.DragonWingFacingLeftLowered,
        EnemyFrameId.DragonBodyFacingRight,
        EnemyFrameId.DragonWingFacingRightRaised,
        EnemyFrameId.DragonWingFacingRightLowered,
        EnemyFrameId.DragonSpittingFacingLeftHeadBack,
        EnemyFrameId.DragonSpittingFacingLeftHeadRaised,
        EnemyFrameId.DragonSpittingFacingLeftHeadForward,
        EnemyFrameId.DragonSpittingFacingRightHeadBack,
        EnemyFrameId.DragonSpittingFacingRightHeadRaised,
        EnemyFrameId.DragonSpittingFacingRightHeadForward,
    ];

    /// <summary>The frames <c>Enemies/Elevator/ElevatorBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Elevator =
    [
        EnemyFrameId.ElevatorPadUnlit,
        EnemyFrameId.ElevatorPadLit,
    ];

    /// <summary>The frames <c>Enemies/Evir/EvirBehavior.cs</c> shows for <c>Evir</c>.</summary>
    private static readonly EnemyFrameId[] EvirFrames =
    [
        EnemyFrameId.EvirBodyFacingLeftLegsTucked,
        EnemyFrameId.EvirBodyFacingLeftLegsCurling,
        EnemyFrameId.EvirBodyFacingLeftLegsLowering,
        EnemyFrameId.EvirBodyFacingLeftLegsHanging,
        EnemyFrameId.EvirBodyFacingLeftLegsStretched,
        EnemyFrameId.EvirBodyFacingLeftLegsLifting,
        EnemyFrameId.EvirArmsFacingLeftReaching,
        EnemyFrameId.EvirArmsFacingLeftCurling,
        EnemyFrameId.EvirArmsFacingLeftFolding,
        EnemyFrameId.EvirArmsFacingLeftSpreading,
        EnemyFrameId.EvirArmsFacingLeftFolded,
        EnemyFrameId.EvirBodyFacingRightLegsTucked,
        EnemyFrameId.EvirBodyFacingRightLegsCurling,
        EnemyFrameId.EvirBodyFacingRightLegsLowering,
        EnemyFrameId.EvirBodyFacingRightLegsHanging,
        EnemyFrameId.EvirBodyFacingRightLegsStretched,
        EnemyFrameId.EvirBodyFacingRightLegsLifting,
        EnemyFrameId.EvirArmsFacingRightReaching,
        EnemyFrameId.EvirArmsFacingRightCurling,
        EnemyFrameId.EvirArmsFacingRightFolding,
        EnemyFrameId.EvirArmsFacingRightSpreading,
        EnemyFrameId.EvirArmsFacingRightFolded,
    ];

    /// <summary>The frames <c>Enemies/Evir/EvirBehavior.cs</c> shows for <c>EvirProjectile</c>.</summary>
    private static readonly EnemyFrameId[] EvirProjectileFrames =
    [
        EnemyFrameId.EvirSpineBall,
        EnemyFrameId.EvirSpineBallRegrowing,
    ];

    /// <summary>The frames <c>Enemies/FaceBlocks/BlueBrinstarFaceBlockBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] BlueBrinstarFaceBlock =
    [
        EnemyFrameId.FaceBlockNeutral,
        EnemyFrameId.BlueBrinstarFaceBlockGlancingLeft,
        EnemyFrameId.BlueBrinstarFaceBlockLookingLeft,
        EnemyFrameId.BlueBrinstarFaceBlockGlancingRight,
        EnemyFrameId.BlueBrinstarFaceBlockLookingRight,
    ];

    /// <summary>The frames <c>Enemies/Fireflea/FirefleaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Fireflea =
    [
        EnemyFrameId.FirefleaGlowBrightestWingsUp,
        EnemyFrameId.FirefleaGlowBrightestWingsLevel,
        EnemyFrameId.FirefleaGlowBrightestWingsDown,
        EnemyFrameId.FirefleaGlowBrighterWingsUp,
        EnemyFrameId.FirefleaGlowBrighterWingsLevel,
        EnemyFrameId.FirefleaGlowBrighterWingsDown,
        EnemyFrameId.FirefleaGlowBrightWingsUp,
        EnemyFrameId.FirefleaGlowBrightWingsLevel,
        EnemyFrameId.FirefleaGlowBrightWingsDown,
        EnemyFrameId.FirefleaGlowMidWingsUp,
        EnemyFrameId.FirefleaGlowMidWingsLevel,
        EnemyFrameId.FirefleaGlowMidWingsDown,
        EnemyFrameId.FirefleaGlowDimWingsUp,
        EnemyFrameId.FirefleaGlowDimWingsLevel,
        EnemyFrameId.FirefleaGlowDimWingsDown,
        EnemyFrameId.FirefleaGlowDimmerWingsUp,
        EnemyFrameId.FirefleaGlowDimmerWingsLevel,
        EnemyFrameId.FirefleaGlowDimmerWingsDown,
        EnemyFrameId.FirefleaGlowDimmestWingsUp,
        EnemyFrameId.FirefleaGlowDimmestWingsLevel,
        EnemyFrameId.FirefleaGlowDimmestWingsDown,
    ];

    /// <summary>The frames <c>Enemies/Flies/FliesBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Flies =
    [
        EnemyFrameId.FlyWingsRaisedOut,
        EnemyFrameId.FlyWingsRaised,
        EnemyFrameId.FlyWingsLevel,
        EnemyFrameId.FlyWingsLowered,
    ];

    /// <summary>The frames <c>Enemies/FuneNamihe/FuneNamiheBehavior.cs</c> shows for <c>Fune</c>.</summary>
    private static readonly EnemyFrameId[] FuneFrames =
    [
        EnemyFrameId.FuneFacingLeftMouthClosed,
        EnemyFrameId.FuneFacingLeftMouthOpening,
        EnemyFrameId.FuneFacingLeftMouthHalfOpen,
        EnemyFrameId.FuneFacingLeftMouthOpen,
        EnemyFrameId.FuneFacingLeftMouthWide,
        EnemyFrameId.FuneFacingRightMouthClosed,
        EnemyFrameId.FuneFacingRightMouthOpening,
        EnemyFrameId.FuneFacingRightMouthHalfOpen,
        EnemyFrameId.FuneFacingRightMouthOpen,
        EnemyFrameId.FuneFacingRightMouthWide,
    ];

    /// <summary>The frames <c>Enemies/FuneNamihe/FuneNamiheBehavior.cs</c> shows for <c>Namihe</c>.</summary>
    private static readonly EnemyFrameId[] NamiheFrames =
    [
        EnemyFrameId.NamiheFacingLeftMouthClosed,
        EnemyFrameId.NamiheFacingLeftMouthOpening,
        EnemyFrameId.NamiheFacingLeftMouthHalfOpen,
        EnemyFrameId.NamiheFacingLeftMouthOpen,
        EnemyFrameId.NamiheFacingLeftMouthWide,
        EnemyFrameId.NamiheFacingLeftMouthWidest,
        EnemyFrameId.NamiheFacingRightMouthClosed,
        EnemyFrameId.NamiheFacingRightMouthOpening,
        EnemyFrameId.NamiheFacingRightMouthHalfOpen,
        EnemyFrameId.NamiheFacingRightMouthOpen,
        EnemyFrameId.NamiheFacingRightMouthWide,
        EnemyFrameId.NamiheFacingRightMouthWidest,
    ];

    /// <summary>The frames <c>Enemies/Gunship/GunshipBehavior.cs</c> shows for <c>ShipBottomEntrance</c>.</summary>
    private static readonly EnemyFrameId[] ShipBottomEntranceFrames =
    [
        EnemyFrameId.GunshipEntrancePadShut,
        EnemyFrameId.GunshipEntrancePadLifting,
        EnemyFrameId.GunshipEntrancePadRising,
        EnemyFrameId.GunshipEntrancePadRaised,
        EnemyFrameId.GunshipEntrancePadSplitting,
        EnemyFrameId.GunshipEntrancePadParting,
        EnemyFrameId.GunshipEntrancePadSeparating,
        EnemyFrameId.GunshipEntrancePadSpreading,
        EnemyFrameId.GunshipEntrancePadWidening,
        EnemyFrameId.GunshipEntrancePadNearlyOpen,
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
        EnemyFrameId.HibashiEmber,
        EnemyFrameId.HibashiFlameBursting,
        EnemyFrameId.HibashiFlameSparking,
        EnemyFrameId.HibashiFlameSpitting,
        EnemyFrameId.HibashiFlameChurning,
        EnemyFrameId.HibashiFlameClimbing,
        EnemyFrameId.HibashiFlameSplashing,
        EnemyFrameId.HibashiFlameSpraying,
        EnemyFrameId.HibashiFlameSwirling,
        EnemyFrameId.HibashiFlameLifting,
        EnemyFrameId.HibashiFlameTrailing,
        EnemyFrameId.HibashiFlameStreaming,
        EnemyFrameId.HibashiFlameFlaring,
        EnemyFrameId.HibashiFlameSoaring,
        EnemyFrameId.HibashiFlameReaching,
        EnemyFrameId.HibashiFlameFullHeight,
        EnemyFrameId.HibashiFlameFullHeightSparking,
        EnemyFrameId.HibashiFlameFullHeightCharring,
        EnemyFrameId.HibashiFlameFullHeightGuttering,
        EnemyFrameId.HibashiFlameTopBreakingUp,
        EnemyFrameId.HibashiFlameTopScattering,
        EnemyFrameId.HibashiFlameTopThinning,
        EnemyFrameId.HibashiFlameTopVanishing,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>Dessgeega</c>.</summary>
    private static readonly EnemyFrameId[] DessgeegaFrames =
    [
        EnemyFrameId.DessgeegaLegsStraight,
        EnemyFrameId.DessgeegaLegsBent,
        EnemyFrameId.DessgeegaLegsCrouched,
        EnemyFrameId.DessgeegaUpsideDownLegsStraight,
        EnemyFrameId.DessgeegaUpsideDownLegsBent,
        EnemyFrameId.DessgeegaUpsideDownLegsCrouched,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>DessgeegaLarge</c>.</summary>
    private static readonly EnemyFrameId[] DessgeegaLargeFrames =
    [
        EnemyFrameId.LargeDessgeegaLegsStraight,
        EnemyFrameId.LargeDessgeegaLegsBent,
        EnemyFrameId.LargeDessgeegaLegsCrouched,
        EnemyFrameId.LargeDessgeegaUpsideDownLegsStraight,
        EnemyFrameId.LargeDessgeegaUpsideDownLegsBent,
        EnemyFrameId.LargeDessgeegaUpsideDownLegsCrouched,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>Sidehopper</c>.</summary>
    private static readonly EnemyFrameId[] SidehopperFrames =
    [
        EnemyFrameId.SidehopperLegsStraight,
        EnemyFrameId.SidehopperLegsBent,
        EnemyFrameId.SidehopperLegsCrouched,
        EnemyFrameId.SidehopperUpsideDownLegsStraight,
        EnemyFrameId.SidehopperUpsideDownLegsBent,
        EnemyFrameId.SidehopperUpsideDownLegsCrouched,
    ];

    /// <summary>The frames <c>Enemies/Hoppers/HoppersBehavior.cs</c> shows for <c>SidehopperLarge</c>, <c>SidehopperTourian</c>.</summary>
    private static readonly EnemyFrameId[] SidehopperLargeFrames =
    [
        EnemyFrameId.LargeSidehopperLegsStraight,
        EnemyFrameId.LargeSidehopperLegsBent,
        EnemyFrameId.LargeSidehopperLegsCrouched,
        EnemyFrameId.LargeSidehopperUpsideDownLegsStraight,
        EnemyFrameId.LargeSidehopperUpsideDownLegsBent,
        EnemyFrameId.LargeSidehopperUpsideDownLegsCrouched,
    ];

    /// <summary>The frames <c>Enemies/Kago/KagoBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Kago =
    [
        EnemyFrameId.KagoHiveAtRest,
        EnemyFrameId.KagoHiveSwelling,
        EnemyFrameId.KagoHiveSwollen,
    ];

    /// <summary>The frames <c>Enemies/KiHunters/KiHunterBehavior.cs</c> shows for <c>KihunterGreen</c>, <c>KihunterRed</c>, <c>KihunterYellow</c>.</summary>
    private static readonly EnemyFrameId[] KihunterGreenFrames =
    [
        EnemyFrameId.KiHunterHoveringFacingLeft,
        EnemyFrameId.KiHunterHoveringFacingLeftDropped,
        EnemyFrameId.KiHunterHoveringFacingRight,
        EnemyFrameId.KiHunterHoveringFacingRightDropped,
        EnemyFrameId.KiHunterSwoopingFacingLeftArmsBack,
        EnemyFrameId.KiHunterSwoopingFacingLeftArmsIn,
        EnemyFrameId.KiHunterSwoopingFacingLeftLegsReaching,
        EnemyFrameId.KiHunterSwoopingFacingLeftLegsStretched,
        EnemyFrameId.KiHunterSwoopingFacingRightArmsBack,
        EnemyFrameId.KiHunterSwoopingFacingRightArmsIn,
        EnemyFrameId.KiHunterSwoopingFacingRightLegsReaching,
        EnemyFrameId.KiHunterSwoopingFacingRightLegsStretched,
        EnemyFrameId.KiHunterCrouchedFacingLeft,
        EnemyFrameId.KiHunterCrouchingLowFacingLeft,
        EnemyFrameId.KiHunterSpringingFacingLeft,
        EnemyFrameId.KiHunterLeapingFacingLeft,
        EnemyFrameId.KiHunterCrouchedFacingRight,
        EnemyFrameId.KiHunterCrouchingLowFacingRight,
        EnemyFrameId.KiHunterSpringingFacingRight,
        EnemyFrameId.KiHunterLeapingFacingRight,
        EnemyFrameId.KiHunterSpittingFacingLeftAcidGathering,
        EnemyFrameId.KiHunterSpittingFacingLeftAcidSwollen,
        EnemyFrameId.KiHunterSpittingFacingLeftAcidSpat,
        EnemyFrameId.KiHunterSpittingFacingLeftAcidSpent,
        EnemyFrameId.KiHunterSpittingFacingRightAcidGathering,
        EnemyFrameId.KiHunterSpittingFacingRightAcidSwollen,
        EnemyFrameId.KiHunterSpittingFacingRightAcidSpat,
        EnemyFrameId.KiHunterSpittingFacingRightAcidSpent,
    ];

    /// <summary>The frames <c>Enemies/KiHunters/KiHunterBehavior.cs</c> shows for <c>KihunterGreenWings</c>, <c>KihunterRedWings</c>, <c>KihunterYellowWings</c>.</summary>
    private static readonly EnemyFrameId[] KihunterGreenWingsFrames =
    [
        EnemyFrameId.KiHunterWingsFacingLeftRaised,
        EnemyFrameId.KiHunterWingsFacingLeftMid,
        EnemyFrameId.KiHunterWingsFacingLeftLevel,
        EnemyFrameId.KiHunterWingsFacingRightRaised,
        EnemyFrameId.KiHunterWingsFacingRightMid,
        EnemyFrameId.KiHunterWingsFacingRightLevel,
        EnemyFrameId.KiHunterDetachedWings,
    ];

    /// <summary>The frames <c>Enemies/Kzan/KzanBehavior.cs</c> shows for <c>KzanBottom</c>.</summary>
    private static readonly EnemyFrameId[] KzanBottomFrames = [];

    /// <summary>The frames <c>Enemies/Kzan/KzanBehavior.cs</c> shows for <c>KzanTop</c>.</summary>
    private static readonly EnemyFrameId[] KzanTopFrames =
    [
        EnemyFrameId.KzanSpikedBlock,
    ];

    /// <summary>The frames <c>Enemies/LavaJumpers/NorfairLavaJumpingEnemyBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NorfairLavaJumpingEnemy =
    [
        EnemyFrameId.NorfairLavaJumperClawsCurledDown,
        EnemyFrameId.NorfairLavaJumperClawsSpreading,
        EnemyFrameId.NorfairLavaJumperClawsSpread,
        EnemyFrameId.NorfairLavaJumperClawsSpreadFlat,
        EnemyFrameId.NorfairLavaJumperClawsRaising,
        EnemyFrameId.NorfairLavaJumperClawsRaised,
        EnemyFrameId.NorfairLavaJumperClawsFolded,
        EnemyFrameId.NorfairLavaJumperFlameFlare,
        EnemyFrameId.NorfairLavaJumperFlameSmall,
        EnemyFrameId.NorfairLavaJumperFlameTall,
    ];

    /// <summary>The frames <c>Enemies/Magdollite/MagdolliteBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Magdollite =
    [
        EnemyFrameId.MagdolliteHeadFacingLeftHigh,
        EnemyFrameId.MagdolliteHeadFacingLeftMid,
        EnemyFrameId.MagdolliteHeadFacingLeftLow,
        EnemyFrameId.MagdolliteRearingFacingLeft,
        EnemyFrameId.MagdolliteLeaningFacingLeft,
        EnemyFrameId.MagdolliteReachingFacingLeft,
        EnemyFrameId.MagdolliteThrowingFacingLeft,
        EnemyFrameId.MagdollitePillarCap,
        EnemyFrameId.MagdolliteMoundFacingLeft,
        EnemyFrameId.MagdolliteSinkingFacingLeft,
        EnemyFrameId.MagdolliteSunkFacingLeft,
        EnemyFrameId.MagdolliteHeadFacingRightHigh,
        EnemyFrameId.MagdolliteHeadFacingRightMid,
        EnemyFrameId.MagdolliteHeadFacingRightLow,
        EnemyFrameId.MagdolliteRearingFacingRight,
        EnemyFrameId.MagdolliteLeaningFacingRight,
        EnemyFrameId.MagdolliteReachingFacingRight,
        EnemyFrameId.MagdolliteThrowingFacingRight,
        EnemyFrameId.MagdolliteMoundFacingRight,
        EnemyFrameId.MagdolliteSinkingFacingRight,
        EnemyFrameId.MagdolliteSunkFacingRight,
        EnemyFrameId.MagdolliteOneSegmentPillar,
        EnemyFrameId.MagdolliteTwoSegmentPillar,
        EnemyFrameId.MagdolliteThreeSegmentPillar,
        EnemyFrameId.MagdolliteFourSegmentPillar,
        EnemyFrameId.MagdolliteFiveSegmentPillar,
        EnemyFrameId.MagdolliteSixSegmentPillar,
        EnemyFrameId.MagdolliteSevenSegmentPillar,
        EnemyFrameId.MagdolliteEightSegmentPillar,
    ];

    /// <summary>The frames <c>Enemies/MamaTurtles/MamaTurtleBehavior.cs</c> shows for <c>BabyTurtle</c>.</summary>
    private static readonly EnemyFrameId[] BabyTurtleFrames =
    [
        EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked,
        EnemyFrameId.TurtleBabyCrawlingLeftLegsReaching,
        EnemyFrameId.TurtleBabyCrawlingLeftLegsPlanted,
        EnemyFrameId.TurtleBabyCrawlingLeftLegsPushing,
        EnemyFrameId.TurtleBabyCrawlingLeftLegsTrailing,
        EnemyFrameId.TurtleBabyCrawlingLeftLegsLifting,
        EnemyFrameId.TurtleBabyCrawlingLeftLegsReturning,
        EnemyFrameId.TurtleBabyShellTurned0,
        EnemyFrameId.TurtleBabyShellTurned90,
        EnemyFrameId.TurtleBabyShellTurned180,
        EnemyFrameId.TurtleBabyShellTurned270,
        EnemyFrameId.TurtleBabyCrawlingRightLegsTucked,
        EnemyFrameId.TurtleBabyCrawlingRightLegsReaching,
        EnemyFrameId.TurtleBabyCrawlingRightLegsPlanted,
        EnemyFrameId.TurtleBabyCrawlingRightLegsPushing,
        EnemyFrameId.TurtleBabyCrawlingRightLegsTrailing,
        EnemyFrameId.TurtleBabyCrawlingRightLegsLifting,
        EnemyFrameId.TurtleBabyCrawlingRightLegsReturning,
    ];

    /// <summary>The frames <c>Enemies/MamaTurtles/MamaTurtleBehavior.cs</c> shows for <c>MamaTurtle</c>.</summary>
    private static readonly EnemyFrameId[] MamaTurtleFrames =
    [
        EnemyFrameId.TurtleMamaInShellFacingLeft,
        EnemyFrameId.TurtleMamaInShellFacingViewer,
        EnemyFrameId.TurtleMamaInShellFacingRight,
        EnemyFrameId.TurtleMamaOutOfShellFacingLeft,
        EnemyFrameId.TurtleMamaWithdrawingFacingLeft,
        EnemyFrameId.TurtleMamaWithdrawnFacingLeft,
        EnemyFrameId.TurtleMamaOutOfShellFacingRight,
        EnemyFrameId.TurtleMamaWithdrawingFacingRight,
        EnemyFrameId.TurtleMamaWithdrawnFacingRight,
    ];

    /// <summary>The frames <c>Enemies/MaridiaSnails/MaridiaLargeSnailBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] MaridiaLargeSnail =
    [
        EnemyFrameId.OumFacingLeftTurned0,
        EnemyFrameId.OumFacingLeftVentOpening,
        EnemyFrameId.OumFacingLeftVentOpen,
        EnemyFrameId.OumFacingLeftJetFlaring,
        EnemyFrameId.OumFacingLeftJetShort,
        EnemyFrameId.OumFacingLeftJetMedium,
        EnemyFrameId.OumFacingLeftJetLong,
        EnemyFrameId.OumFacingLeftJetLongest,
        EnemyFrameId.OumFacingLeftTurned45,
        EnemyFrameId.OumFacingLeftTurned90,
        EnemyFrameId.OumFacingLeftTurned135,
        EnemyFrameId.OumFacingLeftTurned180,
        EnemyFrameId.OumFacingLeftTurned225,
        EnemyFrameId.OumFacingLeftTurned270,
        EnemyFrameId.OumFacingLeftTurned315,
        EnemyFrameId.OumFacingRightTurned0,
        EnemyFrameId.OumFacingRightVentOpening,
        EnemyFrameId.OumFacingRightVentOpen,
        EnemyFrameId.OumFacingRightJetFlaring,
        EnemyFrameId.OumFacingRightJetShort,
        EnemyFrameId.OumFacingRightJetMedium,
        EnemyFrameId.OumFacingRightJetLong,
        EnemyFrameId.OumFacingRightJetLongest,
        EnemyFrameId.OumFacingRightTurned45,
        EnemyFrameId.OumFacingRightTurned90,
        EnemyFrameId.OumFacingRightTurned135,
        EnemyFrameId.OumFacingRightTurned180,
        EnemyFrameId.OumFacingRightTurned225,
        EnemyFrameId.OumFacingRightTurned270,
        EnemyFrameId.OumFacingRightTurned315,
    ];

    /// <summary>The frames <c>Enemies/Metaree/MetareeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Metaree =
    [
        EnemyFrameId.MetareeFacingViewer,
        EnemyFrameId.MetareeTurningOblique,
        EnemyFrameId.MetareeEdgeOn,
        EnemyFrameId.MetareeReturningOblique,
        EnemyFrameId.MetareeWingsFlared,
    ];

    /// <summary>The frames <c>Enemies/Metroids/MetroidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Metroid =
    [
        EnemyFrameId.MetroidNucleiDark,
        EnemyFrameId.MetroidNucleiGlowing,
        EnemyFrameId.MetroidNucleiBright,
        EnemyFrameId.MetroidNucleiBrightest,
    ];

    /// <summary>The frames <c>Enemies/Metroids/MochtroidBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Mochtroid =
    [
        EnemyFrameId.MochtroidFloatingWide,
        EnemyFrameId.MochtroidFloatingSqueezing,
        EnemyFrameId.MochtroidFloatingSqueezed,
        EnemyFrameId.MochtroidLatchedWide,
        EnemyFrameId.MochtroidLatchedSqueezing,
        EnemyFrameId.MochtroidLatchedSqueezed,
    ];

    /// <summary>The frames <c>Enemies/MorphBallEye/MorphBallEyeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] MorphBallEye =
    [
        EnemyFrameId.MorphBallEyeLookingUpUpRight,
        EnemyFrameId.MorphBallEyeLookingUpRight,
        EnemyFrameId.MorphBallEyeLookingRightUpRight,
        EnemyFrameId.MorphBallEyeLookingRight,
        EnemyFrameId.MorphBallEyeLookingRightDownRight,
        EnemyFrameId.MorphBallEyeLookingDownRight,
        EnemyFrameId.MorphBallEyeLookingDownDownRight,
        EnemyFrameId.MorphBallEyeLookingDownDownLeft,
        EnemyFrameId.MorphBallEyeLookingDownLeft,
        EnemyFrameId.MorphBallEyeLookingLeftDownLeft,
        EnemyFrameId.MorphBallEyeLookingLeft,
        EnemyFrameId.MorphBallEyeLookingLeftUpLeft,
        EnemyFrameId.MorphBallEyeLookingUpLeft,
        EnemyFrameId.MorphBallEyeLookingUpUpLeft,
        EnemyFrameId.MorphBallEyeFacingRightHalfOpen,
        EnemyFrameId.MorphBallEyeFacingRightClosed,
        EnemyFrameId.MorphBallEyeFacingLeftHalfOpen,
        EnemyFrameId.MorphBallEyeFacingLeftClosed,
        EnemyFrameId.MorphEyeMountRight,
        EnemyFrameId.MorphEyeMountDown,
        EnemyFrameId.MorphEyeMountLeft,
        EnemyFrameId.MorphEyeMountUp,
    ];

    /// <summary>The frames <c>Enemies/NoobTube/NoobTubeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NoobTube = [];

    /// <summary>The frames <c>Enemies/Nuclear/NuclearWaffleBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NuclearWaffle =
    [
        EnemyFrameId.NuclearWaffleBodyTurned0,
        EnemyFrameId.NuclearWaffleBodyTurned45,
        EnemyFrameId.NuclearWaffleBodyTurned90,
        EnemyFrameId.NuclearWaffleBodyTurned135,
        EnemyFrameId.NuclearWaffleBodyTurned180,
        EnemyFrameId.NuclearWaffleBodyTurned225,
        EnemyFrameId.NuclearWaffleBodyTurned270,
        EnemyFrameId.NuclearWaffleBodyTurned315,
    ];

    /// <summary>The frames <c>Enemies/Owtch/OwtchBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Owtch =
    [
        EnemyFrameId.OwtchCrawlingFeetAtRest,
        EnemyFrameId.OwtchCrawlingFeetShuffling,
        EnemyFrameId.OwtchCrawlingFeetStretched,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Gamet</c>.</summary>
    private static readonly EnemyFrameId[] GametFrames =
    [
        EnemyFrameId.GametFacingLeftWingsUp,
        EnemyFrameId.GametFacingLeftWingsRaised,
        EnemyFrameId.GametFacingLeftWingsLevel,
        EnemyFrameId.GametFacingLeftWingsLowered,
        EnemyFrameId.GametFacingLeftWingsDown,
        EnemyFrameId.GametFacingRightWingsUp,
        EnemyFrameId.GametFacingRightWingsRaised,
        EnemyFrameId.GametFacingRightWingsLevel,
        EnemyFrameId.GametFacingRightWingsLowered,
        EnemyFrameId.GametFacingRightWingsDown,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Geega</c>.</summary>
    private static readonly EnemyFrameId[] GeegaFrames =
    [
        EnemyFrameId.GeegaLevelFacingLeftWingsUp,
        EnemyFrameId.GeegaLevelFacingLeftWingsLevel,
        EnemyFrameId.GeegaLevelFacingLeftWingsDown,
        EnemyFrameId.GeegaTiltedFacingLeftWingsUp,
        EnemyFrameId.GeegaTiltedFacingLeftWingsLevel,
        EnemyFrameId.GeegaTiltedFacingLeftWingsDown,
        EnemyFrameId.GeegaLevelFacingRightWingsUp,
        EnemyFrameId.GeegaLevelFacingRightWingsLevel,
        EnemyFrameId.GeegaLevelFacingRightWingsDown,
        EnemyFrameId.GeegaTiltedFacingRightWingsUp,
        EnemyFrameId.GeegaTiltedFacingRightWingsLevel,
        EnemyFrameId.GeegaTiltedFacingRightWingsDown,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Zeb</c>.</summary>
    private static readonly EnemyFrameId[] ZebFrames =
    [
        EnemyFrameId.ZebFacingLeftWingsUp,
        EnemyFrameId.ZebFacingLeftWingsRaised,
        EnemyFrameId.ZebFacingLeftWingsLevel,
        EnemyFrameId.ZebFacingLeftWingsLowered,
        EnemyFrameId.ZebFacingLeftWingsDown,
        EnemyFrameId.ZebFacingRightWingsUp,
        EnemyFrameId.ZebFacingRightWingsRaised,
        EnemyFrameId.ZebFacingRightWingsLevel,
        EnemyFrameId.ZebFacingRightWingsLowered,
        EnemyFrameId.ZebFacingRightWingsDown,
    ];

    /// <summary>The frames <c>Enemies/PipeBugs/PipeBugsBehavior.cs</c> shows for <c>Zebbo</c>.</summary>
    private static readonly EnemyFrameId[] ZebboFrames =
    [
        EnemyFrameId.ZebboDartingFacingLeftWingsUp,
        EnemyFrameId.ZebboDartingFacingLeftWingsLevel,
        EnemyFrameId.ZebboDartingFacingLeftWingsDown,
        EnemyFrameId.ZebboRisingFacingLeftWingsUp,
        EnemyFrameId.ZebboRisingFacingLeftWingsLevel,
        EnemyFrameId.ZebboRisingFacingLeftWingsDown,
        EnemyFrameId.ZebboDartingFacingRightWingsUp,
        EnemyFrameId.ZebboDartingFacingRightWingsLevel,
        EnemyFrameId.ZebboDartingFacingRightWingsDown,
        EnemyFrameId.ZebboRisingFacingRightWingsUp,
        EnemyFrameId.ZebboRisingFacingRightWingsLevel,
        EnemyFrameId.ZebboRisingFacingRightWingsDown,
    ];

    /// <summary>The frames <c>Enemies/Platforms/PlatformsBehavior.cs</c> shows for <c>SuspensorPlatform</c>.</summary>
    private static readonly EnemyFrameId[] SuspensorPlatformFrames =
    [
        EnemyFrameId.PlatformLightDark,
        EnemyFrameId.PlatformLightDim,
        EnemyFrameId.PlatformLightLit,
        EnemyFrameId.PlatformLightBright,
    ];

    /// <summary>The frames <c>Enemies/Platforms/PlatformsBehavior.cs</c> shows for <c>Tripper</c>.</summary>
    private static readonly EnemyFrameId[] TripperFrames =
    [
        EnemyFrameId.TripperHoveringFacingLeftJetShort,
        EnemyFrameId.TripperHoveringFacingLeftJetLong,
        EnemyFrameId.TripperHoveringFacingRightJetShort,
        EnemyFrameId.TripperHoveringFacingRightJetLong,
        EnemyFrameId.TripperThrustingLeftJetMedium,
        EnemyFrameId.TripperThrustingLeftJetShort,
        EnemyFrameId.TripperThrustingLeftJetLong,
        EnemyFrameId.TripperThrustingRightJetMedium,
        EnemyFrameId.TripperThrustingRightJetShort,
        EnemyFrameId.TripperThrustingRightJetLong,
        EnemyFrameId.TripperFrozenFacingLeft,
        EnemyFrameId.TripperFrozenFacingRight,
    ];

    /// <summary>The frames <c>Enemies/Polyp/PolypBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Polyp =
    [
        EnemyFrameId.PolypRock,
    ];

    /// <summary>The frames <c>Enemies/Powamp/PowampBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Powamp =
    [
        EnemyFrameId.PowampBodyTailLeft,
        EnemyFrameId.PowampBodyTailDown,
        EnemyFrameId.PowampBodyTailRight,
        EnemyFrameId.PowampBalloonSmall,
        EnemyFrameId.PowampBalloonHalfInflated,
        EnemyFrameId.PowampBalloonInflated,
    ];

    /// <summary>The frames <c>Enemies/Puyo/PuyoBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Puyo =
    [
        EnemyFrameId.PuyoGroundedTall,
        EnemyFrameId.PuyoGroundedFlattening,
        EnemyFrameId.PuyoGroundedFlat,
        EnemyFrameId.PuyoAirborneOpening,
        EnemyFrameId.PuyoAirborneHalfOpen,
        EnemyFrameId.PuyoAirborneSpread,
        EnemyFrameId.PuyoAirborneHalfClosed,
        EnemyFrameId.PuyoAirborneClosing,
    ];

    /// <summary>The frames <c>Enemies/Rios/LowerNorfairRioBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] LowerNorfairRio =
    [
        EnemyFrameId.LowerNorfairRioWingsRaised,
        EnemyFrameId.LowerNorfairRioWingsLevel,
        EnemyFrameId.LowerNorfairRioWingsLevelDropped,
        EnemyFrameId.LowerNorfairRioWingsLifting,
        EnemyFrameId.LowerNorfairRioWingsTilting,
        EnemyFrameId.LowerNorfairRioWingsOpening,
        EnemyFrameId.LowerNorfairRioWingsSpread,
        EnemyFrameId.LowerNorfairRioWingsFullySpread,
        EnemyFrameId.LowerNorfairRioWingsClosing,
        EnemyFrameId.LowerNorfairRioWingsFolding,
        EnemyFrameId.LowerNorfairRioWingsFolded,
        EnemyFrameId.LowerNorfairRioWingsSwept,
        EnemyFrameId.LowerNorfairRioFlameLarge,
        EnemyFrameId.LowerNorfairRioFlameMedium,
        EnemyFrameId.LowerNorfairRioFlameSmall,
    ];

    /// <summary>The frames <c>Enemies/Rios/NorfairRioBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NorfairRio =
    [
        EnemyFrameId.NorfairRioWingsLevel,
        EnemyFrameId.NorfairRioWingsSpread,
        EnemyFrameId.NorfairRioWingsDrooping,
        EnemyFrameId.NorfairRioRisingFlameLarge,
        EnemyFrameId.NorfairRioRisingFlameMedium,
        EnemyFrameId.NorfairRioRisingFlameSmall,
        EnemyFrameId.NorfairRioDiving,
        EnemyFrameId.NorfairRioWingsFlattening,
        EnemyFrameId.NorfairRioWingsTilting,
        EnemyFrameId.NorfairRioWingsLifting,
        EnemyFrameId.NorfairRioWingsRaising,
        EnemyFrameId.NorfairRioWingsSweptUp,
        EnemyFrameId.NorfairRioClimbing,
        EnemyFrameId.NorfairRioFallingFlameLarge,
        EnemyFrameId.NorfairRioFallingFlameMedium,
        EnemyFrameId.NorfairRioFallingFlameSmall,
    ];

    /// <summary>The frames <c>Enemies/Rios/RioBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Rio =
    [
        EnemyFrameId.RioWingsUpLegsTucked,
        EnemyFrameId.RioWingsLevelLegsTucked,
        EnemyFrameId.RioWingsUpLegsHanging,
        EnemyFrameId.RioWingsLevelLegsHanging,
        EnemyFrameId.RioWingsUpLegsSpread,
        EnemyFrameId.RioWingsLevelLegsSpread,
        EnemyFrameId.RioWingsUpClawsCurled,
        EnemyFrameId.RioWingsLevelClawsCurled,
    ];

    /// <summary>The frames <c>Enemies/Ripper/RipperBehavior.cs</c> shows for <c>GrappleRipper</c>, <c>JetRipper</c>.</summary>
    private static readonly EnemyFrameId[] GrappleRipperFrames =
    [
        EnemyFrameId.GrappleRipperFlyingLeftFlameMedium,
        EnemyFrameId.GrappleRipperFlyingLeftFlameShort,
        EnemyFrameId.GrappleRipperFlyingLeftFlameLong,
        EnemyFrameId.GrappleRipperFlyingRightFlameMedium,
        EnemyFrameId.GrappleRipperFlyingRightFlameShort,
        EnemyFrameId.GrappleRipperFlyingRightFlameLong,
        EnemyFrameId.RipperFrozenFacingLeft,
        EnemyFrameId.RipperFrozenFacingRight,
    ];

    /// <summary>The frames <c>Enemies/Ripper/RipperBehavior.cs</c> shows for <c>Ripper</c>.</summary>
    private static readonly EnemyFrameId[] RipperFrames =
    [
        EnemyFrameId.RipperFlyingLeftEyeLit,
        EnemyFrameId.RipperFlyingLeftEyeDark,
        EnemyFrameId.RipperFlyingLeftEyeGlinting,
        EnemyFrameId.RipperFlyingRightEyeLit,
        EnemyFrameId.RipperFlyingRightEyeDark,
        EnemyFrameId.RipperFlyingRightEyeGlinting,
    ];

    /// <summary>The frames <c>Enemies/Sbug/SbugBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Sbug =
    [
        EnemyFrameId.SbugFacingRightWingsUp,
        EnemyFrameId.SbugFacingRightWingsLevel,
        EnemyFrameId.SbugFacingRightWingsDown,
        EnemyFrameId.SbugFacingUpRightWingsUp,
        EnemyFrameId.SbugFacingUpRightWingsLevel,
        EnemyFrameId.SbugFacingUpRightWingsDown,
        EnemyFrameId.SbugFacingUpWingsUp,
        EnemyFrameId.SbugFacingUpWingsLevel,
        EnemyFrameId.SbugFacingUpWingsDown,
        EnemyFrameId.SbugFacingUpLeftWingsUp,
        EnemyFrameId.SbugFacingUpLeftWingsLevel,
        EnemyFrameId.SbugFacingUpLeftWingsDown,
        EnemyFrameId.SbugFacingLeftWingsUp,
        EnemyFrameId.SbugFacingLeftWingsLevel,
        EnemyFrameId.SbugFacingLeftWingsDown,
        EnemyFrameId.SbugFacingDownLeftWingsUp,
        EnemyFrameId.SbugFacingDownLeftWingsLevel,
        EnemyFrameId.SbugFacingDownLeftWingsDown,
        EnemyFrameId.SbugFacingDownWingsUp,
        EnemyFrameId.SbugFacingDownWingsLevel,
        EnemyFrameId.SbugFacingDownWingsDown,
        EnemyFrameId.SbugFacingDownRightWingsUp,
        EnemyFrameId.SbugFacingDownRightWingsLevel,
        EnemyFrameId.SbugFacingDownRightWingsDown,
    ];

    /// <summary>The frames <c>Enemies/Shaktool/ShaktoolBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Shaktool =
    [
        EnemyFrameId.ShaktoolSawHandFinalPieceTurned0,
        EnemyFrameId.ShaktoolSawHandFinalPieceTurned30,
        EnemyFrameId.ShaktoolSawHandFinalPieceTurned60,
        EnemyFrameId.ShaktoolArm,
        EnemyFrameId.ShaktoolHeadLeft,
        EnemyFrameId.ShaktoolHeadUpLeft,
        EnemyFrameId.ShaktoolHeadUp,
        EnemyFrameId.ShaktoolHeadUpRight,
        EnemyFrameId.ShaktoolHeadRight,
        EnemyFrameId.ShaktoolHeadDownRight,
        EnemyFrameId.ShaktoolHeadDown,
        EnemyFrameId.ShaktoolHeadDownLeft,
        EnemyFrameId.ShaktoolSawHandPrimaryPieceTurned0,
        EnemyFrameId.ShaktoolSawHandPrimaryPieceTurned30,
        EnemyFrameId.ShaktoolSawHandPrimaryPieceTurned60,
    ];

    /// <summary>The frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> shows for <c>Kamer</c>.</summary>
    private static readonly EnemyFrameId[] KamerFrames =
    [
        EnemyFrameId.PlatformLightDark,
        EnemyFrameId.PlatformLightDim,
        EnemyFrameId.PlatformLightLit,
        EnemyFrameId.PlatformLightBright,
    ];

    /// <summary>The frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> shows for <c>ShutterDestroyable</c>, <c>ShutterShootable</c>.</summary>
    private static readonly EnemyFrameId[] ShutterDestroyableFrames =
    [
        EnemyFrameId.ShutterFullyGrown,
    ];

    /// <summary>The frames <c>Enemies/Shutters/ShuttersBehavior.cs</c> shows for <c>ShutterGrowing</c>.</summary>
    private static readonly EnemyFrameId[] ShutterGrowingFrames =
    [
        EnemyFrameId.ShutterQuarterGrown,
        EnemyFrameId.ShutterHalfGrown,
        EnemyFrameId.ShutterThreeQuartersGrown,
        EnemyFrameId.ShutterFullyGrown,
    ];

    /// <summary>The frames <c>Enemies/Skree/SkreeBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Skree =
    [
        EnemyFrameId.SkreeFacingViewer,
        EnemyFrameId.SkreeTurningOblique,
        EnemyFrameId.SkreeEdgeOn,
        EnemyFrameId.SkreeReturningOblique,
        EnemyFrameId.SkreeWingsFlared,
    ];

    /// <summary>The frames <c>Enemies/Skultera/SkulteraBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Skultera =
    [
        EnemyFrameId.SkulteraSwimmingLeftTailStraight,
        EnemyFrameId.SkulteraSwimmingLeftTailWaving,
        EnemyFrameId.SkulteraSwimmingLeftTailFlicking,
        EnemyFrameId.SkulteraTurningRightBy22,
        EnemyFrameId.SkulteraTurningRightBy45,
        EnemyFrameId.SkulteraTurningRightBy67,
        EnemyFrameId.SkulteraTurningRightBy90,
        EnemyFrameId.SkulteraTurningRightBy112,
        EnemyFrameId.SkulteraTurningRightBy135,
        EnemyFrameId.SkulteraTurningRightBy157,
        EnemyFrameId.SkulteraTurningRightBy180,
        EnemyFrameId.SkulteraSwimmingRightTailStraight,
        EnemyFrameId.SkulteraSwimmingRightTailWaving,
        EnemyFrameId.SkulteraSwimmingRightTailFlicking,
        EnemyFrameId.SkulteraTurningLeftBy22,
        EnemyFrameId.SkulteraTurningLeftBy45,
        EnemyFrameId.SkulteraTurningLeftBy67,
        EnemyFrameId.SkulteraTurningLeftBy90,
        EnemyFrameId.SkulteraTurningLeftBy112,
        EnemyFrameId.SkulteraTurningLeftBy135,
        EnemyFrameId.SkulteraTurningLeftBy157,
        EnemyFrameId.SkulteraTurningLeftBy180,
    ];

    /// <summary>The frames <c>Enemies/SpacePirates/NinjaSpacePirateBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] NinjaSpacePirate =
    [
        EnemyFrameId.NinjaPirateCrouchFacingLeft,
        EnemyFrameId.NinjaPirateClawsReadyFacingLeft,
        EnemyFrameId.NinjaPirateClawDrawnBackFacingLeft,
        EnemyFrameId.NinjaPirateClawRaisedHighFacingLeft,
        EnemyFrameId.NinjaPirateClawRaisedOverheadFacingLeft,
        EnemyFrameId.NinjaPirateClawSwungForwardFacingLeft,
        EnemyFrameId.NinjaPirateClawsSweptDownFacingLeft,
        EnemyFrameId.NinjaPirateUprightClawsLoweredFacingLeft,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned0,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned45,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned90,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned135,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned180,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned225,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned270,
        EnemyFrameId.NinjaPirateSpinningFacingLeftTurned315,
        EnemyFrameId.NinjaPirateReadyFacingLeftFeetClose,
        EnemyFrameId.NinjaPirateReadyFacingLeftFeetApart,
        EnemyFrameId.NinjaPirateReadyFacingLeftFeetWide,
        EnemyFrameId.NinjaPirateFlinchFacingLeft,
        EnemyFrameId.NinjaPirateLeapCrouchFacingLeft,
        EnemyFrameId.NinjaPirateTuckedJumpFacingLeft,
        EnemyFrameId.NinjaPirateDiveKickFacingLeft,
        EnemyFrameId.NinjaPirateRunningLeftFarLegBack,
        EnemyFrameId.NinjaPirateRunningLeftFarLegLifting,
        EnemyFrameId.NinjaPirateRunningLeftFarLegPassing,
        EnemyFrameId.NinjaPirateRunningLeftFarLegPlanting,
        EnemyFrameId.NinjaPirateRunningLeftNearLegBack,
        EnemyFrameId.NinjaPirateRunningLeftNearLegLifting,
        EnemyFrameId.NinjaPirateRunningLeftNearLegPassing,
        EnemyFrameId.NinjaPirateRunningLeftNearLegPlanting,
        EnemyFrameId.NinjaPirateLookingAtViewerFacingLeft,
        EnemyFrameId.NinjaPirateStandingFacingLeft,
        EnemyFrameId.NinjaPirateLookingAwayFacingLeft,
        EnemyFrameId.NinjaPirateFacingViewer,
        EnemyFrameId.NinjaPirateStandingFacingRight,
        EnemyFrameId.NinjaPirateReadyFacingRightFeetClose,
        EnemyFrameId.NinjaPirateReadyFacingRightFeetApart,
        EnemyFrameId.NinjaPirateReadyFacingRightFeetWide,
        EnemyFrameId.NinjaPirateKickChamberedFacingLeft,
        EnemyFrameId.NinjaPirateKickExtendingFacingLeft,
        EnemyFrameId.NinjaPirateKickExtendedFacingLeft,
        EnemyFrameId.NinjaPirateCrouchFacingRight,
        EnemyFrameId.NinjaPirateClawsReadyFacingRight,
        EnemyFrameId.NinjaPirateClawDrawnBackFacingRight,
        EnemyFrameId.NinjaPirateClawRaisedHighFacingRight,
        EnemyFrameId.NinjaPirateClawRaisedOverheadFacingRight,
        EnemyFrameId.NinjaPirateClawSwungForwardFacingRight,
        EnemyFrameId.NinjaPirateClawsSweptDownFacingRight,
        EnemyFrameId.NinjaPirateUprightClawsLoweredFacingRight,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned0,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned45,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned90,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned135,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned180,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned225,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned270,
        EnemyFrameId.NinjaPirateSpinningFacingRightTurned315,
        EnemyFrameId.NinjaPirateFlinchFacingRight,
        EnemyFrameId.NinjaPirateLeapCrouchFacingRight,
        EnemyFrameId.NinjaPirateTuckedJumpFacingRight,
        EnemyFrameId.NinjaPirateDiveKickFacingRight,
        EnemyFrameId.NinjaPirateRunningRightFarLegBack,
        EnemyFrameId.NinjaPirateRunningRightFarLegLifting,
        EnemyFrameId.NinjaPirateRunningRightFarLegPassing,
        EnemyFrameId.NinjaPirateRunningRightFarLegPlanting,
        EnemyFrameId.NinjaPirateRunningRightNearLegBack,
        EnemyFrameId.NinjaPirateRunningRightNearLegLifting,
        EnemyFrameId.NinjaPirateRunningRightNearLegPassing,
        EnemyFrameId.NinjaPirateRunningRightNearLegPlanting,
        EnemyFrameId.NinjaPirateLookingAtViewerFacingRight,
        EnemyFrameId.NinjaPirateLookingAwayFacingRight,
        EnemyFrameId.NinjaPirateKickChamberedFacingRight,
        EnemyFrameId.NinjaPirateKickExtendingFacingRight,
        EnemyFrameId.NinjaPirateKickExtendedFacingRight,
    ];

    /// <summary>The frames <c>Enemies/SpacePirates/WalkingSpacePirateBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] WalkingSpacePirate =
    [
        EnemyFrameId.WalkingPirateFlinchLeft,
        EnemyFrameId.WalkingPirateFlinchRight,
        EnemyFrameId.WalkingPirateWalkingLeftFarLegBack,
        EnemyFrameId.WalkingPirateWalkingLeftFarLegLifting,
        EnemyFrameId.WalkingPirateWalkingLeftFarLegPassing,
        EnemyFrameId.WalkingPirateWalkingLeftFarLegPlanting,
        EnemyFrameId.WalkingPirateWalkingLeftNearLegBack,
        EnemyFrameId.WalkingPirateWalkingLeftNearLegLifting,
        EnemyFrameId.WalkingPirateWalkingLeftNearLegPassing,
        EnemyFrameId.WalkingPirateWalkingLeftNearLegPlanting,
        EnemyFrameId.WalkingPirateStandingFacingLeft,
        EnemyFrameId.WalkingPirateClawsLiftingFacingLeft,
        EnemyFrameId.WalkingPirateClawsForwardFacingLeft,
        EnemyFrameId.WalkingPirateClawsRaisedFacingLeft,
        EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingLeft,
        EnemyFrameId.WalkingPirateFiringFacingLeft,
        EnemyFrameId.WalkingPirateLookingAtViewerFacingLeft,
        EnemyFrameId.WalkingPirateLookingAwayFacingLeft,
        EnemyFrameId.WalkingPirateFacingViewer,
        EnemyFrameId.WalkingPirateWalkingRightFarLegBack,
        EnemyFrameId.WalkingPirateWalkingRightFarLegLifting,
        EnemyFrameId.WalkingPirateWalkingRightFarLegPassing,
        EnemyFrameId.WalkingPirateWalkingRightFarLegPlanting,
        EnemyFrameId.WalkingPirateWalkingRightNearLegBack,
        EnemyFrameId.WalkingPirateWalkingRightNearLegLifting,
        EnemyFrameId.WalkingPirateWalkingRightNearLegPassing,
        EnemyFrameId.WalkingPirateWalkingRightNearLegPlanting,
        EnemyFrameId.WalkingPirateStandingFacingRight,
        EnemyFrameId.WalkingPirateClawsLiftingFacingRight,
        EnemyFrameId.WalkingPirateClawsForwardFacingRight,
        EnemyFrameId.WalkingPirateClawsRaisedFacingRight,
        EnemyFrameId.WalkingPirateClawsRaisedLeaningFacingRight,
        EnemyFrameId.WalkingPirateFiringFacingRight,
        EnemyFrameId.WalkingPirateLookingAtViewerFacingRight,
        EnemyFrameId.WalkingPirateLookingAwayFacingRight,
    ];

    /// <summary>The frames <c>Enemies/SpacePirates/WallSpacePirateBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] WallSpacePirate =
    [
        EnemyFrameId.WallPirateOnRightWallClawsTogether,
        EnemyFrameId.WallPirateOnRightWallClawsSpread,
        EnemyFrameId.WallPirateReachingForLeftWall,
        EnemyFrameId.WallPirateCurledLeapingLeft,
        EnemyFrameId.WallPirateOnLeftWallLegsHanging,
        EnemyFrameId.WallPirateOnLeftWallKneesBending,
        EnemyFrameId.WallPirateOnLeftWallKneesTucked,
        EnemyFrameId.WallPirateOnLeftWallLegsPushing,
        EnemyFrameId.WallPirateOnLeftWallLegsStretched,
        EnemyFrameId.WallPirateOnLeftWallClawsTogether,
        EnemyFrameId.WallPirateOnLeftWallClawsSpread,
        EnemyFrameId.WallPirateReachingForRightWall,
        EnemyFrameId.WallPirateCurledLeapingRight,
        EnemyFrameId.WallPirateOnRightWallLegsHanging,
        EnemyFrameId.WallPirateOnRightWallKneesBending,
        EnemyFrameId.WallPirateOnRightWallKneesTucked,
        EnemyFrameId.WallPirateOnRightWallLegsPushing,
        EnemyFrameId.WallPirateOnRightWallLegsStretched,
    ];

    /// <summary>The frames <c>Enemies/Sparks/SparkBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Spark =
    [
        EnemyFrameId.SparkLargeCross,
        EnemyFrameId.SparkLargeSlant,
        EnemyFrameId.SparkLargeCrossScattering,
        EnemyFrameId.SparkLargeSlantScattering,
        EnemyFrameId.SparkSmallCross,
        EnemyFrameId.SparkSmallSlantTrailing,
        EnemyFrameId.SparkSmallCrossTrailing,
        EnemyFrameId.SparkSmallSlantTrailingFar,
    ];

    /// <summary>The frames <c>Enemies/Waver/WaverBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Waver =
    [
        EnemyFrameId.WaverLevelFacingLeft,
        EnemyFrameId.WaverLevelFacingRight,
        EnemyFrameId.WaverRollingFacingLeftEdgeOn,
        EnemyFrameId.WaverRollingFacingLeftTilted,
        EnemyFrameId.WaverRollingFacingLeftUnderside,
        EnemyFrameId.WaverRollingFacingLeftTurningBack,
        EnemyFrameId.WaverRollingFacingRightEdgeOn,
        EnemyFrameId.WaverRollingFacingRightTilted,
        EnemyFrameId.WaverRollingFacingRightUnderside,
        EnemyFrameId.WaverRollingFacingRightTurningBack,
    ];

    /// <summary>The frames <c>Enemies/WorkRobots/WorkRobotBehavior.cs</c> shows for <c>WorkRobot</c>.</summary>
    private static readonly EnemyFrameId[] WorkRobotFrames =
    [
        EnemyFrameId.WorkRobotWalkingLeftNearFootLeanBack,
        EnemyFrameId.WorkRobotWalkingLeftNearFootEasingUp,
        EnemyFrameId.WorkRobotWalkingLeftNearFootUpright,
        EnemyFrameId.WorkRobotWalkingLeftNearFootEasingForward,
        EnemyFrameId.WorkRobotWalkingLeftNearFootLeanForward,
        EnemyFrameId.WorkRobotWalkingLeftNearFootSettling,
        EnemyFrameId.WorkRobotWalkingLeftFarFootLeanBack,
        EnemyFrameId.WorkRobotWalkingLeftFarFootEasingUp,
        EnemyFrameId.WorkRobotWalkingLeftFarFootUpright,
        EnemyFrameId.WorkRobotWalkingLeftFarFootEasingForward,
        EnemyFrameId.WorkRobotWalkingLeftFarFootLeanForward,
        EnemyFrameId.WorkRobotWalkingLeftFarFootSettling,
        EnemyFrameId.WorkRobotWalkingRightNearFootLeanBack,
        EnemyFrameId.WorkRobotWalkingRightNearFootEasingUp,
        EnemyFrameId.WorkRobotWalkingRightNearFootUpright,
        EnemyFrameId.WorkRobotWalkingRightNearFootEasingForward,
        EnemyFrameId.WorkRobotWalkingRightNearFootLeanForward,
        EnemyFrameId.WorkRobotWalkingRightNearFootSettling,
        EnemyFrameId.WorkRobotWalkingRightFarFootLeanBack,
        EnemyFrameId.WorkRobotWalkingRightFarFootEasingUp,
        EnemyFrameId.WorkRobotWalkingRightFarFootUpright,
        EnemyFrameId.WorkRobotWalkingRightFarFootEasingForward,
        EnemyFrameId.WorkRobotWalkingRightFarFootLeanForward,
        EnemyFrameId.WorkRobotWalkingRightFarFootSettling,
        EnemyFrameId.WorkRobotUnpoweredUpright,
        EnemyFrameId.WorkRobotUnpoweredTiltedLeft,
        EnemyFrameId.WorkRobotUnpoweredTiltedRight,
    ];

    /// <summary>The frames <c>Enemies/WorkRobots/WorkRobotBehavior.cs</c> shows for <c>UnpoweredWorkRobot</c>.</summary>
    private static readonly EnemyFrameId[] UnpoweredWorkRobotFrames =
    [
        EnemyFrameId.WorkRobotUnpoweredUpright,
        EnemyFrameId.WorkRobotUnpoweredTiltedLeft,
        EnemyFrameId.WorkRobotUnpoweredTiltedRight,
    ];

    /// <summary>The frames <c>Enemies/WreckedShip/WreckedShipGhostBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] WreckedShipGhost =
    [
        EnemyFrameId.WreckedShipGhostWispsAtRest,
        EnemyFrameId.WreckedShipGhostWispsCurling,
        EnemyFrameId.WreckedShipGhostWispsStretched,
    ];

    /// <summary>The frames <c>Enemies/YappingMaws/YappingMawBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] YappingMaw =
    [
        EnemyFrameId.YappingMawFacingUpJawsWide,
        EnemyFrameId.YappingMawFacingUpJawsClosing,
        EnemyFrameId.YappingMawFacingUpJawsShut,
        EnemyFrameId.YappingMawFacingUpRightJawsWide,
        EnemyFrameId.YappingMawFacingUpRightJawsClosing,
        EnemyFrameId.YappingMawFacingUpRightJawsShut,
        EnemyFrameId.YappingMawFacingRightJawsWide,
        EnemyFrameId.YappingMawFacingRightJawsClosing,
        EnemyFrameId.YappingMawFacingRightJawsShut,
        EnemyFrameId.YappingMawFacingDownRightJawsWide,
        EnemyFrameId.YappingMawFacingDownRightJawsClosing,
        EnemyFrameId.YappingMawFacingDownRightJawsShut,
        EnemyFrameId.YappingMawFacingDownJawsWide,
        EnemyFrameId.YappingMawFacingDownJawsClosing,
        EnemyFrameId.YappingMawFacingDownJawsShut,
        EnemyFrameId.YappingMawFacingDownLeftJawsWide,
        EnemyFrameId.YappingMawFacingDownLeftJawsClosing,
        EnemyFrameId.YappingMawFacingDownLeftJawsShut,
        EnemyFrameId.YappingMawFacingLeftJawsWide,
        EnemyFrameId.YappingMawFacingLeftJawsClosing,
        EnemyFrameId.YappingMawFacingLeftJawsShut,
        EnemyFrameId.YappingMawFacingUpLeftJawsWide,
        EnemyFrameId.YappingMawFacingUpLeftJawsClosing,
        EnemyFrameId.YappingMawFacingUpLeftJawsShut,
    ];

    /// <summary>The frames <c>Enemies/Yard/YardEnemyBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] YardEnemy =
    [
        EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingUpTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingUpTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingUpTurned67,
        EnemyFrameId.YardCrawlingUpsideUpFacingLeftLegsTucked,
        EnemyFrameId.YardCrawlingUpsideUpFacingLeftLegsSpread,
        EnemyFrameId.YardCrawlingUpsideUpFacingLeftLegsStepping,
        EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingLeftTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingLeftTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingLeftTurned67,
        EnemyFrameId.YardCrawlingUpsideLeftFacingDownLegsTucked,
        EnemyFrameId.YardCrawlingUpsideLeftFacingDownLegsSpread,
        EnemyFrameId.YardCrawlingUpsideLeftFacingDownLegsStepping,
        EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingDownTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingDownTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingDownTurned67,
        EnemyFrameId.YardCrawlingUpsideDownFacingRightLegsTucked,
        EnemyFrameId.YardCrawlingUpsideDownFacingRightLegsSpread,
        EnemyFrameId.YardCrawlingUpsideDownFacingRightLegsStepping,
        EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingRightTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingRightTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingRightTurned67,
        EnemyFrameId.YardCrawlingUpsideRightFacingUpLegsTucked,
        EnemyFrameId.YardCrawlingUpsideRightFacingUpLegsSpread,
        EnemyFrameId.YardCrawlingUpsideRightFacingUpLegsStepping,
        EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingUpTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingUpTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingUpTurned67,
        EnemyFrameId.YardCrawlingUpsideUpFacingRightLegsTucked,
        EnemyFrameId.YardCrawlingUpsideUpFacingRightLegsSpread,
        EnemyFrameId.YardCrawlingUpsideUpFacingRightLegsStepping,
        EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingRightTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingRightTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingRightTurned67,
        EnemyFrameId.YardCrawlingUpsideRightFacingDownLegsTucked,
        EnemyFrameId.YardCrawlingUpsideRightFacingDownLegsSpread,
        EnemyFrameId.YardCrawlingUpsideRightFacingDownLegsStepping,
        EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingDownTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingDownTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingDownTurned67,
        EnemyFrameId.YardCrawlingUpsideDownFacingLeftLegsTucked,
        EnemyFrameId.YardCrawlingUpsideDownFacingLeftLegsSpread,
        EnemyFrameId.YardCrawlingUpsideDownFacingLeftLegsStepping,
        EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingLeftTurned22,
        EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingLeftTurned45,
        EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingLeftTurned67,
        EnemyFrameId.YardCrawlingUpsideLeftFacingUpLegsTucked,
        EnemyFrameId.YardCrawlingUpsideLeftFacingUpLegsSpread,
        EnemyFrameId.YardCrawlingUpsideLeftFacingUpLegsStepping,
        EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingLeftTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingLeftTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingLeftTurned67,
        EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingUpTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingUpTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingUpTurned67,
        EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingRightTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingRightTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingRightTurned67,
        EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingDownTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingDownTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingDownTurned67,
        EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingRightTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingRightTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingRightTurned67,
        EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingUpTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingUpTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingUpTurned67,
        EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingLeftTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingLeftTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingLeftTurned67,
        EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingDownTurned22,
        EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingDownTurned45,
        EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingDownTurned67,
        EnemyFrameId.YardUpsideUpFacingLeftLegsTucking,
        EnemyFrameId.YardUpsideUpFacingLeftShellClosed,
        EnemyFrameId.YardUpsideUpFacingLeftLegsEmerging,
        EnemyFrameId.YardUpsideDownFacingLeftLegsTucking,
        EnemyFrameId.YardUpsideDownFacingLeftShellClosed,
        EnemyFrameId.YardUpsideDownFacingLeftLegsEmerging,
        EnemyFrameId.YardUpsideDownFacingRightLegsTucking,
        EnemyFrameId.YardUpsideDownFacingRightShellClosed,
        EnemyFrameId.YardUpsideDownFacingRightLegsEmerging,
        EnemyFrameId.YardUpsideUpFacingRightLegsTucking,
        EnemyFrameId.YardUpsideUpFacingRightShellClosed,
        EnemyFrameId.YardUpsideUpFacingRightLegsEmerging,
        EnemyFrameId.YardUpsideRightFacingUpLegsTucking,
        EnemyFrameId.YardUpsideRightFacingUpShellClosed,
        EnemyFrameId.YardUpsideRightFacingUpLegsEmerging,
        EnemyFrameId.YardUpsideLeftFacingUpLegsTucking,
        EnemyFrameId.YardUpsideLeftFacingUpShellClosed,
        EnemyFrameId.YardUpsideLeftFacingUpLegsEmerging,
        EnemyFrameId.YardUpsideLeftFacingDownLegsTucking,
        EnemyFrameId.YardUpsideLeftFacingDownShellClosed,
        EnemyFrameId.YardUpsideLeftFacingDownLegsEmerging,
        EnemyFrameId.YardUpsideRightFacingDownLegsTucking,
        EnemyFrameId.YardUpsideRightFacingDownShellClosed,
        EnemyFrameId.YardUpsideRightFacingDownLegsEmerging,
    ];

    /// <summary>The frames <c>Enemies/Zoa/ZoaBehavior.cs</c> shows.</summary>
    private static readonly EnemyFrameId[] Zoa =
    [
        EnemyFrameId.ZoaDartingFacingLeftTailTucked,
        EnemyFrameId.ZoaDartingFacingLeftTailKicking,
        EnemyFrameId.ZoaDartingFacingLeftTailExtended,
        EnemyFrameId.ZoaSwimmingFacingLeftLegsIn,
        EnemyFrameId.ZoaSwimmingFacingLeftLegsPaddling,
        EnemyFrameId.ZoaSwimmingFacingLeftLegsOut,
        EnemyFrameId.ZoaDartingFacingRightTailTucked,
        EnemyFrameId.ZoaDartingFacingRightTailKicking,
        EnemyFrameId.ZoaDartingFacingRightTailExtended,
        EnemyFrameId.ZoaSwimmingFacingRightLegsIn,
        EnemyFrameId.ZoaSwimmingFacingRightLegsPaddling,
        EnemyFrameId.ZoaSwimmingFacingRightLegsOut,
    ];

    /// <summary>The frames each enemy definition can show.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>> Frames { get; } =
        new Dictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>>
        {
            [EnemyDefinitionId.CeresDoor] = CeresDoorFrames,
            [EnemyDefinitionId.CeresRidleyRoomDoor] = CeresRidleyRoomDoorFrames,
            [EnemyDefinitionId.CeresElevatorRoomWall] = CeresElevatorRoomWallFrames,
            [EnemyDefinitionId.CeresGetawayChamberWall] = CeresGetawayChamberWallFrames,
            [EnemyDefinitionId.CeresElevatorDoor] = CeresElevatorDoor,
            [EnemyDefinitionId.CeresSteam] = CeresSteam,
            [EnemyDefinitionId.TourianStatue] = TourianEntranceStatue,
            [EnemyDefinitionId.Botwoon] = Botwoon,
            [EnemyDefinitionId.Crocomire] = CrocomireFrames,
            [EnemyDefinitionId.CrocomireTongue] = CrocomireTongueFrames,
            [EnemyDefinitionId.DraygonArms] = DraygonArmsFrames,
            [EnemyDefinitionId.DraygonTail] = DraygonTailFrames,
            [EnemyDefinitionId.FakeKraid] = FakeKraid,
            [EnemyDefinitionId.KraidArm] = KraidArmFrames,
            [EnemyDefinitionId.KraidFoot] = KraidFootFrames,
            [EnemyDefinitionId.KraidLintBottom] = KraidLintBottomFrames,
            [EnemyDefinitionId.KraidLintMiddle] = KraidLintBottomFrames,
            [EnemyDefinitionId.KraidLintTop] = KraidLintBottomFrames,
            [EnemyDefinitionId.KraidFirstNail] = KraidFirstNailFrames,
            [EnemyDefinitionId.KraidSecondNail] = KraidFirstNailFrames,
            [EnemyDefinitionId.BabyMetroidCutscene] = BabyMetroidCutscene,
            [EnemyDefinitionId.MotherBrainBody] = MotherBrainBodyFrames,
            [EnemyDefinitionId.MotherBrainHead] = MotherBrainHeadFrames,
            [EnemyDefinitionId.MotherBrainTubes] = MotherBrainTubesFrames,
            [EnemyDefinitionId.Rinka] = Rinka,
            [EnemyDefinitionId.BabyMetroid] = BabyMetroid,
            [EnemyDefinitionId.Zebetite] = Zebetite,
            [EnemyDefinitionId.PhantoonBody] = Phantoon,
            [EnemyDefinitionId.PhantoonEye] = Phantoon,
            [EnemyDefinitionId.PhantoonMouth] = Phantoon,
            [EnemyDefinitionId.PhantoonTentacles] = Phantoon,
            [EnemyDefinitionId.Ridley] = RidleyFrames,
            [EnemyDefinitionId.RidleyCeres] = RidleyCeresFrames,
            [EnemyDefinitionId.RidleyBreakupPiece] = RidleyBreakupPieceFrames,
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
            [EnemyDefinitionId.Chozo] = ChozoStatue,
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
            [EnemyDefinitionId.ArmoredZoomer] = ZeelaFrames,
            [EnemyDefinitionId.OrangeZoomer] = ZeelaFrames,
            [EnemyDefinitionId.Multiviola] = Multiviola,
            [EnemyDefinitionId.Dragon] = Dragon,
            [EnemyDefinitionId.Elevator] = Elevator,
            [EnemyDefinitionId.Evir] = EvirFrames,
            [EnemyDefinitionId.EvirProjectile] = EvirProjectileFrames,
            [EnemyDefinitionId.FaceBlock] = BlueBrinstarFaceBlock,
            [EnemyDefinitionId.Fireflea] = Fireflea,
            [EnemyDefinitionId.Mella] = Flies,
            [EnemyDefinitionId.Mellow] = Flies,
            [EnemyDefinitionId.MaridiaFly] = Flies,
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
            [EnemyDefinitionId.MorphBallEye] = MorphBallEye,
            [EnemyDefinitionId.NoobTubeCrack] = NoobTube,
            [EnemyDefinitionId.NuclearWaffle] = NuclearWaffle,
            [EnemyDefinitionId.Owtch] = Owtch,
            [EnemyDefinitionId.Gamet] = GametFrames,
            [EnemyDefinitionId.Geega] = GeegaFrames,
            [EnemyDefinitionId.Zeb] = ZebFrames,
            [EnemyDefinitionId.Zebbo] = ZebboFrames,
            [EnemyDefinitionId.SuspensorPlatform] = SuspensorPlatformFrames,
            [EnemyDefinitionId.Tripper] = TripperFrames,
            [EnemyDefinitionId.Polyp] = Polyp,
            [EnemyDefinitionId.Powamp] = Powamp,
            [EnemyDefinitionId.Puyo] = Puyo,
            [EnemyDefinitionId.Holtz] = LowerNorfairRio,
            [EnemyDefinitionId.Geruta] = NorfairRio,
            [EnemyDefinitionId.Rio] = Rio,
            [EnemyDefinitionId.GrappleRipper] = GrappleRipperFrames,
            [EnemyDefinitionId.Ripper] = RipperFrames,
            [EnemyDefinitionId.JetRipper] = GrappleRipperFrames,
            [EnemyDefinitionId.Sbug] = Sbug,
            [EnemyDefinitionId.Shaktool] = Shaktool,
            [EnemyDefinitionId.Kamer] = KamerFrames,
            [EnemyDefinitionId.ShutterDestroyable] = ShutterDestroyableFrames,
            [EnemyDefinitionId.ShutterGrowing] = ShutterGrowingFrames,
            [EnemyDefinitionId.ShutterShootable] = ShutterDestroyableFrames,
            [EnemyDefinitionId.Skree] = Skree,
            [EnemyDefinitionId.Skultera] = Skultera,
            [EnemyDefinitionId.PirateGoldNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateSilverNinja] = NinjaSpacePirate,
            [EnemyDefinitionId.PirateGoldWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateGreenWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateGreyWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateMagentaWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateRedWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateSilverWalking] = WalkingSpacePirate,
            [EnemyDefinitionId.PirateGoldWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateGreyWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateMagentaWall] = WallSpacePirate,
            [EnemyDefinitionId.PirateSilverWall] = WallSpacePirate,
            [EnemyDefinitionId.Spark] = Spark,
            [EnemyDefinitionId.Waver] = Waver,
            [EnemyDefinitionId.WorkRobot] = WorkRobotFrames,
            [EnemyDefinitionId.UnpoweredWorkRobot] = UnpoweredWorkRobotFrames,
            [EnemyDefinitionId.Coven] = WreckedShipGhost,
            [EnemyDefinitionId.YappingMaw] = YappingMaw,
            [EnemyDefinitionId.Yard] = YardEnemy,
            [EnemyDefinitionId.Zoa] = Zoa,
        };
}
