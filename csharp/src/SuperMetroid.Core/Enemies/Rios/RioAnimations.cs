using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Rios.RioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.RioSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="RioCue"/> naming what it does.
/// </summary>
internal static class RioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RioSegment, EnemyFrameId, RioCue> Segments { get; } = new(
        new Dictionary<RioSegment, AnimationBeat<EnemyFrameId, RioCue>[]>
    {
        [P.Idle] =
        [
            Show(EnemyFrameId.RioIdleFirst, 4),
            Show(EnemyFrameId.RioIdleSecond, 4),
        ],
        [P.PostSwoopIdle] =
        [
            Show(EnemyFrameId.RioIdleFirst, 4),
            Show(EnemyFrameId.RioIdleSecond, 4),
            Show(EnemyFrameId.RioIdleFirst, 4),
            Show(EnemyFrameId.RioIdleSecond, 4),
            Show(EnemyFrameId.RioIdleFirst, 4),
            Show(EnemyFrameId.RioIdleSecond, 4),
            Show(EnemyFrameId.RioPostSwoopIdleThird, 4),
            Show(EnemyFrameId.RioPostSwoopIdleFourth, 4),
            Show(EnemyFrameId.RioPostSwoopIdleThird, 4),
            Show(EnemyFrameId.RioPostSwoopIdleFourth, 4),
        ],
        [P.SwoopOpening] =
        [
            Show(EnemyFrameId.RioPostSwoopIdleThird, 3),
            Show(EnemyFrameId.RioPostSwoopIdleFourth, 3),
            Show(EnemyFrameId.RioSwoopingPartOneThird, 3),
            Show(EnemyFrameId.RioSwoopingPartOneFourth, 3),
            Show(EnemyFrameId.RioSwoopingPartOneFifth, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.SwoopLoop] =
        [
            Show(EnemyFrameId.RioSwoopingPartTwoFirst, 3),
            Show(EnemyFrameId.RioSwoopingPartOneFifth, 3),
        ],
        [P.SwoopCooldown] =
        [
            Show(EnemyFrameId.RioSwoopingPartOneFifth, 3),
            Show(EnemyFrameId.RioSwoopingPartOneFourth, 3),
            Show(EnemyFrameId.RioSwoopingPartOneThird, 3),
            Show(EnemyFrameId.RioPostSwoopIdleFourth, 3),
            Show(EnemyFrameId.RioPostSwoopIdleThird, 3),
            Cue(new(K.AnimationFinished)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RioSegment, Animation<RioSegment>> StartingAt { get; } = new Dictionary<RioSegment, Animation<RioSegment>>
    {
        [P.Idle] = new([new(P.Idle, 0, 2, 1), new(P.PostSwoopIdle, 0, 10, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PostSwoopIdle] = new([new(P.PostSwoopIdle, 0, 10, 1), new(P.Idle, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopOpening] = new([new(P.SwoopOpening, 0, 6, 1)], AnimationEnd.Holds),
        [P.SwoopLoop] = new([new(P.SwoopLoop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopCooldown] = new([new(P.SwoopCooldown, 0, 6, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RioSegment> StartingWith(RioSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, RioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, RioCue> Cue(RioCue cue) => new AnimationBeat<EnemyFrameId, RioCue>.Event(cue);
}
