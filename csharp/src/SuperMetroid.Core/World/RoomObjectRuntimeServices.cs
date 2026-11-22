using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Earthquakes;
namespace SuperMetroid.Core.World;

/// <summary>
/// The room's enemies as the services resident room objects call (#633): the room's shake and the enemy pool. The room's
/// enemies are built before its room objects (#2554), so every room object reaches the room's own enemies.
/// </summary>
/// <param name="enemies">The room's enemies.</param>
/// <param name="system">The game's shared state, which an eye door's opening records.</param>
[RoomLifetime]
internal sealed partial class RoomObjectRuntimeServices(RoomEnemySystem enemies, SharedGameState system) :
    IRoomEarthquakeWriter, IRoomObjectEnemyEffects, IMotherBrainGlassShardSpawner, IBombTorizoStatueFragmentSpawner
{
    /// <summary>The room's enemies.</summary>
    private readonly RoomEnemySystem _enemies = Ensure.NotNull(enemies);

    /// <summary>The game's shared state.</summary>
    private readonly SharedGameState _system = Ensure.NotNull(system);

    /// <summary>Starts the room's earthquake for a room object.</summary>
    public void Start(EarthquakeShake shake, int frames) => _enemies.Earthquake.Start(shake, frames);

    /// <summary>Stops the room's earthquake for a room object.</summary>
    public void Stop() => _enemies.Earthquake.Stop();

    /// <summary>Spawns a shard Mother Brain's glass threw.</summary>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request) =>
        _enemies.RoomObjectSpawns.SpawnMotherBrainGlassProjectile(request);

    /// <summary>Spawns a fragment the Bomb Torizo statue's hand threw.</summary>
    public void SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request) =>
        _enemies.RoomObjectSpawns.SpawnBombTorizoStatueBreakingProjectile(request);

    /// <summary>Spawns a Noob Tube projectile at its room object's block.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        _enemies.RoomObjectSpawns.SpawnNoobTubeProjectile(request);

    /// <summary>Spawns an eye-door sweat or smoke actor at its door's block.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        _enemies.RoomObjectSpawns.SpawnEyeDoorProjectile(request);

    /// <summary>Spawns an eye's attack at its door's block, with the system state that records the door's opening.</summary>
    public void SpawnEyeDoorAttack(EyeDoorAttackRequest request) =>
        _enemies.RoomObjectSpawns.SpawnEyeDoorAttack(request, _system);

    /// <summary>Delegates the old Tourian escape shaft wall blast to its resident enemy behavior.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        _enemies.RoomObjectSpawns.SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Lets a room object disable the Draygon wall cannon it controls.</summary>
    public void DisableDraygonCannon(DraygonWallCannon cannon) => _enemies.RoomObjectSpawns.DisableDraygonCannon(cannon);
}
