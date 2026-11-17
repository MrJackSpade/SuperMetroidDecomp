using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonProjectileSegment;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's typed projectile animations (#641, #1600, #2011): the goop's and wall-turret shot's segments, each beat the
/// installed <see cref="EnemyProjectileFrameId"/> it shows, and the animation starting at each.
/// </summary>
internal static class DraygonProjectileAnimations
{
    /// <summary>
    /// Every goop and wall-turret segment's beats: the goop loops six ten-update frames and bursts in two eight-update
    /// frames; the wall turret's shot forms over two ramps, the second a tick faster, charges over four ten-update frames,
    /// fires, and flies in three eight-update frames.
    /// </summary>
    internal static AnimationSegments<DraygonProjectileSegment, EnemyProjectileFrameId, DraygonProjectileCue> Segments { get; } = new(
        new Dictionary<DraygonProjectileSegment, AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>[]>
        {
            [P.GoopTouch] = [Cue(DraygonProjectileCue.StickToSamus)],
            [P.Goop] =
            [
                Show(EnemyProjectileFrameId.DraygonGoopSpiky, 10),
                Show(EnemyProjectileFrameId.DraygonGoopPuffed, 10),
                Show(EnemyProjectileFrameId.DraygonGoopRounded, 10),
                Show(EnemyProjectileFrameId.DraygonGoopLopsided, 10),
                Show(EnemyProjectileFrameId.DraygonGoopRounded, 10),
                Show(EnemyProjectileFrameId.DraygonGoopPuffed, 10),
            ],
            [P.GoopShot] =
            [
                Show(EnemyProjectileFrameId.DraygonGoopCurled, 8),
                Show(EnemyProjectileFrameId.DraygonGoopScattered, 8),
                Cue(DraygonProjectileCue.SpawnDrops),
                Cue(DraygonProjectileCue.Delete),
            ],
            [P.WallTurretBloom] =
            [
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksApart32, 5),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksApart28, 4),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksApart24, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksApart20, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksApart16, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksApart14, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedApart32, 4),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedApart28, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedApart24, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedApart20, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedApart16, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedApart14, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretGlowFaint, 10),
                Show(EnemyProjectileFrameId.DraygonWallTurretGlowDim, 10),
                Show(EnemyProjectileFrameId.DraygonWallTurretGlowBright, 10),
                Show(EnemyProjectileFrameId.DraygonWallTurretOrbForming, 10),
                Cue(DraygonProjectileCue.Fire),
            ],
            [P.WallTurretFlight] =
            [
                Show(EnemyProjectileFrameId.DraygonWallTurretOrbBright, 8),
                Show(EnemyProjectileFrameId.DraygonWallTurretOrbFlickering, 8),
                Show(EnemyProjectileFrameId.DraygonWallTurretOrbDimmed, 8),
            ],
        });

    /// <summary>
    /// The animation that starts at each segment: the touch runs on into the goop loop, the burst holds after its deletion,
    /// and the wall turret's shot blooms and fires once, then loops its flight (#2166).
    /// </summary>
    internal static IReadOnlyDictionary<DraygonProjectileSegment, Animation<DraygonProjectileSegment>> StartingAt { get; } =
        new Dictionary<DraygonProjectileSegment, Animation<DraygonProjectileSegment>>
        {
            [P.GoopTouch] = new([Whole(P.GoopTouch), Whole(P.Goop)], AnimationEnd.LoopsFrom(1)),
            [P.Goop] = new([Whole(P.Goop)], AnimationEnd.LoopsFrom(0)),
            [P.GoopShot] = new([Whole(P.GoopShot)], AnimationEnd.Holds),
            [P.WallTurretBloom] = new([Whole(P.WallTurretBloom), Whole(P.WallTurretFlight)], AnimationEnd.LoopsFrom(1)),
        };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DraygonProjectileSegment> StartingWith(DraygonProjectileSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<DraygonProjectileSegment> Whole(DraygonProjectileSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue> Cue(DraygonProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>.Event(cue);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>.Frame(frame, duration);
}
