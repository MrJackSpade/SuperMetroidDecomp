using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.SaveStationElectricitySegment;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296). Each frame shows a <see cref="EnemyProjectileFrameId"/> and each
/// command becomes an <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class SaveStationElectricityAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SaveStationElectricitySegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<SaveStationElectricitySegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Start] =
        [
        ],
        [P.Cycle] =
        [
            Show(EnemyProjectileFrameId.SaveStationElectricityStartFirst, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityStartSecond, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityStartThird, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityStartFourth, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityStartFifth, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityStartSixth, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityStartSeventh, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityStartEighth, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>> StartingAt { get; } = new Dictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>>
    {
        [P.Start] = new([new(P.Cycle, 0, 8, 20), new(P.Cycle, 8, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SaveStationElectricitySegment> StartingWith(SaveStationElectricitySegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
