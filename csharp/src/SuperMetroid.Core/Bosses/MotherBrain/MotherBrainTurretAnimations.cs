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
        [new(S.TurretPose, D.Left)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingLeft)],
        [new(S.TurretPose, D.DownLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingDownLeft)],
        [new(S.TurretPose, D.Down)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingDown)],
        [new(S.TurretPose, D.DownRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingDownRight)],
        [new(S.TurretPose, D.Right)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingRight)],
        [new(S.TurretPose, D.UpRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingUpRight)],
        [new(S.TurretPose, D.Up)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingUp)],
        [new(S.TurretPose, D.UpLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingUpLeft)],
        [new(S.BulletPose, D.Left)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingLeft)],
        [new(S.BulletPose, D.DownLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownLeft)],
        [new(S.BulletPose, D.Down)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingDown)],
        [new(S.BulletPose, D.DownRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownRight)],
        [new(S.BulletPose, D.Right)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingRight)],
        [new(S.BulletPose, D.UpRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpRight)],
        [new(S.BulletPose, D.Up)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingUp)],
        [new(S.BulletPose, D.UpLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpLeft)],
        [MotherBrainTurretPose.BulletImpact] =
        [
            new Beat.Event(MotherBrainTurretCue.DrawFromSharedSheet),
            new Beat.Event(MotherBrainTurretCue.StopMoving),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloud, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudSpreading, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudBreaking, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudEmbers, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletSpeck, 32),
            new Beat.Event(MotherBrainTurretCue.Delete),
        ],
    }, Poses);

    /// <summary>Each segment's animation, played once: a pose then rests on its frame, and the deleted smoke rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainTurretPose, Animation<MotherBrainTurretPose>> StartingAt { get; } =
        Poses.ToDictionary(pose => pose, pose => new Animation<MotherBrainTurretPose>([new(pose, 0, Segments.Beats(pose).Count, 1)], AnimationEnd.Holds));

    /// <summary>Shows a still pose's <paramref name="frame"/> for one update.</summary>
    private static Beat Still(EnemyProjectileFrameId frame) => new Beat.Frame(frame, 1);
}
