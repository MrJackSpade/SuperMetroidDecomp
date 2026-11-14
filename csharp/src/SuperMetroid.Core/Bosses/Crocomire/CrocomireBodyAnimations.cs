using F = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyFrame;
using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodyCueKind;
using P = SuperMetroid.Core.Bosses.Crocomire.CrocomireBodySegment;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's typed body animations (#641, #1600): each body, melting and skeleton segment's beats, and the animation starting at each.</summary>
internal static class CrocomireBodyAnimations
{
    /// <summary>Every body, melting and skeleton segment's beats.</summary>
    internal static AnimationSegments<CrocomireBodySegment, CrocomireBodyFrame, CrocomireBodyCue> Segments { get; } = new(
        new Dictionary<CrocomireBodySegment, AnimationBeat<CrocomireBodyFrame, CrocomireBodyCue>[]>
    {
        [P.Initial] =
        [
            Show(F.Initial, 1),
            Cue(K.FightAi),
        ],
        [P.ProjectileAttack] =
        [
            Dust(16),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(0),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(-16),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(40),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Show(F.ProjectileAttackSixth, 5),
            Cue(K.Cry),
            Show(F.ProjectileAttackSeventh, 5),
            Show(F.ProjectileAttackEighth, 5),
        ],
        [P.ProjectileAttackLoop] =
        [
            Cue(K.FightAi),
            Show(F.ProjectileAttackLoopFirst, 8),
            Show(F.ProjectileAttackLoopSecond, 7),
            Show(F.ProjectileAttackLoopThird, 7),
            Show(F.ProjectileAttackLoopFourth, 7),
            Show(F.ProjectileAttackLoopFifth, 7),
        ],
        [P.StepForwardAfterDelay] =
        [
            Show(F.StepForwardAfterDelay, 180),
        ],
        [P.StepForward] =
        [
            Cue(K.MaybeStartProjectileAttack),
            Show(F.ProjectileAttackSixth, 5),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSeventh, 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 5),
            Cue(K.FightAi),
            Show(F.SteppingForwardFirst, 16),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(F.SteppingForwardSecond, 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(F.SteppingForwardThird, 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(F.SteppingForwardFourth, 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(F.SteppingForwardFifth, 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(F.SteppingForwardSixth, 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(F.SteppingForwardSeventh, 4),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(F.SteppingForwardEighth, 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(F.SteppingForwardNinth, 4),
            Cue(K.FightAi),
        ],
        [P.StepBack] =
        [
            Show(F.ProjectileAttackSeventh, 2),
        ],
        [P.SteppingBack] =
        [
            Show(F.ChargingTwelfth, 8),
            Cue(K.MoveRightIfOnScreenWithDust),
            Show(F.ChargingEleventh, 8),
            Cue(K.MoveRightIfOnScreen),
            Show(F.ChargingTenth, 8),
            Cue(K.MoveRightIfOnScreen),
            Show(F.ChargingNinth, 8),
            Cue(K.MoveRightIfOnScreenWithDust),
            Show(F.ChargingEighth, 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveRightIfOnScreen),
            Cue(K.FightAi),
        ],
        [P.WaitForFirstSecondDamage] =
        [
            Show(F.WaitForFirstSecondDamageFirst, 34),
            Show(F.WaitForFirstSecondDamageSecond, 2),
            Show(F.WaitForFirstSecondDamageThird, 2),
            Show(F.WaitForFirstSecondDamageFourth, 2),
            Show(F.WaitForFirstSecondDamageFifth, 2),
            Show(F.WaitForFirstSecondDamageSixth, 2),
            Show(F.Initial, 2),
            Show(F.Initial, 16),
            Show(F.WaitForFirstSecondDamageSixth, 1),
            Show(F.WaitForFirstSecondDamageFifth, 1),
            Show(F.WaitForFirstSecondDamageFourth, 1),
            Show(F.WaitForFirstSecondDamageThird, 1),
            Show(F.WaitForFirstSecondDamageSecond, 1),
            Show(F.WaitForFirstSecondDamageFirst, 16),
            Show(F.WaitForFirstSecondDamageSecond, 1),
            Show(F.WaitForFirstSecondDamageThird, 1),
            Show(F.WaitForFirstSecondDamageFourth, 1),
            Show(F.WaitForFirstSecondDamageFifth, 1),
            Show(F.WaitForFirstSecondDamageSixth, 1),
            Show(F.Initial, 1),
            Show(F.WaitForFirstSecondDamageSixth, 1),
            Show(F.WaitForFirstSecondDamageFifth, 1),
            Show(F.WaitForFirstSecondDamageFourth, 1),
            Show(F.WaitForFirstSecondDamageThird, 1),
            Show(F.WaitForFirstSecondDamageSecond, 1),
            Show(F.WaitForFirstSecondDamageFirst, 1),
            Show(F.WaitForFirstSecondDamageSecond, 1),
            Show(F.WaitForFirstSecondDamageThird, 1),
            Show(F.WaitForFirstSecondDamageFourth, 1),
            Show(F.WaitForFirstSecondDamageFifth, 1),
            Show(F.WaitForFirstSecondDamageSixth, 1),
            Show(F.Initial, 1),
            Cue(K.FightAi),
        ],
        [P.MovingClaws] =
        [
            Dust(16),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(0),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(-16),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(40),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Cue(K.FightAi),
        ],
        [P.Roar] =
        [
            Show(F.ProjectileAttackSixth, 48),
            Cue(K.Cry),
            Show(F.ProjectileAttackSeventh, 5),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 32),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 1),
            Cue(K.FightAi),
        ],
        [P.RoarCloseMouth] =
        [
            Show(F.ProjectileAttackEighth, 32),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSeventh, 5),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSeventh, 8),
            Show(F.ProjectileAttackSixth, 2),
        ],
        [P.RoarCloseMouthLoop] =
        [
            Show(F.ProjectileAttackSixth, 1),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSixth, 1),
            Cue(K.FightAi),
        ],
        [P.PowerBombReactionMouthFullyOpen] =
        [
            Show(F.ProjectileAttackSeventh, 2),
        ],
        [P.PowerBombReactionMouthPartiallyOpen] =
        [
            Show(F.ProjectileAttackSixth, 2),
        ],
        [P.PowerBombReactionMouthNotOpen] =
        [
            Dust(16),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(0),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(-16),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(F.ProjectileAttackFirst, 5),
            Show(F.ProjectileAttackSecond, 5),
            Dust(40),
            Show(F.ProjectileAttackThird, 5),
            Show(F.ProjectileAttackFourth, 5),
            Show(F.ProjectileAttackFifth, 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
        ],
        [P.PowerBombReactionMouthNotOpenLoop] =
        [
            Show(F.ChargingFirst, 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingSecond, 4),
            Cue(K.MoveLeft),
            Show(F.ChargingThird, 4),
            Cue(K.MoveLeft),
            Show(F.ChargingFourth, 4),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingFifth, 4),
            Cue(K.MoveLeft),
            Show(F.ChargingSixth, 4),
            Cue(K.MoveLeft),
            Show(F.ChargingSeventh, 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingEighth, 4),
            Cue(K.MoveLeft),
            Show(F.ChargingNinth, 4),
            Cue(K.MoveLeft),
            Show(F.ChargingTenth, 4),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingEleventh, 4),
            Cue(K.MoveLeft),
            Show(F.ChargingTwelfth, 4),
            Cue(K.FightAi),
        ],
        [P.NearSpikeWallCharge] =
        [
            Show(F.ProjectileAttackSixth, 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSeventh, 5),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackEighth, 2),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSeventh, 5),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSeventh, 8),
            Cue(K.FightAi),
            Show(F.ProjectileAttackSixth, 2),
            Cue(K.FightAi),
        ],
        [P.NearSpikeWallChargeLoop] =
        [
            Show(F.ChargingFirst, 3),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingSecond, 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(F.ChargingThird, 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(F.ChargingFourth, 3),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingFifth, 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(F.ChargingSixth, 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(F.ChargingSeventh, 3),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingEighth, 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(F.ChargingNinth, 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(F.ChargingTenth, 3),
            Cue(K.MoveLeftWithDust),
            Show(F.ChargingEleventh, 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(F.ChargingTwelfth, 3),
            Cue(K.FightAi),
        ],
        [P.BackOffFromSpikeWall] =
        [
            Show(F.ChargingTwelfth, 8),
            Cue(K.MoveRightWithDust),
            Show(F.ChargingEleventh, 8),
            Cue(K.MoveRight),
            Show(F.ChargingTenth, 8),
            Cue(K.MoveRight),
            Show(F.ChargingNinth, 8),
            Cue(K.MoveRightWithDust),
            Show(F.ChargingEighth, 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveRight),
            Cue(K.FightAi),
        ],
        [P.MeltingOneTopRow] =
        [
            Show(F.MeltingOneTopRow, LoopingMeltFrameDuration),
        ],
        [P.MeltingOneTopTwoRows] =
        [
            Hold(F.MeltingOneTopTwoRows),
        ],
        [P.MeltingOneTopThreeRows] =
        [
            Hold(F.MeltingOneTopThreeRows),
        ],
        [P.MeltingOneTopFourRows] =
        [
            Hold(F.MeltingOneTopFourRows),
        ],
        [P.MeltingTwoTopRow] =
        [
            Show(F.MeltingTwoTopRow, LoopingMeltFrameDuration),
        ],
        [P.MeltingTwoTopTwoRows] =
        [
            Hold(F.MeltingTwoTopTwoRows),
        ],
        [P.MeltingTwoTopThreeRows] =
        [
            Hold(F.MeltingTwoTopThreeRows),
        ],
        [P.MeltingTwoTopFourRows] =
        [
            Hold(F.MeltingTwoTopFourRows),
        ],
        [P.BridgeCollapsed] =
        [
            Show(F.ProjectileAttackSeventh, 5),
            Show(F.ProjectileAttackSixth, 5),
            Cue(K.Cry),
            Show(F.ProjectileAttackSeventh, 5),
            Show(F.ProjectileAttackEighth, 5),
        ],
        [P.SkeletonFalling] =
        [
            Show(F.SkeletonFallingFirst, 10),
            Show(F.SkeletonFallingSecond, 10),
            Show(F.SkeletonFallingThird, 10),
        ],
        [P.SkeletonFallsApart] =
        [
            Show(F.SkeletonFallsApartFirst, 10),
            Dust(-32),
            Show(F.SkeletonFallsApartSecond, 5),
            Dust(0),
            Show(F.SkeletonFallsApartSecond, 5),
            Dust(-16),
            Show(F.SkeletonFallsApartThird, 5),
            Dust(16),
            Show(F.SkeletonFallsApartThird, 5),
            Show(F.SkeletonFallsApartFourth, 10),
            Show(F.SkeletonFallsApartFifth, 32),
            Show(F.SkeletonFallsApartSixth, 16),
            Cue(K.SkeletonCollapseSound),
            Show(F.SkeletonFallsApartSeventh, 10),
            Show(F.SkeletonFallsApartEighth, 9),
            Show(F.SkeletonFallsApartNinth, 9),
            Dust(0),
            Show(F.SkeletonFallsApartTenth, 8),
            Dust(8),
            Show(F.SkeletonFallsApartEleventh, 8),
            Dust(16),
            Show(F.SkeletonFallsApartTwelfth, 7),
            Dust(24),
            Show(F.SkeletonFallsApartThirteenth, 7),
            Dust(32),
            Show(F.SkeletonFallsApartFourteenth, 6),
            Dust(40),
            Show(F.SkeletonFallsApartFifteenth, 6),
            Dust(48),
            Show(F.SkeletonFallsApartSixteenth, 6),
            Dust(56),
            Show(F.SkeletonFallsApartSeventeenth, 5),
            Dust(64),
            Cue(K.BigExplosionSound),
            Show(F.SkeletonFallsApartEighteenth, 5),
        ],
        [P.SkeletonStable] =
        [
            Hold(F.SkeletonStable),
        ],
        [P.Dead] =
        [
            Hold(F.Dead),
        ],
        [P.SkeletonFlowingDownRiver] =
        [
            Show(F.SkeletonFlowingDownRiverFirst, 4),
            Show(F.SkeletonFlowingDownRiverSecond, 4),
            Show(F.SkeletonFlowingDownRiverThird, 4),
            Show(F.SkeletonFlowingDownRiverFourth, 4),
            Show(F.SkeletonFlowingDownRiverFifth, 4),
            Show(F.SkeletonFlowingDownRiverSixth, 4),
            Show(F.SkeletonFlowingDownRiverSeventh, 4),
            Show(F.SkeletonFlowingDownRiverEighth, 4),
            Show(F.SkeletonFlowingDownRiverNinth, 4),
            Show(F.SkeletonFlowingDownRiverTenth, 20),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CrocomireBodySegment, Animation<CrocomireBodySegment>> StartingAt { get; } = new Dictionary<CrocomireBodySegment, Animation<CrocomireBodySegment>>
    {
        [P.Initial] = new([new(P.Initial, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
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
    internal static Animation<CrocomireBodySegment> StartingWith(CrocomireBodySegment segment) => StartingAt[segment];

    /// <summary>
    /// The segment each segment that runs on past its last beat continues into, directly after that beat: a body
    /// resting after that beat already stands in the next, which the fight AI and the skeleton's rest test read.
    /// </summary>
    internal static IReadOnlyDictionary<CrocomireBodySegment, CrocomireBodySegment> RunsOnInto { get; } = new Dictionary<CrocomireBodySegment, CrocomireBodySegment>
    {
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

    /// <summary>
    /// Shows <paramref name="frame"/> for good (#1627): the body rests on it until its owner starts another animation. A shot
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
