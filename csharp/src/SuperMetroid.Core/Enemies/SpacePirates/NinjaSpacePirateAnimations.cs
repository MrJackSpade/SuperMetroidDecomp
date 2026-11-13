using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using K = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="NinjaSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class NinjaSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NinjaSpacePirateProgram, EnemyFrameId, NinjaSpacePirateCue> Segments { get; } = new(
        new Dictionary<NinjaSpacePirateProgram, AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>[]>
    {
        [P.ClawAttackLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose0, 5),
            Show(EnemyFrameId.NinjaPiratePose1, 5),
            Show(EnemyFrameId.NinjaPiratePose2, 5),
            Show(EnemyFrameId.NinjaPiratePose3, 5),
            Show(EnemyFrameId.NinjaPiratePose4, 5),
            Show(EnemyFrameId.NinjaPiratePose5, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose6, 2),
            Show(EnemyFrameId.NinjaPiratePose7, 2),
            Show(EnemyFrameId.NinjaPiratePose8, 2),
            Show(EnemyFrameId.NinjaPiratePose0, 5),
            Show(EnemyFrameId.NinjaPiratePose1, 5),
            Show(EnemyFrameId.NinjaPiratePose2, 5),
            Show(EnemyFrameId.NinjaPiratePose3, 5),
            Show(EnemyFrameId.NinjaPiratePose4, 5),
            Show(EnemyFrameId.NinjaPiratePose5, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-16, 8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose6, 2),
            Show(EnemyFrameId.NinjaPiratePose7, 2),
            Show(EnemyFrameId.NinjaPiratePose8, 2),
        ],
        [P.SpinJumpLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose0, 5),
            Show(EnemyFrameId.NinjaPiratePose1, 5),
            Show(EnemyFrameId.NinjaPiratePose2, 5),
            Show(EnemyFrameId.NinjaPiratePose3, 5),
            Show(EnemyFrameId.NinjaPiratePose4, 5),
            Show(EnemyFrameId.NinjaPiratePose5, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose6, 2),
            Show(EnemyFrameId.NinjaPiratePose7, 2),
            Show(EnemyFrameId.NinjaPiratePose8, 2),
            Cue(new NinjaSpacePirateCue.Command(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose0, 8),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.SpinJumpLeftRising)),
        ],
        [P.SpinJumpLeftLoop] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(EnemyFrameId.NinjaPiratePose17, 1),
            Show(EnemyFrameId.NinjaPiratePose18, 1),
            Show(EnemyFrameId.NinjaPiratePose19, 1),
            Show(EnemyFrameId.NinjaPiratePose20, 1),
            Show(EnemyFrameId.NinjaPiratePose21, 1),
            Show(EnemyFrameId.NinjaPiratePose22, 1),
            Show(EnemyFrameId.NinjaPiratePose23, 1),
            Show(EnemyFrameId.NinjaPiratePose24, 1),
        ],
        [P.ActiveFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Active)),
        ],
        [P.ActiveFacingLeftLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose49, 10),
            Show(EnemyFrameId.NinjaPiratePose50, 10),
            Show(EnemyFrameId.NinjaPiratePose51, 10),
            Show(EnemyFrameId.NinjaPiratePose50, 10),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Cue(new NinjaSpacePirateCue.Command(K.StartActive)),
        ],
        [P.FlinchFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose55, 16),
        ],
        [P.DivekickLeftJump] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose65, 8),
            Cue(new NinjaSpacePirateCue.Command(K.StartLeftDivekick)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickLeftJump)),
        ],
        [P.DivekickLeftJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPiratePose57, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPiratePose57, 4),
        ],
        [P.DivekickLeftDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickLeftDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose58, 1),
        ],
        [P.WalkToLeftPost] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickLeftWalkToPost)),
        ],
        [P.WalkToLeftPostLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose33, 5),
            Show(EnemyFrameId.NinjaPiratePose34, 5),
            Show(EnemyFrameId.NinjaPiratePose35, 5),
            Show(EnemyFrameId.NinjaPiratePose36, 5),
            Show(EnemyFrameId.NinjaPiratePose37, 5),
            Show(EnemyFrameId.NinjaPiratePose38, 5),
            Show(EnemyFrameId.NinjaPiratePose39, 5),
            Show(EnemyFrameId.NinjaPiratePose40, 5),
        ],
        [P.InitialFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Initial)),
        ],
        [P.InitialFacingLeftLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose69, 32),
            Show(EnemyFrameId.NinjaPiratePose70, 10),
            Show(EnemyFrameId.NinjaPiratePose71, 32),
            Show(EnemyFrameId.NinjaPiratePose70, 10),
            Show(EnemyFrameId.NinjaPiratePose69, 32),
        ],
        [P.LandFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose70, 4),
            Show(EnemyFrameId.NinjaPiratePose69, 8),
            Show(EnemyFrameId.NinjaPiratePose75, 4),
            Show(EnemyFrameId.NinjaPiratePose73, 4),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.ReadyToDivekick)),
        ],
        [P.LandFacingLeftLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose52, 10),
            Show(EnemyFrameId.NinjaPiratePose53, 10),
            Show(EnemyFrameId.NinjaPiratePose54, 10),
            Show(EnemyFrameId.NinjaPiratePose53, 10),
        ],
        [P.KickFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose61, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose67, 4),
            Show(EnemyFrameId.NinjaPiratePose62, 32),
            Show(EnemyFrameId.NinjaPiratePose67, 4),
        ],
        [P.ClawAttackRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose9, 5),
            Show(EnemyFrameId.NinjaPiratePose10, 5),
            Show(EnemyFrameId.NinjaPiratePose11, 5),
            Show(EnemyFrameId.NinjaPiratePose12, 5),
            Show(EnemyFrameId.NinjaPiratePose13, 5),
            Show(EnemyFrameId.NinjaPiratePose14, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose10, 2),
            Show(EnemyFrameId.NinjaPiratePose15, 2),
            Show(EnemyFrameId.NinjaPiratePose16, 2),
            Show(EnemyFrameId.NinjaPiratePose9, 5),
            Show(EnemyFrameId.NinjaPiratePose10, 5),
            Show(EnemyFrameId.NinjaPiratePose11, 5),
            Show(EnemyFrameId.NinjaPiratePose12, 5),
            Show(EnemyFrameId.NinjaPiratePose13, 5),
            Show(EnemyFrameId.NinjaPiratePose14, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(16, 8))),
            Show(EnemyFrameId.NinjaPiratePose10, 2),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose15, 2),
            Show(EnemyFrameId.NinjaPiratePose16, 2),
        ],
        [P.SpinJumpRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose9, 5),
            Show(EnemyFrameId.NinjaPiratePose10, 5),
            Show(EnemyFrameId.NinjaPiratePose11, 5),
            Show(EnemyFrameId.NinjaPiratePose12, 5),
            Show(EnemyFrameId.NinjaPiratePose13, 5),
            Show(EnemyFrameId.NinjaPiratePose14, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose10, 2),
            Show(EnemyFrameId.NinjaPiratePose15, 2),
            Show(EnemyFrameId.NinjaPiratePose16, 2),
            Cue(new NinjaSpacePirateCue.Command(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose9, 8),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.SpinJumpRightRising)),
        ],
        [P.SpinJumpRightLoop] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(EnemyFrameId.NinjaPiratePose25, 1),
            Show(EnemyFrameId.NinjaPiratePose26, 1),
            Show(EnemyFrameId.NinjaPiratePose27, 1),
            Show(EnemyFrameId.NinjaPiratePose28, 1),
            Show(EnemyFrameId.NinjaPiratePose29, 1),
            Show(EnemyFrameId.NinjaPiratePose30, 1),
            Show(EnemyFrameId.NinjaPiratePose31, 1),
            Show(EnemyFrameId.NinjaPiratePose32, 1),
        ],
        [P.ActiveFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Active)),
        ],
        [P.ActiveFacingRightLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose52, 10),
            Show(EnemyFrameId.NinjaPiratePose53, 10),
            Show(EnemyFrameId.NinjaPiratePose54, 10),
            Show(EnemyFrameId.NinjaPiratePose53, 10),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Cue(new NinjaSpacePirateCue.Command(K.StartActive)),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose56, 16),
        ],
        [P.DivekickRightJump] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose66, 8),
            Cue(new NinjaSpacePirateCue.Command(K.StartRightDivekick)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickRightJump)),
        ],
        [P.DivekickRightJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(EnemyFrameId.NinjaPiratePose59, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(EnemyFrameId.NinjaPiratePose59, 4),
        ],
        [P.DivekickRightDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickRightDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose60, 1),
        ],
        [P.WalkToRightPost] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickRightWalkToPost)),
        ],
        [P.WalkToRightPostLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose41, 5),
            Show(EnemyFrameId.NinjaPiratePose42, 5),
            Show(EnemyFrameId.NinjaPiratePose43, 5),
            Show(EnemyFrameId.NinjaPiratePose44, 5),
            Show(EnemyFrameId.NinjaPiratePose45, 5),
            Show(EnemyFrameId.NinjaPiratePose46, 5),
            Show(EnemyFrameId.NinjaPiratePose47, 5),
            Show(EnemyFrameId.NinjaPiratePose48, 5),
        ],
        [P.InitialFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Initial)),
        ],
        [P.InitialFacingRightLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose72, 32),
            Show(EnemyFrameId.NinjaPiratePose73, 10),
            Show(EnemyFrameId.NinjaPiratePose74, 32),
            Show(EnemyFrameId.NinjaPiratePose73, 10),
            Show(EnemyFrameId.NinjaPiratePose72, 32),
        ],
        [P.LandFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose73, 4),
            Show(EnemyFrameId.NinjaPiratePose72, 8),
            Show(EnemyFrameId.NinjaPiratePose75, 4),
            Show(EnemyFrameId.NinjaPiratePose70, 4),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.ReadyToDivekick)),
        ],
        [P.LandFacingRightLoop] =
        [
            Show(EnemyFrameId.NinjaPiratePose49, 10),
            Show(EnemyFrameId.NinjaPiratePose50, 10),
            Show(EnemyFrameId.NinjaPiratePose51, 10),
            Show(EnemyFrameId.NinjaPiratePose50, 10),
        ],
        [P.KickFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(EnemyFrameId.NinjaPiratePose63, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(EnemyFrameId.NinjaPiratePose68, 4),
            Show(EnemyFrameId.NinjaPiratePose64, 32),
            Show(EnemyFrameId.NinjaPiratePose68, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NinjaSpacePirateProgram, Animation<NinjaSpacePirateProgram>> StartingAt { get; } = new Dictionary<NinjaSpacePirateProgram, Animation<NinjaSpacePirateProgram>>
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
    internal static Animation<NinjaSpacePirateProgram> StartingWith(NinjaSpacePirateProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, NinjaSpacePirateCue> Cue(NinjaSpacePirateCue cue) => new AnimationBeat<EnemyFrameId, NinjaSpacePirateCue>.Event(cue);
}
