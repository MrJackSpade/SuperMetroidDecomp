using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyCueKind;
using P = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodySegment;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's typed body animations (#641, #1600): each body, melting and skeleton segment's beats, and the animation starting at each.</summary>
internal static class CrocomireBodyAnimations
{
    /// <summary>Every body, melting and skeleton segment's beats.</summary>
    internal static AnimationSegments<CrocomireBodySegment, EnemyFrameId, CrocomireBodyCue> Segments { get; } = new(
        new Dictionary<CrocomireBodySegment, AnimationBeat<EnemyFrameId, CrocomireBodyCue>[]>
    {
        [P.Initial] =
        [
            Show(F.CrocomireBodyArmUprightMouthOpen, 1),
            Cue(K.FightAi),
        ],
        [P.ProjectileAttack] =
        [
            Dust(16),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(0),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(-16),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(40),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 5),
            Cue(K.Cry),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyLungingArmBent, 8),
            Show(F.CrocomireBodyLungingArmRaised, 7),
            Show(F.CrocomireBodyLungingArmOutstretched, 7),
            Show(F.CrocomireBodyLungingArmLowering, 7),
            Show(F.CrocomireBodyLungingArmReturning, 7),
        ],
        [P.StepForwardAfterDelay] =
        [
            Show(F.CrocomireBodyNearFootForwardArmBentTwin, 180),
        ],
        [P.StepForward] =
        [
            Cue(K.MaybeStartProjectileAttack),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyNearFootForwardArmSweepingTwin, 16),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Cue(K.FightAi),
            Show(F.CrocomireBodyNearFootForwardArmLiftingTwin, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyNearFootForwardArmUprightTwin, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmBentTwin, 4),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmSquareTwin, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmReachingTwin, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmSweepingTwin, 4),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmLiftingTwin, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwin, 4),
            Cue(K.FightAi),
        ],
        [P.StepBack] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 2),
        ],
        [P.SteppingBack] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUpright, 8),
            Step(CrocomireStepDefinitions.StepBackRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmLifting, 8),
            Step(CrocomireStepDefinitions.StepBack),
            Show(F.CrocomireBodyFarFootForwardArmSweeping, 8),
            Step(CrocomireStepDefinitions.StepBack),
            Show(F.CrocomireBodyFarFootForwardArmReaching, 8),
            Step(CrocomireStepDefinitions.StepBackRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmSquare, 8),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.StepBack),
            Cue(K.FightAi),
        ],
        [P.WaitForFirstSecondDamage] =
        [
            Show(F.CrocomireBodyArmUprightMouthShut, 34),
            Show(F.CrocomireBodyArmUprightMouthNearlyShut, 2),
            Show(F.CrocomireBodyArmUprightMouthMostlyClosed, 2),
            Show(F.CrocomireBodyArmUprightMouthHalfClosed, 2),
            Show(F.CrocomireBodyArmUprightMouthClosing, 2),
            Show(F.CrocomireBodyArmUprightMouthOpenLess, 2),
            Show(F.CrocomireBodyArmUprightMouthOpen, 2),
            Show(F.CrocomireBodyArmUprightMouthOpen, 16),
            Show(F.CrocomireBodyArmUprightMouthOpenLess, 1),
            Show(F.CrocomireBodyArmUprightMouthClosing, 1),
            Show(F.CrocomireBodyArmUprightMouthHalfClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthMostlyClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthNearlyShut, 1),
            Show(F.CrocomireBodyArmUprightMouthShut, 16),
            Show(F.CrocomireBodyArmUprightMouthNearlyShut, 1),
            Show(F.CrocomireBodyArmUprightMouthMostlyClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthHalfClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthClosing, 1),
            Show(F.CrocomireBodyArmUprightMouthOpenLess, 1),
            Show(F.CrocomireBodyArmUprightMouthOpen, 1),
            Show(F.CrocomireBodyArmUprightMouthOpenLess, 1),
            Show(F.CrocomireBodyArmUprightMouthClosing, 1),
            Show(F.CrocomireBodyArmUprightMouthHalfClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthMostlyClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthNearlyShut, 1),
            Show(F.CrocomireBodyArmUprightMouthShut, 1),
            Show(F.CrocomireBodyArmUprightMouthNearlyShut, 1),
            Show(F.CrocomireBodyArmUprightMouthMostlyClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthHalfClosed, 1),
            Show(F.CrocomireBodyArmUprightMouthClosing, 1),
            Show(F.CrocomireBodyArmUprightMouthOpenLess, 1),
            Show(F.CrocomireBodyArmUprightMouthOpen, 1),
            Cue(K.FightAi),
        ],
        [P.MovingClaws] =
        [
            Dust(16),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(0),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(-16),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(40),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Cue(K.FightAi),
        ],
        [P.Roar] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 48),
            Cue(K.Cry),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 32),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 1),
            Cue(K.FightAi),
        ],
        [P.RoarCloseMouth] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 32),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 8),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 2),
        ],
        [P.RoarCloseMouthLoop] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 1),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 1),
            Cue(K.FightAi),
        ],
        [P.PowerBombReactionMouthFullyOpen] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 2),
        ],
        [P.PowerBombReactionMouthPartiallyOpen] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 2),
        ],
        [P.PowerBombReactionMouthNotOpen] =
        [
            Dust(16),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(0),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(-16),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(F.CrocomireBodySwingingArmBent, 5),
            Show(F.CrocomireBodySwingingArmRaised, 5),
            Dust(40),
            Show(F.CrocomireBodySwingingArmOutstretched, 5),
            Show(F.CrocomireBodySwingingArmLowering, 5),
            Show(F.CrocomireBodySwingingArmReturning, 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Show(F.CrocomireBodyNearFootForwardArmBent, 4),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyNearFootForwardArmSquare, 4),
            Step(CrocomireStepDefinitions.Advance),
            Show(F.CrocomireBodyNearFootForwardArmReaching, 4),
            Step(CrocomireStepDefinitions.Advance),
            Show(F.CrocomireBodyNearFootForwardArmSweeping, 4),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyNearFootForwardArmLifting, 4),
            Step(CrocomireStepDefinitions.Advance),
            Show(F.CrocomireBodyNearFootForwardArmUpright, 4),
            Step(CrocomireStepDefinitions.Advance),
            Show(F.CrocomireBodyFarFootForwardArmBent, 4),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmSquare, 4),
            Step(CrocomireStepDefinitions.Advance),
            Show(F.CrocomireBodyFarFootForwardArmReaching, 4),
            Step(CrocomireStepDefinitions.Advance),
            Show(F.CrocomireBodyFarFootForwardArmSweeping, 4),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmLifting, 4),
            Step(CrocomireStepDefinitions.Advance),
            Show(F.CrocomireBodyFarFootForwardArmUpright, 4),
            Cue(K.FightAi),
        ],
        [P.NearSpikeWallCharge] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 8),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyNearFootForwardArmBent, 3),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyNearFootForwardArmSquare, 3),
            Cue(K.StepTowardSpikeWall),
            Show(F.CrocomireBodyNearFootForwardArmReaching, 3),
            Cue(K.StepTowardSpikeWall),
            Show(F.CrocomireBodyNearFootForwardArmSweeping, 3),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyNearFootForwardArmLifting, 3),
            Cue(K.StepTowardSpikeWall),
            Show(F.CrocomireBodyNearFootForwardArmUpright, 3),
            Cue(K.StepTowardSpikeWall),
            Show(F.CrocomireBodyFarFootForwardArmBent, 3),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmSquare, 3),
            Cue(K.StepTowardSpikeWall),
            Show(F.CrocomireBodyFarFootForwardArmReaching, 3),
            Cue(K.StepTowardSpikeWall),
            Show(F.CrocomireBodyFarFootForwardArmSweeping, 3),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmLifting, 3),
            Cue(K.StepTowardSpikeWall),
            Show(F.CrocomireBodyFarFootForwardArmUpright, 3),
            Cue(K.FightAi),
        ],
        [P.BackOffFromSpikeWall] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUpright, 8),
            Step(CrocomireStepDefinitions.BackOffRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmLifting, 8),
            Step(CrocomireStepDefinitions.BackOff),
            Show(F.CrocomireBodyFarFootForwardArmSweeping, 8),
            Step(CrocomireStepDefinitions.BackOff),
            Show(F.CrocomireBodyFarFootForwardArmReaching, 8),
            Step(CrocomireStepDefinitions.BackOffRaisingDust),
            Show(F.CrocomireBodyFarFootForwardArmSquare, 8),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.BackOff),
            Cue(K.FightAi),
        ],
        [P.MeltingOneTopRow] =
        [
            Show(F.CrocomireBodyMeltingOneTopRow, LoopingMeltFrameDuration),
        ],
        [P.MeltingOneTopTwoRows] =
        [
            Hold(F.CrocomireBodyMeltingOneTopTwoRows),
        ],
        [P.MeltingOneTopThreeRows] =
        [
            Hold(F.CrocomireBodyMeltingOneTopThreeRows),
        ],
        [P.MeltingOneTopFourRows] =
        [
            Hold(F.CrocomireBodyMeltingOneTopFourRows),
        ],
        [P.MeltingTwoTopRow] =
        [
            Show(F.CrocomireBodyMeltingTwoTopRow, LoopingMeltFrameDuration),
        ],
        [P.MeltingTwoTopTwoRows] =
        [
            Hold(F.CrocomireBodyMeltingTwoTopTwoRows),
        ],
        [P.MeltingTwoTopThreeRows] =
        [
            Hold(F.CrocomireBodyMeltingTwoTopThreeRows),
        ],
        [P.MeltingTwoTopFourRows] =
        [
            Hold(F.CrocomireBodyMeltingTwoTopFourRows),
        ],
        [P.BridgeCollapsed] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinB, 5),
            Cue(K.Cry),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinC, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightTwinD, 5),
        ],
        [P.SkeletonFalling] =
        [
            Show(F.CrocomireBodySkeletonToppling, 10),
            Show(F.CrocomireBodySkeletonLeaning, 10),
            Show(F.CrocomireBodySkeletonTipping, 10),
        ],
        [P.SkeletonFallsApart] =
        [
            Show(F.CrocomireBodySkeletonCollapsing158By134, 10),
            Dust(-32),
            Show(F.CrocomireBodySkeletonCollapsing141By132, 5),
            Dust(0),
            Show(F.CrocomireBodySkeletonCollapsing141By132, 5),
            Dust(-16),
            Show(F.CrocomireBodySkeletonCollapsing142By127, 5),
            Dust(16),
            Show(F.CrocomireBodySkeletonCollapsing142By127, 5),
            Show(F.CrocomireBodySkeletonCollapsing143By130, 10),
            Show(F.CrocomireBodySkeletonCollapsing143By131, 32),
            Show(F.CrocomireBodySkeletonCollapsing160By130, 16),
            Cue(K.SkeletonCollapseSound),
            Show(F.CrocomireBodySkeletonCollapsing159By122, 10),
            Show(F.CrocomireBodySkeletonCollapsing158By118, 9),
            Show(F.CrocomireBodySkeletonCollapsing158By108, 9),
            Dust(0),
            Show(F.CrocomireBodySkeletonCollapsing158By109, 8),
            Dust(8),
            Show(F.CrocomireBodySkeletonCollapsing177By87, 8),
            Dust(16),
            Show(F.CrocomireBodySkeletonCollapsing174By82, 7),
            Dust(24),
            Show(F.CrocomireBodySkeletonCollapsing185By79, 7),
            Dust(32),
            Show(F.CrocomireBodySkeletonCollapsing189By77, 6),
            Dust(40),
            Show(F.CrocomireBodySkeletonCollapsing173By77, 6),
            Dust(48),
            Show(F.CrocomireBodySkeletonCollapsing153By84, 6),
            Dust(56),
            Show(F.CrocomireBodySkeletonCollapsing120By71, 5),
            Dust(64),
            Cue(K.BigExplosionSound),
            Show(F.CrocomireBodySkeletonCollapsing120By68, 5),
        ],
        [P.SkeletonStable] =
        [
            Hold(F.CrocomireBodySkeletonStable),
        ],
        [P.Dead] =
        [
            Hold(F.CrocomireBodyDead),
        ],
        [P.SkeletonFlowingDownRiver] =
        [
            Show(F.CrocomireBubbles6By7Risen0, 4),
            Show(F.CrocomireBubbles12By12Risen6, 4),
            Show(F.CrocomireBubbles14By13Risen8, 4),
            Show(F.CrocomireBubbles12By14Risen11, 4),
            Show(F.CrocomireBubbles12By14Risen14, 4),
            Show(F.CrocomireBubbles10By13Risen15, 4),
            Show(F.CrocomireBubbles10By12Risen16, 4),
            Show(F.CrocomireBubbles8By10Risen16, 4),
            Show(F.CrocomireBubbles4By6Risen14, 4),
            Show(F.CrocomireBubbles4By6Adrift, 20),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at, each listing only its own segments
    /// (#1938). The step back and the damage wait roar again and again until he is hit (#2180): once the roar has closed
    /// his mouth he shows it closed for an update, and after that update's fight cue, which steps him back on a hit, he
    /// roars again. The wait never runs on into the Power Bomb reaction authored after it.
    /// </summary>
    internal static IReadOnlyDictionary<CrocomireBodySegment, Animation<CrocomireBodySegment>> StartingAt { get; } = new Dictionary<CrocomireBodySegment, Animation<CrocomireBodySegment>>
    {
        [P.Initial] = new([new(P.Initial, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ProjectileAttack] = new([new(P.ProjectileAttack, 0, 29, 1), new(P.ProjectileAttack, 29, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.StepForwardAfterDelay] = new([new(P.StepForwardAfterDelay, 0, 1, 1), new(P.StepForward, 0, 35, 1)], AnimationEnd.LoopsFrom(1)),
        [P.StepForward] = new([new(P.StepForward, 0, 35, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepBack] = new([new(P.StepBack, 0, 1, 1), new(P.SteppingBack, 0, 12, 1), new(P.WaitForFirstSecondDamage, 0, 33, 1), new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(4)),
        [P.SteppingBack] = new([new(P.SteppingBack, 0, 12, 1), new(P.WaitForFirstSecondDamage, 0, 33, 1), new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(3)),
        [P.WaitForFirstSecondDamage] = new([new(P.WaitForFirstSecondDamage, 0, 33, 1), new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(2)),
        [P.MovingClaws] = new([new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.Roar] = new([new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RoarCloseMouth] = new([new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 2, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(2)),
        [P.PowerBombReactionMouthFullyOpen] = new([new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpen, 25, 26, 1)], AnimationEnd.LoopsFrom(3)),
        [P.PowerBombReactionMouthPartiallyOpen] = new([new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpen, 25, 26, 1)], AnimationEnd.LoopsFrom(2)),
        [P.PowerBombReactionMouthNotOpen] = new([new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpen, 25, 26, 1)], AnimationEnd.LoopsFrom(1)),
        [P.NearSpikeWallCharge] = new([new(P.NearSpikeWallCharge, 0, 37, 1), new(P.NearSpikeWallCharge, 37, 26, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BackOffFromSpikeWall] = new([new(P.BackOffFromSpikeWall, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MeltingOneTopRow] = new([new(P.MeltingOneTopRow, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MeltingOneTopTwoRows] = new([new(P.MeltingOneTopTwoRows, 0, 1, 1)], AnimationEnd.Holds),
        [P.MeltingOneTopThreeRows] = new([new(P.MeltingOneTopThreeRows, 0, 1, 1)], AnimationEnd.Holds),
        [P.MeltingOneTopFourRows] = new([new(P.MeltingOneTopFourRows, 0, 1, 1)], AnimationEnd.Holds),
        [P.MeltingTwoTopRow] = new([new(P.MeltingTwoTopRow, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.MeltingTwoTopTwoRows] = new([new(P.MeltingTwoTopTwoRows, 0, 1, 1)], AnimationEnd.Holds),
        [P.MeltingTwoTopThreeRows] = new([new(P.MeltingTwoTopThreeRows, 0, 1, 1)], AnimationEnd.Holds),
        [P.MeltingTwoTopFourRows] = new([new(P.MeltingTwoTopFourRows, 0, 1, 1)], AnimationEnd.Holds),
        [P.BridgeCollapsed] = new([new(P.BridgeCollapsed, 0, 5, 1)], AnimationEnd.Holds),
        [P.SkeletonFalling] = new([new(P.SkeletonFalling, 0, 3, 1)], AnimationEnd.Holds),
        [P.SkeletonFallsApart] = new([new(P.SkeletonFallsApart, 0, 35, 1), new(P.SkeletonStable, 0, 1, 1)], AnimationEnd.Holds),
        [P.SkeletonStable] = new([new(P.SkeletonStable, 0, 1, 1)], AnimationEnd.Holds),
        [P.Dead] = new([new(P.Dead, 0, 1, 1)], AnimationEnd.Holds),
        [P.SkeletonFlowingDownRiver] = new([new(P.SkeletonFlowingDownRiver, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrocomireBodySegment> StartingWith(CrocomireBodySegment segment) => StartingAt[segment];

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#1627): the body rests on it until its owner starts another animation. A shot
    /// that adds to or resets the body's frame timer only moves the update the rest is reached again, which plays nothing.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Hold(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.HeldFrame(frame);

    /// <summary>
    /// The updates each melting pass's first-row frame shows before its animation shows it again, the longest duration a
    /// frame can have, kept exact (#1627): these two animations loop on their frame, so the frame shows anew, raising the new-frame flag,
    /// each time the count runs out, and a shot's added updates move when.
    /// </summary>
    private const int LoopingMeltFrameDuration = 32767;

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Frame(frame, duration);

    /// <summary>Publishes a signalling cue of <paramref name="kind"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Cue(CrocomireBodyCueKind kind) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Event(new CrocomireBodyCue.Signal(kind));

    /// <summary>Publishes a big dust cloud <paramref name="offset"/> pixels from the body.</summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Dust(int offset) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Event(new CrocomireBodyCue.Dust(offset));

    /// <summary>Publishes a walk cue taking <paramref name="step"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Step(CrocomireStep step) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Event(new CrocomireBodyCue.Step(step));
}
