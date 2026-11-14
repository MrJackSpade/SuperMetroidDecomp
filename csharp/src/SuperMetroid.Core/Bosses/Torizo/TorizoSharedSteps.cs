using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Bosses.Torizo.TorizoCueKind;
using A = SuperMetroid.Core.Bosses.Torizo.TorizoAnimation;
using static SuperMetroid.Core.Bosses.Torizo.TorizoAnimationSteps;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The runs of steps several of Torizo's animations play alike (#1788): its walking steps, and the rising, falling and
/// fall-start steps its jumps and falls share. Each is one array every animation playing it reuses.
/// </summary>
internal static class TorizoSharedSteps
{
    /// <summary>Bomb Torizo's step left on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftRightLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedLeftWithLeftFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst, 10),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingLeftExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 1)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 2)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 3)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 4)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth, 6),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 5)),
    ];

    /// <summary>Bomb Torizo's step left on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftLeftLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedLeftWithRightFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst, 10),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingLeftExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 6)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 7)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 8)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingLeftJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 9)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth, 6),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 0)),
    ];

    /// <summary>Torizo starts a fall facing left: its jump lands it in the landing.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFallStart =
    [
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: A.FacingLeftLanding)),
    ];

    /// <summary>Torizo falling facing left, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFallingPose =
    [
        Show(EnemyFrameId.TorizoFallingLeft, 5),
    ];

    /// <summary>Torizo starts its backward fall facing left: its jump lands it on its left foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallStartToLeftFoot =
    [
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: A.FacingLeftLandingBackwardOnLeftFoot)),
    ];

    /// <summary>Torizo falling backward facing left, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallingToLeftFoot =
    [
        Show(EnemyFrameId.TorizoFallingLeft, 5),
    ];

    /// <summary>Torizo starts its backward fall facing left: its jump lands it on its right foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallStartToRightFoot =
    [
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: A.FacingLeftLandingBackwardOnRightFoot)),
    ];

    /// <summary>Torizo falling backward facing left, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallingToRightFoot =
    [
        Show(EnemyFrameId.TorizoFallingLeft, 5),
    ];

    /// <summary>Faceless Bomb Torizo's step left on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFacelessRightLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedLeftWithLeftFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst, 1),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingLeftExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 1)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 2)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 3)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 4)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 5)),
    ];

    /// <summary>Faceless Bomb Torizo's step left on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFacelessLeftLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedLeftWithRightFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst, 1),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingLeftExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 6)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 7)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 8)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 9)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 0)),
    ];

    /// <summary>Bomb Torizo's step right on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightLeftLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedRightWithRightFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst, 10),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingRightExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 11)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 12)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 13)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 14)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 15)),
    ];

    /// <summary>Bomb Torizo's step right on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightRightLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedRightWithLeftFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst, 10),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingRightExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 16)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 17)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 18)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFront, Target: A.FacingRightJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 19)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth, 6),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Step: 10)),
    ];

    /// <summary>Torizo starts a fall facing right: its jump lands it in the landing.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFallStart =
    [
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: A.FacingRightLanding)),
    ];

    /// <summary>Torizo falling facing right, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFallingPose =
    [
        Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
    ];

    /// <summary>Torizo starts its backward fall facing right: its jump lands it on its right foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallStartToRightFoot =
    [
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: A.FacingRightLandingBackwardOnRightFoot)),
    ];

    /// <summary>Torizo falling backward facing right, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallingToRightFoot =
    [
        Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
    ];

    /// <summary>Torizo starts its backward fall facing right: its jump lands it on its left foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallStartToLeftFoot =
    [
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.Jump)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.Link, Target: A.FacingRightLandingBackwardOnLeftFoot)),
    ];

    /// <summary>Torizo falling backward facing right, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallingToLeftFoot =
    [
        Show(EnemyFrameId.TorizoFacingRightJumpingForwardsSecond, 5),
    ];

    /// <summary>Faceless Bomb Torizo's step right on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFacelessLeftLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedRightWithRightFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst, 1),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingRightExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 11)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 12)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 13)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 14)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 15)),
    ];

    /// <summary>Faceless Bomb Torizo's step right on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFacelessRightLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedRightWithLeftFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.AirTransition)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst, 1),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfSamusCloseBehind, Target: A.FacingRightExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 16)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 17)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 18)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 19)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Step: 10)),
    ];

    /// <summary>Golden Torizo's step left on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingLeftRightLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedLeftWithLeftFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenFrameBehavior)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenAirTransition)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFirst, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpForwardIfSamusFar, Target: A.FacingLeftJumpingForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingLeftJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfStunHealthHigh, Target: A.Stunned)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfSamusMorphedInFront, Target: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 1)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingSecond, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 2)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingThird, 4),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfHealthLow, Target: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingLeftJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 3)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFourth, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 4)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegMovingFifth, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 5)),
    ];

    /// <summary>Golden Torizo's step left on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingLeftLeftLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedLeftWithRightFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenFrameBehavior)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenAirTransition)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFirst, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfSamusMorphedBehind, Target: A.GoldenFacingLeftSittingDownAttack)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpForwardIfSamusFar, Target: A.FacingLeftJumpingForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingLeftJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfStunHealthHigh, Target: A.Stunned)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfSamusMorphedInFront, Target: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 6)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingSecond, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 7)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingThird, 4),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfHealthLow, Target: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingLeftJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 8)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFourth, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 9)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegMovingFifth, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 0)),
    ];

    /// <summary>Golden Torizo's step right on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingRightLeftLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedRightWithRightFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenFrameBehavior)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenAirTransition)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFirst, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpForwardIfSamusFar, Target: A.FacingRightJumpingForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingRightJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfStunHealthHigh, Target: A.Stunned)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfSamusMorphedInFront, Target: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 11)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingSecond, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 12)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingThird, 4),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfHealthLow, Target: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 13)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFourth, 4),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingRightJumpingBackwardToRightFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 14)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegMovingFifth, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 15)),
    ];

    /// <summary>Golden Torizo's step right on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingRightRightLegStep =
    [
        Cue(new TorizoCue.Signal(K.SteppedRightWithLeftFoot)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenFrameBehavior)),
        Cue(new TorizoCue.SetFrameBehavior(FrameBehavior: TorizoFrameBehavior.GoldenAirTransition)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFirst, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoToIfSamusMorphedBehind, Target: A.GoldenFacingRightSittingDownAttack)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpForwardIfSamusFar, Target: A.FacingRightJumpingForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingRightJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallIfStunHealthHigh, Target: A.Stunned)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfSamusMorphedInFront, Target: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 16)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingSecond, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 17)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingThird, 4),
        Cue(new TorizoCue.Branch(TorizoBranchKind.CallSometimesIfHealthLow, Target: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden, Target: A.FacingRightJumpingBackwardToLeftFoot)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 18)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFourth, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 19)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegMovingFifth, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Step: 10)),
    ];
}
