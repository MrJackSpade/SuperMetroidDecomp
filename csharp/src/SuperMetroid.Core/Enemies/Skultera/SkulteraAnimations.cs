using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Skultera.SkulteraCueKind;
using P = SuperMetroid.Core.Enemies.Skultera.SkulteraProgram;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="SkulteraFrame"/> pose and each callback becomes a <see cref="SkulteraCue"/> with its typed operands.
/// </summary>
internal static class SkulteraAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SkulteraProgram, SkulteraFrame, SkulteraCue> Programs { get; } = new(
        new Dictionary<SkulteraProgram, AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue>[]>
    {
        // $A3:902A
        [P.SwimmingLeft] =
        [
            Cue(new(K.DrawBelowScenery)),
            FallInto(P.SwimmingLeftLoop),
        ],
        // $A3:902C
        [P.SwimmingLeftLoop] =
        [
            Show(0, 14),
            Show(1, 14),
            Show(2, 14),
            GoTo(P.SwimmingLeftLoop),
        ],
        // $A3:903C
        [P.TurningRight] =
        [
            Show(3, 13),
            Show(4, 10),
            Show(5, 8),
            Show(6, 6),
            Show(7, 6),
            Show(8, 8),
            Show(9, 10),
            Show(10, 13),
            Cue(new(K.FinishTurn)),
            Hold,
        ],
        // $A3:9060
        [P.SwimmingRight] =
        [
            Cue(new(K.DrawAboveScenery)),
            FallInto(P.SwimmingRightLoop),
        ],
        // $A3:9062
        [P.SwimmingRightLoop] =
        [
            Show(11, 14),
            Show(12, 14),
            Show(13, 14),
            GoTo(P.SwimmingRightLoop),
        ],
        // $A3:9072
        [P.TurningLeft] =
        [
            Show(14, 13),
            Show(15, 10),
            Show(16, 8),
            Show(17, 6),
            Show(18, 6),
            Show(19, 8),
            Show(20, 10),
            Show(21, 13),
            Cue(new(K.FinishTurn)),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SkulteraProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => SkulteraFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue> Show(int pose, int duration) => AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue> Cue(SkulteraCue cue) => AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue> GoTo(SkulteraProgram program) => AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue> FallInto(SkulteraProgram program) => AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue> Hold => AnimationProgramStep<SkulteraProgram, SkulteraFrame, SkulteraCue>.Hold;

}
