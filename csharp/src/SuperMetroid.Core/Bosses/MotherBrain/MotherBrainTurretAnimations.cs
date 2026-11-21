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

    /// <summary>The frame the turret shows facing each direction.</summary>
    private static readonly IReadOnlyDictionary<D, EnemyProjectileFrameId> TurretFrames = new Dictionary<D, EnemyProjectileFrameId>
    {
        [D.Left] = EnemyProjectileFrameId.MotherBrainTurretFacingLeft,
        [D.DownLeft] = EnemyProjectileFrameId.MotherBrainTurretFacingDownLeft,
        [D.Down] = EnemyProjectileFrameId.MotherBrainTurretFacingDown,
        [D.DownRight] = EnemyProjectileFrameId.MotherBrainTurretFacingDownRight,
        [D.Right] = EnemyProjectileFrameId.MotherBrainTurretFacingRight,
        [D.UpRight] = EnemyProjectileFrameId.MotherBrainTurretFacingUpRight,
        [D.Up] = EnemyProjectileFrameId.MotherBrainTurretFacingUp,
        [D.UpLeft] = EnemyProjectileFrameId.MotherBrainTurretFacingUpLeft,
    };

    /// <summary>The frame a turret's bullet shows flying each direction.</summary>
    private static readonly IReadOnlyDictionary<D, EnemyProjectileFrameId> BulletFrames = new Dictionary<D, EnemyProjectileFrameId>
    {
        [D.Left] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingLeft,
        [D.DownLeft] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownLeft,
        [D.Down] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingDown,
        [D.DownRight] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingDownRight,
        [D.Right] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingRight,
        [D.UpRight] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpRight,
        [D.Up] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingUp,
        [D.UpLeft] = EnemyProjectileFrameId.MotherBrainTurretBulletFacingUpLeft,
    };

    /// <summary>
    /// Every turret segment's beats: each turret and bullet pose by the direction it faces holds its frame; the
    /// impact smoke stops flying and shows four eight-update puffs and a 32-update fade, room pictures drawn in
    /// their standard colours (#2260), then deletes itself.
    /// </summary>
    internal static AnimationSegments<MotherBrainTurretPose, EnemyProjectileFrameId, MotherBrainTurretCue> Segments { get; } = new(
        new Dictionary<MotherBrainTurretPose, Beat[]>(
            TurretFrames.Select(facing => KeyValuePair.Create(MotherBrainTurretPose.Turret(facing.Key), new[] { Still(facing.Value) }))
                .Concat(BulletFrames.Select(facing => KeyValuePair.Create(MotherBrainTurretPose.Bullet(facing.Key), new[] { Still(facing.Value) }))))
    {
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
        Poses.ToDictionary(pose => pose, pose => new Animation<MotherBrainTurretPose>([Segments.Whole(pose)], AnimationEnd.Holds));

    /// <summary>Shows a still pose's <paramref name="frame"/> for good (#2318).</summary>
    private static Beat Still(EnemyProjectileFrameId frame) => new Beat.HeldFrame(frame);
}
