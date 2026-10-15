using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.KiHunters.KiHunterCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterProgram;
using SuperMetroid.Core.Enemies.KiHunters.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="KiHunterFrame"/> pose and each callback becomes a <see cref="KiHunterCue"/> with its typed operands.
/// </summary>
internal static class KiHunterAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<KiHunterProgram, KiHunterFrame, KiHunterCue> Programs { get; } = new(
        new Dictionary<KiHunterProgram, AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue>[]>
    {
        // $A8:E9FA
        [P.FlyingLeft] =
        [
            Show(0, 2),
            Show(1, 2),
            Show(2, 1),
            Cue(new(K.ReturnToFlying)),
            FallInto(P.SwoopLeft),
        ],
        // $A8:EA08
        [P.SwoopLeft] =
        [
            Show(14, 2),
            Show(15, 6),
            Show(14, 2),
            Show(13, 2),
            Show(16, 2),
            Show(17, 32),
            GoTo(P.FlyingLeft),
        ],
        // $A8:EA24
        [P.FlyingRight] =
        [
            Show(6, 2),
            Show(7, 2),
            Show(8, 1),
            Cue(new(K.ReturnToFlying)),
            FallInto(P.SwoopRight),
        ],
        // $A8:EA32
        [P.SwoopRight] =
        [
            Show(19, 2),
            Show(20, 6),
            Show(19, 2),
            Show(18, 2),
            Show(21, 2),
            Show(22, 32),
            GoTo(P.FlyingRight),
        ],
        // $A8:EA4E
        [P.WingsLeft] =
        [
            Show(3, 2),
            Show(4, 2),
            Show(5, 1),
            GoTo(P.WingsLeft),
        ],
        // $A8:EA5E
        [P.WingsRight] =
        [
            Show(9, 2),
            Show(10, 2),
            Show(11, 1),
            GoTo(P.WingsRight),
        ],
        // $A8:EA7E
        [P.DetachedWings] =
        [
            Show(12, 1),
            Hold,
        ],
        // $A8:EA8A
        [P.JumpLeft] =
        [
            Show(23, 8),
            Show(24, 8),
            Show(25, 11),
            Show(24, 2),
            Show(23, 2),
            Cue(new(K.StartHop)),
            Show(26, 1),
            Hold,
        ],
        // $A8:EAA6
        [P.JumpRight] =
        [
            Show(27, 8),
            Show(28, 8),
            Show(29, 11),
            Show(28, 2),
            Show(27, 2),
            Cue(new(K.StartHop)),
            Show(30, 1),
            Hold,
        ],
        // $A8:EAC2
        [P.LandLeft] =
        [
            Show(23, 8),
            Show(24, 8),
            Show(25, 11),
            Show(24, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(23, 1),
            Hold,
        ],
        // $A8:EADA
        [P.LandRight] =
        [
            Show(27, 8),
            Show(28, 8),
            Show(29, 11),
            Show(28, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(27, 1),
            Hold,
        ],
        // $A8:EAF2
        [P.SpitLeft] =
        [
            Show(31, 32),
            Show(32, 6),
            Show(33, 16),
            Show(34, 2),
            Cue(new(K.SpitLeft)),
            Show(35, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(31, 1),
            Hold,
        ],
        // $A8:EB10
        [P.SpitRight] =
        [
            Show(36, 32),
            Show(37, 6),
            Show(38, 16),
            Show(39, 2),
            Cue(new(K.SpitRight)),
            Show(40, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(36, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KiHunterProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => KiHunterFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue> Show(int pose, int duration) => AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue> Cue(KiHunterCue cue) => AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue> GoTo(KiHunterProgram program) => AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue> FallInto(KiHunterProgram program) => AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue> Hold => AnimationProgramStep<KiHunterProgram, KiHunterFrame, KiHunterCue>.Hold;

}
