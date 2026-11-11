using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.SaveStation.Artwork;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>The save-station electricity's typed animation.</summary>
internal sealed class SaveStationElectricityAnimation(SaveStationElectricityProgram program)
    : EnemyProjectileTypedAnimation<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>(SaveStationElectricityAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue> Segments => SaveStationElectricityAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SaveStationElectricityProgram, Animation<SaveStationElectricityProgram>> StartingAt => SaveStationElectricityAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SaveStationElectricityFrame frame) =>
        SaveStationElectricityFrameDefinitions.ShownProjectileFrames[frame];
}
