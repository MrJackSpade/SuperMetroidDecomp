using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Choot.ChootCueKind;
using P = SuperMetroid.Core.Enemies.Choot.ChootProgram;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Choot's idle, jumping and falling animations as typed programs (#641). Each
/// frame shows a <see cref="ChootFrame"/> pose and each callback becomes a <see cref="ChootCue"/> with its typed operands.
/// </summary>
internal static class ChootAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ChootProgram, ChootFrame, ChootCue> Segments { get; } = new(
        new Dictionary<ChootProgram, AnimationBeat<ChootFrame, ChootCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Show(0, 1),
        ],
        [P.Jumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(1, 8),
            Show(2, 1),
        ],
        [P.Falling] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(1, 8),
            Show(3, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChootProgram, Animation<ChootProgram>> StartingAt { get; } = new Dictionary<ChootProgram, Animation<ChootProgram>>
    {
        [P.Idle] = new([new(P.Idle, 0, 2, 1)], AnimationEnd.Holds),
        [P.Jumping] = new([new(P.Jumping, 0, 3, 1)], AnimationEnd.Holds),
        [P.Falling] = new([new(P.Falling, 0, 3, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChootProgram> StartingWith(ChootProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ChootFrame, ChootCue> Show(int pose, int duration) => new AnimationBeat<ChootFrame, ChootCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<ChootFrame, ChootCue> Cue(ChootCue cue) => new AnimationBeat<ChootFrame, ChootCue>.Event(cue);
}
