using P = SuperMetroid.Core.Enemies.Puyo.PuyoProgram;
using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="PuyoFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PuyoAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PuyoProgram, PuyoFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<PuyoProgram, AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue>[]>
    {
        // $A2:99AD
        [P.GroundedFast] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(1, 5),
            GoTo(P.GroundedFast),
        ],
        // $A2:99C1
        [P.GroundedMedium] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(1, 8),
            GoTo(P.GroundedMedium),
        ],
        // $A2:99D5
        [P.GroundedSlow] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
            GoTo(P.GroundedSlow),
        ],
        // $A2:99E9
        [P.RightFrame0LeftFrame4] =
        [
            Show(7, 1),
            Hold,
        ],
        // $A2:99EF
        [P.RightFrame1LeftFrame3] =
        [
            Show(6, 1),
            Hold,
        ],
        // $A2:99F5
        [P.Frame2] =
        [
            Show(5, 1),
            Hold,
        ],
        // $A2:99FB
        [P.RightFrame3LeftFrame1] =
        [
            Show(4, 1),
            Hold,
        ],
        // $A2:9A01
        [P.RightFrame4LeftFrame0] =
        [
            Show(3, 1),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PuyoProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => PuyoFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue> GoTo(PuyoProgram program) => AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue> Hold => AnimationProgramStep<PuyoProgram, PuyoFrame, NoAnimationCue>.Hold;

}
