using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>The save-station electricity's typed animation: its discharge (#2061).</summary>
internal sealed class SaveStationElectricityAnimation()
    : EnemyProjectileCommonFrameAnimation<SaveStationElectricitySegment>(SaveStationElectricityAnimations.Discharge)
{
    /// <summary>The segment the discharge starts at: its cycle.</summary>
    private protected override SaveStationElectricitySegment SegmentStartedAt(Animation<SaveStationElectricitySegment> started) =>
        Ensure.NotNull(started).Phases[0].Segment;

    /// <inheritdoc/>
    private protected override AnimationSegments<SaveStationElectricitySegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => SaveStationElectricityAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>> StartingAt => SaveStationElectricityAnimations.StartingAt;
}
