using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using S = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using static SuperMetroid.Core.Bosses.Torizo.TorizoSide;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Torizo's animations either way it faces (#2411): turning, Bomb Torizo's attacks, jumps, falls and landings, and its
/// walks with its face and without. Each is written once over the poses of a side, a stance or a stride, and each segment
/// names the side, the foot and the animations it continues in; the one way the two facings differ, Bomb Torizo's
/// right-facing step on its left leg deciding once more whether to jump back before its last pace, is written where that
/// step is.
/// </summary>
internal static class TorizoFacedAnimations
{
    /// <summary>Each segment's steps, in the order it plays them: the left-facing segments, then the right-facing.</summary>
    internal static Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [S.Turning] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingViewer, 24),
        ],
        [S.FacingLeftSpewingChozoOrbsRightFootForward] = BombSpewing(Left.FarFootForward),
        [S.FacingLeftSpewingChozoOrbsRightFootForwardEnd] = BombSpewingEnd(Left.FarFootForward, A.FacingLeftWalkingLeftLegMoving),
        [S.FacingLeftSpewingChozoOrbsLeftFootForward] = BombSpewing(Left.NearFootForward),
        [S.FacingLeftSpewingChozoOrbsLeftFootForwardEnd] = BombSpewingEnd(Left.NearFootForward, A.FacingLeftWalkingRightLegMoving),
        [S.StandingToThrowSonicBooms] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
        ],
        [S.FacingLeftSonicBoomsRightFootForwardThrowing] = BombSonicBooms(Left.FarFootForward),
        [S.FacingLeftSonicBoomsRightFootForwardEnd] = WalkingOn(A.FacingLeftWalkingLeftLegMoving),
        [S.FacingLeftSonicBoomsLeftFootForwardThrowing] = BombSonicBooms(Left.NearFootForward),
        [S.FacingLeftSonicBoomsLeftFootForwardEnd] = WalkingOn(A.FacingLeftWalkingRightLegMoving),
        [S.FacingLeftExplosiveSwipeRightFootForward] = ExplosiveSwipe(Left.FarFootForward),
        [S.FacingLeftExplosiveSwipeLeftFootForward] = ExplosiveSwipe(Left.NearFootForward),
        [S.FacingLeftJumpingForward] = JumpingOff(Left, fall: A.FacingLeftFalling),
        [S.FacingLeftFalling] = Falling(landing: A.FacingLeftLanding),
        [S.FacingLeftLanding] = Landing(faceless: A.FacingLeftFacelessWalkingLeftLegMoving, golden: A.GoldenWalkingLeftLeftLegMoving),
        [S.FacingLeftJumpingBackwardToLeftFoot] = JumpingOff(Left, fall: A.FacingLeftFallingBackwardToLeftFoot),
        [S.FacingLeftFallingBackwardToLeftFoot] = Falling(landing: A.FacingLeftLandingBackwardOnLeftFoot),
        [S.FacingLeftLandingBackwardOnLeftFoot] = LandingBackward(faceless: A.FacingLeftFacelessWalkingRightLegMoving,
            golden: A.GoldenFacingLeftLandedBackwardOnLeftFoot, spewing: A.FacingLeftSpewingChozoOrbsLeftFootForward,
            sonicBooms: A.FacingLeftSonicBoomsLeftFootForward),
        [S.FacingLeftJumpingBackwardToRightFoot] = JumpingOff(Left, fall: A.FacingLeftFallingBackwardToRightFoot),
        [S.FacingLeftFallingBackwardToRightFoot] = Falling(landing: A.FacingLeftLandingBackwardOnRightFoot),
        [S.FacingLeftLandingBackwardOnRightFoot] = LandingBackward(faceless: A.FacingLeftFacelessWalkingLeftLegMoving,
            golden: A.GoldenFacingLeftLandedBackwardOnRightFoot, spewing: A.FacingLeftSpewingChozoOrbsRightFootForward,
            sonicBooms: A.FacingLeftSonicBoomsRightFootForward),
        [S.TurningQuickly] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingViewer, 8),
        ],
        [S.FacingLeftFacelessWalkingRightLegMoving] = FacelessStep(Left, Left.FarLegStep, swipe: A.FacingLeftExplosiveSwipeLeftFootForward),
        [S.FacingLeftFacelessWalkingLeftLegMoving] = FacelessStep(Left, Left.NearLegStep, swipe: A.FacingLeftExplosiveSwipeRightFootForward),
        [S.FacingLeftWalkingRightLegMoving] = BombStep(Left, Left.FarLegStep, swipe: A.FacingLeftExplosiveSwipeLeftFootForward, beforeLastPace: []),
        [S.FacingLeftWalkingLeftLegMoving] = BombStep(Left, Left.NearLegStep, swipe: A.FacingLeftExplosiveSwipeRightFootForward, beforeLastPace: []),
        [S.FacingLeftRising] = Rising(Left),
        [S.FacingLeftFallingPose] = FallingPose(Left),
        [S.BombChozoOrbBurst] =
        [
            Cue(new TorizoCue.Signal(K.SpawnBombTorizoChozoOrbs)),
            Wait(16),
        ],
        [S.FacingRightSpewingChozoOrbsLeftFootForward] = BombSpewing(Right.FarFootForward),
        [S.FacingRightSpewingChozoOrbsLeftFootForwardEnd] = BombSpewingEnd(Right.FarFootForward, A.FacingRightWalkingRightLegMoving),
        [S.FacingRightSpewingChozoOrbsRightFootForward] = BombSpewing(Right.NearFootForward),
        [S.FacingRightSpewingChozoOrbsRightFootForwardEnd] = BombSpewingEnd(Right.NearFootForward, A.FacingRightWalkingLeftLegMoving),
        [S.FacingRightSonicBoomsLeftFootForwardThrowing] = BombSonicBooms(Right.FarFootForward),
        [S.FacingRightSonicBoomsLeftFootForwardEnd] = WalkingOn(A.FacingRightWalkingRightLegMoving),
        [S.FacingRightSonicBoomsRightFootForwardThrowing] = BombSonicBooms(Right.NearFootForward),
        [S.FacingRightSonicBoomsRightFootForwardEnd] = WalkingOn(A.FacingRightWalkingLeftLegMoving),
        [S.FacingRightExplosiveSwipeLeftFootForward] = ExplosiveSwipe(Right.FarFootForward),
        [S.FacingRightExplosiveSwipeRightFootForward] = ExplosiveSwipe(Right.NearFootForward),
        [S.FacingRightJumpingForward] = JumpingOff(Right, fall: A.FacingRightFalling),
        [S.FacingRightFalling] = Falling(landing: A.FacingRightLanding),
        [S.FacingRightLanding] = Landing(faceless: A.FacingRightFacelessWalkingRightLegMoving, golden: A.GoldenWalkingRightRightLegMoving),
        [S.FacingRightJumpingBackwardToRightFoot] = JumpingOff(Right, fall: A.FacingRightFallingBackwardToRightFoot),
        [S.FacingRightFallingBackwardToRightFoot] = Falling(landing: A.FacingRightLandingBackwardOnRightFoot),
        [S.FacingRightLandingBackwardOnRightFoot] = LandingBackward(faceless: A.FacingRightFacelessWalkingLeftLegMoving,
            golden: A.GoldenFacingRightLandedBackwardOnRightFoot, spewing: A.FacingRightSpewingChozoOrbsRightFootForward,
            sonicBooms: A.FacingRightSonicBoomsRightFootForward),
        [S.FacingRightJumpingBackwardToLeftFoot] = JumpingOff(Right, fall: A.FacingRightFallingBackwardToLeftFoot),
        [S.FacingRightFallingBackwardToLeftFoot] = Falling(landing: A.FacingRightLandingBackwardOnLeftFoot),
        [S.FacingRightLandingBackwardOnLeftFoot] = LandingBackward(faceless: A.FacingRightFacelessWalkingRightLegMoving,
            golden: A.GoldenFacingRightLandedBackwardOnLeftFoot, spewing: A.FacingRightSpewingChozoOrbsLeftFootForward,
            sonicBooms: A.FacingRightSonicBoomsLeftFootForward),
        [S.FacingRightFacelessWalkingLeftLegMoving] = FacelessStep(Right, Right.FarLegStep, swipe: A.FacingRightExplosiveSwipeRightFootForward),
        [S.FacingRightFacelessWalkingRightLegMoving] = FacelessStep(Right, Right.NearLegStep, swipe: A.FacingRightExplosiveSwipeLeftFootForward),
        [S.FacingRightWalkingLeftLegMoving] = BombStep(Right, Right.FarLegStep, swipe: A.FacingRightExplosiveSwipeRightFootForward,
            beforeLastPace: BombJumpingBack),
        [S.FacingRightWalkingRightLegMoving] = BombStep(Right, Right.NearLegStep, swipe: A.FacingRightExplosiveSwipeLeftFootForward, beforeLastPace: []),
        [S.FacingRightRising] = Rising(Right),
        [S.FacingRightFallingPose] = FallingPose(Right),
    };

    /// <summary>Bomb Torizo's decision to jump back from Samus close in front of it.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] BombJumpingBack =>
    [
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
    ];

    /// <summary>Bomb Torizo standing in <paramref name="stance"/> starts spewing Chozo orbs: it lowers its head, opens its mouth, thrusts and draws back its arm, and its shot sounds.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] BombSpewing(TorizoStance stance) =>
    [
        Cue(new TorizoCue.Signal(K.StartStanding)),
        Show(stance.HeadLowered, 16),
        Show(stance.HeadRaised, 8),
        Show(stance.MouthOpening, 8),
        Show(stance.ArmThrust, 8),
        Show(stance.ArmRaised, 8),
        Show(stance.ArmDrawnBack, 16),
        Cue(new TorizoCue.Signal(K.PlayShotSound)),
    ];

    /// <summary>Bomb Torizo standing in <paramref name="stance"/> lowers its arm after spewing and walks on in <paramref name="walk"/>.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] BombSpewingEnd(TorizoStance stance, TorizoAnimation walk) =>
    [
        Show(stance.ArmRaised, 8),
        Show(stance.ArmThrust, 8),
        Show(stance.MouthOpening, 8),
        Show(stance.HeadRaised, 72),
        .. WalkingOn(walk),
    ];

    /// <summary>Bomb Torizo standing in <paramref name="stance"/> throws a sonic boom with each arm, the near arm's first.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] BombSonicBooms(TorizoStance stance) =>
    [
        .. SonicBoomThrow(stance.HeadLowered, stance.NearArm, poseUpdates: 6, snapUpdates: 2, loweredUpdates: 24, K.SpawnBombTorizoSonicBoom),
        .. SonicBoomThrow(stance.HeadLowered, stance.FarArm, poseUpdates: 6, snapUpdates: 2, loweredUpdates: 56, K.SpawnBombTorizoSonicBoom),
    ];

    /// <summary>
    /// A Torizo standing in <paramref name="stance"/> swipes behind it, an explosion at its hand as each arm sweeps down, then
    /// starts walking and resumes the walk it interrupted. The hand's places are a left-facing Torizo's, which a right-facing
    /// one's swipe mirrors.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] ExplosiveSwipe(TorizoStance stance) =>
    [
        Cue(new TorizoCue.Signal(K.StartStanding)),
        Show(stance.HeadLowered, 3),
        Show(stance.NearArm.Down, 3),
        Show(stance.NearArm.Lifting, 3),
        Show(stance.NearArm.Level, 3),
        Show(stance.NearArm.Raised, 3),
        Cue(new TorizoCue.Swipe(Hand: new(-30, -52))),
        Show(stance.NearArm.Overhead, 3),
        Cue(new TorizoCue.Swipe(Hand: new(-40, -28))),
        Show(stance.NearArm.Raised, 1),
        Cue(new TorizoCue.Swipe(Hand: new(-47, -11))),
        Show(stance.NearArm.Level, 1),
        Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
        Show(stance.NearArm.Lifting, 1),
        Cue(new TorizoCue.Swipe(Hand: new(-21, 21))),
        Show(stance.NearArm.Down, 1),
        Cue(new TorizoCue.Swipe(Hand: new(-1, 20))),
        Show(stance.HeadLowered, 3),
        Show(stance.FarArm.Down, 3),
        Show(stance.FarArm.Lifting, 3),
        Show(stance.FarArm.Level, 3),
        Show(stance.FarArm.Raised, 3),
        Cue(new TorizoCue.Swipe(Hand: new(-28, -52))),
        Show(stance.FarArm.Overhead, 3),
        Cue(new TorizoCue.Swipe(Hand: new(-43, -27))),
        Show(stance.FarArm.Raised, 1),
        Cue(new TorizoCue.Swipe(Hand: new(-48, -10))),
        Show(stance.FarArm.Level, 1),
        Cue(new TorizoCue.Swipe(Hand: new(-31, 9))),
        Show(stance.FarArm.Lifting, 1),
        Show(stance.FarArm.Down, 16),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.ResumeWalk)),
    ];

    /// <summary>A Torizo facing <paramref name="side"/>'s way leaps, naming <paramref name="fall"/>, the fall its rise ends in, then crouches and springs.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] JumpingOff(TorizoSide side, TorizoAnimation fall) =>
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: fall)),
        Show(side.JumpCrouch, 5),
        Show(side.Springing, 5),
    ];

    /// <summary>
    /// A rising Torizo facing <paramref name="side"/>'s way shows its leaping pose, then marks it shown: its jump loops on the
    /// pose, and its owner starts its fall once it no longer rises (#2411).
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] Rising(TorizoSide side) =>
    [
        Show(side.Leaping, 1),
        Mark(K.LeapingPoseShown),
    ];

    /// <summary>A Torizo starts falling, naming <paramref name="landing"/>, the animation its fall lands it in.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] Falling(TorizoAnimation landing) =>
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: landing)),
    ];

    /// <summary>A Torizo falling facing <paramref name="side"/>'s way, the pose its fall rests in.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] FallingPose(TorizoSide side) =>
    [
        Show(side.Springing, 5),
    ];

    /// <summary>
    /// A Torizo lands: its footstep sounds and its feet raise dust; faceless Bomb Torizo walks on in <paramref name="faceless"/>,
    /// Golden Torizo in <paramref name="golden"/>, and Bomb Torizo with its face runs on into its own walk.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] Landing(TorizoAnimation faceless, TorizoAnimation golden) =>
    [
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Cue(new TorizoCue.Signal(K.SpawnLandingDust)),
        Cue(new TorizoCue.Choice(TorizoChoiceKind.LandFacelessOrGolden, Target: faceless, Alternate: golden)),
    ];

    /// <summary>
    /// A Torizo lands from its backward jump: as <see cref="Landing"/>, but Bomb Torizo with its face then attacks, spewing in
    /// <paramref name="spewing"/> or throwing in <paramref name="sonicBooms"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] LandingBackward(
        TorizoAnimation faceless, TorizoAnimation golden, TorizoAnimation spewing, TorizoAnimation sonicBooms) =>
    [
        .. Landing(faceless, golden),
        Cue(new TorizoCue.Choice(TorizoChoiceKind.BombAttack, Target: spewing, Alternate: sonicBooms)),
    ];

    /// <summary>
    /// A step of faceless Bomb Torizo facing <paramref name="side"/>'s way on <paramref name="stride"/>: it records the step,
    /// walks, drools when its health is low and its footstep sounds; as its leg lifts it may swipe at Samus behind it in
    /// <paramref name="swipe"/>, and it moves forward after each pose.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] FacelessStep(TorizoSide side, TorizoStride stride, TorizoAnimation swipe) =>
    [
        Cue(new TorizoCue.Stepped(side.Facing, Foot: stride.Foot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(stride.Lifting, 1),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: swipe)),
        Walk(TorizoWalkKind.WalkFacelessBombTorizo, side, stride.Paces.AfterLifting),
        Show(stride.Swinging, 5),
        Walk(TorizoWalkKind.WalkFacelessBombTorizo, side, stride.Paces.AfterSwinging),
        Show(stride.Reaching, 5),
        Walk(TorizoWalkKind.WalkFacelessBombTorizo, side, stride.Paces.AfterReaching),
        Show(stride.Planting, 5),
        Walk(TorizoWalkKind.WalkFacelessBombTorizo, side, stride.Paces.AfterPlanting),
        Show(stride.Planted, 5),
        Walk(TorizoWalkKind.WalkFacelessBombTorizo, side, stride.Paces.AfterPlanted),
    ];

    /// <summary>
    /// A step of Bomb Torizo facing <paramref name="side"/>'s way on <paramref name="stride"/>: it records the step, walks,
    /// drools when its health is low and its footstep sounds; as its leg lifts it may swipe at Samus behind it in
    /// <paramref name="swipe"/>, before each of its first four paces it may jump back from Samus close in front, and before its
    /// last it decides as <paramref name="beforeLastPace"/> says.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] BombStep(
        TorizoSide side, TorizoStride stride, TorizoAnimation swipe, AnimationBeat<EnemyFrameId, TorizoCue>[] beforeLastPace) =>
    [
        Cue(new TorizoCue.Stepped(side.Facing, Foot: stride.Foot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(stride.Lifting, 10),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: swipe)),
        .. BombJumpingBack,
        Walk(TorizoWalkKind.WalkBombTorizo, side, stride.Paces.AfterLifting),
        Show(stride.Swinging, 6),
        .. BombJumpingBack,
        Walk(TorizoWalkKind.WalkBombTorizo, side, stride.Paces.AfterSwinging),
        Show(stride.Reaching, 6),
        .. BombJumpingBack,
        Walk(TorizoWalkKind.WalkBombTorizo, side, stride.Paces.AfterReaching),
        Show(stride.Planting, 6),
        .. BombJumpingBack,
        Walk(TorizoWalkKind.WalkBombTorizo, side, stride.Paces.AfterPlanting),
        Show(stride.Planted, 6),
        .. beforeLastPace,
        Walk(TorizoWalkKind.WalkBombTorizo, side, stride.Paces.AfterPlanted),
    ];
}
