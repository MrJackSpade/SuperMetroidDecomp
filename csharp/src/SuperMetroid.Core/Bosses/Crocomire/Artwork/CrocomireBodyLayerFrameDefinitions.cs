using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>The 42 fight-body frames Crocomire draws partly on his body layer (#1466); their sprite parts are among his installed frames.</summary>
internal static class CrocomireBodyLayerFrameDefinitions
{
    /// <summary>The frames Crocomire draws on his body layer, each a frame of his installed body sheet.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } = new HashSet<EnemyFrameId>
    {
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
    };
}
