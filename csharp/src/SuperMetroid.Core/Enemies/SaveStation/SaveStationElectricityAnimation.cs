using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>The save-station electricity's typed animation.</summary>
internal sealed class SaveStationElectricityAnimation(SaveStationElectricityProgram program)
    : EnemyProjectileFrameAnimation<SaveStationElectricityProgram, EnemyProjectileCommonCue>(SaveStationElectricityAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SaveStationElectricityProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => SaveStationElectricityAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SaveStationElectricityProgram, Animation<SaveStationElectricityProgram>> StartingAt => SaveStationElectricityAnimations.StartingAt;
}
