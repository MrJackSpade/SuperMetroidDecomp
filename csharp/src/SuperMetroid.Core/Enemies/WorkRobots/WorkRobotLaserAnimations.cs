using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.WorkRobots.WorkRobotLaserSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's laser animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class WorkRobotLaserAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WorkRobotLaserSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<WorkRobotLaserSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.WorkRobotLaserFlareGlint, 4),
            Show(EnemyProjectileFrameId.WorkRobotLaserFlareGlow, 4),
            Show(EnemyProjectileFrameId.WorkRobotLaserFlareBall, 4),
            Show(EnemyProjectileFrameId.WorkRobotLaserFlareRingCored, 4),
            Show(EnemyProjectileFrameId.WorkRobotLaserFlareRing, 4),
            Show(EnemyProjectileFrameId.WorkRobotLaserFlareRingFading, 4),
            Show(EnemyProjectileFrameId.WorkRobotLaserFlareRingTwin, 4),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WorkRobotLaserSegment, Animation<WorkRobotLaserSegment>> StartingAt { get; } = new Dictionary<WorkRobotLaserSegment, Animation<WorkRobotLaserSegment>>
    {
        [P.Initial] = new([new(P.Initial, 0, 3, 1), new(P.Initial, 3, 4, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WorkRobotLaserSegment> StartingWith(WorkRobotLaserSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
