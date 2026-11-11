using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Sparks.SparkCueKind;
using P = SuperMetroid.Core.Enemies.Sparks.SparkProgram;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The Wrecked Ship Spark's flicker, active and emitter animations as typed programs (#641). Each
/// frame shows a <see cref="SparkFrame"/> pose and each callback becomes a <see cref="SparkCue"/> with its typed operands.
/// </summary>
internal static class SparkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SparkProgram, SparkFrame, SparkCue> Segments { get; } = new(
        new Dictionary<SparkProgram, AnimationBeat<SparkFrame, SparkCue>[]>
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
        ],
        [P.Active] =
        [
            Show(1, 3),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
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
        ],
        [P.Emitter] =
        [
            Show(5, 3),
            Show(6, 3),
            Show(7, 3),
            Show(8, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SparkProgram, Animation<SparkProgram>> StartingAt { get; } = new Dictionary<SparkProgram, Animation<SparkProgram>>
    {
        [P.FlickerOn] = new([new(P.FlickerOn, 0, 11, 1), new(P.Active, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.Active] = new([new(P.Active, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlickerOut] = new([new(P.FlickerOut, 0, 9, 1)], AnimationEnd.Holds),
        [P.Emitter] = new([new(P.Emitter, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SparkProgram> StartingWith(SparkProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SparkFrame, SparkCue> Show(int pose, int duration) => new AnimationBeat<SparkFrame, SparkCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<SparkFrame, SparkCue> Cue(SparkCue cue) => new AnimationBeat<SparkFrame, SparkCue>.Event(cue);
}
