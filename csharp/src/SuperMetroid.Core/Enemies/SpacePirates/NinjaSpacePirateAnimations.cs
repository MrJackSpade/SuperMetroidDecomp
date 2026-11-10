using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using K = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateProgram;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations as typed programs (#641). Each
/// frame shows a <see cref="NinjaSpacePirateFrame"/> pose and each callback becomes a <see cref="NinjaSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class NinjaSpacePirateAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue> Segments { get; } = new(
        new Dictionary<NinjaSpacePirateProgram, AnimationBeat<NinjaSpacePirateFrame, NinjaSpacePirateCue>[]>
    {
        [P.ClawAttackLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-16, 8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
        ],
        [P.SpinJumpLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Left, Offset: new(-32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Cue(new NinjaSpacePirateCue.Command(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(0, 8),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.SpinJumpLeftRising)),
        ],
        [P.SpinJumpLeftLoop] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(17, 1),
            Show(18, 1),
            Show(19, 1),
            Show(20, 1),
            Show(21, 1),
            Show(22, 1),
            Show(23, 1),
            Show(24, 1),
        ],
        [P.ActiveFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Active)),
        ],
        [P.ActiveFacingLeftLoop] =
        [
            Show(49, 10),
            Show(50, 10),
            Show(51, 10),
            Show(50, 10),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Cue(new NinjaSpacePirateCue.Command(K.StartActive)),
        ],
        [P.FlinchFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(55, 16),
        ],
        [P.DivekickLeftJump] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(65, 8),
            Cue(new NinjaSpacePirateCue.Command(K.StartLeftDivekick)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickLeftJump)),
        ],
        [P.DivekickLeftJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(57, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(57, 4),
        ],
        [P.DivekickLeftDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickLeftDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(58, 1),
        ],
        [P.WalkToLeftPost] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickLeftWalkToPost)),
        ],
        [P.WalkToLeftPostLoop] =
        [
            Show(33, 5),
            Show(34, 5),
            Show(35, 5),
            Show(36, 5),
            Show(37, 5),
            Show(38, 5),
            Show(39, 5),
            Show(40, 5),
        ],
        [P.InitialFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Initial)),
        ],
        [P.InitialFacingLeftLoop] =
        [
            Show(69, 32),
            Show(70, 10),
            Show(71, 32),
            Show(70, 10),
            Show(69, 32),
        ],
        [P.LandFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(70, 4),
            Show(69, 8),
            Show(75, 4),
            Show(73, 4),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.ReadyToDivekick)),
        ],
        [P.LandFacingLeftLoop] =
        [
            Show(52, 10),
            Show(53, 10),
            Show(54, 10),
            Show(53, 10),
        ],
        [P.KickFacingLeft] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(61, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(67, 4),
            Show(62, 32),
            Show(67, 4),
        ],
        [P.ClawAttackRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Show(12, 5),
            Show(13, 5),
            Show(14, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(10, 2),
            Show(15, 2),
            Show(16, 2),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Show(12, 5),
            Show(13, 5),
            Show(14, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(16, 8))),
            Show(10, 2),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(15, 2),
            Show(16, 2),
        ],
        [P.SpinJumpRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Show(12, 5),
            Show(13, 5),
            Show(14, 2),
            Cue(new NinjaSpacePirateCue.ThrowClaw(Throw: HorizontalDirection.Right, Offset: new(32, -8))),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(10, 2),
            Show(15, 2),
            Show(16, 2),
            Cue(new NinjaSpacePirateCue.Command(K.ResetSpeed)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(9, 8),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.SpinJumpRightRising)),
        ],
        [P.SpinJumpRightLoop] =
        [
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(25, 1),
            Show(26, 1),
            Show(27, 1),
            Show(28, 1),
            Show(29, 1),
            Show(30, 1),
            Show(31, 1),
            Show(32, 1),
        ],
        [P.ActiveFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Active)),
        ],
        [P.ActiveFacingRightLoop] =
        [
            Show(52, 10),
            Show(53, 10),
            Show(54, 10),
            Show(53, 10),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Cue(new NinjaSpacePirateCue.Command(K.StartActive)),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(56, 16),
        ],
        [P.DivekickRightJump] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(66, 8),
            Cue(new NinjaSpacePirateCue.Command(K.StartRightDivekick)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickRightJump)),
        ],
        [P.DivekickRightJumpLoop] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Show(59, 4),
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Show(59, 4),
        ],
        [P.DivekickRightDive] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.GoldWallFlash)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickRightDive)),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(60, 1),
        ],
        [P.WalkToRightPost] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.DivekickRightWalkToPost)),
        ],
        [P.WalkToRightPostLoop] =
        [
            Show(41, 5),
            Show(42, 5),
            Show(43, 5),
            Show(44, 5),
            Show(45, 5),
            Show(46, 5),
            Show(47, 5),
            Show(48, 5),
        ],
        [P.InitialFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.Initial)),
        ],
        [P.InitialFacingRightLoop] =
        [
            Show(72, 32),
            Show(73, 10),
            Show(74, 32),
            Show(73, 10),
            Show(72, 32),
        ],
        [P.LandFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetShading(NinjaSpacePirateShades.Stance)),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(73, 4),
            Show(72, 8),
            Show(75, 4),
            Show(70, 4),
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.ReadyToDivekick)),
        ],
        [P.LandFacingRightLoop] =
        [
            Show(49, 10),
            Show(50, 10),
            Show(51, 10),
            Show(50, 10),
        ],
        [P.KickFacingRight] =
        [
            Cue(new NinjaSpacePirateCue.SetFunction(Function: NinjaSpacePirateFunction.NoOperation)),
            Show(63, 4),
            Cue(new NinjaSpacePirateCue.PlaySound(Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(68, 4),
            Show(64, 32),
            Show(68, 4),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NinjaSpacePirateFrame, NinjaSpacePirateCue> Show(int pose, int duration) => new AnimationBeat<NinjaSpacePirateFrame, NinjaSpacePirateCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<NinjaSpacePirateFrame, NinjaSpacePirateCue> Cue(NinjaSpacePirateCue cue) => new AnimationBeat<NinjaSpacePirateFrame, NinjaSpacePirateCue>.Event(cue);
}
