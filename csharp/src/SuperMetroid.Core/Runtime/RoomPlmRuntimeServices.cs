using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The runtime's Samus, Tourian statue, room shake and enemy pool as the services resident room
/// PLMs call (#633). Every member reads the runtime when called, never a load-time snapshot.
/// </summary>
internal sealed class RoomPlmRuntimeServices(
    RoomEnemySystem enemies,
    SamusFrameState player,
    ActiveRoomState room,
    Bank80SystemState system) :
    IRoomSamusAccess, ITourianStatueProgress, IRoomEarthquakeWriter, IRoomPlmEnemyEffects
{
    public SamusState? Current => player.Samus;

    public bool IsFinished => enemies.TourianEntranceStatueBehavior.TourianEntranceStatueFinished;

    public void SetTimer(ushort timer) => enemies.Earthquake.EarthquakeTimer = timer;

    public void SetType(ushort type) => enemies.Earthquake.EarthquakeType = type;

    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        enemies.NoobTubeBehavior.SpawnNoobTubeProjectile(request, room.LevelData.Size.X);

    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, room.LevelData.Size.X, system);

    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        enemies.OldTourianEscapeShaftWallBehavior.SpawnOldTourianEscapeShaftWallExplosion();

    public void DisableDraygonCannon(ushort variablePointer) => enemies.DraygonBehavior.DisableDraygonCannon(variablePointer);
}
