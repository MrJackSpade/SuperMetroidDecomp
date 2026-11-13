using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Sparks.SparkCueKind;
using P = SuperMetroid.Core.Enemies.Sparks.SparkProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The Wrecked Ship Spark's flicker, active and emitter animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="SparkCue"/> with its typed operands.
/// </summary>
internal static class SparkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SparkProgram, EnemyFrameId, SparkCue> Segments { get; } = new(
        new Dictionary<SparkProgram, AnimationBeat<EnemyFrameId, SparkCue>[]>
    {
        [P.FlickerOn] =
        [
            Cue(new(K.BecomeTangible)),
            Show(EnemyFrameId.Spark00, 1),
            Show(EnemyFrameId.NorfairLavaJumperPose0, 2),
            Show(EnemyFrameId.Spark00, 1),
            Show(EnemyFrameId.NorfairLavaJumperPose0, 2),
            Show(EnemyFrameId.Spark01, 1),
            Show(EnemyFrameId.NorfairLavaJumperPose0, 2),
            Show(EnemyFrameId.Spark01, 1),
            Show(EnemyFrameId.NorfairLavaJumperPose0, 1),
            Show(EnemyFrameId.Spark02, 2),
            Show(EnemyFrameId.Spark03, 2),
        ],
        [P.Active] =
        [
            Show(EnemyFrameId.Spark04, 3),
            Show(EnemyFrameId.Spark05, 3),
            Show(EnemyFrameId.Spark06, 3),
            Show(EnemyFrameId.Spark07, 3),
        ],
        [P.FlickerOut] =
        [
            Show(EnemyFrameId.Spark00, 1),
            Show(EnemyFrameId.Spark00, 1),
            Show(EnemyFrameId.Spark01, 1),
            Show(EnemyFrameId.Spark01, 1),
            Show(EnemyFrameId.Spark02, 1),
            Show(EnemyFrameId.NorfairLavaJumperPose0, 1),
            Show(EnemyFrameId.Spark02, 1),
            Show(EnemyFrameId.NorfairLavaJumperPose0, 1),
            Cue(new(K.BecomeIntangible)),
        ],
        [P.Emitter] =
        [
            Show(EnemyFrameId.Spark00, 3),
            Show(EnemyFrameId.Spark01, 3),
            Show(EnemyFrameId.Spark02, 3),
            Show(EnemyFrameId.Spark03, 3),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SparkCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SparkCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SparkCue> Cue(SparkCue cue) => new AnimationBeat<EnemyFrameId, SparkCue>.Event(cue);
}
