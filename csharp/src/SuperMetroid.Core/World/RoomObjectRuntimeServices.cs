using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.World;

/// <summary>
/// The runtime's Samus, Tourian statue, room shake and enemy pool as the services resident room
/// room objects call (#633). Every member reads the runtime when called, never a load-time snapshot.
/// </summary>
internal sealed class RoomObjectRuntimeServices(
    RoomScopeConstruction destination,
    SamusFrameState player,
    ActiveRoomState room,
    SharedGameState system) :
    IRoomSamusAccess, ITourianStatueProgress, IRoomEarthquakeWriter, IRoomObjectEnemyEffects
{
    /// <summary>
    /// The destination room's enemies. The room-object population runs before the enemy step builds them; a room object that reached them
    /// during setup would be writing enemy state the enemy load replaces, so that fails here.
    /// </summary>
    private RoomEnemySystem Enemies => destination.Enemies ?? throw new InvalidOperationException(
        "A room object reached the room's enemies before the enemy step built them.");

    /// <summary>Reads the current player state each time a room object asks, including after a room transition.</summary>
    public IRoomSamus? Current => player.Samus?.RoomView;

    /// <summary>Reads the live Tourian entrance statue completion state.</summary>
    public bool IsFinished => Enemies.TourianStatue.Finished;

    /// <summary>Starts the room's earthquake for a room object.</summary>
    public void Start(EarthquakeShake shake, int frames) => Enemies.Earthquake.Start(shake, frames);

    /// <summary>Stops the room's earthquake for a room object.</summary>
    public void Stop() => Enemies.Earthquake.Stop();

    /// <summary>Spawns a Noob Tube projectile with the active room width for placement.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        Enemies.RoomObjectSpawns.SpawnNoobTubeProjectile(request, room.LoadedLevelData.Size.X);

    /// <summary>Spawns an eye-door projectile using the active room width and system state.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        Enemies.RoomObjectSpawns.SpawnEyeDoorProjectile(request, room.LoadedLevelData.Size.X, system);

    /// <summary>Delegates the old Tourian escape shaft wall blast to its resident enemy behavior.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        Enemies.RoomObjectSpawns.SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Lets a room object disable the Draygon wall cannon it controls.</summary>
    public void DisableDraygonCannon(DraygonWallCannon cannon) => Enemies.RoomObjectSpawns.DisableDraygonCannon(cannon);
}
