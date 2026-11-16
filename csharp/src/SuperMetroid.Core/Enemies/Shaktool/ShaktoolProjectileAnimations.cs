using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's attack-circle animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="ShaktoolCircleCue"/>.
/// </summary>
internal static class ShaktoolProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ShaktoolProjectileSegment, EnemyProjectileFrameId, ShaktoolCircleCue> Segments { get; } = new(
        new Dictionary<ShaktoolProjectileSegment, AnimationBeat<EnemyProjectileFrameId, ShaktoolCircleCue>[]>
    {
        [P.Front] =
        [
            Show(EnemyProjectileFrameId.ShaktoolFrontFirst, 4),
            Show(EnemyProjectileFrameId.ShaktoolFrontSecond, 4),
        ],
        [P.FrontLoop] =
        [
            Hold(EnemyProjectileFrameId.ShaktoolFrontLoop),
        ],
        [P.Middle] =
        [
            Show(EnemyProjectileFrameId.ShaktoolMiddleFirst, 6),
            Cue(ShaktoolCircleCue.StartTrailing),
            Show(EnemyProjectileFrameId.ShaktoolMiddleSecond, 4),
        ],
        [P.MiddleLoop] =
        [
            Hold(EnemyProjectileFrameId.ShaktoolMiddleLoop),
        ],
        [P.Back] =
        [
            Show(EnemyProjectileFrameId.ShaktoolBack, 10),
            Cue(ShaktoolCircleCue.StartTrailing),
        ],
        [P.BackLoop] =
        [
            Hold(EnemyProjectileFrameId.ShaktoolBackLoop),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ShaktoolProjectileSegment, Animation<ShaktoolProjectileSegment>> StartingAt { get; } = new Dictionary<ShaktoolProjectileSegment, Animation<ShaktoolProjectileSegment>>
    {
        [P.Front] = new([new(P.Front, 0, 2, 1), new(P.FrontLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FrontLoop] = new([new(P.FrontLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Middle] = new([new(P.Middle, 0, 3, 1), new(P.MiddleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.MiddleLoop] = new([new(P.MiddleLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Back] = new([new(P.Back, 0, 2, 1), new(P.BackLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BackLoop] = new([new(P.BackLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ShaktoolProjectileSegment> StartingWith(ShaktoolProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, ShaktoolCircleCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, ShaktoolCircleCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2068): a circle rests on its final pose.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, ShaktoolCircleCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, ShaktoolCircleCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, ShaktoolCircleCue> Cue(ShaktoolCircleCue cue) => new AnimationBeat<EnemyProjectileFrameId, ShaktoolCircleCue>.Event(cue);
}
