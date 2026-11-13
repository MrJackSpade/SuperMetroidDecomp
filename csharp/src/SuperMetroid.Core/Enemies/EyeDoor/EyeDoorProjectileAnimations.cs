using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// The eye door projectile's flight, impact and shot programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EyeDoorProjectileCue"/> with its typed operands.
/// </summary>
internal static class EyeDoorProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EyeDoorProjectileProgram, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments { get; } = new(
        new Dictionary<EyeDoorProjectileProgram, AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.EyeDoorFrame00, 4),
            Show(EnemyProjectileFrameId.EyeDoorFrame01, 3),
            Show(EnemyProjectileFrameId.EyeDoorFrame02, 2),
            Cue(new(K.AimAtSamus)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.EyeDoorShotFlight)),
        ],
        [P.FlyingLoop] =
        [
            Show(EnemyProjectileFrameId.EyeDoorFrame03, 16),
        ],
        [P.Impact] =
        [
            Cue(new(K.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.EyeDoorFrame04, 2),
            Show(EnemyProjectileFrameId.EyeDoorFrame05, 3),
            Show(EnemyProjectileFrameId.EyeDoorFrame06, 4),
            Cue(new(K.Delete)),
        ],
        [P.Shot] =
        [
            Show(EnemyProjectileFrameId.EyeDoorFrame07, 4),
            Show(EnemyProjectileFrameId.EyeDoorFrame08, 4),
            Show(EnemyProjectileFrameId.EyeDoorFrame09, 4),
            Show(EnemyProjectileFrameId.EyeDoorFrame10, 4),
            Cue(new(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EyeDoorProjectileProgram, Animation<EyeDoorProjectileProgram>> StartingAt { get; } = new Dictionary<EyeDoorProjectileProgram, Animation<EyeDoorProjectileProgram>>
    {
        [P.Initial] = new([new(P.Initial, 0, 5, 1), new(P.FlyingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FlyingLoop] = new([new(P.FlyingLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Impact] = new([new(P.Impact, 0, 5, 1)], AnimationEnd.Holds),
        [P.Shot] = new([new(P.Shot, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EyeDoorProjectileProgram> StartingWith(EyeDoorProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue> Cue(EyeDoorProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>.Event(cue);
}
