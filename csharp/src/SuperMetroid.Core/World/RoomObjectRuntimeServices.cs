using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World;

/// <summary>
/// The runtime's Samus, room shake and enemy pool as the services resident room objects call (#633). Every member
/// reads the runtime when called, never a load-time snapshot.
/// </summary>
internal sealed class RoomObjectRuntimeServices(
    RoomScopeConstruction destination,
    SamusFrameState player,
    SharedGameState system) :
    IRoomSamusAccess, IRoomEarthquakeWriter, IRoomObjectEnemyEffects
{
    /// <summary>
    /// The destination room's enemies. The room-object population runs before the enemy step builds them; a room object that reached them
    /// during setup would be writing enemy state the enemy load replaces, so that fails here.
    /// </summary>
    private RoomEnemySystem Enemies => destination.Enemies ?? throw new InvalidOperationException(
        "A room object reached the room's enemies before the enemy step built them.");

    /// <summary>
    /// Reads the current player state each time a room object asks, including after a room transition; a room object
    /// reading her before the scenario introduces her fails there.
    /// </summary>
    public IRoomSamus Current => player.Samus.RoomView;

    /// <summary>Starts the room's earthquake for a room object.</summary>
    public void Start(EarthquakeShake shake, int frames) => Enemies.Earthquake.Start(shake, frames);

    /// <summary>Stops the room's earthquake for a room object.</summary>
    public void Stop() => Enemies.Earthquake.Stop();

    /// <summary>Spawns a Noob Tube projectile at its room object's block.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        Enemies.RoomObjectSpawns.SpawnNoobTubeProjectile(request);

    /// <summary>Spawns an eye-door sweat or smoke actor at its door's block.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        Enemies.RoomObjectSpawns.SpawnEyeDoorProjectile(request);

    /// <summary>Spawns an eye's attack at its door's block, with the system state that records the door's opening.</summary>
    public void SpawnEyeDoorAttack(EyeDoorAttackRequest request) =>
        Enemies.RoomObjectSpawns.SpawnEyeDoorAttack(request, system);

    /// <summary>Delegates the old Tourian escape shaft wall blast to its resident enemy behavior.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        Enemies.RoomObjectSpawns.SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Lets a room object disable the Draygon wall cannon it controls.</summary>
    public void DisableDraygonCannon(DraygonWallCannon cannon) => Enemies.RoomObjectSpawns.DisableDraygonCannon(cannon);

    /// <summary>Starts the room's Chozo statue hand sequence, then spawns the room objects it asks for at once.</summary>
    public void ActivateChozoStatueHand(RoomLevelData level)
    {
        ChozoStatueObjects statueObjects = (destination.RoomObjects
            ?? throw new InvalidOperationException("A Chozo hand activated before the room's room objects were built.")).ChozoStatue;
        foreach (RoomObjectSpawnRequest<ChozoStatueRoomObject> request in Enemies.RoomObjectRequests.ActivateChozoStatueHandTrigger())
            statueObjects.SpawnChozoStatueObject(level, request);
    }
}
