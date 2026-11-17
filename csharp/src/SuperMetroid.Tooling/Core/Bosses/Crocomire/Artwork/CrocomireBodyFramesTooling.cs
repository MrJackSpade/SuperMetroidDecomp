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
