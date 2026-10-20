using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateCueKind;
using P = SuperMetroid.Core.Enemies.SpacePirates.NinjaSpacePirateProgram;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The ninja Space Pirates' stance, kick, claw, spin-jump and divekick animations as typed programs (#641), in place of their bank-$B2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="NinjaSpacePirateFrame"/> pose and each callback becomes a <see cref="NinjaSpacePirateCue"/> with its typed operands.
/// </summary>
internal static class NinjaSpacePirateAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue> Programs { get; } = new(
        new Dictionary<NinjaSpacePirateProgram, AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue>[]>
    {
        [P.ClawAttackLeft] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 2),
            Cue(new(K.ThrowClaw, ThrowsRight: false, Offset: -32, OffsetY: -8)),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 2),
            Cue(new(K.ThrowClaw, ThrowsRight: false, Offset: -16, OffsetY: 8)),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            GoTo(P.ActiveFacingLeft),
        ],
        [P.SpinJumpLeft] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 2),
            Cue(new(K.ThrowClaw, ThrowsRight: false, Offset: -32, OffsetY: -8)),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(6, 2),
            Show(7, 2),
            Show(8, 2),
            Cue(new(K.ResetSpeed)),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(0, 8),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.SpinJumpLeftRising)),
            FallInto(P.SpinJumpLeftLoop),
        ],
        [P.SpinJumpLeftLoop] =
        [
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(17, 1),
            Show(18, 1),
            Show(19, 1),
            Show(20, 1),
            Show(21, 1),
            Show(22, 1),
            Show(23, 1),
            Show(24, 1),
            GoTo(P.SpinJumpLeftLoop),
        ],
        [P.ActiveFacingLeft] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.Active)),
            FallInto(P.ActiveFacingLeftLoop),
        ],
        [P.ActiveFacingLeftLoop] =
        [
            Show(49, 10),
            Show(50, 10),
            Show(51, 10),
            Show(50, 10),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Cue(new(K.StartActive)),
            GoTo(P.ActiveFacingLeftLoop),
        ],
        [P.FlinchFacingLeft] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(55, 16),
            GoTo(P.ActiveFacingLeft),
        ],
        [P.DivekickLeftJump] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(65, 8),
            Cue(new(K.StartLeftDivekick)),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.DivekickLeftJump)),
            FallInto(P.DivekickLeftJumpLoop),
        ],
        [P.DivekickLeftJumpLoop] =
        [
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(1))),
            Show(57, 4),
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(7))),
            Show(57, 4),
            GoTo(P.DivekickLeftJumpLoop),
        ],
        [P.DivekickLeftDive] =
        [
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(7))),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.DivekickLeftDive)),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(58, 1),
            Hold,
        ],
        [P.WalkToLeftPost] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.DivekickLeftWalkToPost)),
            FallInto(P.WalkToLeftPostLoop),
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
            GoTo(P.WalkToLeftPostLoop),
        ],
        [P.InitialFacingLeft] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.Initial)),
            FallInto(P.InitialFacingLeftLoop),
        ],
        [P.InitialFacingLeftLoop] =
        [
            Show(69, 32),
            Show(70, 10),
            Show(71, 32),
            Show(70, 10),
            Show(69, 32),
            GoTo(P.InitialFacingLeftLoop),
        ],
        [P.LandFacingLeft] =
        [
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(1))),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(70, 4),
            Show(69, 8),
            Show(75, 4),
            Show(73, 4),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.ReadyToDivekick)),
            FallInto(P.LandFacingLeftLoop),
        ],
        [P.LandFacingLeftLoop] =
        [
            Show(52, 10),
            Show(53, 10),
            Show(54, 10),
            Show(53, 10),
            GoTo(P.LandFacingLeftLoop),
        ],
        [P.KickFacingLeft] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(61, 4),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(67, 4),
            Show(62, 32),
            Show(67, 4),
            GoTo(P.ActiveFacingLeft),
        ],
        [P.ClawAttackRight] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Show(12, 5),
            Show(13, 5),
            Show(14, 2),
            Cue(new(K.ThrowClaw, ThrowsRight: true, Offset: 32, OffsetY: -8)),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(10, 2),
            Show(15, 2),
            Show(16, 2),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Show(12, 5),
            Show(13, 5),
            Show(14, 2),
            Cue(new(K.ThrowClaw, ThrowsRight: true, Offset: 16, OffsetY: 8)),
            Show(10, 2),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(15, 2),
            Show(16, 2),
            GoTo(P.ActiveFacingRight),
        ],
        [P.SpinJumpRight] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Show(12, 5),
            Show(13, 5),
            Show(14, 2),
            Cue(new(K.ThrowClaw, ThrowsRight: true, Offset: 32, OffsetY: -8)),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(10, 2),
            Show(15, 2),
            Show(16, 2),
            Cue(new(K.ResetSpeed)),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(9, 8),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.SpinJumpRightRising)),
            FallInto(P.SpinJumpRightLoop),
        ],
        [P.SpinJumpRightLoop] =
        [
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.SpinJump)),
            Show(25, 1),
            Show(26, 1),
            Show(27, 1),
            Show(28, 1),
            Show(29, 1),
            Show(30, 1),
            Show(31, 1),
            Show(32, 1),
            GoTo(P.SpinJumpRightLoop),
        ],
        [P.ActiveFacingRight] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.Active)),
            FallInto(P.ActiveFacingRightLoop),
        ],
        [P.ActiveFacingRightLoop] =
        [
            Show(52, 10),
            Show(53, 10),
            Show(54, 10),
            Show(53, 10),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Cue(new(K.StartActive)),
            GoTo(P.ActiveFacingRightLoop),
        ],
        [P.FlinchFacingRight] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(56, 16),
            GoTo(P.ActiveFacingRight),
        ],
        [P.DivekickRightJump] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(66, 8),
            Cue(new(K.StartRightDivekick)),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.DivekickRightJump)),
            FallInto(P.DivekickRightJumpLoop),
        ],
        [P.DivekickRightJumpLoop] =
        [
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(1))),
            Show(59, 4),
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(7))),
            Show(59, 4),
            GoTo(P.DivekickRightJumpLoop),
        ],
        [P.DivekickRightDive] =
        [
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(7))),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.DivekickRightDive)),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(60, 1),
            Hold,
        ],
        [P.WalkToRightPost] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.DivekickRightWalkToPost)),
            FallInto(P.WalkToRightPostLoop),
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
            GoTo(P.WalkToRightPostLoop),
        ],
        [P.InitialFacingRight] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.Initial)),
            FallInto(P.InitialFacingRightLoop),
        ],
        [P.InitialFacingRightLoop] =
        [
            Show(72, 32),
            Show(73, 10),
            Show(74, 32),
            Show(73, 10),
            Show(72, 32),
            GoTo(P.InitialFacingRightLoop),
        ],
        [P.LandFacingRight] =
        [
            Cue(new(K.SetPalette, Palette: SnesObjPalettes.Palette(1))),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(73, 4),
            Show(72, 8),
            Show(75, 4),
            Show(70, 4),
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.ReadyToDivekick)),
            FallInto(P.LandFacingRightLoop),
        ],
        [P.LandFacingRightLoop] =
        [
            Show(49, 10),
            Show(50, 10),
            Show(51, 10),
            Show(50, 10),
            GoTo(P.LandFacingRightLoop),
        ],
        [P.KickFacingRight] =
        [
            Cue(new(K.SetFunction, Function: NinjaSpacePirateFunction.NoOperation)),
            Show(63, 4),
            Cue(new(K.PlaySound, Sound: NinjaSpacePirateSoundDefinitions.Attack)),
            Show(68, 4),
            Show(64, 32),
            Show(68, 4),
            GoTo(P.ActiveFacingRight),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NinjaSpacePirateProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => NinjaSpacePirateFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue> Show(int pose, int duration) => AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue> Cue(NinjaSpacePirateCue cue) => AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue> GoTo(NinjaSpacePirateProgram program) => AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue> FallInto(NinjaSpacePirateProgram program) => AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue> Hold => AnimationProgramStep<NinjaSpacePirateProgram, NinjaSpacePirateFrame, NinjaSpacePirateCue>.Hold;

}
