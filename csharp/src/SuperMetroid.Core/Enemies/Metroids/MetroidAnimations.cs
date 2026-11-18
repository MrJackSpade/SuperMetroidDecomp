using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Metroids.MetroidCueKind;
using P = SuperMetroid.Core.Enemies.Metroids.MetroidSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="MetroidCue"/> naming what it does.
/// </summary>
internal static class MetroidAnimations
{
    /// <summary>How many pulses a chasing Metroid's nuclei make between its cries.</summary>
    private const int PulsesBetweenCries = 4;

    /// <summary>
    /// One pulse of the nuclei from dark to brightest and back (#2337), which a chasing Metroid plays between its cries and
    /// a draining one before each draining sound.
    /// </summary>
    private static readonly AnimationBeat<EnemyFrameId, MetroidCue>[] NucleiPulse =
    [
        Show(EnemyFrameId.MetroidNucleiDark, 16),
        Show(EnemyFrameId.MetroidNucleiGlowing, 16),
        Show(EnemyFrameId.MetroidNucleiBright, 6),
        Show(EnemyFrameId.MetroidNucleiBrightest, 10),
        Show(EnemyFrameId.MetroidNucleiGlowing, 16),
    ];

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MetroidSegment, EnemyFrameId, MetroidCue> Segments { get; } = new(
        new Dictionary<MetroidSegment, AnimationBeat<EnemyFrameId, MetroidCue>[]>
    {
        [P.ChasingSamus] = NucleiPulse,
        [P.ChasingCry] =
        [
            Cue(new(K.PlayRandomCry)),
        ],
        [P.DrainingSamus] =
        [
            .. NucleiPulse,
            Cue(new(K.PlayDrainingSound)),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2337): a chasing Metroid's nuclei
    /// pulse four times before each cry, and a draining one's pulse once before each draining sound.
    /// </summary>
    internal static IReadOnlyDictionary<MetroidSegment, Animation<MetroidSegment>> StartingAt { get; } = new Dictionary<MetroidSegment, Animation<MetroidSegment>>
    {
        [P.ChasingSamus] = Animation<MetroidSegment>.Looping([Segments.Repeated(P.ChasingSamus, PulsesBetweenCries), Segments.Whole(P.ChasingCry)]),
        [P.DrainingSamus] = Animation<MetroidSegment>.Looping([Segments.Whole(P.DrainingSamus)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MetroidSegment> StartingWith(MetroidSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MetroidCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MetroidCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MetroidCue> Cue(MetroidCue cue) => new AnimationBeat<EnemyFrameId, MetroidCue>.Event(cue);
}
