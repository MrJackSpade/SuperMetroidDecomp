using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Development-tool members locating the cartridge roots of the frames Crocomire's body animations show; never linked by
/// player hosts.
/// </summary>
internal static class CrocomireBodyFramesTooling
{
    /// <summary>The fifty fight-body and melting frames in the order of their cartridge roots.</summary>
    internal static readonly EnemyFrameId[] BodyOrder =
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

    /// <summary>The thirty-three skeleton frames in the order of their cartridge roots.</summary>
    internal static readonly EnemyFrameId[] SkeletonOrder =
    [
        EnemyFrameId.CrocomireBodySkeletonToppling,
        EnemyFrameId.CrocomireBodySkeletonLeaning,
        EnemyFrameId.CrocomireBodySkeletonTipping,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage1,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage2,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage3,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage4,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage5,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage6,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage7,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage8,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage9,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage10,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage11,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage12,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage13,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage14,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage15,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage16,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage17,
        EnemyFrameId.CrocomireBodySkeletonCollapseStage18,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBubblesRisingStage1,
        EnemyFrameId.CrocomireBubblesRisingStage2,
        EnemyFrameId.CrocomireBubblesRisingStage3,
        EnemyFrameId.CrocomireBubblesRisingStage4,
        EnemyFrameId.CrocomireBubblesRisingStage5,
        EnemyFrameId.CrocomireBubblesRisingStage6,
        EnemyFrameId.CrocomireBubblesRisingStage7,
        EnemyFrameId.CrocomireBubblesRisingStage8,
        EnemyFrameId.CrocomireBubblesRisingStage9,
        EnemyFrameId.CrocomireBubblesAdrift,
    ];

    /// <summary>The installed extended spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame)
    {
        int body = Array.IndexOf(BodyOrder, frame);
        if (body >= 0)
            return CrocomireBodyVisualToolingDefinitions.FramePointer(body);
        int skeleton = Array.IndexOf(SkeletonOrder, frame);
        return skeleton >= 0
            ? CrocomireSkeletonVisualToolingDefinitions.FramePointer(skeleton)
            : throw new InvalidOperationException($"Undefined Crocomire body frame {frame}.");
    }
}
