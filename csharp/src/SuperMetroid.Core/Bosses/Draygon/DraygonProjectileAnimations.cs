using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Bosses.Draygon.DraygonProjectileSegment;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's typed projectile animations (#641, #1600, #2011): the goop's and wall-turret shot's segments, each beat the
/// installed <see cref="EnemyProjectileFrameId"/> it shows, and the animation starting at each.
/// </summary>
internal static partial class DraygonProjectileAnimations
{
    /// <summary>
    /// Every goop and wall-turret segment's beats: the goop loops its frames and bursts; the wall turret's shot forms over
    /// two ramps, the second a tick faster, charges, fires, and flies in its looping flight frames.
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
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksGathering, 5),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksDrawingIn, 4),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksClosingIn, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksConverging, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksNearing, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksMeeting, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedGathering, 4),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedDrawingIn, 3),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedClosingIn, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedConverging, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedNearing, 2),
                Show(EnemyProjectileFrameId.DraygonWallTurretSparksTwistedMeeting, 2),
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
            [P.GoopTouch] = Animation<DraygonProjectileSegment>.LeadingInto([Segments.Whole(P.GoopTouch)], [Segments.Whole(P.Goop)]),
            [P.Goop] = Animation<DraygonProjectileSegment>.Looping([Segments.Whole(P.Goop)]),
            [P.GoopShot] = new([Segments.Whole(P.GoopShot)], AnimationEnd.Holds),
            [P.WallTurretBloom] = Animation<DraygonProjectileSegment>.LeadingInto([Segments.Whole(P.WallTurretBloom)], [Segments.Whole(P.WallTurretFlight)]),
        };

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue> Cue(DraygonProjectileCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>.Event(cue);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, DraygonProjectileCue>.Frame(frame, duration);
}
