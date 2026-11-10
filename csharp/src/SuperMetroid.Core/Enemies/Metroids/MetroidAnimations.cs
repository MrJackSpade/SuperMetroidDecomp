using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Metroids.MetroidCueKind;
using P = SuperMetroid.Core.Enemies.Metroids.MetroidProgram;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops as typed programs (#641). Each
/// frame shows a <see cref="MetroidFrame"/> pose and each callback becomes a <see cref="MetroidCue"/> with its typed operands.
/// </summary>
internal static class MetroidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MetroidProgram, MetroidFrame, MetroidCue> Segments { get; } = new(
        new Dictionary<MetroidProgram, AnimationBeat<MetroidFrame, MetroidCue>[]>
    {
        [P.ChasingSamus] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Cue(new(K.PlayRandomCry)),
        ],
        [P.DrainingSamus] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Cue(new(K.PlayDrainingSound)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MetroidProgram, Animation<MetroidProgram>> StartingAt { get; } = new Dictionary<MetroidProgram, Animation<MetroidProgram>>
    {
        [P.ChasingSamus] = new([new(P.ChasingSamus, 0, 21, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DrainingSamus] = new([new(P.DrainingSamus, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MetroidProgram> StartingWith(MetroidProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MetroidFrame, MetroidCue> Show(int pose, int duration) => new AnimationBeat<MetroidFrame, MetroidCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<MetroidFrame, MetroidCue> Cue(MetroidCue cue) => new AnimationBeat<MetroidFrame, MetroidCue>.Event(cue);
}
