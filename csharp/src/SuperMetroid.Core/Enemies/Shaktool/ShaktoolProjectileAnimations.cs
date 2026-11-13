using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Enemies.Shaktool.ShaktoolProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's attack-circle animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class ShaktoolProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ShaktoolProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<ShaktoolProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Front] =
        [
            Show(EnemyProjectileFrameId.ShaktoolFrontFirst, 4),
            Show(EnemyProjectileFrameId.ShaktoolFrontSecond, 4),
        ],
        [P.FrontLoop] =
        [
            Show(EnemyProjectileFrameId.ShaktoolFrontLoop, 119),
        ],
        [P.Middle] =
        [
            Show(EnemyProjectileFrameId.ShaktoolMiddleFirst, 6),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.ShaktoolTrailingCircleFlight)),
            Show(EnemyProjectileFrameId.ShaktoolMiddleSecond, 4),
        ],
        [P.MiddleLoop] =
        [
            Show(EnemyProjectileFrameId.ShaktoolMiddleLoop, 119),
        ],
        [P.Back] =
        [
            Show(EnemyProjectileFrameId.ShaktoolBack, 10),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.ShaktoolTrailingCircleFlight)),
        ],
        [P.BackLoop] =
        [
            Show(EnemyProjectileFrameId.ShaktoolBackLoop, 119),
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
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
