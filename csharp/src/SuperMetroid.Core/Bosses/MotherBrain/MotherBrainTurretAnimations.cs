using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretCue>;
using D = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretDirection;
using S = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretSegment;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The turrets' and bullets' typed animations (#641, #1600, #2011), each beat the installed
/// <see cref="EnemyProjectileFrameId"/> it shows.
/// </summary>
internal static class MotherBrainTurretAnimations
{
    /// <summary>Every turret segment: each turret and bullet pose by direction, and the impact smoke.</summary>
    private static readonly MotherBrainTurretPose[] Poses =
    [
        .. new[] { S.TurretPose, S.BulletPose }
            .SelectMany(segment => MotherBrainTurretDefinitions.Ring.Members.Select(direction => new MotherBrainTurretPose(segment, direction))),
        MotherBrainTurretPose.BulletImpact,
    ];

    /// <summary>
    /// Every turret segment's beats: each turret and bullet pose by the direction it faces shows its frame for one update; the
    /// impact smoke takes its first colouring, stops flying, shows four eight-update puffs and a 32-update fade, then deletes
    /// itself.
    /// </summary>
    internal static AnimationSegments<MotherBrainTurretPose, EnemyProjectileFrameId, MotherBrainTurretCue> Segments { get; } = new(
        new Dictionary<MotherBrainTurretPose, Beat[]>
    {
        [new(S.TurretPose, D.Left)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseFirst)],
        [new(S.TurretPose, D.DownLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseSecond)],
        [new(S.TurretPose, D.Down)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseThird)],
        [new(S.TurretPose, D.DownRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseFourth)],
        [new(S.TurretPose, D.Right)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseFifth)],
        [new(S.TurretPose, D.UpRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseSixth)],
        [new(S.TurretPose, D.Up)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseSeventh)],
        [new(S.TurretPose, D.UpLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretPoseEighth)],
        [new(S.BulletPose, D.Left)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseFirst)],
        [new(S.BulletPose, D.DownLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseSecond)],
        [new(S.BulletPose, D.Down)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseThird)],
        [new(S.BulletPose, D.DownRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseFourth)],
        [new(S.BulletPose, D.Right)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseFifth)],
        [new(S.BulletPose, D.UpRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseSixth)],
        [new(S.BulletPose, D.Up)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseSeventh)],
        [new(S.BulletPose, D.UpLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletPoseEighth)],
        [MotherBrainTurretPose.BulletImpact] =
        [
            new Beat.Event(MotherBrainTurretCue.DrawFromSharedSheet),
            new Beat.Event(MotherBrainTurretCue.StopMoving),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletImpactFirst, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletImpactSecond, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletImpactThird, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletImpactFourth, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletImpactFifth, 32),
            new Beat.Event(MotherBrainTurretCue.Delete),
        ],
    }, Poses);

    /// <summary>Each segment's animation, played once: a pose then rests on its frame, and the deleted smoke rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainTurretPose, Animation<MotherBrainTurretPose>> StartingAt { get; } =
        Poses.ToDictionary(pose => pose, pose => new Animation<MotherBrainTurretPose>([new(pose, 0, Segments.Beats(pose).Count, 1)], AnimationEnd.Holds));

    /// <summary>Shows a still pose's <paramref name="frame"/> for one update.</summary>
    private static Beat Still(EnemyProjectileFrameId frame) => new Beat.Frame(frame, 1);
}
