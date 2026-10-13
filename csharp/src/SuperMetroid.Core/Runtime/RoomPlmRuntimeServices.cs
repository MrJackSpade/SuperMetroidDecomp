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
    /// <summary>Reads the current player state each time a PLM asks, including after a room transition.</summary>
    public SamusState? Current => player.Samus;

    /// <summary>Reads the live Tourian entrance statue completion state.</summary>
    public bool IsFinished => enemies.TourianEntranceStatueBehavior.TourianEntranceStatueFinished;

    /// <summary>Publishes a PLM-triggered quake countdown to the shared room earthquake.</summary>
    public void SetTimer(ushort timer) => enemies.Earthquake.EarthquakeTimer = NativeCountdown.FromRaw(timer);

    /// <summary>Publishes the native quake selector alongside the shared countdown.</summary>
    public void SetType(ushort type) => enemies.Earthquake.EarthquakeType = type;

    /// <summary>Spawns a Noob Tube projectile with the active room width for placement.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        enemies.NoobTubeBehavior.SpawnNoobTubeProjectile(request, room.LevelData.Size.X);

    /// <summary>Spawns an eye-door projectile using the active room width and system state.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(request, room.LevelData.Size.X, system);

    /// <summary>Delegates the old Tourian escape shaft wall blast to its resident enemy behavior.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        enemies.OldTourianEscapeShaftWallBehavior.SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Lets a PLM disable the Draygon cannon identified by its enemy variable pointer.</summary>
    public void DisableDraygonCannon(ushort variablePointer) => enemies.DraygonBehavior.DisableDraygonCannon(variablePointer);
}
