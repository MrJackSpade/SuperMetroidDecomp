using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's typed tongue and projectile animations (#641, #1600, #2011).</summary>
internal static class CrocomireAnimations
{
    /// <summary>The fight tongue's frames, five updates each (#1885).</summary>
    private static readonly EnemyFrameId[] FightTongueFrames =
    [
        EnemyFrameId.CrocomireTongueCurlingDown, EnemyFrameId.CrocomireTongueFlatRaised, EnemyFrameId.CrocomireTongueCurlingUp,
        EnemyFrameId.CrocomireTongueFlat,
    ];

    /// <summary>The melting tongue's frames, five updates each (#1885).</summary>
    private static readonly EnemyFrameId[] MeltingTongueFrames =
    [
        EnemyFrameId.CrocomireArmAloneBent, EnemyFrameId.CrocomireArmAloneRaised,
        EnemyFrameId.CrocomireArmAloneOutstretched, EnemyFrameId.CrocomireArmAloneLowering,
        EnemyFrameId.CrocomireArmAloneReturning,
    ];

    /// <summary>
    /// The tongue's segments, each beat naming the installed frame it shows (#1885): the fight loop's frames, none for the
    /// hidden tongue, and the melting loop's frames.
    /// </summary>
    internal static AnimationSegments<CrocomireTongueSegment, EnemyFrameId, NoAnimationCue> TongueSegments { get; } = new(
        new Dictionary<CrocomireTongueSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
        {
            [CrocomireTongueSegment.Fight] = [.. FightTongueFrames.Select(frame => TongueFrame(frame, 5))],
            [CrocomireTongueSegment.Hidden] = [],
            [CrocomireTongueSegment.Melting] = [.. MeltingTongueFrames.Select(frame => TongueFrame(frame, 5))],
        });

    /// <summary>The tongue's animations: the fight and melting frames loop, and the hidden tongue rests.</summary>
    internal static IReadOnlyDictionary<CrocomireTongueSegment, Animation<CrocomireTongueSegment>> TongueStartingAt { get; } =
        new Dictionary<CrocomireTongueSegment, Animation<CrocomireTongueSegment>>
        {
            [CrocomireTongueSegment.Fight] = new([new(CrocomireTongueSegment.Fight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
            [CrocomireTongueSegment.Hidden] = new([], AnimationEnd.Holds),
            [CrocomireTongueSegment.Melting] = new([new(CrocomireTongueSegment.Melting, 0, 5, 1)], AnimationEnd.LoopsFrom(0)),
        };

    /// <summary>The tongue animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrocomireTongueSegment> TongueStartingWith(CrocomireTongueSegment segment) => TongueStartingAt[segment];

    /// <summary>
    /// The projectile segments, one per projectile kind, each beat the installed <see cref="EnemyProjectileFrameId"/> it shows
    /// (#2011): the fight projectile's six three-update frames; the bridge fragment's and spike-wall piece's one still frame,
    /// held for good (#1627: it shows for the longest duration a frame can have, after which it still shows that frame); the
    /// shot fight projectile's five four-update explosion frames, then a drop with Crocomire's chances and deletion.
    /// </summary>
    internal static AnimationSegments<CrocomireProjectileSegment, EnemyProjectileFrameId, CrocomireProjectileCue> ProjectileSegments { get; } = new(
        new Dictionary<CrocomireProjectileSegment, AnimationBeat<EnemyProjectileFrameId, CrocomireProjectileCue>[]>
        {
            [CrocomireProjectileSegment.Mouth] =
            [
                ProjectileFrame(EnemyProjectileFrameId.CrocomireMouthBallSmall, 3),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireMouthBallMedium, 3),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireMouthBallLarge, 3),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireMouthBallLargest, 3),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireMouthBallLarge, 3),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireMouthBallMedium, 3),
            ],
            [CrocomireProjectileSegment.BridgeFragment] =
                [new AnimationBeat<EnemyProjectileFrameId, CrocomireProjectileCue>.HeldFrame(EnemyProjectileFrameId.CrocomireBridgeFragment)],
            [CrocomireProjectileSegment.SpikeWallPiece] =
                [new AnimationBeat<EnemyProjectileFrameId, CrocomireProjectileCue>.HeldFrame(EnemyProjectileFrameId.CrocomireSpikeWallPiece)],
            [CrocomireProjectileSegment.Shot] =
            [
                ProjectileFrame(EnemyProjectileFrameId.CrocomireShotSpark, 4),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireShotFlareBall, 4),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireShotFlareRingCored, 4),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireShotFlareRing, 4),
                ProjectileFrame(EnemyProjectileFrameId.CrocomireShotFlareRingFading, 4),
                new AnimationBeat<EnemyProjectileFrameId, CrocomireProjectileCue>.Event(CrocomireProjectileCue.SpawnDrops),
                new AnimationBeat<EnemyProjectileFrameId, CrocomireProjectileCue>.Event(CrocomireProjectileCue.Delete),
            ],
        });

    /// <summary>
    /// The projectile animations: the mouth projectile loops its frames, the bridge fragment and spike-wall piece rest on
    /// their held frame, and the shot holds after its deletion.
    /// </summary>
    internal static IReadOnlyDictionary<CrocomireProjectileSegment, Animation<CrocomireProjectileSegment>> ProjectileStartingAt { get; } =
        new Dictionary<CrocomireProjectileSegment, Animation<CrocomireProjectileSegment>>
        {
            [CrocomireProjectileSegment.Mouth] = new([WholeProjectile(CrocomireProjectileSegment.Mouth)], AnimationEnd.LoopsFrom(0)),
            [CrocomireProjectileSegment.BridgeFragment] = new([WholeProjectile(CrocomireProjectileSegment.BridgeFragment)], AnimationEnd.Holds),
            [CrocomireProjectileSegment.SpikeWallPiece] = new([WholeProjectile(CrocomireProjectileSegment.SpikeWallPiece)], AnimationEnd.Holds),
            [CrocomireProjectileSegment.Shot] = new([WholeProjectile(CrocomireProjectileSegment.Shot)], AnimationEnd.Holds),
        };

    /// <summary>The projectile animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CrocomireProjectileSegment> ProjectileStartingWith(CrocomireProjectileSegment segment) => ProjectileStartingAt[segment];

    /// <summary>One play of every beat of projectile segment <paramref name="segment"/>.</summary>
    private static AnimationPhase<CrocomireProjectileSegment> WholeProjectile(CrocomireProjectileSegment segment) =>
        new(segment, 0, ProjectileSegments.Beats(segment).Count, 1);

    /// <summary>Shows tongue frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> TongueFrame(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);

    /// <summary>Shows projectile frame <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, CrocomireProjectileCue> ProjectileFrame(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, CrocomireProjectileCue>.Frame(frame, duration);
}
