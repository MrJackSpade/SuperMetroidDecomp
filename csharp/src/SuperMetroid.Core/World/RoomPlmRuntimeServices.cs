using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.World;

/// <summary>
/// The runtime's Samus, Tourian statue, room shake and enemy pool as the services resident room
/// PLMs call (#633). Every member reads the runtime when called, never a load-time snapshot.
/// </summary>
internal sealed class RoomPlmRuntimeServices(
    RoomScopeConstruction destination,
    SamusFrameState player,
    ActiveRoomState room,
    Bank80SystemState system) :
    IRoomSamusAccess, ITourianStatueProgress, IRoomEarthquakeWriter, IRoomPlmEnemyEffects
{
    /// <summary>
    /// The destination room's enemies. PLM population runs before the enemy step builds them; a PLM that reached them
    /// during setup would be writing enemy state the enemy load replaces, so that fails here.
    /// </summary>
    private RoomEnemySystem Enemies => destination.Enemies ?? throw new InvalidOperationException(
        "A room PLM reached the room's enemies before the enemy step built them.");

    /// <summary>Reads the current player state each time a PLM asks, including after a room transition.</summary>
    public IRoomSamus? Current => player.Samus?.RoomView;

    /// <summary>Reads the live Tourian entrance statue completion state.</summary>
    public bool IsFinished => Enemies.TourianStatue.Finished;

    /// <summary>Publishes a PLM-triggered quake countdown to the shared room earthquake.</summary>
    public void SetTimer(ushort timer) => Enemies.Earthquake.EarthquakeTimer = NativeCountdown.FromRaw(timer);

    /// <summary>Publishes the native quake selector alongside the shared countdown.</summary>
    public void SetType(ushort type) => Enemies.Earthquake.EarthquakeType = type;

    /// <summary>Spawns a Noob Tube projectile with the active room width for placement.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        Enemies.PlmSpawns.SpawnNoobTubeProjectile(request, room.LoadedLevelData.Size.X);

    /// <summary>Spawns an eye-door projectile using the active room width and system state.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        Enemies.PlmSpawns.SpawnEyeDoorProjectile(request, room.LoadedLevelData.Size.X, system);

    /// <summary>Delegates the old Tourian escape shaft wall blast to its resident enemy behavior.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        Enemies.PlmSpawns.SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Lets a PLM disable the Draygon cannon identified by its enemy variable pointer.</summary>
    public void DisableDraygonCannon(DraygonCannonControlWord word) => Enemies.PlmSpawns.DisableDraygonCannon(word);
}
