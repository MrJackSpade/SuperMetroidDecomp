using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperCueKind;
using P = SuperMetroid.Core.Enemies.LavaJumpers.NorfairLavaJumperProgram;

namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>
/// The Norfair lava jumper's hidden, jump and follower animations as typed programs (#641). Each
/// frame shows a <see cref="NorfairLavaJumperFrame"/> pose and each callback becomes a <see cref="NorfairLavaJumperCue"/> with its typed operands.
/// </summary>
internal static class NorfairLavaJumperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NorfairLavaJumperProgram, NorfairLavaJumperFrame, NorfairLavaJumperCue> Segments { get; } = new(
        new Dictionary<NorfairLavaJumperProgram, AnimationBeat<NorfairLavaJumperFrame, NorfairLavaJumperCue>[]>
    {
        [P.Hidden] =
        [
            Show(1, 1),
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
        ],
        [P.Follower] =
        [
            Show(8, 1),
            Show(0, 1),
        ],
        [P.FollowerLoop] =
        [
            Show(10, 1),
            Show(0, 1),
            Show(9, 1),
            Show(0, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NorfairLavaJumperProgram, Animation<NorfairLavaJumperProgram>> StartingAt { get; } = new Dictionary<NorfairLavaJumperProgram, Animation<NorfairLavaJumperProgram>>
    {
        [P.Hidden] = new([new(P.Hidden, 0, 1, 1)], AnimationEnd.Holds),
        [P.Jump] = new([new(P.Jump, 0, 8, 1)], AnimationEnd.Holds),
        [P.Follower] = new([new(P.Follower, 0, 2, 1), new(P.FollowerLoop, 0, 4, 1, AnimationPhaseCounting.EveryPlay)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NorfairLavaJumperProgram> StartingWith(NorfairLavaJumperProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NorfairLavaJumperFrame, NorfairLavaJumperCue> Show(int pose, int duration) => new AnimationBeat<NorfairLavaJumperFrame, NorfairLavaJumperCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<NorfairLavaJumperFrame, NorfairLavaJumperCue> Cue(NorfairLavaJumperCue cue) => new AnimationBeat<NorfairLavaJumperFrame, NorfairLavaJumperCue>.Event(cue);
}
