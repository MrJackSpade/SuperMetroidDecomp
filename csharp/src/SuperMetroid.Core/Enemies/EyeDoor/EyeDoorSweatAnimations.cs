using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorSweatSegment;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// The eye-door sweat drop's falling and floor-splash animations (#1296, #1600). Each frame shows a <see cref="EnemyProjectileFrameId"/> and each
/// cue is an <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class EyeDoorSweatAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EyeDoorSweatSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<EyeDoorSweatSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(EnemyProjectileFrameId.EyeDoorSweatFalling, 6),
        ],
        [P.HitFloor] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.EyeDoorSweatHitFloorFirst, 6),
            Show(EnemyProjectileFrameId.EyeDoorSweatHitFloorSecond, 6),
            Show(EnemyProjectileFrameId.EyeDoorSweatHitFloorThird, 6),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment: the fall loops, and the splash holds once it has deleted the drop.</summary>
    internal static IReadOnlyDictionary<EyeDoorSweatSegment, Animation<EyeDoorSweatSegment>> StartingAt { get; } = new Dictionary<EyeDoorSweatSegment, Animation<EyeDoorSweatSegment>>
    {
        [P.Falling] = new([new(P.Falling, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HitFloor] = new([new(P.HitFloor, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EyeDoorSweatSegment> StartingWith(EyeDoorSweatSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
