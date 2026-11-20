using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// How far each of Crocomire's fight-body and melting frames shows his mouth open (#1747, #2176): the opening picks his
/// Power Bomb reaction. The skeleton's frames show no mouth and are absent.
/// </summary>
internal static class CrocomireMouthDefinitions
{
    /// <summary>How far each fight-body and melting frame shows the mouth open.</summary>
    internal static IReadOnlyDictionary<EnemyFrameId, CrocomireMouth> Mouths { get; } = new Dictionary<EnemyFrameId, CrocomireMouth>
    {
        [EnemyFrameId.CrocomireBodyNearFootForwardArmBent] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmSquare] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmReaching] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmSweeping] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmLifting] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmUpright] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmBent] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmSquare] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmReaching] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmSweeping] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmLifting] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmUpright] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyArmUprightMouthOpen] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyArmUprightMouthOpenLess] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyArmUprightMouthClosing] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyArmUprightMouthHalfClosed] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyArmUprightMouthMostlyClosed] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyArmUprightMouthNearlyShut] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyArmUprightMouthShut] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodySwingingArmBent] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodySwingingArmRaised] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodySwingingArmOutstretched] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodySwingingArmLowering] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodySwingingArmReturning] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShut] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthParted] = CrocomireMouth.PartlyOpen,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmBentMouthShut] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmSweepingMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmLiftingMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyNearFootForwardArmUprightMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmBentMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmSquareMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmReachingMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmSweepingMouthGaping] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmLiftingMouthParted] = CrocomireMouth.PartlyOpen,
        [EnemyFrameId.CrocomireBodyFarFootForwardArmUprightMouthShutting] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyLungingArmBent] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyLungingArmRaised] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyLungingArmOutstretched] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyLungingArmLowering] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyLungingArmReturning] = CrocomireMouth.FullyOpen,
        [EnemyFrameId.CrocomireBodyMeltingFleshTopRow] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyMeltingFleshTopTwoRows] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyMeltingFleshTopThreeRows] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyMeltingFleshTopFourRows] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyMeltingBareSkullTopRow] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyMeltingBareSkullTopTwoRows] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyMeltingBareSkullTopThreeRows] = CrocomireMouth.Closed,
        [EnemyFrameId.CrocomireBodyMeltingBareSkullTopFourRows] = CrocomireMouth.Closed,
    };
}
