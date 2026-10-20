using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Sparks.SparkCueKind;
using P = SuperMetroid.Core.Enemies.Sparks.SparkProgram;
using SuperMetroid.Core.Enemies.Sparks.Artwork;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The Wrecked Ship Spark's flicker, active and emitter animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="SparkFrame"/> pose and each callback becomes a <see cref="SparkCue"/> with its typed operands.
/// </summary>
internal static class SparkAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SparkProgram, SparkFrame, SparkCue> Programs { get; } = new(
        new Dictionary<SparkProgram, AnimationProgramStep<SparkProgram, SparkFrame, SparkCue>[]>
    {
        [P.FlickerOn] =
        [
            Cue(new(K.BecomeTangible)),
            Show(5, 1),
            Show(0, 2),
            Show(5, 1),
            Show(0, 2),
            Show(6, 1),
            Show(0, 2),
            Show(6, 1),
            Show(0, 1),
            Show(7, 2),
            Show(8, 2),
            FallInto(P.Active),
        ],
        [P.Active] =
        [
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            GoTo(P.Active),
        ],
        [P.FlickerOut] =
        [
            Show(5, 1),
            Show(5, 1),
            Show(6, 1),
            Show(6, 1),
            Show(7, 1),
            Show(0, 1),
            Show(7, 1),
            Show(0, 1),
            Cue(new(K.BecomeIntangible)),
            Hold,
        ],
        [P.Emitter] =
        [
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Show(8, 3),
            GoTo(P.Emitter),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SparkProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => SparkFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SparkProgram, SparkFrame, SparkCue> Show(int pose, int duration) => AnimationProgramStep<SparkProgram, SparkFrame, SparkCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<SparkProgram, SparkFrame, SparkCue> Cue(SparkCue cue) => AnimationProgramStep<SparkProgram, SparkFrame, SparkCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SparkProgram, SparkFrame, SparkCue> GoTo(SparkProgram program) => AnimationProgramStep<SparkProgram, SparkFrame, SparkCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<SparkProgram, SparkFrame, SparkCue> FallInto(SparkProgram program) => AnimationProgramStep<SparkProgram, SparkFrame, SparkCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<SparkProgram, SparkFrame, SparkCue> Hold => AnimationProgramStep<SparkProgram, SparkFrame, SparkCue>.Hold;

}
