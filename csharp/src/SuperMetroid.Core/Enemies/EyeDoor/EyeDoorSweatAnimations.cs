using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorSweatProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// The eye-door sweat drop's falling and floor-splash animations (#1296, #1600). Each frame shows a <see cref="EyeDoorSweatFrame"/> pose and each
/// command becomes an <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class EyeDoorSweatAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<EyeDoorSweatProgram, AnimationBeat<EyeDoorSweatFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(0, 6),
        ],
        [P.HitFloor] =
        [
            Cue(new EnemyProjectileCommonCue.Command(CK.ClearFrameBehavior)),
            Show(1, 6),
            Show(2, 6),
            Show(3, 6),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment: the fall loops, and the splash holds once it has deleted the drop.</summary>
    internal static IReadOnlyDictionary<EyeDoorSweatProgram, Animation<EyeDoorSweatProgram>> StartingAt { get; } = new Dictionary<EyeDoorSweatProgram, Animation<EyeDoorSweatProgram>>
    {
        [P.Falling] = new([new(P.Falling, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HitFloor] = new([new(P.HitFloor, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EyeDoorSweatProgram> StartingWith(EyeDoorSweatProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EyeDoorSweatFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<EyeDoorSweatFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EyeDoorSweatFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EyeDoorSweatFrame, EnemyProjectileCommonCue>.Event(cue);
}
