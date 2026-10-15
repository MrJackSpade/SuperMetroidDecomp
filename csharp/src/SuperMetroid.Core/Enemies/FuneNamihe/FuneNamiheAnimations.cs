using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheCueKind;
using P = SuperMetroid.Core.Enemies.FuneNamihe.FuneNamiheProgram;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The Fune's and Namihe's idle and spitting animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="FuneNamiheFrame"/> pose and each callback becomes a <see cref="FuneNamiheCue"/> with its typed operands.
/// </summary>
internal static class FuneNamiheAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue> Programs { get; } = new(
        new Dictionary<FuneNamiheProgram, AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue>[]>
    {
        // $A8:9399
        [P.FuneIdleLeft] =
        [
            Show(0, 1),
            Hold,
        ],
        // $A8:939F
        [P.FuneActiveLeft] =
        [
            Show(0, 16),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Cue(new(K.FuneFireLeft)),
            Cue(new(K.SpitSound)),
            Show(4, 16),
            Show(3, 8),
            Show(2, 8),
            Show(1, 8),
            Cue(new(K.FinishActivity)),
            GoTo(P.FuneIdleLeft),
        ],
        // $A8:93C9
        [P.FuneIdleRight] =
        [
            Show(5, 1),
            Hold,
        ],
        // $A8:93CF
        [P.FuneActiveRight] =
        [
            Show(5, 16),
            Show(6, 8),
            Show(7, 8),
            Show(8, 8),
            Cue(new(K.FuneFireRight)),
            Cue(new(K.SpitSound)),
            Show(9, 16),
            Show(8, 8),
            Show(7, 8),
            Show(6, 8),
            Cue(new(K.FinishActivity)),
            GoTo(P.FuneIdleRight),
        ],
        // $A8:95BD
        [P.NamiheIdleLeft] =
        [
            Show(10, 1),
            Hold,
        ],
        // $A8:95C3
        [P.NamiheActiveLeft] =
        [
            Show(10, 8),
            Show(11, 8),
            Show(12, 8),
            Show(13, 8),
            Show(14, 8),
            Cue(new(K.NamiheFireLeft)),
            Cue(new(K.SpitSound)),
            Show(15, 8),
            Show(14, 8),
            Show(13, 8),
            Show(12, 8),
            Cue(new(K.FinishActivity)),
            GoTo(P.NamiheIdleLeft),
        ],
        // $A8:95F1
        [P.NamiheIdleRight] =
        [
            Show(16, 1),
            Hold,
        ],
        // $A8:95F7
        [P.NamiheActiveRight] =
        [
            Show(16, 8),
            Show(17, 8),
            Show(18, 8),
            Show(19, 8),
            Show(20, 8),
            Cue(new(K.NamiheFireRight)),
            Cue(new(K.SpitSound)),
            Show(21, 8),
            Show(20, 8),
            Show(19, 8),
            Show(18, 8),
            Cue(new(K.FinishActivity)),
            GoTo(P.NamiheIdleRight),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params FuneNamiheProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => FuneNamiheFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue> Show(int pose, int duration) => AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue> Cue(FuneNamiheCue cue) => AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue> GoTo(FuneNamiheProgram program) => AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue> Hold => AnimationProgramStep<FuneNamiheProgram, FuneNamiheFrame, FuneNamiheCue>.Hold;

}
