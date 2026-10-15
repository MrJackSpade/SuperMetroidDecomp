using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Every enemy-side effect room PLMs trigger (#633): the union of the PLM families' narrow
/// contracts; each family keeps only its own.
/// </summary>
public interface IRoomPlmEnemyEffects :
    INoobTubeProjectileSpawner, IEyeDoorProjectileSpawner, IOldTourianWallExplosion, IDraygonCannonControl;
