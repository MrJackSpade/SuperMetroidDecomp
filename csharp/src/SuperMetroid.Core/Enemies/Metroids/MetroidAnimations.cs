using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Metroids.MetroidCueKind;
using P = SuperMetroid.Core.Enemies.Metroids.MetroidProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="MetroidCue"/> with its typed operands.
/// </summary>
internal static class MetroidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MetroidProgram, EnemyFrameId, MetroidCue> Segments { get; } = new(
        new Dictionary<MetroidProgram, AnimationBeat<EnemyFrameId, MetroidCue>[]>
    {
        [P.ChasingSamus] =
        [
            Show(EnemyFrameId.MetroidChasingSamusFirst, 16),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusThird, 6),
            Show(EnemyFrameId.MetroidChasingSamusFourth, 10),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusFirst, 16),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusThird, 6),
            Show(EnemyFrameId.MetroidChasingSamusFourth, 10),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusFirst, 16),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusThird, 6),
            Show(EnemyFrameId.MetroidChasingSamusFourth, 10),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusFirst, 16),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusThird, 6),
            Show(EnemyFrameId.MetroidChasingSamusFourth, 10),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Cue(new(K.PlayRandomCry)),
        ],
        [P.DrainingSamus] =
        [
            Show(EnemyFrameId.MetroidChasingSamusFirst, 16),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
            Show(EnemyFrameId.MetroidChasingSamusThird, 6),
            Show(EnemyFrameId.MetroidChasingSamusFourth, 10),
            Show(EnemyFrameId.MetroidChasingSamusSecond, 16),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MetroidCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MetroidCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MetroidCue> Cue(MetroidCue cue) => new AnimationBeat<EnemyFrameId, MetroidCue>.Event(cue);
}
