using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Every enemy-side effect room objects trigger (#633): the union of the room-object families' narrow
/// contracts; each family keeps only its own.
/// </summary>
public interface IRoomObjectEnemyEffects :
    INoobTubeProjectileSpawner, IEyeDoorProjectileSpawner, IOldTourianWallExplosion, IDraygonCannonControl;
