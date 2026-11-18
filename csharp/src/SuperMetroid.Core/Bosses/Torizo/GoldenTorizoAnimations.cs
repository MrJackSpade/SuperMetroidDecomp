using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using S = SuperMetroid.Core.Bosses.Torizo.TorizoBodySegment;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;
using P = SuperMetroid.Core.Bosses.Torizo.TorizoPostureDefinitions;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using static SuperMetroid.Core.Bosses.Torizo.TorizoSide;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Golden Torizo's own animations. Each faced one is written once over the poses of a side, a stance or a stride (#2411),
/// and each segment names the side, the foot and the animations it continues in; where the facings differ, the difference
/// is written where its segment is: facing left, Golden Torizo lowers its arm again before it throws a caught Super Missile,
/// and facing right lifts it straight on; facing right on its left leg, it decides whether to jump back as its leg plants
/// rather than as it reaches. A pose shown twice running, with nothing between, is one beat of both durations
/// (#2331): a Torizo is no boss-body part, so nothing waits on a second showing of a picture, and no owner resumes a
/// catch partway.
/// </summary>
internal static class GoldenTorizoAnimations
{
    /// <summary>Each segment's steps, in the order it plays them.</summary>
    internal static Dictionary<TorizoBodySegment, AnimationBeat<EnemyFrameId, TorizoCue>[]> Segments { get; } = new()
    {
        [S.GoldenTorizoDormant] =
        [
            ShowArtwork(L.Awakening),
            Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenWaitForSamus)),
            Show(Left.HalfRisen, 1),
        ],
        [S.GoldenTorizoAwakening] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
        ],
        [S.GoldenTorizoDropping] =
        [
            Show(Left.HalfRisen, 1),
            Mark(K.DroppingPoseShown),
        ],
        [S.GoldenTorizoSettling] =
        [
            Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
            Show(Left.HalfRisen, 3),
            SitDown(Left, P.CrouchedToHalfRisen),
            Show(Left.Crouched, 4),
            SitDown(Left, P.RisingToCrouched),
            Show(Left.RisingFromSeat, 5),
            SitDown(Left, P.SeatedToRising),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Idle)),
            Show(Left.Seated, 48),
            ShowArtwork(L.EyesDark),
            Show(Left.Seated, 32),
            ShowArtwork(L.EyesGlowingFaintly),
            Show(Left.Seated, 16),
            ShowArtwork(L.EyesGlowingBrightly),
            Show(Left.Seated, 8),
            ShowArtwork(L.EyesGlowingFully),
            Show(Left.Seated, 32),
        ],
        [S.GoldenTorizoStandingUp] =
        [
            Show(Left.Seated, 32),
            StandUp(Left, P.SeatedToRising),
            Show(Left.RisingFromSeat, 12),
            StandUp(Left, P.RisingToCrouched),
            Show(Left.Crouched, 8),
            StandUp(Left, P.CrouchedToHalfRisen),
            Show(Left.HalfRisen, 8),
            StandUp(Left, P.HalfRisenToNearlyUpright),
            Show(Left.NearlyUpright, 8),
            StandUp(Left, P.NearlyUprightToStepping),
            Show(Left.UprightStepping, 8),
            StandUp(Left, P.SteppingToWideStance),
            Cue(new TorizoCue.Signal(K.FadeToGoldenColors)),
        ],
        [S.GoldenTorizoFightStart] =
        [
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.Signal(K.StartGoldenTorizoFight)),
            Show(Left.UprightWideStance, 16),
        ],
        [S.GoldenFacingLeftSpewingChozoOrbsRightFootForward] = Spewing(Left.FarFootForward),
        [S.GoldenFacingLeftSpewingChozoOrbsRightFootForwardEnd] = SpewingEnd(Left.FarFootForward, A.GoldenWalkingLeftLeftLegMoving),
        [S.GoldenFacingLeftSpewingChozoOrbsLeftFootForward] = Spewing(Left.NearFootForward),
        [S.GoldenFacingLeftSpewingChozoOrbsLeftFootForwardEnd] = SpewingEnd(Left.NearFootForward, A.GoldenWalkingLeftRightLegMoving),
        [S.GoldenFacingLeftSonicBoomsRightFootForwardThrowing] = SonicBooms(Left.FarFootForward),
        [S.GoldenFacingLeftSonicBoomsRightFootForwardEnd] = WalkingOn(A.GoldenWalkingLeftLeftLegMoving),
        [S.GoldenFacingLeftSonicBoomsLeftFootForwardThrowing] = SonicBooms(Left.NearFootForward),
        [S.GoldenFacingLeftSonicBoomsLeftFootForwardEnd] = WalkingOn(A.GoldenWalkingLeftRightLegMoving),
        [S.GoldenFacingRightSpewingChozoOrbsLeftFootForward] = Spewing(Right.FarFootForward),
        [S.GoldenFacingRightSpewingChozoOrbsLeftFootForwardEnd] = SpewingEnd(Right.FarFootForward, A.GoldenWalkingRightRightLegMoving),
        [S.GoldenFacingRightSpewingChozoOrbsRightFootForward] = Spewing(Right.NearFootForward),
        [S.GoldenFacingRightSpewingChozoOrbsRightFootForwardEnd] = SpewingEnd(Right.NearFootForward, A.GoldenWalkingRightLeftLegMoving),
        [S.GoldenFacingRightSonicBoomsLeftFootForwardThrowing] = SonicBooms(Right.FarFootForward),
        [S.GoldenFacingRightSonicBoomsLeftFootForwardEnd] = WalkingOn(A.GoldenWalkingRightRightLegMoving),
        [S.GoldenFacingRightSonicBoomsRightFootForwardThrowing] = SonicBooms(Right.NearFootForward),
        [S.GoldenFacingRightSonicBoomsRightFootForwardEnd] = WalkingOn(A.GoldenWalkingRightLeftLegMoving),
        [S.GoldenFacingLeftLandedBackwardOnLeftFoot] = Attacking(spewing: A.GoldenFacingLeftSpewingChozoOrbsLeftFootForward,
            sonicBooms: A.GoldenFacingLeftSonicBoomsLeftFootForward),
        [S.GoldenFacingLeftLandedBackwardOnRightFoot] = Attacking(spewing: A.GoldenFacingLeftSpewingChozoOrbsRightFootForward,
            sonicBooms: A.GoldenFacingLeftSonicBoomsRightFootForward),
        [S.GoldenFacingRightLandedBackwardOnRightFoot] = Attacking(spewing: A.GoldenFacingRightSpewingChozoOrbsRightFootForward,
            sonicBooms: A.GoldenFacingRightSonicBoomsRightFootForward),
        [S.GoldenFacingRightLandedBackwardOnLeftFoot] = Attacking(spewing: A.GoldenFacingRightSpewingChozoOrbsLeftFootForward,
            sonicBooms: A.GoldenFacingRightSonicBoomsLeftFootForward),
        [S.GoldenFacingLeftCatchingSuperMissileLeftLegForward] = CatchingSuperMissile(Left.NearFootForward,
            ArmDippingBeforeThrow(Left.NearFootForward.NearArm)),
        [S.GoldenFacingLeftCatchingSuperMissileRightLegForward] = CatchingSuperMissile(Left.FarFootForward,
            ArmDippingBeforeThrow(Left.FarFootForward.NearArm)),
        [S.GoldenFacingRightCatchingSuperMissileRightLegForward] = CatchingSuperMissile(Right.NearFootForward,
            ArmLiftingBeforeThrow(Right.NearFootForward.NearArm)),
        [S.GoldenFacingRightCatchingSuperMissileLeftLegForward] = CatchingSuperMissile(Right.FarFootForward,
            ArmLiftingBeforeThrow(Right.FarFootForward.NearArm)),
        [S.GoldenFacingLeftSittingDownAttack] = SittingDownAttack(Left),
        [S.GoldenFacingRightSittingDownAttack] = SittingDownAttack(Right),
        [S.GoldenReleasingEggs] =
        [
            Cue(new TorizoCue.Signal(K.StartStanding)),
            ShowArtwork(L.ChestParting),
            Wait(8),
            ShowArtwork(L.ChestHalfOpen),
            Wait(8),
            ShowArtwork(L.ChestOpen),
            Wait(16),
        ],
        [S.GoldenEggLaying] =
        [
            Cue(new TorizoCue.Signal(K.PlayEggReleasedSound)),
            Cue(new TorizoCue.Signal(K.SpawnEgg)),
            Wait(8),
        ],
        [S.GoldenChestHeldOpen] =
        [
            Wait(1),
            Mark(K.ChestHeldOpen),
        ],
        [S.GoldenChestClosing] =
        [
            ShowArtwork(L.ChestHeldOpen),
            Wait(8),
            ShowArtwork(L.ChestHalfClosed),
            Wait(8),
            ShowArtwork(L.ChestNearlyClosed),
            Wait(8),
            ShowArtwork(L.ChestClosed),
            Cue(new TorizoCue.Signal(K.StartWalking)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
        [S.GoldenEyeBeamAttack] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.DisableEyeBeamExplosions)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Wait(8),
            Cue(new TorizoCue.Signal(K.PlayLaserSound)),
        ],
        [S.GoldenEyeBeamFiring] =
        [
            Cue(new TorizoCue.Signal(K.SpawnEyeBeam)),
            Wait(4),
        ],
        [S.GoldenEyeBeamPause] =
        [
            Wait(8),
        ],
        [S.GoldenEyeBeamGlow] =
        [
            Wait(3),
            ShowArtwork(L.EyeBeamEyesDark),
            Wait(3),
            ShowArtwork(L.EyeBeamEyesGlowingFaintly),
            Wait(3),
            ShowArtwork(L.EyeBeamEyesGlowingBrightly),
            Wait(3),
            ShowArtwork(L.EyeBeamEyesGlowingFully),
        ],
        [S.GoldenEyeBeamEnd] =
        [
            Cue(new TorizoCue.Signal(K.EnableEyeBeamExplosions)),
            Wait(8),
            Cue(new TorizoCue.Signal(K.DisableEyeBeamExplosions)),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenActive)),
            Cue(new TorizoCue.Signal(K.ResumeWalk)),
        ],
        [S.DodgeTurning] =
        [
            Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
            Cue(new TorizoCue.Signal(K.LockAnimation)),
            Cue(new TorizoCue.Signal(K.SetTurningAround)),
            Show(EnemyFrameId.TorizoFacingViewer, 24),
            Cue(new TorizoCue.Signal(K.UnlockAnimation)),
        ],
        [S.GoldenWalkingLeftLeftLegMoving] = Step(Left, Left.NearLegStep, NearLegDecisions, beforeReachingPace: JumpingBack, beforePlantingPace: []),
        [S.GoldenWalkingRightRightLegMoving] = Step(Right, Right.NearLegStep, NearLegDecisions, beforeReachingPace: JumpingBack, beforePlantingPace: []),
        [S.GoldenWalkingLeftRightLegMoving] = Step(Left, Left.FarLegStep, FarLegDecisions, beforeReachingPace: JumpingBack, beforePlantingPace: []),
        [S.GoldenWalkingRightLeftLegMoving] = Step(Right, Right.FarLegStep, FarLegDecisions, beforeReachingPace: [], beforePlantingPace: JumpingBack),
        [S.GoldenChozoOrbBurst] =
        [
            Cue(new TorizoCue.Signal(K.PlayShotSound)),
            Cue(new TorizoCue.Signal(K.SpawnGoldenTorizoChozoOrbs)),
            Wait(6),
        ],
    };

    /// <summary>Golden Torizo's decision to jump back from Samus close in front of it, or after enough steps without a decision.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] JumpingBack =>
    [
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
    ];

    /// <summary>The decisions Golden Torizo makes as the leg on the side it faces lifts: to sit on Samus morphed behind it, to leap at her far ahead or to jump back.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] NearLegDecisions =>
    [
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenSitOnSamusMorphedBehind)),
        .. FarLegDecisions,
    ];

    /// <summary>The decisions Golden Torizo makes as the leg away from the side it faces lifts: to leap at Samus far ahead or to jump back.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] FarLegDecisions =>
    [
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenLeapAtDistantSamus)),
        .. JumpingBack,
    ];

    /// <summary>Golden Torizo standing in <paramref name="stance"/> starts spewing Chozo orbs: it lowers its head, opens its mouth, thrusts and draws back its arm.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] Spewing(TorizoStance stance) =>
    [
        Cue(new TorizoCue.Signal(K.StartStanding)),
        Show(stance.HeadLowered, 6),
        Show(stance.HeadRaised, 3),
        Show(stance.MouthOpening, 3),
        Show(stance.ArmThrust, 3),
        Show(stance.ArmRaised, 3),
        Show(stance.ArmDrawnBack, 6),
    ];

    /// <summary>Golden Torizo standing in <paramref name="stance"/> lowers its arm after spewing and walks on in <paramref name="walk"/>.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] SpewingEnd(TorizoStance stance, TorizoAnimation walk) =>
    [
        Show(stance.ArmRaised, 3),
        Show(stance.ArmThrust, 3),
        Show(stance.MouthOpening, 3),
        Show(stance.HeadRaised, 3),
        .. WalkingOn(walk),
    ];

    /// <summary>Golden Torizo standing in <paramref name="stance"/> throws a sonic boom with each arm, the near arm's first.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] SonicBooms(TorizoStance stance) =>
    [
        .. SonicBoomThrow(stance.HeadLowered, stance.NearArm, poseUpdates: 3, snapUpdates: 1, loweredUpdates: 4, K.SpawnGoldenTorizoSonicBoom),
        .. SonicBoomThrow(stance.HeadLowered, stance.FarArm, poseUpdates: 3, snapUpdates: 1, loweredUpdates: 4, K.SpawnGoldenTorizoSonicBoom),
    ];

    /// <summary>Golden Torizo, landed from a backward jump, attacks: spewing in <paramref name="spewing"/> or throwing in <paramref name="sonicBooms"/>.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] Attacking(TorizoAnimation spewing, TorizoAnimation sonicBooms) =>
    [
        Cue(new TorizoCue.Choice(TorizoChoiceKind.GoldenAttack, Target: spewing, Alternate: sonicBooms)),
    ];

    /// <summary>
    /// Golden Torizo standing in <paramref name="stance"/> catches a Super Missile: its sound, then its near arm sweeps up and
    /// back down, lifts again as <paramref name="armBeforeThrow"/> says, holds the missile overhead and throws it, and the
    /// caught missile is cleared.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] CatchingSuperMissile(TorizoStance stance, AnimationBeat<EnemyFrameId, TorizoCue>[] armBeforeThrow) =>
    [
        Cue(new TorizoCue.Signal(K.PlaySonicBoomSound)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Falling)),
        Show(stance.HeadLowered, 1),
        Show(stance.NearArm.Down, 1),
        Show(stance.NearArm.Lifting, 1),
        Show(stance.NearArm.Level, 1),
        Show(stance.NearArm.Raised, 1),
        Show(stance.NearArm.Overhead, 1),
        Show(stance.NearArm.Raised, 1),
        Show(stance.NearArm.Level, 1),
        .. armBeforeThrow,
        Show(stance.NearArm.Level, 2),
        Show(stance.NearArm.Raised, 2),
        Cue(new TorizoCue.Signal(K.SpawnSuperMissile)),
        Show(stance.NearArm.Overhead, 48),
        Show(stance.NearArm.Raised, 2),
        Show(stance.NearArm.Level, 2),
        Show(stance.NearArm.Lifting, 2),
        Show(stance.NearArm.Down, 2),
        Show(stance.HeadLowered, 2),
        Show(stance.FarArm.Down, 16),
        Cue(new TorizoCue.Signal(K.ClearCaughtSuperMissile)),
    ];

    /// <summary>A catching arm lifts, dips to its lowered pose, and lifts again.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] ArmDippingBeforeThrow(TorizoArmPoses arm) =>
    [
        Show(arm.Lifting, 1),
        Show(arm.Down, 3),
        Show(arm.Lifting, 2),
    ];

    /// <summary>A catching arm lifts straight on.</summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] ArmLiftingBeforeThrow(TorizoArmPoses arm) =>
    [
        Show(arm.Lifting, 3),
    ];

    /// <summary>
    /// Golden Torizo facing <paramref name="side"/>'s way sits down on Samus morphed behind it, through each pose from its
    /// wide stance to its seat, and stands up again through them, then starts walking.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] SittingDownAttack(TorizoSide side) =>
    [
        Cue(new TorizoCue.Signal(K.StartStanding)),
        Show(side.UprightWideStance, 8),
        SitDown(side, P.SteppingToWideStance),
        Show(side.UprightStepping, 8),
        SitDown(side, P.NearlyUprightToStepping),
        Show(side.NearlyUpright, 6),
        SitDown(side, P.HalfRisenToNearlyUpright),
        Show(side.HalfRisen, 6),
        SitDown(side, P.CrouchedToHalfRisen),
        Show(side.Crouched, 4),
        SitDown(side, P.RisingToCrouched),
        Show(side.RisingFromSeat, 4),
        SitDown(side, P.SeatedToRising),
        Show(side.Seated, 8),
        StandUp(side, P.SeatedToRising),
        Show(side.RisingFromSeat, 8),
        StandUp(side, P.RisingToCrouched),
        Show(side.Crouched, 8),
        StandUp(side, P.CrouchedToHalfRisen),
        Show(side.HalfRisen, 8),
        StandUp(side, P.HalfRisenToNearlyUpright),
        Show(side.NearlyUpright, 10),
        StandUp(side, P.NearlyUprightToStepping),
        Show(side.UprightStepping, 10),
        StandUp(side, P.SteppingToWideStance),
        Cue(new TorizoCue.Signal(K.StartWalking)),
    ];

    /// <summary>
    /// A step of Golden Torizo facing <paramref name="side"/>'s way on <paramref name="stride"/>: it records the step, walks
    /// and its footstep sounds; as its leg lifts it decides as <paramref name="liftingDecisions"/> say, then whether to stagger
    /// or fire its eye beam; after its leg reaches it may lay eggs, deciding as <paramref name="beforeReachingPace"/> says;
    /// after its leg plants it decides as <paramref name="beforePlantingPace"/> says; and it moves forward after each pose.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, TorizoCue>[] Step(TorizoSide side, TorizoStride stride,
        AnimationBeat<EnemyFrameId, TorizoCue>[] liftingDecisions, AnimationBeat<EnemyFrameId, TorizoCue>[] beforeReachingPace,
        AnimationBeat<EnemyFrameId, TorizoCue>[] beforePlantingPace) =>
    [
        Cue(new TorizoCue.Stepped(side.Facing, Foot: stride.Foot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenActive)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(stride.Lifting, 8),
        .. liftingDecisions,
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenStaggerWhenStunned, Animation: A.Stunned)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEyeBeamAtMediumRange, Animation: A.GoldenEyeBeamAttack)),
        Walk(TorizoWalkKind.WalkGoldenTorizo, side, stride.Paces.AfterLifting),
        Show(stride.Swinging, 4),
        Walk(TorizoWalkKind.WalkGoldenTorizo, side, stride.Paces.AfterSwinging),
        Show(stride.Reaching, 4),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEggsAtLowHealth, Animation: A.GoldenReleasingEggs)),
        .. beforeReachingPace,
        Walk(TorizoWalkKind.WalkGoldenTorizo, side, stride.Paces.AfterReaching),
        Show(stride.Planting, 4),
        .. beforePlantingPace,
        Walk(TorizoWalkKind.WalkGoldenTorizo, side, stride.Paces.AfterPlanting),
        Show(stride.Planted, 4),
        Walk(TorizoWalkKind.WalkGoldenTorizo, side, stride.Paces.AfterPlanted),
    ];
}
