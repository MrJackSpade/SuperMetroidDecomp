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
    /// <summary>Gets current.</summary>
    public SamusState? Current => player.Samus;

    /// <summary>Gets is finished.</summary>
    public bool IsFinished => enemies.TourianEntranceStatueBehavior.TourianEntranceStatueFinished;

    /// <summary>Sets timer.</summary>
    public void SetTimer(ushort timer) => enemies.Earthquake.EarthquakeTimer = timer;

    /// <summary>Sets type.</summary>
    public void SetType(ushort type) => enemies.Earthquake.EarthquakeType = type;

    /// <summary>Spawn noob tube projectile.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        enemies.NoobTubeBehavior.SpawnNoobTubeProjectile(request, room.LevelData.Size.X);

    /// <summary>Spawn eye door projectile.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, room.LevelData.Size.X, system);

    /// <summary>Spawn old tourian escape shaft wall explosion.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        enemies.OldTourianEscapeShaftWallBehavior.SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Disable draygon cannon.</summary>
    public void DisableDraygonCannon(ushort variablePointer) => enemies.DraygonBehavior.DisableDraygonCannon(variablePointer);
}
