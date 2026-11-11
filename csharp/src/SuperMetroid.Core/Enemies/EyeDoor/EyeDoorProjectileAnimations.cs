using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// The eye door projectile's flight, impact and shot programs (#641). Each
/// frame shows a <see cref="EyeDoorProjectileFrame"/> pose and each command becomes a <see cref="EyeDoorProjectileCue"/> with its typed operands.
/// </summary>
internal static class EyeDoorProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EyeDoorProjectileProgram, EyeDoorProjectileFrame, EyeDoorProjectileCue> Segments { get; } = new(
        new Dictionary<EyeDoorProjectileProgram, AnimationBeat<EyeDoorProjectileFrame, EyeDoorProjectileCue>[]>
    {
        [P.Initial] =
        [
            Show(0, 4),
            Show(1, 3),
            Show(2, 2),
            Cue(new(K.AimAtSamus)),
            Cue(new(K.SetFrameBehavior, FrameBehavior: EnemyProjectileFrameBehavior.EyeDoorShotFlight)),
        ],
        [P.FlyingLoop] =
        [
            Show(3, 16),
        ],
        [P.Impact] =
        [
            Cue(new(K.ClearFrameBehavior)),
            Show(4, 2),
            Show(5, 3),
            Show(6, 4),
            Cue(new(K.Delete)),
        ],
        [P.Shot] =
        [
            Show(7, 4),
            Show(8, 4),
            Show(9, 4),
            Show(10, 4),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EyeDoorProjectileFrame, EyeDoorProjectileCue> Show(int pose, int duration) => new AnimationBeat<EyeDoorProjectileFrame, EyeDoorProjectileCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EyeDoorProjectileFrame, EyeDoorProjectileCue> Cue(EyeDoorProjectileCue cue) => new AnimationBeat<EyeDoorProjectileFrame, EyeDoorProjectileCue>.Event(cue);
}
