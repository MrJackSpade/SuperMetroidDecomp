using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Boyon.BoyonCueKind;
using P = SuperMetroid.Core.Enemies.Boyon.BoyonProgram;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops as typed programs (#641). Each
/// frame shows a <see cref="BoyonFrame"/> pose and each callback becomes a <see cref="BoyonCue"/> with its typed operands.
/// </summary>
internal static class BoyonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BoyonProgram, BoyonFrame, BoyonCue> Segments { get; } = new(
        new Dictionary<BoyonProgram, AnimationBeat<BoyonFrame, BoyonCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.Return)),
        ],
        [P.IdleLoop] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(1, 10),
        ],
        [P.Bouncing] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.StartBounce)),
        ],
        [P.BouncingLoop] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            Show(6, 5),
            Show(5, 5),
            Show(4, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BoyonProgram, Animation<BoyonProgram>> StartingAt { get; } = new Dictionary<BoyonProgram, Animation<BoyonProgram>>
    {
        [P.Idle] = new([new(P.Idle, 0, 2, 1), new(P.IdleLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IdleLoop] = new([new(P.IdleLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Bouncing] = new([new(P.Bouncing, 0, 2, 1), new(P.BouncingLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BouncingLoop] = new([new(P.BouncingLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoyonProgram> StartingWith(BoyonProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BoyonFrame, BoyonCue> Show(int pose, int duration) => new AnimationBeat<BoyonFrame, BoyonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<BoyonFrame, BoyonCue> Cue(BoyonCue cue) => new AnimationBeat<BoyonFrame, BoyonCue>.Event(cue);
}
