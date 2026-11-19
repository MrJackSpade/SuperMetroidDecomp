using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Animation;
using A = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyAnimation;
using K = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyCueKind;
using P = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodySegment;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's typed body animations (#641, #1600): each body, melting and skeleton segment's beats, and each animation his
/// fight and death start, which plays its segments whole or repeated (#2296, #2399): a run several animations show is one
/// segment each plays, and a run shown again and again is one segment played that often. Where he shows one picture twice in a row (his damage
/// wait's open mouth) the showings stay two beats: an uncharged beam on his mouth holds the frame shown for a fixed count,
/// after which the second showing still plays its own duration, and his body shows the picture as each showing begins.
/// </summary>
internal static class CrocomireBodyAnimations
{
    /// <summary>Every body, melting and skeleton segment's beats.</summary>
    internal static AnimationSegments<CrocomireBodySegment, EnemyFrameId, CrocomireBodyCue> Segments { get; } = new(
        new Dictionary<CrocomireBodySegment, AnimationBeat<EnemyFrameId, CrocomireBodyCue>[]>
    {
        [P.IdleSway] =
        [
            Show(F.CrocomireBodyArmUprightMouthOpen, 1),
            Cue(K.FightAi),
        ],
        [P.ClawSwing] =
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
        [P.ProjectileAttackOpenMouth] =
        [
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
        [P.AwaitingHit] =
        [
            Show(F.CrocomireBodyArmUprightMouthShut, 34),
            .. MouthOpening(2),
            Show(F.CrocomireBodyArmUprightMouthOpen, 16),
            .. MouthClosing(1),
            Show(F.CrocomireBodyArmUprightMouthShut, 16),
            .. MouthOpening(1),
            .. MouthClosing(1),
            Show(F.CrocomireBodyArmUprightMouthShut, 1),
            .. MouthOpening(1),
            Cue(K.FightAi),
        ],
        [P.ClawSwingFightCue] =
        [
            Cue(K.FightAi),
        ],
        [P.RoarOpenMouth] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 48),
            Cue(K.Cry),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
        ],
        [P.Gaping] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthGaping, 2),
            Cue(K.FightAi),
        ],
        [P.RoarGapingHold] =
        [
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
        ],
        [P.PowerBombReactionMouthFullyOpen] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 2),
        ],
        [P.PowerBombReactionMouthPartiallyOpen] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 2),
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
        [P.NearSpikeWallChargeOpenMouth] =
        [
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthShut, 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(F.CrocomireBodyFarFootForwardArmUprightMouthParted, 5),
            Cue(K.FightAi),
        ],
        [P.NearSpikeWallChargeCloseMouth] =
        [
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
            Hold(F.CrocomireBodyMeltingFleshTopRow),
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
            Hold(F.CrocomireBodyMeltingBareSkullTopRow),
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
            Show(F.CrocomireBodySkeletonArmHeldLevel, 10),
            Dust(-32),
            Show(F.CrocomireBodySkeletonArmDropping, 5),
            Dust(0),
            Show(F.CrocomireBodySkeletonArmDropping, 5),
            Dust(-16),
            Show(F.CrocomireBodySkeletonArmLowered, 5),
            Dust(16),
            Show(F.CrocomireBodySkeletonArmLowered, 5),
            Show(F.CrocomireBodySkeletonArmPlanted, 10),
            Show(F.CrocomireBodySkeletonBodySettling, 32),
            Show(F.CrocomireBodySkeletonArmSplaying, 16),
            Cue(K.SkeletonCollapseSound),
            Show(F.CrocomireBodySkeletonArmFlat, 10),
            Show(F.CrocomireBodySkeletonRibsSagging, 9),
            Show(F.CrocomireBodySkeletonRibsSlumped, 9),
            Dust(0),
            Show(F.CrocomireBodySkeletonArmFallenOff, 8),
            Dust(8),
            Show(F.CrocomireBodySkeletonTailScattering, 8),
            Dust(16),
            Show(F.CrocomireBodySkeletonSpineFallingApart, 7),
            Dust(24),
            Show(F.CrocomireBodySkeletonRibsSplaying, 7),
            Dust(32),
            Show(F.CrocomireBodySkeletonRibsFlattened, 6),
            Dust(40),
            Show(F.CrocomireBodySkeletonBonesGathering, 6),
            Dust(48),
            Show(F.CrocomireBodySkeletonBonesHuddled, 6),
            Dust(56),
            Show(F.CrocomireBodySkeletonRibsBesideSkull, 5),
            Dust(64),
            Cue(K.BigExplosionSound),
            Mark(K.SkeletonCollapsed),
            Show(F.CrocomireBodySkeletonLastBones, 5),
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
            Show(F.CrocomireBubblesForming, 4),
            Show(F.CrocomireBubblesWelling, 4),
            Show(F.CrocomireBubblesSpreading, 4),
            Show(F.CrocomireBubblesBursting, 4),
            Show(F.CrocomireBubblesSpraying, 4),
            Show(F.CrocomireBubblesScattered, 4),
            Show(F.CrocomireBubblesThinning, 4),
            Show(F.CrocomireBubblesFading, 4),
            Show(F.CrocomireBubblesVanishing, 4),
            Show(F.CrocomireBubblesAdrift, 20),
        ],
    });

    /// <summary>How many times his roar shows his gaping mouth, each time with its fight cue, before its long gaping hold.</summary>
    private const int RoarGapings = 13;

    /// <summary>How many times his charge near the spike wall shows his gaping mouth, each time with its fight cue, before closing it.</summary>
    private const int NearSpikeWallChargeGapings = 13;

    /// <summary>
    /// His roar while he waits for a hit (#2180): it opens his mouth, gapes, holds it gaping and closes it; once the roar has
    /// closed his mouth he shows it closed for an update, and after that update's fight cue, which steps him back on a hit,
    /// he roars again. The wait never runs on into the Power Bomb reaction authored after it.
    /// </summary>
    private static AnimationPhase<CrocomireBodySegment>[] Roaring { get; } =
    [
        Segments.Whole(P.RoarOpenMouth),
        Segments.Repeated(P.Gaping, RoarGapings),
        Segments.Whole(P.RoarGapingHold),
        Segments.Whole(P.RoarCloseMouth),
        Segments.Whole(P.RoarCloseMouthLoop),
    ];

    /// <summary>
    /// Each animation his fight and death can start or restart, each listing only its own segments (#1938, #2399). The step
    /// back and the damage wait lead into his roar, which plays again and again until he is hit.
    /// </summary>
    internal static IReadOnlyDictionary<CrocomireBodyAnimation, Animation<CrocomireBodySegment>> StartingAt { get; } = new Dictionary<CrocomireBodyAnimation, Animation<CrocomireBodySegment>>
    {
        [A.IdleSway] = Animation<CrocomireBodySegment>.Looping([Segments.Whole(P.IdleSway)]),
        [A.ProjectileAttack] = Animation<CrocomireBodySegment>.LeadingInto(
            [Segments.Whole(P.ClawSwing), Segments.Whole(P.ProjectileAttackOpenMouth)], [Segments.Whole(P.ProjectileAttackSpitting)]),
        [A.StepForwardAfterDelay] = Animation<CrocomireBodySegment>.LeadingInto([Segments.Whole(P.StepForwardAfterDelay)], [Segments.Whole(P.StepForward)]),
        [A.StepForward] = Animation<CrocomireBodySegment>.Looping([Segments.Whole(P.StepForward)]),
        [A.StepBack] = Animation<CrocomireBodySegment>.LeadingInto(
            [Segments.Whole(P.StepBack), Segments.Whole(P.SteppingBack), Segments.Whole(P.AwaitingHit), Segments.Whole(P.ClawSwing), Segments.Whole(P.ClawSwingFightCue)],
            Roaring),
        [A.SteppingBack] = Animation<CrocomireBodySegment>.LeadingInto(
            [Segments.Whole(P.SteppingBack), Segments.Whole(P.AwaitingHit), Segments.Whole(P.ClawSwing), Segments.Whole(P.ClawSwingFightCue)], Roaring),
        [A.AwaitingHit] = Animation<CrocomireBodySegment>.LeadingInto(
            [Segments.Whole(P.AwaitingHit), Segments.Whole(P.ClawSwing), Segments.Whole(P.ClawSwingFightCue)], Roaring),
        [A.MovingClaws] = Animation<CrocomireBodySegment>.LeadingInto([Segments.Whole(P.ClawSwing), Segments.Whole(P.ClawSwingFightCue)], Roaring),
        [A.Roar] = Animation<CrocomireBodySegment>.Looping(Roaring),
        [A.RoarCloseMouth] = Animation<CrocomireBodySegment>.LeadingInto([Segments.Whole(P.RoarCloseMouth), Segments.Whole(P.RoarCloseMouthLoop)], Roaring),
        [A.PowerBombReactionMouthFullyOpen] = Animation<CrocomireBodySegment>.LeadingInto(
            [Segments.Whole(P.PowerBombReactionMouthFullyOpen), Segments.Whole(P.PowerBombReactionMouthPartiallyOpen), Segments.Whole(P.ClawSwing)],
            [Segments.Whole(P.PowerBombReactionCharge)]),
        [A.PowerBombReactionMouthPartiallyOpen] = Animation<CrocomireBodySegment>.LeadingInto(
            [Segments.Whole(P.PowerBombReactionMouthPartiallyOpen), Segments.Whole(P.ClawSwing)], [Segments.Whole(P.PowerBombReactionCharge)]),
        [A.PowerBombReactionMouthNotOpen] = Animation<CrocomireBodySegment>.LeadingInto([Segments.Whole(P.ClawSwing)], [Segments.Whole(P.PowerBombReactionCharge)]),
        [A.NearSpikeWallCharge] = Animation<CrocomireBodySegment>.LeadingInto(
            [Segments.Whole(P.NearSpikeWallChargeOpenMouth), Segments.Repeated(P.Gaping, NearSpikeWallChargeGapings), Segments.Whole(P.NearSpikeWallChargeCloseMouth)],
            [Segments.Whole(P.NearSpikeWallChargeSteps)]),
        [A.BackOffFromSpikeWall] = Animation<CrocomireBodySegment>.Looping([Segments.Whole(P.BackOffFromSpikeWall)]),
        [A.MeltingFleshTopRow] = new([Segments.Whole(P.MeltingFleshTopRow)], AnimationEnd.Holds),
        [A.MeltingFleshTopTwoRows] = new([Segments.Whole(P.MeltingFleshTopTwoRows)], AnimationEnd.Holds),
        [A.MeltingFleshTopThreeRows] = new([Segments.Whole(P.MeltingFleshTopThreeRows)], AnimationEnd.Holds),
        [A.MeltingFleshTopFourRows] = new([Segments.Whole(P.MeltingFleshTopFourRows)], AnimationEnd.Holds),
        [A.MeltingBareSkullTopRow] = new([Segments.Whole(P.MeltingBareSkullTopRow)], AnimationEnd.Holds),
        [A.MeltingBareSkullTopTwoRows] = new([Segments.Whole(P.MeltingBareSkullTopTwoRows)], AnimationEnd.Holds),
        [A.MeltingBareSkullTopThreeRows] = new([Segments.Whole(P.MeltingBareSkullTopThreeRows)], AnimationEnd.Holds),
        [A.MeltingBareSkullTopFourRows] = new([Segments.Whole(P.MeltingBareSkullTopFourRows)], AnimationEnd.Holds),
        [A.BridgeCollapsed] = new([Segments.Whole(P.BridgeCollapsed)], AnimationEnd.Holds),
        [A.SkeletonFalling] = new([Segments.Whole(P.SkeletonFalling)], AnimationEnd.Holds),
        [A.SkeletonFallsApart] = new([Segments.Whole(P.SkeletonFallsApart), Segments.Whole(P.SkeletonStable)], AnimationEnd.Holds),
        [A.SkeletonStable] = new([Segments.Whole(P.SkeletonStable)], AnimationEnd.Holds),
        [A.Dead] = new([Segments.Whole(P.Dead)], AnimationEnd.Holds),
        [A.SkeletonFlowingDownRiver] = Animation<CrocomireBodySegment>.Looping([Segments.Whole(P.SkeletonFlowingDownRiver)]),
    };

    /// <summary>The animation <paramref name="animation"/> names.</summary>
    internal static Animation<CrocomireBodySegment> StartingWith(CrocomireBodyAnimation animation) => StartingAt[animation];

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#1627): the body rests on it until its owner starts another animation. A shot
    /// that adds to or resets the body's frame timer only moves the update the rest is reached again, which plays nothing.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue> Hold(EnemyFrameId frame) =>
        new AnimationBeat<EnemyFrameId, CrocomireBodyCue>.HeldFrame(frame);

    /// <summary>
    /// His mouth opening as he waits for damage, his arm upright (#2465): from nearly shut through closing and opening less
    /// to open, each picture for <paramref name="updates"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue>[] MouthOpening(int updates) =>
    [
        Show(F.CrocomireBodyArmUprightMouthNearlyShut, updates),
        Show(F.CrocomireBodyArmUprightMouthMostlyClosed, updates),
        Show(F.CrocomireBodyArmUprightMouthHalfClosed, updates),
        Show(F.CrocomireBodyArmUprightMouthClosing, updates),
        Show(F.CrocomireBodyArmUprightMouthOpenLess, updates),
        Show(F.CrocomireBodyArmUprightMouthOpen, updates),
    ];

    /// <summary>
    /// His mouth closing as he waits for damage, his arm upright (#2465): from opening less back to nearly shut, each
    /// picture for <paramref name="updates"/>; the shut mouth that follows is held as long as the wait says.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, CrocomireBodyCue>[] MouthClosing(int updates) =>
    [
        Show(F.CrocomireBodyArmUprightMouthOpenLess, updates),
        Show(F.CrocomireBodyArmUprightMouthClosing, updates),
        Show(F.CrocomireBodyArmUprightMouthHalfClosed, updates),
        Show(F.CrocomireBodyArmUprightMouthMostlyClosed, updates),
        Show(F.CrocomireBodyArmUprightMouthNearlyShut, updates),
    ];

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
