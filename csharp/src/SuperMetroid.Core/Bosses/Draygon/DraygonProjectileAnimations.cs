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
                Show(EnemyProjectileFrameId.DraygonGoopFirst, 10),
                Show(EnemyProjectileFrameId.DraygonGoopSecond, 10),
                Show(EnemyProjectileFrameId.DraygonGoopThird, 10),
                Show(EnemyProjectileFrameId.DraygonGoopFourth, 10),
                Show(EnemyProjectileFrameId.DraygonGoopFifth, 10),
                Show(EnemyProjectileFrameId.DraygonGoopSixth, 10),
            ],
            [P.GoopShot] =
            [
                Show(EnemyProjectileFrameId.DraygonGoopShotFirst, 8),
                Show(EnemyProjectileFrameId.DraygonGoopShotSecond, 8),
                Cue(DraygonProjectileCue.SpawnDrops),
                Cue(DraygonProjectileCue.Delete),
            ],
            [P.WallTurretBloom] =
            [
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomFirst, 5),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomSecond, 4),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomThird, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomFourth, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomFifth, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomSixth, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomSeventh, 4),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomEighth, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomNinth, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomTenth, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomEleventh, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomTwelfth, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomThirteenth, 10),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomFourteenth, 10),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomFifteenth, 10),
                Show(EnemyProjectileFrameId.DraygonWallTurretBloomSixteenth, 10),
                Cue(DraygonProjectileCue.Fire),
                Show(EnemyProjectileFrameId.DraygonWallTurretFlightFirst, 8),
                Show(EnemyProjectileFrameId.DraygonWallTurretFlightSecond, 8),
                Show(EnemyProjectileFrameId.DraygonWallTurretFlightThird, 8),
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
            [P.WallTurretBloom] = new([new(P.WallTurretBloom, 0, WallTurretBloomBeats, 1), new(P.WallTurretBloom, WallTurretBloomBeats, WallTurretFlightBeats, 1)],
                AnimationEnd.LoopsFrom(1)),
        };

    /// <summary>The wall turret shot's bloom: its sixteen frames and the cue that fires it.</summary>
    private const int WallTurretBloomBeats = 17;

    /// <summary>The wall turret shot's flight, the three frames it loops once fired.</summary>
    private const int WallTurretFlightBeats = 3;

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
