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
        Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Left)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting, 10),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -5)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -19)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -16)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted, 6),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -7)),
    ];

    /// <summary>Bomb Torizo's step left on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftLeftLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting, 10),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -7)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -17)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -18)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted, 6),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: -5)),
    ];

    /// <summary>Torizo starts a fall facing left: its jump lands it in the landing.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFallStart =
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: A.FacingLeftLanding)),
    ];

    /// <summary>Torizo falling facing left, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFallingPose =
    [
        Show(EnemyFrameId.TorizoFallingLeft, 5),
    ];

    /// <summary>Torizo starts its backward fall facing left: its jump lands it on its left foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallStartToLeftFoot =
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: A.FacingLeftLandingBackwardOnLeftFoot)),
    ];

    /// <summary>Torizo falling backward facing left, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallingToLeftFoot =
    [
        Show(EnemyFrameId.TorizoFallingLeft, 5),
    ];

    /// <summary>Torizo starts its backward fall facing left: its jump lands it on its right foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallStartToRightFoot =
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: A.FacingLeftLandingBackwardOnRightFoot)),
    ];

    /// <summary>Torizo falling backward facing left, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftBackwardFallingToRightFoot =
    [
        Show(EnemyFrameId.TorizoFallingLeft, 5),
    ];

    /// <summary>Faceless Bomb Torizo's step left on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFacelessRightLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Left)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting, 1),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -5)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -19)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -16)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -7)),
    ];

    /// <summary>Faceless Bomb Torizo's step left on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingLeftFacelessLeftLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting, 1),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingLeftExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -7)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -17)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -18)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: -5)),
    ];

    /// <summary>Bomb Torizo's step right on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightLeftLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Right)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegLifting, 10),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingRightExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegSwinging, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 5)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegReaching, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 19)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanting, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 16)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanted, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 7)),
    ];

    /// <summary>Bomb Torizo's step right on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightRightLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Left)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegLifting, 10),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingRightExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegSwinging, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 7)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegReaching, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 17)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegPlanting, 6),
        Cue(new TorizoCue.Branch(TorizoBranchKind.BombJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 18)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegPlanted, 6),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkBombTorizo, Velocity: 5)),
    ];

    /// <summary>Torizo starts a fall facing right: its jump lands it in the landing.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFallStart =
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: A.FacingRightLanding)),
    ];

    /// <summary>Torizo falling facing right, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFallingPose =
    [
        Show(EnemyFrameId.TorizoFacingRightSpringing, 5),
    ];

    /// <summary>Torizo starts its backward fall facing right: its jump lands it on its right foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallStartToRightFoot =
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: A.FacingRightLandingBackwardOnRightFoot)),
    ];

    /// <summary>Torizo falling backward facing right, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallingToRightFoot =
    [
        Show(EnemyFrameId.TorizoFacingRightSpringing, 5),
    ];

    /// <summary>Torizo starts its backward fall facing right: its jump lands it on its left foot.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallStartToLeftFoot =
    [
        Cue(new TorizoCue.Signal(K.StartLeaping)),
        Cue(new TorizoCue.LandIn(Landing: A.FacingRightLandingBackwardOnLeftFoot)),
    ];

    /// <summary>Torizo falling backward facing right, the pose its fall rests in.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightBackwardFallingToLeftFoot =
    [
        Show(EnemyFrameId.TorizoFacingRightSpringing, 5),
    ];

    /// <summary>Faceless Bomb Torizo's step right on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFacelessLeftLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Right)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegLifting, 1),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingRightExplosiveSwipeRightFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegSwinging, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 5)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegReaching, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 19)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanting, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 16)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanted, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 7)),
    ];

    /// <summary>Faceless Bomb Torizo's step right on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] FacingRightFacelessRightLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Left)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.Active)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.SpawnLowHealthDrool)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegLifting, 1),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.BombSwipeAtSamusBehind, Animation: A.FacingRightExplosiveSwipeLeftFootForward)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegSwinging, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 7)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegReaching, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 17)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegPlanting, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 18)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegPlanted, 5),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkFacelessBombTorizo, Velocity: 5)),
    ];

    /// <summary>Golden Torizo's step left on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingLeftRightLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Left)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenActive)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegLifting, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenLeapAtDistantSamus)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenStaggerWhenStunned, Animation: A.Stunned)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEyeBeamAtMediumRange, Animation: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegSwinging, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -5)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegReaching, 4),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEggsAtLowHealth, Animation: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -19)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanting, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -16)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingRightLegPlanted, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -7)),
    ];

    /// <summary>Golden Torizo's step left on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingLeftLeftLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Left, Foot: HorizontalDirection.Right)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenActive)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegLifting, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenSitOnSamusMorphedBehind)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenLeapAtDistantSamus)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenStaggerWhenStunned, Animation: A.Stunned)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEyeBeamAtMediumRange, Animation: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegSwinging, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -7)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegReaching, 4),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEggsAtLowHealth, Animation: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -17)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanting, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -18)),
        Show(EnemyFrameId.TorizoFacingLeftWalkingLeftLegPlanted, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: -5)),
    ];

    /// <summary>Golden Torizo's step right on its left leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingRightLeftLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Right)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenActive)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegLifting, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenLeapAtDistantSamus)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenStaggerWhenStunned, Animation: A.Stunned)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEyeBeamAtMediumRange, Animation: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegSwinging, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 5)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegReaching, 4),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEggsAtLowHealth, Animation: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 19)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanting, 4),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 16)),
        Show(EnemyFrameId.TorizoFacingRightWalkingLeftLegPlanted, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 7)),
    ];

    /// <summary>Golden Torizo's step right on its right leg.</summary>
    internal static readonly AnimationBeat<EnemyFrameId, TorizoCue>[] GoldenFacingRightRightLegStep =
    [
        Cue(new TorizoCue.Stepped(HorizontalDirection.Right, Foot: HorizontalDirection.Left)),
        Cue(new TorizoCue.SetPhase(Phase: TorizoPhase.GoldenActive)),
        Cue(new TorizoCue.Signal(K.StartWalking)),
        Cue(new TorizoCue.Signal(K.PlayFootstepSound)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegLifting, 8),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenSitOnSamusMorphedBehind)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenLeapAtDistantSamus)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenStaggerWhenStunned, Animation: A.Stunned)),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEyeBeamAtMediumRange, Animation: A.GoldenEyeBeamAttack)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 0)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegSwinging, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 7)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegReaching, 4),
        Cue(new TorizoCue.Interrupt(TorizoInterruptKind.GoldenEggsAtLowHealth, Animation: A.GoldenReleasingEggs)),
        Cue(new TorizoCue.Branch(TorizoBranchKind.GoldenJumpBackFromSamusInFront)),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 17)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegPlanting, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 18)),
        Show(EnemyFrameId.TorizoFacingRightWalkingRightLegPlanted, 4),
        Cue(new TorizoCue.Walk(TorizoWalkKind.WalkGoldenTorizo, Velocity: 5)),
    ];
}
