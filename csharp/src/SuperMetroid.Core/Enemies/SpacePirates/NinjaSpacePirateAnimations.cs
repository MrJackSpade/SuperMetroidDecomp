using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using K = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="NinjaSpacePirateCue"/> with the values it acts on.
/// </summary>
internal static class NinjaSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NinjaSpacePirateSegment, EnemyFrameId, NinjaSpacePirateCue> Segments { get; } = new(
        new Dictionary<NinjaSpacePirateSegment, AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[]>
    {
        [P.ClawAttackLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFirst, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSecond, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftThird, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFourth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFifth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSixth, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSeventh, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftEighth, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftNinth, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFirst, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSecond, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftThird, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFourth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFifth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSixth, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-16, 8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSeventh, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftEighth, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftNinth, 2),
        ],
        [P.SpinJumpLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFirst, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSecond, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftThird, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFourth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFifth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSixth, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftSeventh, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftEighth, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftNinth, 2),
            Cue(new NinjaSpacePirateCue.Signal(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateClawAttackLeftFirst, 8),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.SpinJumpLeftRising)),
        ],
        [P.SpinJumpLeftLoop] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopFirst, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopSecond, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopThird, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopFourth, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopFifth, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopSixth, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopSeventh, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpLeftLoopEighth, 1),
        ],
        [P.ActiveFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.Active)),
        ],
        [P.ActiveFacingLeftLoop] =
        [
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopFirst, 10),
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopSecond, 10),
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopThird, 10),
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopSecond, 10),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Cue(new NinjaSpacePirateCue.Signal(K.StartActive)),
        ],
        [P.FlinchFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateFlinchFacingLeft, 16),
        ],
        [P.DivekickLeftJump] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateDivekickLeftJump, 8),
            Cue(new NinjaSpacePirateCue.Signal(K.StartLeftDivekick)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.DivekickLeftJump)),
        ],
        [P.DivekickLeftJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPirateDivekickLeftJumpLoop, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPirateDivekickLeftJumpLoop, 4),
        ],
        [P.DivekickLeftDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.DivekickLeftDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateDivekickLeftDive, 1),
        ],
        [P.WalkToLeftPost] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.DivekickLeftWalkToPost)),
        ],
        [P.WalkToLeftPostLoop] =
        [
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopFirst, 5),
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopSecond, 5),
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopThird, 5),
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopFourth, 5),
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopFifth, 5),
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopSixth, 5),
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopSeventh, 5),
            Show(EnemyFrameId.NinjaPirateWalkToLeftPostLoopEighth, 5),
        ],
        [P.InitialFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.Initial)),
        ],
        [P.InitialFacingLeftLoop] =
        [
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopFirst, 32),
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopSecond, 10),
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopThird, 32),
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopSecond, 10),
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopFirst, 32),
        ],
        [P.LandFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopSecond, 4),
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopFirst, 8),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftThird, 4),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftFourth, 4),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.ReadyToDivekick)),
        ],
        [P.LandFacingLeftLoop] =
        [
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopFirst, 10),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopSecond, 10),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopThird, 10),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopSecond, 10),
        ],
        [P.KickFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateKickFacingLeftFirst, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateKickFacingLeftSecond, 4),
            Show(EnemyFrameId.NinjaPirateKickFacingLeftThird, 32),
            Show(EnemyFrameId.NinjaPirateKickFacingLeftSecond, 4),
        ],
        [P.ClawAttackRight] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFirst, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSecond, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightThird, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFourth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFifth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSixth, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSecond, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSeventh, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackRightEighth, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFirst, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSecond, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightThird, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFourth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFifth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSixth, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(16, 8))),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSecond, 2),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSeventh, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackRightEighth, 2),
        ],
        [P.SpinJumpRight] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFirst, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSecond, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightThird, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFourth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFifth, 5),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSixth, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSecond, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackRightSeventh, 2),
            Show(EnemyFrameId.NinjaPirateClawAttackRightEighth, 2),
            Cue(new NinjaSpacePirateCue.Signal(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateClawAttackRightFirst, 8),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.SpinJumpRightRising)),
        ],
        [P.SpinJumpRightLoop] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopFirst, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopSecond, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopThird, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopFourth, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopFifth, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopSixth, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopSeventh, 1),
            Show(EnemyFrameId.NinjaPirateSpinJumpRightLoopEighth, 1),
        ],
        [P.ActiveFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.Active)),
        ],
        [P.ActiveFacingRightLoop] =
        [
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopFirst, 10),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopSecond, 10),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopThird, 10),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftLoopSecond, 10),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Cue(new NinjaSpacePirateCue.Signal(K.StartActive)),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateFlinchFacingRight, 16),
        ],
        [P.DivekickRightJump] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateDivekickRightJump, 8),
            Cue(new NinjaSpacePirateCue.Signal(K.StartRightDivekick)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.DivekickRightJump)),
        ],
        [P.DivekickRightJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPirateDivekickRightJumpLoop, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPirateDivekickRightJumpLoop, 4),
        ],
        [P.DivekickRightDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.DivekickRightDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateDivekickRightDive, 1),
        ],
        [P.WalkToRightPost] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.DivekickRightWalkToPost)),
        ],
        [P.WalkToRightPostLoop] =
        [
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopFirst, 5),
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopSecond, 5),
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopThird, 5),
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopFourth, 5),
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopFifth, 5),
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopSixth, 5),
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopSeventh, 5),
            Show(EnemyFrameId.NinjaPirateWalkToRightPostLoopEighth, 5),
        ],
        [P.InitialFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.Initial)),
        ],
        [P.InitialFacingRightLoop] =
        [
            Show(EnemyFrameId.NinjaPirateInitialFacingRightLoopFirst, 32),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftFourth, 10),
            Show(EnemyFrameId.NinjaPirateInitialFacingRightLoopThird, 32),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftFourth, 10),
            Show(EnemyFrameId.NinjaPirateInitialFacingRightLoopFirst, 32),
        ],
        [P.LandFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftFourth, 4),
            Show(EnemyFrameId.NinjaPirateInitialFacingRightLoopFirst, 8),
            Show(EnemyFrameId.NinjaPirateLandFacingLeftThird, 4),
            Show(EnemyFrameId.NinjaPirateInitialFacingLeftLoopSecond, 4),
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.ReadyToDivekick)),
        ],
        [P.LandFacingRightLoop] =
        [
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopFirst, 10),
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopSecond, 10),
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopThird, 10),
            Show(EnemyFrameId.NinjaPirateActiveFacingLeftLoopSecond, 10),
        ],
        [P.KickFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetAction(Action: NinjaSpacePirateAction.NoOperation)),
            Show(EnemyFrameId.NinjaPirateKickFacingRightFirst, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPirateKickFacingRightSecond, 4),
            Show(EnemyFrameId.NinjaPirateKickFacingRightThird, 32),
            Show(EnemyFrameId.NinjaPirateKickFacingRightSecond, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NinjaSpacePirateSegment, Animation<NinjaSpacePirateSegment>> StartingAt { get; } = new Dictionary<NinjaSpacePirateSegment, Animation<NinjaSpacePirateSegment>>
    {
        [P.ClawAttackLeft] = new([new(P.ClawAttackLeft, 0, 23, 1), new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.SpinJumpLeft] = new([new(P.SpinJumpLeft, 0, 16, 1), new(P.SpinJumpLeftLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpinJumpLeftLoop] = new([new(P.SpinJumpLeftLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ActiveFacingLeft] = new([new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ActiveFacingLeftLoop] = new([new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlinchFacingLeft] = new([new(P.FlinchFacingLeft, 0, 2, 1), new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.DivekickLeftJump] = new([new(P.DivekickLeftJump, 0, 4, 1), new(P.DivekickLeftJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DivekickLeftJumpLoop] = new([new(P.DivekickLeftJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DivekickLeftDive] = new([new(P.DivekickLeftDive, 0, 4, 1)], AnimationEnd.Holds),
        [P.WalkToLeftPost] = new([new(P.WalkToLeftPost, 0, 1, 1), new(P.WalkToLeftPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkToLeftPostLoop] = new([new(P.WalkToLeftPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.InitialFacingLeft] = new([new(P.InitialFacingLeft, 0, 1, 1), new(P.InitialFacingLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InitialFacingLeftLoop] = new([new(P.InitialFacingLeftLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LandFacingLeft] = new([new(P.LandFacingLeft, 0, 7, 1), new(P.LandFacingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LandFacingLeftLoop] = new([new(P.LandFacingLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KickFacingLeft] = new([new(P.KickFacingLeft, 0, 6, 1), new(P.ActiveFacingLeft, 0, 1, 1), new(P.ActiveFacingLeftLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.ClawAttackRight] = new([new(P.ClawAttackRight, 0, 23, 1), new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.SpinJumpRight] = new([new(P.SpinJumpRight, 0, 16, 1), new(P.SpinJumpRightLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpinJumpRightLoop] = new([new(P.SpinJumpRightLoop, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ActiveFacingRight] = new([new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ActiveFacingRightLoop] = new([new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlinchFacingRight] = new([new(P.FlinchFacingRight, 0, 2, 1), new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.DivekickRightJump] = new([new(P.DivekickRightJump, 0, 4, 1), new(P.DivekickRightJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DivekickRightJumpLoop] = new([new(P.DivekickRightJumpLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DivekickRightDive] = new([new(P.DivekickRightDive, 0, 4, 1)], AnimationEnd.Holds),
        [P.WalkToRightPost] = new([new(P.WalkToRightPost, 0, 1, 1), new(P.WalkToRightPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WalkToRightPostLoop] = new([new(P.WalkToRightPostLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.InitialFacingRight] = new([new(P.InitialFacingRight, 0, 1, 1), new(P.InitialFacingRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InitialFacingRightLoop] = new([new(P.InitialFacingRightLoop, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        [P.LandFacingRight] = new([new(P.LandFacingRight, 0, 7, 1), new(P.LandFacingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.LandFacingRightLoop] = new([new(P.LandFacingRightLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.KickFacingRight] = new([new(P.KickFacingRight, 0, 6, 1), new(P.ActiveFacingRight, 0, 1, 1), new(P.ActiveFacingRightLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NinjaSpacePirateSegment> StartingWith(NinjaSpacePirateSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Cue(NinjaSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Event(cue);
}
