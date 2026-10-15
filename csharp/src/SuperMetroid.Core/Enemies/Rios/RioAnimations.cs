using K = SuperMetroid.Core.Enemies.Rios.RioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.RioProgram;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="RioFrame"/> pose and each callback becomes a <see cref="RioCue"/> with its typed operands.
/// </summary>
internal static class RioAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<RioProgram, RioFrame, RioCue> Programs { get; } = new(
        new Dictionary<RioProgram, AnimationProgramStep<RioProgram, RioFrame, RioCue>[]>
    {
        // $A2:BB4B
        [P.Idle] =
        [
            Show(0, 4),
            Show(1, 4),
            FallInto(P.PostSwoopIdle),
        ],
        // $A2:BB53
        [P.PostSwoopIdle] =
        [
            Show(0, 4),
            Show(1, 4),
            Show(0, 4),
            Show(1, 4),
            Show(0, 4),
            Show(1, 4),
            Show(2, 4),
            Show(3, 4),
            Show(2, 4),
            Show(3, 4),
            GoTo(P.Idle),
        ],
        // $A2:BB7F
        [P.SwoopingPart1] =
        [
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            Cue(new(K.AnimationFinished)),
            Hold,
        ],
        // $A2:BB97
        [P.SwoopingPart2] =
        [
            Show(7, 3),
            Show(6, 3),
            GoTo(P.SwoopingPart2),
        ],
        // $A2:BBA3
        [P.SwoopCooldown] =
        [
            Show(6, 3),
            Show(5, 3),
            Show(4, 3),
            Show(3, 3),
            Show(2, 3),
            Cue(new(K.AnimationFinished)),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params RioProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => RioFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<RioProgram, RioFrame, RioCue> Show(int pose, int duration) => AnimationProgramStep<RioProgram, RioFrame, RioCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<RioProgram, RioFrame, RioCue> Cue(RioCue cue) => AnimationProgramStep<RioProgram, RioFrame, RioCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<RioProgram, RioFrame, RioCue> GoTo(RioProgram program) => AnimationProgramStep<RioProgram, RioFrame, RioCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<RioProgram, RioFrame, RioCue> FallInto(RioProgram program) => AnimationProgramStep<RioProgram, RioFrame, RioCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<RioProgram, RioFrame, RioCue> Hold => AnimationProgramStep<RioProgram, RioFrame, RioCue>.Hold;

}
