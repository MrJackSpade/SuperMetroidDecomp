using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretCue>;
using D = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretDirection;

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
        .. MotherBrainTurretDefinitions.Ring.Members.Select(MotherBrainTurretPose.Turret),
        .. MotherBrainTurretDefinitions.Ring.Members.Select(MotherBrainTurretPose.Bullet),
        MotherBrainTurretPose.BulletImpact,
    ];

    /// <summary>
    /// Every turret segment's beats: each turret and bullet pose by the direction it faces holds its frame; the
    /// impact smoke stops flying and shows four eight-update puffs and a 32-update fade, room pictures drawn in
    /// their standard colours (#2260), then deletes itself.
    /// </summary>
    internal static AnimationSegments<MotherBrainTurretPose, EnemyProjectileFrameId, MotherBrainTurretCue> Segments { get; } = new(
        new Dictionary<MotherBrainTurretPose, Beat[]>
    {
        [MotherBrainTurretPose.Turret(D.Left)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingLeft)],
        [MotherBrainTurretPose.Turret(D.DownLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingDownLeft)],
        [MotherBrainTurretPose.Turret(D.Down)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingDown)],
        [MotherBrainTurretPose.Turret(D.DownRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingDownRight)],
        [MotherBrainTurretPose.Turret(D.Right)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingRight)],
        [MotherBrainTurretPose.Turret(D.UpRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingUpRight)],
        [MotherBrainTurretPose.Turret(D.Up)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingUp)],
        [MotherBrainTurretPose.Turret(D.UpLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretFacingUpLeft)],
        [MotherBrainTurretPose.Bullet(D.Left)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingLeft)],
        [MotherBrainTurretPose.Bullet(D.DownLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownLeft)],
        [MotherBrainTurretPose.Bullet(D.Down)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingDown)],
        [MotherBrainTurretPose.Bullet(D.DownRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownRight)],
        [MotherBrainTurretPose.Bullet(D.Right)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingRight)],
        [MotherBrainTurretPose.Bullet(D.UpRight)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpRight)],
        [MotherBrainTurretPose.Bullet(D.Up)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingUp)],
        [MotherBrainTurretPose.Bullet(D.UpLeft)] = [Still(EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpLeft)],
        [MotherBrainTurretPose.BulletImpact] =
        [
            new Beat.Event(MotherBrainTurretCue.StopMoving),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloud, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudSpreading, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudBreaking, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletFireCloudEmbers, 8),
            new Beat.Frame(EnemyProjectileFrameId.MotherBrainTurretBulletSpeck, 32),
            new Beat.Event(MotherBrainTurretCue.Delete),
        ],
    }, Poses);

    /// <summary>Each segment's animation, played once: a pose holds its frame, and the deleted smoke rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainTurretPose, Animation<MotherBrainTurretPose>> StartingAt { get; } =
        Poses.ToDictionary(pose => pose, pose => new Animation<MotherBrainTurretPose>([new(pose, 0, Segments.Beats(pose).Count, 1)], AnimationEnd.Holds));

    /// <summary>Shows a still pose's <paramref name="frame"/> for good (#2318).</summary>
    private static Beat Still(EnemyProjectileFrameId frame) => new Beat.HeldFrame(frame);
}
