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
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwinB,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwinC,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwinD,
        EnemyFrameId.CrocomireBodyNearFootForwardArmBentTwin,
        EnemyFrameId.CrocomireBodyNearFootForwardArmSweepingTwin,
        EnemyFrameId.CrocomireBodyNearFootForwardArmLiftingTwin,
        EnemyFrameId.CrocomireBodyNearFootForwardArmUprightTwin,
        EnemyFrameId.CrocomireBodyFarFootForwardArmBentTwin,
        EnemyFrameId.CrocomireBodyFarFootForwardArmSquareTwin,
        EnemyFrameId.CrocomireBodyFarFootForwardArmReachingTwin,
        EnemyFrameId.CrocomireBodyFarFootForwardArmSweepingTwin,
        EnemyFrameId.CrocomireBodyFarFootForwardArmLiftingTwin,
        EnemyFrameId.CrocomireBodyFarFootForwardArmUprightTwin,
        EnemyFrameId.CrocomireBodyLungingArmBent,
        EnemyFrameId.CrocomireBodyLungingArmRaised,
        EnemyFrameId.CrocomireBodyLungingArmOutstretched,
        EnemyFrameId.CrocomireBodyLungingArmLowering,
        EnemyFrameId.CrocomireBodyLungingArmReturning,
        EnemyFrameId.CrocomireBodyMeltingOneTopRow,
        EnemyFrameId.CrocomireBodyMeltingOneTopTwoRows,
        EnemyFrameId.CrocomireBodyMeltingOneTopThreeRows,
        EnemyFrameId.CrocomireBodyMeltingOneTopFourRows,
        EnemyFrameId.CrocomireBodyMeltingTwoTopRow,
        EnemyFrameId.CrocomireBodyMeltingTwoTopTwoRows,
        EnemyFrameId.CrocomireBodyMeltingTwoTopThreeRows,
        EnemyFrameId.CrocomireBodyMeltingTwoTopFourRows,
    ];

    /// <summary>The thirty-three skeleton frames in the order of their cartridge roots.</summary>
    internal static readonly EnemyFrameId[] SkeletonOrder =
    [
        EnemyFrameId.CrocomireBodySkeletonToppling,
        EnemyFrameId.CrocomireBodySkeletonLeaning,
        EnemyFrameId.CrocomireBodySkeletonTipping,
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By134,
        EnemyFrameId.CrocomireBodySkeletonCollapsing141By132,
        EnemyFrameId.CrocomireBodySkeletonCollapsing142By127,
        EnemyFrameId.CrocomireBodySkeletonCollapsing143By130,
        EnemyFrameId.CrocomireBodySkeletonCollapsing143By131,
        EnemyFrameId.CrocomireBodySkeletonCollapsing160By130,
        EnemyFrameId.CrocomireBodySkeletonCollapsing159By122,
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By118,
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By108,
        EnemyFrameId.CrocomireBodySkeletonCollapsing158By109,
        EnemyFrameId.CrocomireBodySkeletonCollapsing177By87,
        EnemyFrameId.CrocomireBodySkeletonCollapsing174By82,
        EnemyFrameId.CrocomireBodySkeletonCollapsing185By79,
        EnemyFrameId.CrocomireBodySkeletonCollapsing189By77,
        EnemyFrameId.CrocomireBodySkeletonCollapsing173By77,
        EnemyFrameId.CrocomireBodySkeletonCollapsing153By84,
        EnemyFrameId.CrocomireBodySkeletonCollapsing120By71,
        EnemyFrameId.CrocomireBodySkeletonCollapsing120By68,
        EnemyFrameId.CrocomireBodySkeletonStable,
        EnemyFrameId.CrocomireBodyDead,
        EnemyFrameId.CrocomireBubbles6By7Risen0,
        EnemyFrameId.CrocomireBubbles12By12Risen6,
        EnemyFrameId.CrocomireBubbles14By13Risen8,
        EnemyFrameId.CrocomireBubbles12By14Risen11,
        EnemyFrameId.CrocomireBubbles12By14Risen14,
        EnemyFrameId.CrocomireBubbles10By13Risen15,
        EnemyFrameId.CrocomireBubbles10By12Risen16,
        EnemyFrameId.CrocomireBubbles8By10Risen16,
        EnemyFrameId.CrocomireBubbles4By6Risen14,
        EnemyFrameId.CrocomireBubbles4By6Adrift,
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
