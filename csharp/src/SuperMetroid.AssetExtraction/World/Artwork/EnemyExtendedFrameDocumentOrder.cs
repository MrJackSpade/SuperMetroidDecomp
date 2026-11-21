using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// The frames the cartridge composes from several placed spritemaps, as the installed extended enemy frames document lists
/// them (#1402), run by run of one enemy family each (<see cref="EnemyExtendedFrameDefinitions.FamilyRuns"/>): each run
/// names its own frames, so no position assigns a frame its family (#2565).
/// </summary>
internal static class EnemyExtendedFrameDocumentOrder
{
    /// <summary>The walking Pirates' frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> WalkingPirate { get; } =
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

    /// <summary>The wall Pirates' frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> WallPirate { get; } =
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

    /// <summary>The ninja Pirates' frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> NinjaPirate { get; } =
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

    /// <summary>Ridley's body frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> RidleyBody { get; } =
    [
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

    /// <summary>Draygon's sprite frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> DraygonSprites { get; } =
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

    /// <summary>Spore Spawn's frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> SporeSpawn { get; } =
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

    /// <summary>The Ceres steam frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> CeresSteam { get; } =
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

    /// <summary>Oum's frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> Oum { get; } =
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

    /// <summary>Crocomire's tongue and claw frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> CrocomireTongue { get; } =
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

    /// <summary>Crocomire's fight-body frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> CrocomireBody { get; } =
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
    ];

    /// <summary>Bomb Torizo's dormant statue frame, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> BombTorizoDormant { get; } =
    [
        EnemyFrameId.BombTorizoDormant,
    ];

    /// <summary>The Torizos' third stand-up pose facing left, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> TorizoFacingLeftStandingUpThird { get; } =
    [
        EnemyFrameId.TorizoFacingLeftHalfRisen,
    ];

    /// <summary>Kraid's arm frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> KraidArm { get; } =
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

    /// <summary>Golden Torizo's awakening, walking and rightward turning poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> GoldenTorizoAwakeningWalkingAndTurning { get; } =
    [
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
    ];

    /// <summary>The Torizos' backward-jump poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> TorizoJumpBack { get; } =
    [
        EnemyFrameId.TorizoFacingRightJumpCrouch,
        EnemyFrameId.TorizoFacingRightSpringing,
        EnemyFrameId.TorizoFacingRightLeaping,
    ];

    /// <summary>Golden Torizo's right-facing orb and sonic attack poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> GoldenTorizoRightOrbAndSonic { get; } =
    [
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
    ];

    /// <summary>The Torizos' left-facing springing pose, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> TorizoFacingLeftSpringing { get; } =
    [
        EnemyFrameId.TorizoFacingLeftSpringing,
    ];

    /// <summary>Golden Torizo's left-foot orb poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> GoldenTorizoLeftFootOrb { get; } =
    [
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingHeadRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingMouthOpening,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmThrust,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmRaised,
        EnemyFrameId.TorizoFacingRightLeftFootForwardSpewingArmDrawnBack,
    ];

    /// <summary>The Torizos' further left-facing backward-jump poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> TorizoJumpBackLeft { get; } =
    [
        EnemyFrameId.TorizoFacingLeftJumpCrouch,
        EnemyFrameId.TorizoFacingLeftLeaping,
    ];

    /// <summary>Golden Torizo's left-facing orb poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> GoldenTorizoLeftOrb { get; } =
    [
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
    ];

    /// <summary>The Torizos' further combat poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> TorizoCombat { get; } =
    [
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

    /// <summary>Mother Brain's mixed body poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> MotherBrainBody { get; } =
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
        EnemyFrameId.MotherBrainBodyHandCurling,
        EnemyFrameId.MotherBrainBodyArmDrawnBack,
        EnemyFrameId.MotherBrainBodyArmFlungOut,
        EnemyFrameId.MotherBrainBodyArmReturning,
    ];

    /// <summary>Phantoon's BG2-only poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> PhantoonBg2 { get; } =
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

    /// <summary>Draygon's BG2-only poses, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> DraygonBg2 { get; } =
    [
        EnemyFrameId.DraygonLimpArmFacingLeftClawRaised,
        EnemyFrameId.DraygonLimpArmFacingLeftClawDropping,
        EnemyFrameId.DraygonLimpArmFacingLeftClawDropped,
        EnemyFrameId.DraygonLimpArmFacingLeftClawCurled,
        EnemyFrameId.DraygonBodyFacingLeftJawShut,
        EnemyFrameId.DraygonBodyFacingLeftJawParting,
        EnemyFrameId.DraygonBodyFacingLeftJawOpen,
        EnemyFrameId.DraygonBodyFacingLeftJawWide,
        EnemyFrameId.DraygonEyeFacingLeftShut,
        EnemyFrameId.DraygonEyeFacingLeftOpening,
        EnemyFrameId.DraygonEyeFacingLeftHalfOpen,
        EnemyFrameId.DraygonEyeFacingLeftWide,
        EnemyFrameId.DraygonEyeFacingLeftOpen,
        EnemyFrameId.DraygonEyeFacingLeftBloodshot,
        EnemyFrameId.DraygonEyeFacingLeftNarrowing,
        EnemyFrameId.DraygonEyeFacingLeftClosing,
        EnemyFrameId.DraygonLimpArmFacingRightClawRaised,
        EnemyFrameId.DraygonLimpArmFacingRightClawDropping,
        EnemyFrameId.DraygonLimpArmFacingRightClawDropped,
        EnemyFrameId.DraygonLimpArmFacingRightClawCurled,
        EnemyFrameId.DraygonBodyFacingRightJawShut,
        EnemyFrameId.DraygonBodyFacingRightJawParting,
        EnemyFrameId.DraygonBodyFacingRightJawOpen,
        EnemyFrameId.DraygonBodyFacingRightJawWide,
        EnemyFrameId.DraygonEyeFacingRightShut,
        EnemyFrameId.DraygonEyeFacingRightOpening,
        EnemyFrameId.DraygonEyeFacingRightHalfOpen,
        EnemyFrameId.DraygonEyeFacingRightWide,
        EnemyFrameId.DraygonEyeFacingRightOpen,
        EnemyFrameId.DraygonEyeFacingRightBloodshot,
        EnemyFrameId.DraygonEyeFacingRightNarrowing,
        EnemyFrameId.DraygonEyeFacingRightClosing,
    ];

    /// <summary>Crocomire's corpse and skeleton composite frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> CrocomireSkeleton { get; } =
    [
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

    /// <summary>Kraid's foot frames, in the document's order.</summary>
    internal static ImmutableArray<EnemyFrameId> KraidFoot { get; } =
    [
        EnemyFrameId.KraidFootPlanted,
        EnemyFrameId.KraidFootLifting,
        EnemyFrameId.KraidFootRaised,
        EnemyFrameId.KraidFootSwinging,
        EnemyFrameId.KraidFootReaching,
    ];
}
