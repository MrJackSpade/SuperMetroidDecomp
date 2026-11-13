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
            Show(EnemyFrameId.SparkFlickerOnFirst, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 2),
            Show(EnemyFrameId.SparkFlickerOnFirst, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 2),
            Show(EnemyFrameId.SparkFlickerOnThird, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 2),
            Show(EnemyFrameId.SparkFlickerOnThird, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 1),
            Show(EnemyFrameId.SparkFlickerOnFourth, 2),
            Show(EnemyFrameId.SparkFlickerOnFifth, 2),
        ],
        [P.Active] =
        [
            Show(EnemyFrameId.SparkActiveFirst, 3),
            Show(EnemyFrameId.SparkActiveSecond, 3),
            Show(EnemyFrameId.SparkActiveThird, 3),
            Show(EnemyFrameId.SparkActiveFourth, 3),
        ],
        [P.FlickerOut] =
        [
            Show(EnemyFrameId.SparkFlickerOnFirst, 1),
            Show(EnemyFrameId.SparkFlickerOnFirst, 1),
            Show(EnemyFrameId.SparkFlickerOnThird, 1),
            Show(EnemyFrameId.SparkFlickerOnThird, 1),
            Show(EnemyFrameId.SparkFlickerOnFourth, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 1),
            Show(EnemyFrameId.SparkFlickerOnFourth, 1),
            Show(EnemyFrameId.NorfairLavaJumperFollowerSecond, 1),
            Cue(new(K.BecomeIntangible)),
        ],
        [P.Emitter] =
        [
            Show(EnemyFrameId.SparkFlickerOnFirst, 3),
            Show(EnemyFrameId.SparkFlickerOnThird, 3),
            Show(EnemyFrameId.SparkFlickerOnFourth, 3),
            Show(EnemyFrameId.SparkFlickerOnFifth, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SparkSegment, Animation<SparkSegment>> StartingAt { get; } = new Dictionary<SparkSegment, Animation<SparkSegment>>
    {
        [P.FlickerOn] = new([new(P.FlickerOn, 0, 11, 1), new(P.Active, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.Active] = new([new(P.Active, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlickerOut] = new([new(P.FlickerOut, 0, 9, 1)], AnimationEnd.Holds),
        [P.Emitter] = new([new(P.Emitter, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SparkSegment> StartingWith(SparkSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SparkCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SparkCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SparkCue> Cue(SparkCue cue) => new AnimationBeat<EnemyFrameId, SparkCue>.Event(cue);
}
