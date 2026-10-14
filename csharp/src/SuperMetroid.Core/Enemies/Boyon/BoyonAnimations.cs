using K = SuperMetroid.Core.Enemies.Boyon.BoyonCueKind;
using P = SuperMetroid.Core.Enemies.Boyon.BoyonProgram;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="BoyonFrame"/> pose and each callback becomes a <see cref="BoyonCue"/> with its typed operands.
/// </summary>
internal static class BoyonAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<BoyonProgram, BoyonFrame, BoyonCue> Programs { get; } = new(
        new Dictionary<BoyonProgram, AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue>[]>
    {
        // $A2:86A7
        [P.Idle] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.Return)),
            FallInto(P.IdleLoop),
        ],
        // $A2:86AB
        [P.IdleLoop] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
            GoTo(P.IdleLoop),
        ],
        // $A2:86BF
        [P.Bouncing] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.StartBounce)),
            FallInto(P.BouncingLoop),
        ],
        // $A2:86C3
        [P.BouncingLoop] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            Show(6, 5),
            Show(5, 5),
            Show(4, 5),
            GoTo(P.BouncingLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BoyonProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => BoyonFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue> Show(int pose, int duration) => AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue> Cue(BoyonCue cue) => AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue> GoTo(BoyonProgram program) => AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue> FallInto(BoyonProgram program) => AnimationProgramStep<BoyonProgram, BoyonFrame, BoyonCue>.FallInto(program);

}
