using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// The eye door projectile's flight, impact and shot animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EyeDoorProjectileCue"/> with the values it acts on.
/// </summary>
internal static class EyeDoorProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EyeDoorProjectileSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments { get; } = new(
        new Dictionary<EyeDoorProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.EyeDoorInitialFirst, 4),
            Show(EnemyProjectileFrameId.EyeDoorInitialSecond, 3),
            Show(EnemyProjectileFrameId.EyeDoorInitialThird, 2),
            Cue(new(K.AimAtSamus)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.EyeDoorShotFlight)),
        ],
        [P.FlyingLoop] =
        [
            Show(EnemyProjectileFrameId.EyeDoorFlyingLoop, 16),
        ],
        [P.Impact] =
        [
            Cue(new(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.EyeDoorImpactFirst, 2),
            Show(EnemyProjectileFrameId.EyeDoorImpactSecond, 3),
            Show(EnemyProjectileFrameId.EyeDoorImpactThird, 4),
            Cue(new(K.Delete)),
        ],
        [P.Shot] =
        [
            Show(EnemyProjectileFrameId.EyeDoorShotFirst, 4),
            Show(EnemyProjectileFrameId.EyeDoorShotSecond, 4),
            Show(EnemyProjectileFrameId.EyeDoorShotThird, 4),
            Show(EnemyProjectileFrameId.EyeDoorShotFourth, 4),
            Cue(new(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EyeDoorProjectileSegment, Animation<EyeDoorProjectileSegment>> StartingAt { get; } = new Dictionary<EyeDoorProjectileSegment, Animation<EyeDoorProjectileSegment>>
    {
        [P.Initial] = new([new(P.Initial, 0, 5, 1), new(P.FlyingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FlyingLoop] = new([new(P.FlyingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Impact] = new([new(P.Impact, 0, 5, 1)], AnimationEnd.Holds),
        [P.Shot] = new([new(P.Shot, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EyeDoorProjectileSegment> StartingWith(EyeDoorProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue> Cue(EyeDoorProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>.Event(cue);
}
