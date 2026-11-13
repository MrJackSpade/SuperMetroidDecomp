using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyCueKind;
using P = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyProgram;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's typed body animations (#641, #1600): each body, melting and skeleton segment's beats, and the animation starting at each.</summary>
internal static class CrocomireBodyAnimations
{
    /// <summary>Every body, melting and skeleton segment's beats.</summary>
    internal static AnimationSegments<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> Segments { get; } = new(
        new Dictionary<CrocomireBodyProgram, AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue>[]>
    {
        [P.Initial] =
        [
            Show(Body(12), 1),
            Cue(K.FightAi),
        ],
        [P.UnusedChargeForwardOneStep] =
        [
            Show(Body(0), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(1), 8),
            Cue(K.MoveLeft),
            Show(Body(2), 8),
            Cue(K.MoveLeft),
            Show(Body(3), 8),
            Cue(K.MoveLeftWithDust),
            Show(Body(4), 8),
            Cue(K.MoveLeft),
            Show(Body(5), 8),
            Cue(K.MoveLeft),
            Show(Body(6), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(7), 8),
            Cue(K.MoveLeft),
            Show(Body(8), 8),
            Cue(K.MoveLeft),
            Show(Body(9), 8),
            Cue(K.MoveLeftWithDust),
            Show(Body(10), 8),
            Cue(K.MoveLeft),
            Show(Body(11), 8),
            Cue(K.FightAi),
        ],
        [P.ProjectileAttack] =
        [
            Dust(16),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(0),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(-16),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(40),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Show(Body(24), 5),
            Cue(K.Cry),
            Show(Body(25), 5),
            Show(Body(26), 5),
        ],
        [P.ProjectileAttackLoop] =
        [
            Cue(K.FightAi),
            Show(Body(37), 8),
            Show(Body(38), 7),
            Show(Body(39), 7),
            Show(Body(40), 7),
            Show(Body(41), 7),
        ],
        [P.StepForwardAfterDelay] =
        [
            Show(Body(27), 180),
        ],
        [P.StepForward] =
        [
            Cue(K.MaybeStartProjectileAttack),
            Show(Body(24), 5),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(Body(26), 5),
            Cue(K.FightAi),
            Show(Body(28), 16),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(Body(29), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(30), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(31), 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(Body(32), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(33), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(34), 4),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(Body(35), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(36), 4),
            Cue(K.FightAi),
        ],
        [P.StepBack] =
        [
            Show(Body(25), 2),
        ],
        [P.SteppingBack] =
        [
            Show(Body(11), 8),
            Cue(K.MoveRightIfOnScreenWithDust),
            Show(Body(10), 8),
            Cue(K.MoveRightIfOnScreen),
            Show(Body(9), 8),
            Cue(K.MoveRightIfOnScreen),
            Show(Body(8), 8),
            Cue(K.MoveRightIfOnScreenWithDust),
            Show(Body(7), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveRightIfOnScreen),
            Cue(K.FightAi),
        ],
        [P.WaitForFirstSecondDamage] =
        [
            Show(Body(18), 34),
            Show(Body(17), 2),
            Show(Body(16), 2),
            Show(Body(15), 2),
            Show(Body(14), 2),
            Show(Body(13), 2),
            Show(Body(12), 2),
            Show(Body(12), 16),
            Show(Body(13), 1),
            Show(Body(14), 1),
            Show(Body(15), 1),
            Show(Body(16), 1),
            Show(Body(17), 1),
            Show(Body(18), 16),
            Show(Body(17), 1),
            Show(Body(16), 1),
            Show(Body(15), 1),
            Show(Body(14), 1),
            Show(Body(13), 1),
            Show(Body(12), 1),
            Show(Body(13), 1),
            Show(Body(14), 1),
            Show(Body(15), 1),
            Show(Body(16), 1),
            Show(Body(17), 1),
            Show(Body(18), 1),
            Show(Body(17), 1),
            Show(Body(16), 1),
            Show(Body(15), 1),
            Show(Body(14), 1),
            Show(Body(13), 1),
            Show(Body(12), 1),
            Cue(K.FightAi),
        ],
        [P.MovingClaws] =
        [
            Dust(16),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(0),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(-16),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(40),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Cue(K.FightAi),
        ],
        [P.Roar] =
        [
            Show(Body(24), 48),
            Cue(K.Cry),
            Show(Body(25), 5),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 32),
            Cue(K.FightAi),
            Show(Body(26), 1),
            Cue(K.FightAi),
        ],
        [P.RoarCloseMouth] =
        [
            Show(Body(26), 32),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.FightAi),
            Show(Body(25), 8),
            Show(Body(24), 2),
        ],
        [P.RoarCloseMouthLoop] =
        [
            Show(Body(24), 1),
            Cue(K.FightAi),
            Show(Body(24), 1),
            Cue(K.FightAi),
        ],
        [P.PowerBombReactionMouthFullyOpen] =
        [
            Show(Body(25), 2),
        ],
        [P.PowerBombReactionMouthPartiallyOpen] =
        [
            Show(Body(24), 2),
        ],
        [P.PowerBombReactionMouthNotOpen] =
        [
            Dust(16),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(0),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(-16),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(40),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
        ],
        [P.PowerBombReactionMouthNotOpenLoop] =
        [
            Show(Body(0), 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(1), 4),
            Cue(K.MoveLeft),
            Show(Body(2), 4),
            Cue(K.MoveLeft),
            Show(Body(3), 4),
            Cue(K.MoveLeftWithDust),
            Show(Body(4), 4),
            Cue(K.MoveLeft),
            Show(Body(5), 4),
            Cue(K.MoveLeft),
            Show(Body(6), 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(7), 4),
            Cue(K.MoveLeft),
            Show(Body(8), 4),
            Cue(K.MoveLeft),
            Show(Body(9), 4),
            Cue(K.MoveLeftWithDust),
            Show(Body(10), 4),
            Cue(K.MoveLeft),
            Show(Body(11), 4),
            Cue(K.FightAi),
        ],
        [P.NearSpikeWallCharge] =
        [
            Show(Body(24), 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.FightAi),
            Show(Body(25), 8),
            Cue(K.FightAi),
            Show(Body(24), 2),
            Cue(K.FightAi),
        ],
        [P.NearSpikeWallChargeLoop] =
        [
            Show(Body(0), 3),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(1), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(2), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(3), 3),
            Cue(K.MoveLeftWithDust),
            Show(Body(4), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(5), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(6), 3),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(7), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(8), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(9), 3),
            Cue(K.MoveLeftWithDust),
            Show(Body(10), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(11), 3),
            Cue(K.FightAi),
        ],
        [P.BackOffFromSpikeWall] =
        [
            Show(Body(11), 8),
            Cue(K.MoveRightWithDust),
            Show(Body(10), 8),
            Cue(K.MoveRight),
            Show(Body(9), 8),
            Cue(K.MoveRight),
            Show(Body(8), 8),
            Cue(K.MoveRightWithDust),
            Show(Body(7), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveRight),
            Cue(K.FightAi),
        ],
        [P.MeltingOneTopRow] =
        [
            Show(Body(42), LoopingMeltFrameDuration),
        ],
        [P.MeltingOneTopTwoRows] =
        [
            Hold(Body(43)),
        ],
        [P.MeltingOneTopThreeRows] =
        [
            Hold(Body(44)),
        ],
        [P.MeltingOneTopFourRows] =
        [
            Hold(Body(45)),
        ],
        [P.MeltingTwoTopRow] =
        [
            Show(Body(46), LoopingMeltFrameDuration),
        ],
        [P.MeltingTwoTopTwoRows] =
        [
            Hold(Body(47)),
        ],
        [P.MeltingTwoTopThreeRows] =
        [
            Hold(Body(48)),
        ],
        [P.MeltingTwoTopFourRows] =
        [
            Hold(Body(49)),
        ],
        [P.BridgeCollapsed] =
        [
            Show(Body(25), 5),
            Show(Body(24), 5),
            Cue(K.Cry),
            Show(Body(25), 5),
            Show(Body(26), 5),
        ],
        [P.SkeletonFalling] =
        [
            Show(Skeleton(0), 10),
            Show(Skeleton(1), 10),
            Show(Skeleton(2), 10),
        ],
        [P.SkeletonFallsApart] =
        [
            Show(Skeleton(3), 10),
            Dust(-32),
            Show(Skeleton(4), 5),
            Dust(0),
            Show(Skeleton(4), 5),
            Dust(-16),
            Show(Skeleton(5), 5),
            Dust(16),
            Show(Skeleton(5), 5),
            Show(Skeleton(6), 10),
            Show(Skeleton(7), 32),
            Show(Skeleton(8), 16),
            Cue(K.SkeletonCollapseSound),
            Show(Skeleton(9), 10),
            Show(Skeleton(10), 9),
            Show(Skeleton(11), 9),
            Dust(0),
            Show(Skeleton(12), 8),
            Dust(8),
            Show(Skeleton(13), 8),
            Dust(16),
            Show(Skeleton(14), 7),
            Dust(24),
            Show(Skeleton(15), 7),
            Dust(32),
            Show(Skeleton(16), 6),
            Dust(40),
            Show(Skeleton(17), 6),
            Dust(48),
            Show(Skeleton(18), 6),
            Dust(56),
            Show(Skeleton(19), 5),
            Dust(64),
            Cue(K.BigExplosionSound),
            Show(Skeleton(20), 5),
        ],
        [P.SkeletonStable] =
        [
            Hold(Skeleton(21)),
        ],
        [P.Dead] =
        [
            Hold(Skeleton(22)),
        ],
        [P.SkeletonFlowingDownRiver] =
        [
            Show(Skeleton(23), 4),
            Show(Skeleton(24), 4),
            Show(Skeleton(25), 4),
            Show(Skeleton(26), 4),
            Show(Skeleton(27), 4),
            Show(Skeleton(28), 4),
            Show(Skeleton(29), 4),
            Show(Skeleton(30), 4),
            Show(Skeleton(31), 4),
            Show(Skeleton(32), 20),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CrocomireBodyProgram, Animation<CrocomireBodyProgram>> StartingAt { get; } = new Dictionary<CrocomireBodyProgram, Animation<CrocomireBodyProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UnusedChargeForwardOneStep] = new([new(P.UnusedChargeForwardOneStep, 0, 26, 1), new(P.ProjectileAttack, 0, 29, 1), new(P.ProjectileAttackLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.ProjectileAttack] = new([new(P.ProjectileAttack, 0, 29, 1), new(P.ProjectileAttackLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ProjectileAttackLoop] = new([new(P.ProjectileAttackLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepForwardAfterDelay] = new([new(P.StepForwardAfterDelay, 0, 1, 1), new(P.StepForward, 0, 35, 1)], AnimationEnd.LoopsFrom(1)),
        [P.StepForward] = new([new(P.StepForward, 0, 35, 1)], AnimationEnd.LoopsFrom(0)),
        [P.StepBack] = new([new(P.StepBack, 0, 1, 1), new(P.SteppingBack, 0, 12, 1), new(P.WaitForFirstSecondDamage, 0, 33, 1), new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 4, 1), new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(10)),
        [P.SteppingBack] = new([new(P.SteppingBack, 0, 12, 1), new(P.WaitForFirstSecondDamage, 0, 33, 1), new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 4, 1), new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(9)),
        [P.WaitForFirstSecondDamage] = new([new(P.WaitForFirstSecondDamage, 0, 33, 1), new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 4, 1), new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(8)),
        [P.MovingClaws] = new([new(P.MovingClaws, 0, 26, 1), new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 4, 1), new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(7)),
        [P.Roar] = new([new(P.Roar, 0, 33, 1), new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 4, 1), new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(6)),
        [P.RoarCloseMouth] = new([new(P.RoarCloseMouth, 0, 6, 1), new(P.RoarCloseMouthLoop, 0, 4, 1), new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(5)),
        [P.RoarCloseMouthLoop] = new([new(P.RoarCloseMouthLoop, 0, 4, 1), new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(4)),
        [P.PowerBombReactionMouthFullyOpen] = new([new(P.PowerBombReactionMouthFullyOpen, 0, 1, 1), new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(3)),
        [P.PowerBombReactionMouthPartiallyOpen] = new([new(P.PowerBombReactionMouthPartiallyOpen, 0, 1, 1), new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(2)),
        [P.PowerBombReactionMouthNotOpen] = new([new(P.PowerBombReactionMouthNotOpen, 0, 25, 1), new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(1)),
        [P.PowerBombReactionMouthNotOpenLoop] = new([new(P.PowerBombReactionMouthNotOpenLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(0)),
        [P.NearSpikeWallCharge] = new([new(P.NearSpikeWallCharge, 0, 37, 1), new(P.NearSpikeWallChargeLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(1)),
        [P.NearSpikeWallChargeLoop] = new([new(P.NearSpikeWallChargeLoop, 0, 26, 1)], AnimationEnd.LoopsFrom(0)),
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
    internal static Animation<CrocomireBodyProgram> StartingWith(CrocomireBodyProgram segment) => StartingAt[segment];

    /// <summary>
    /// The segment each segment that runs on past its last beat continues into, directly after that beat: a body
    /// resting after that beat already stands in the next, which the fight AI and the skeleton's rest test read.
    /// </summary>
    internal static IReadOnlyDictionary<CrocomireBodyProgram, CrocomireBodyProgram> RunsOnInto { get; } = new Dictionary<CrocomireBodyProgram, CrocomireBodyProgram>
    {
        [P.UnusedChargeForwardOneStep] = P.ProjectileAttack,
        [P.ProjectileAttack] = P.ProjectileAttackLoop,
        [P.StepForwardAfterDelay] = P.StepForward,
        [P.StepBack] = P.SteppingBack,
        [P.SteppingBack] = P.WaitForFirstSecondDamage,
        [P.WaitForFirstSecondDamage] = P.MovingClaws,
        [P.MovingClaws] = P.Roar,
        [P.Roar] = P.RoarCloseMouth,
        [P.RoarCloseMouth] = P.RoarCloseMouthLoop,
        [P.RoarCloseMouthLoop] = P.PowerBombReactionMouthFullyOpen,
        [P.PowerBombReactionMouthFullyOpen] = P.PowerBombReactionMouthPartiallyOpen,
        [P.PowerBombReactionMouthPartiallyOpen] = P.PowerBombReactionMouthNotOpen,
        [P.PowerBombReactionMouthNotOpen] = P.PowerBombReactionMouthNotOpenLoop,
        [P.NearSpikeWallCharge] = P.NearSpikeWallChargeLoop,
        [P.SkeletonFallsApart] = P.SkeletonStable,
    };

    /// <summary>A fight-body or melting frame.</summary>
    private static CrocomireBodyFrame Body(int index) => new(CrocomireBodyFrameSet.Body, index);

    /// <summary>A skeleton frame.</summary>
    private static CrocomireBodyFrame Skeleton(int index) => new(CrocomireBodyFrameSet.Skeleton, index);

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#1627): the body rests on it until its owner starts another program. A shot
    /// that adds to or resets the body's frame timer only moves the update the rest is reached again, which plays nothing.
    /// </summary>
    private static AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue> Hold(CrocomireBodyFrame frame) =>
        new AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue>.HeldFrame(frame);

    /// <summary>
    /// The updates each melting pass's first-row frame shows before its animation shows it again, the longest duration a
    /// frame can have, kept exact (#1627): these two animations loop on their frame, so the frame shows anew, raising the new-frame flag,
    /// each time the count runs out, and a shot's added updates move when.
    /// </summary>
    private const int LoopingMeltFrameDuration = 32767;

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue> Show(CrocomireBodyFrame frame, int duration) =>
        new AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue>.Frame(frame, duration);

    /// <summary>Publishes a cue of <paramref name="kind"/>.</summary>
    private static AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue> Cue(CrocomireBodyCueKind kind) =>
        new AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue>.Event(new(kind, 0));

    /// <summary>Publishes a big dust cloud <paramref name="offset"/> pixels from the body.</summary>
    private static AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue> Dust(int offset) =>
        new AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue>.Event(new(CrocomireBodyCueKind.Dust, offset));
}
