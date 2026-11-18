using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.SaveStationElectricitySegment;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296). Each frame shows a <see cref="EnemyProjectileFrameId"/> and each
/// cue is an <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class SaveStationElectricityAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SaveStationElectricitySegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<SaveStationElectricitySegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Cycle] =
        [
            Show(EnemyProjectileFrameId.SaveStationElectricityRaised32, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityRaised24, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityRaised16, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityRaised8, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityLevel, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityLowered8, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityLowered16, 1),
            Show(EnemyProjectileFrameId.SaveStationElectricityLowered24, 1),
        ],
        [P.Deletion] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
    });

    /// <summary>No animation starts at a segment: the electricity's one animation is started by name (#2061).</summary>
    internal static IReadOnlyDictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>> StartingAt { get; } = new Dictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>>();

    /// <summary>The electricity's discharge (#2061): twenty eight-pose cycles, then the deletion.</summary>
    internal static Animation<SaveStationElectricitySegment> Discharge { get; } = new([Segments.Repeated(P.Cycle, 20), Segments.Whole(P.Deletion)], AnimationEnd.Holds);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
