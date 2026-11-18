using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Sparks.SparkCueKind;
using P = SuperMetroid.Core.Enemies.Sparks.SparkSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The Wrecked Ship Spark's flicker, active and emitter animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="SparkCue"/> naming what it does.
/// </summary>
internal static class SparkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SparkSegment, EnemyFrameId, SparkCue> Segments { get; } = new(
        new Dictionary<SparkSegment, AnimationBeat<EnemyFrameId, SparkCue>[]>
    {
        [P.FlickerOn] =
        [
            Cue(new(K.BecomeTangible)),
            Show(EnemyFrameId.SparkLargeCross, 1),
            Show(EnemyFrameId.Empty, 2),
            Show(EnemyFrameId.SparkLargeCross, 1),
            Show(EnemyFrameId.Empty, 2),
            Show(EnemyFrameId.SparkLargeSlant, 1),
            Show(EnemyFrameId.Empty, 2),
            Show(EnemyFrameId.SparkLargeSlant, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.SparkLargeCrossScattering, 2),
            Show(EnemyFrameId.SparkLargeSlantScattering, 2),
        ],
        [P.Active] =
        [
            Show(EnemyFrameId.SparkSmallCross, 3),
            Show(EnemyFrameId.SparkSmallSlantTrailing, 3),
            Show(EnemyFrameId.SparkSmallCrossTrailing, 3),
            Show(EnemyFrameId.SparkSmallSlantTrailingFar, 3),
        ],
        [P.FlickerOut] =
        [
            Show(EnemyFrameId.SparkLargeCross, 1),
            Show(EnemyFrameId.SparkLargeCross, 1),
            Show(EnemyFrameId.SparkLargeSlant, 1),
            Show(EnemyFrameId.SparkLargeSlant, 1),
            Show(EnemyFrameId.SparkLargeCrossScattering, 1),
            Show(EnemyFrameId.Empty, 1),
            Show(EnemyFrameId.SparkLargeCrossScattering, 1),
            Show(EnemyFrameId.Empty, 1),
            Cue(new(K.BecomeIntangible)),
        ],
        [P.Emitter] =
        [
            Show(EnemyFrameId.SparkLargeCross, 3),
            Show(EnemyFrameId.SparkLargeSlant, 3),
            Show(EnemyFrameId.SparkLargeCrossScattering, 3),
            Show(EnemyFrameId.SparkLargeSlantScattering, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SparkSegment, Animation<SparkSegment>> StartingAt { get; } = new Dictionary<SparkSegment, Animation<SparkSegment>>
    {
        [P.FlickerOn] = new([Segments.Whole(P.FlickerOn), Segments.Whole(P.Active)], AnimationEnd.LoopsFrom(1)),
        [P.Active] = new([Segments.Whole(P.Active)], AnimationEnd.LoopsFrom(0)),
        [P.FlickerOut] = new([Segments.Whole(P.FlickerOut)], AnimationEnd.Holds),
        [P.Emitter] = new([Segments.Whole(P.Emitter)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SparkSegment> StartingWith(SparkSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SparkCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SparkCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SparkCue> Cue(SparkCue cue) => new AnimationBeat<EnemyFrameId, SparkCue>.Event(cue);
}
