using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyCueKind;
using P = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodySegment;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's typed body animations (#641, #1600): each body, melting and skeleton segment's beats, and the animation
/// starting at each, which plays each of its segments whole (#2296). Where he shows one picture twice in a row (his damage
/// wait's open mouth) the showings stay two beats: an uncharged beam on his mouth holds the frame shown for a fixed count,
/// after which the second showing still plays its own duration, and each showing raises the new-frame flag from which his
/// body shows the picture.
/// </summary>
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
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 5),
            Cue(K.Cry),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 5),
        ],
        [P.ProjectileAttackSpitting] =
        [
            Cue(K.FightAi),
            Show(F.CrocomireBodyLungingArmBent, 8),
            Show(F.CrocomireBodyLungingArmRaised, 7),
            Show(F.CrocomireBodyLungingArmOutstretched, 7),
            Show(F.CrocomireBodyLungingArmLowering, 7),
            Show(F.CrocomireBodyLungingArmReturning, 7),
        ],
        [P.StepForwardAfterDelay] =
        [
            Show(F.CrocomireBodyNearFootForwardArmBentMouthShut, 180),
        ],
        [P.StepForward] =
        [
            Cue(K.MaybeStartProjectileAttack),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyNearFootForwardArmSweepingMouthGaping, 16),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Cue(K.FightAi),
            Show(F.CrocomireBodyNearFootForwardArmLiftingMouthGaping, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyNearFootForwardArmUprightMouthGaping, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmBentMouthGaping, 4),
            Cue(K.ShakeScreen),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmSquareMouthGaping, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmReachingMouthGaping, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmSweepingMouthGaping, 4),
            Step(CrocomireStepDefinitions.AdvanceRaisingDust),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmLiftingMouthParted, 4),
            Step(CrocomireStepDefinitions.Advance),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShutting, 4),
            Cue(K.FightAi),
        ],
        [P.StepBack] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 2),
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
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 48),
            Cue(K.Cry),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 32),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 1),
            Cue(K.FightAi),
        ],
        [P.RoarCloseMouth] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 32),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 8),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 2),
        ],
        [P.RoarCloseMouthLoop] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 1),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 1),
            Cue(K.FightAi),
        ],
        [P.PowerBombReactionMouthFullyOpen] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 2),
        ],
        [P.PowerBombReactionMouthPartiallyOpen] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 2),
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
        ],
        [P.PowerBombReactionCharge] =
        [
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
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 8),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 2),
            Cue(K.FightAi),
        ],
        [P.NearSpikeWallChargeSteps] =
        [
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
        [P.MeltingFleshTopRow] =
        [
            Show(F.CrocomireBodyMeltingFleshTopRow, LoopingMeltFrameDuration),
        ],
        [P.MeltingFleshTopTwoRows] =
        [
            Hold(F.CrocomireBodyMeltingFleshTopTwoRows),
        ],
        [P.MeltingFleshTopThreeRows] =
        [
            Hold(F.CrocomireBodyMeltingFleshTopThreeRows),
        ],
        [P.MeltingFleshTopFourRows] =
        [
            Hold(F.CrocomireBodyMeltingFleshTopFourRows),
        ],
        [P.MeltingBareSkullTopRow] =
        [
            Show(F.CrocomireBodyMeltingBareSkullTopRow, LoopingMeltFrameDuration),
        ],
        [P.MeltingBareSkullTopTwoRows] =
        [
            Hold(F.CrocomireBodyMeltingBareSkullTopTwoRows),
        ],
        [P.MeltingBareSkullTopThreeRows] =
        [
            Hold(F.CrocomireBodyMeltingBareSkullTopThreeRows),
        ],
        [P.MeltingBareSkullTopFourRows] =
        [
            Hold(F.CrocomireBodyMeltingBareSkullTopFourRows),
        ],
        [P.BridgeCollapsed] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 5),
            Cue(K.Cry),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 5),
        ],
        [P.SkeletonFalling] =
        [
            Show(F.CrocomireBodySkeletonToppling, 10),
            Show(F.CrocomireBodySkeletonLeaning, 10),
            Show(F.CrocomireBodySkeletonTipping, 10),
        ],
        [P.SkeletonFallsApart] =
        [
            Show(F.CrocomireBodySkeletonCollapseStage1, 10),
            Dust(-32),
            Show(F.CrocomireBodySkeletonCollapseStage2, 5),
            Dust(0),
            Show(F.CrocomireBodySkeletonCollapseStage2, 5),
            Dust(-16),
            Show(F.CrocomireBodySkeletonCollapseStage3, 5),
            Dust(16),
            Show(F.CrocomireBodySkeletonCollapseStage3, 5),
            Show(F.CrocomireBodySkeletonCollapseStage4, 10),
            Show(F.CrocomireBodySkeletonCollapseStage5, 32),
            Show(F.CrocomireBodySkeletonCollapseStage6, 16),
            Cue(K.SkeletonCollapseSound),
            Show(F.CrocomireBodySkeletonCollapseStage7, 10),
            Show(F.CrocomireBodySkeletonCollapseStage8, 9),
            Show(F.CrocomireBodySkeletonCollapseStage9, 9),
            Dust(0),
            Show(F.CrocomireBodySkeletonCollapseStage10, 8),
            Dust(8),
            Show(F.CrocomireBodySkeletonCollapseStage11, 8),
            Dust(16),
            Show(F.CrocomireBodySkeletonCollapseStage12, 7),
            Dust(24),
            Show(F.CrocomireBodySkeletonCollapseStage13, 7),
            Dust(32),
            Show(F.CrocomireBodySkeletonCollapseStage14, 6),
            Dust(40),
            Show(F.CrocomireBodySkeletonCollapseStage15, 6),
            Dust(48),
            Show(F.CrocomireBodySkeletonCollapseStage16, 6),
            Dust(56),
            Show(F.CrocomireBodySkeletonCollapseStage17, 5),
            Dust(64),
            Cue(K.BigExplosionSound),
            Mark(K.SkeletonCollapsed),
            Show(F.CrocomireBodySkeletonCollapseStage18, 5),
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
            Show(F.CrocomireBubblesRisingStage1, 4),
            Show(F.CrocomireBubblesRisingStage2, 4),
            Show(F.CrocomireBubblesRisingStage3, 4),
            Show(F.CrocomireBubblesRisingStage4, 4),
            Show(F.CrocomireBubblesRisingStage5, 4),
            Show(F.CrocomireBubblesRisingStage6, 4),
            Show(F.CrocomireBubblesRisingStage7, 4),
            Show(F.CrocomireBubblesRisingStage8, 4),
            Show(F.CrocomireBubblesRisingStage9, 4),
            Show(F.CrocomireBubblesAdrift, 20),
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
        [P.Initial] = new([Whole(P.Initial)], AnimationEnd.LoopsFrom(0)),
        [P.ProjectileAttack] = new([Whole(P.ProjectileAttack), Whole(P.ProjectileAttackSpitting)], AnimationEnd.LoopsFrom(1)),
        [P.StepForwardAfterDelay] = new([Whole(P.StepForwardAfterDelay), Whole(P.StepForward)], AnimationEnd.LoopsFrom(1)),
        [P.StepForward] = new([Whole(P.StepForward)], AnimationEnd.LoopsFrom(0)),
        [P.StepBack] = new([Whole(P.StepBack), Whole(P.SteppingBack), Whole(P.WaitForFirstSecondDamage), Whole(P.MovingClaws), Whole(P.Roar), Whole(P.RoarCloseMouth), ClosedMouthToFightCue], AnimationEnd.LoopsFrom(4)),
        [P.SteppingBack] = new([Whole(P.SteppingBack), Whole(P.WaitForFirstSecondDamage), Whole(P.MovingClaws), Whole(P.Roar), Whole(P.RoarCloseMouth), ClosedMouthToFightCue], AnimationEnd.LoopsFrom(3)),
        [P.WaitForFirstSecondDamage] = new([Whole(P.WaitForFirstSecondDamage), Whole(P.MovingClaws), Whole(P.Roar), Whole(P.RoarCloseMouth), ClosedMouthToFightCue], AnimationEnd.LoopsFrom(2)),
        [P.MovingClaws] = new([Whole(P.MovingClaws), Whole(P.Roar), Whole(P.RoarCloseMouth), ClosedMouthToFightCue], AnimationEnd.LoopsFrom(1)),
        [P.Roar] = new([Whole(P.Roar), Whole(P.RoarCloseMouth), ClosedMouthToFightCue], AnimationEnd.LoopsFrom(0)),
        [P.RoarCloseMouth] = new([Whole(P.RoarCloseMouth), ClosedMouthToFightCue, Whole(P.Roar), Whole(P.RoarCloseMouth), ClosedMouthToFightCue], AnimationEnd.LoopsFrom(2)),
        [P.PowerBombReactionMouthFullyOpen] = new([Whole(P.PowerBombReactionMouthFullyOpen), Whole(P.PowerBombReactionMouthPartiallyOpen), Whole(P.PowerBombReactionMouthNotOpen), Whole(P.PowerBombReactionCharge)], AnimationEnd.LoopsFrom(3)),
        [P.PowerBombReactionMouthPartiallyOpen] = new([Whole(P.PowerBombReactionMouthPartiallyOpen), Whole(P.PowerBombReactionMouthNotOpen), Whole(P.PowerBombReactionCharge)], AnimationEnd.LoopsFrom(2)),
        [P.PowerBombReactionMouthNotOpen] = new([Whole(P.PowerBombReactionMouthNotOpen), Whole(P.PowerBombReactionCharge)], AnimationEnd.LoopsFrom(1)),
        [P.NearSpikeWallCharge] = new([Whole(P.NearSpikeWallCharge), Whole(P.NearSpikeWallChargeSteps)], AnimationEnd.LoopsFrom(1)),
        [P.BackOffFromSpikeWall] = new([Whole(P.BackOffFromSpikeWall)], AnimationEnd.LoopsFrom(0)),
        [P.MeltingFleshTopRow] = new([Whole(P.MeltingFleshTopRow)], AnimationEnd.LoopsFrom(0)),
        [P.MeltingFleshTopTwoRows] = new([Whole(P.MeltingFleshTopTwoRows)], AnimationEnd.Holds),
        [P.MeltingFleshTopThreeRows] = new([Whole(P.MeltingFleshTopThreeRows)], AnimationEnd.Holds),
        [P.MeltingFleshTopFourRows] = new([Whole(P.MeltingFleshTopFourRows)], AnimationEnd.Holds),
        [P.MeltingBareSkullTopRow] = new([Whole(P.MeltingBareSkullTopRow)], AnimationEnd.LoopsFrom(0)),
        [P.MeltingBareSkullTopTwoRows] = new([Whole(P.MeltingBareSkullTopTwoRows)], AnimationEnd.Holds),
        [P.MeltingBareSkullTopThreeRows] = new([Whole(P.MeltingBareSkullTopThreeRows)], AnimationEnd.Holds),
        [P.MeltingBareSkullTopFourRows] = new([Whole(P.MeltingBareSkullTopFourRows)], AnimationEnd.Holds),
        [P.BridgeCollapsed] = new([Whole(P.BridgeCollapsed)], AnimationEnd.Holds),
        [P.SkeletonFalling] = new([Whole(P.SkeletonFalling)], AnimationEnd.Holds),
        [P.SkeletonFallsApart] = new([Whole(P.SkeletonFallsApart), Whole(P.SkeletonStable)], AnimationEnd.Holds),
        [P.SkeletonStable] = new([Whole(P.SkeletonStable)], AnimationEnd.Holds),
        [P.Dead] = new([Whole(P.Dead)], AnimationEnd.Holds),
        [P.SkeletonFlowingDownRiver] = new([Whole(P.SkeletonFlowingDownRiver)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrocomireBodySegment> StartingWith(CrocomireBodySegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/> (#2296).</summary>
    private static AnimationPhase<CrocomireBodySegment> Whole(CrocomireBodySegment segment) =>
        new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>
    /// The roar's closed mouth played up to and with its first fight cue, after which the animations that play it roar again
    /// (#2180). The segment shows the closed mouth and runs the fight cue once more after that cue, which no play reaches.
    /// </summary>
    private static AnimationPhase<CrocomireBodySegment> ClosedMouthToFightCue =>
        new(P.RoarCloseMouthLoop, 0, Segments.Beats(P.RoarCloseMouthLoop).ToList().FindIndex(IsFightCue) + 1, 1);

    /// <summary>Whether <paramref name="beat"/> runs his fight AI.</summary>
    private static bool IsFightCue(AnimationBeat<EnemyFrameId, CrocomireBodyCue> beat) =>
        beat is AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Event { Raised: CrocomireBodyCue.Signal { Kind: K.FightAi } };

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

    /// <summary>Marks the moment <paramref name="kind"/> names (#2301), which his state notes and which plays nothing.</summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Mark(CrocomireBodyCueKind kind) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Marker(new CrocomireBodyCue.Signal(kind));

    /// <summary>Publishes a big dust cloud <paramref name="offset"/> pixels from the body.</summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Dust(int offset) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Event(new CrocomireBodyCue.Dust(offset));

    /// <summary>Publishes a walk cue taking <paramref name="step"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Step(CrocomireStep step) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Event(new CrocomireBodyCue.Step(step));
}
