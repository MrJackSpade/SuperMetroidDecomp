using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>The save-station electricity's typed animation.</summary>
internal sealed class SaveStationElectricityAnimation(SaveStationElectricitySegment segment)
    : EnemyProjectileFrameAnimation<SaveStationElectricitySegment, EnemyProjectileCommonCue>(SaveStationElectricityAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SaveStationElectricitySegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => SaveStationElectricityAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SaveStationElectricitySegment, Animation<SaveStationElectricitySegment>> StartingAt => SaveStationElectricityAnimations.StartingAt;
}
