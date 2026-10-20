using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperCueKind;
using P = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperProgram;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="NorfairLavaJumperFrame"/> pose and each callback becomes a <see cref="NorfairLavaJumperCue"/> with its typed operands.
/// </summary>
internal static class NorfairLavaJumperAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> Programs { get; } = new(
        new Dictionary<NorfairLavaJumperProgram, AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>[]>
    {
        [P.Hidden] =
        [
            Show(1, 1),
            Hold,
        ],
        [P.Jump] =
        [
            Show(1, 1),
            Show(2, 5),
            Show(3, 9),
            Show(4, 7),
            Show(5, 3),
            Show(6, 10),
            Show(7, 1),
            Cue(new(K.FinishAnimation)),
            Hold,
        ],
        [P.Follower] =
        [
            Show(8, 1),
            Show(0, 1),
            SetCounter(1),
            FallInto(P.FollowerLoop),
        ],
        [P.FollowerLoop] =
        [
            Show(10, 1),
            Show(0, 1),
            Show(9, 1),
            Show(0, 1),
            RepeatWhileCounting(P.FollowerLoop),
            GoTo(P.Follower),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NorfairLavaJumperProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => NorfairLavaJumperFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> Show(int pose, int duration) => AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> Cue(NorfairLavaJumperCue cue) => AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> SetCounter(int count) => AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> RepeatWhileCounting(NorfairLavaJumperProgram program) => AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> GoTo(NorfairLavaJumperProgram program) => AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> FallInto(NorfairLavaJumperProgram program) => AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> Hold => AnimationProgramStep<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue>.Hold;

}
