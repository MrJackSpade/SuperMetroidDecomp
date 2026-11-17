using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Rooms.Doors.EyeDoorProjectileCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorSweatSegment;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// The eye-door sweat drop's falling and floor-splash animations (#1296, #1600). Each frame shows a <see cref="EnemyProjectileFrameId"/> and each
/// cue is an <see cref="EyeDoorProjectileCue"/>.
/// </summary>
internal static class EyeDoorSweatAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EyeDoorSweatSegment, EnemyProjectileFrameId, EyeDoorProjectileCue> Segments { get; } = new(
        new Dictionary<EyeDoorSweatSegment, AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>[]>
    {
        [P.Falling] = [Hold(EnemyProjectileFrameId.EyeDoorSweatDrop)],
        [P.HitFloor] =
        [
            Cue(new(K.StopMoving)),
            Show(EnemyProjectileFrameId.EyeDoorSweatSmokePuffLow, 6),
            Show(EnemyProjectileFrameId.EyeDoorSweatSmokePuffRising, 6),
            Show(EnemyProjectileFrameId.EyeDoorSweatSmokePuffScattering, 6),
            Cue(new(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment: the falling drop holds its picture, and the splash holds once it has deleted the drop.</summary>
    internal static IReadOnlyDictionary<EyeDoorSweatSegment, Animation<EyeDoorSweatSegment>> StartingAt { get; } = new Dictionary<EyeDoorSweatSegment, Animation<EyeDoorSweatSegment>>
    {
        [P.Falling] = new([Whole(P.Falling)], AnimationEnd.Holds),
        [P.HitFloor] = new([Whole(P.HitFloor)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EyeDoorSweatSegment> StartingWith(EyeDoorSweatSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<EyeDoorSweatSegment> Whole(EyeDoorSweatSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>.HeldFrame(frame);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue> Cue(EyeDoorProjectileCue cue) => new AnimationBeat<EnemyProjectileFrameId, EyeDoorProjectileCue>.Event(cue);
}
