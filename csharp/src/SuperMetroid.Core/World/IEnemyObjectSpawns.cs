using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.World;

/// <summary>The enemy-side effects room objects and room effects trigger.</summary>
internal interface IEnemyObjectSpawns
{
    /// <summary>Spawns a n00b-tube projectile.</summary>
    void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request);

    /// <summary>Spawns an eye-door sweat or smoke actor.</summary>
    void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request);

    /// <summary>Spawns an eye door's attack, which bursts once <paramref name="system"/> records its door opened.</summary>
    void SpawnEyeDoorAttack(EyeDoorAttackRequest request, SharedGameState system);

    /// <summary>Spawns the old Tourian escape-shaft wall's explosion.</summary>
    void SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Disables the Draygon cannon a room object identifies.</summary>
    void DisableDraygonCannon(DraygonWallCannon cannon);

    /// <summary>Spawns a projectile Mother Brain's glass animation requested.</summary>
    void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request);

    /// <summary>Spawns a fragment of Bomb Torizo's breaking statue.</summary>
    void SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request);

    /// <summary>Shows the crumbling Chozo hand's debris over Bomb Torizo's sprite characters (#2623).</summary>
    void ShowBombTorizoChozoDebris();

    /// <summary>Spawns one escape explosion, of the kind its place gives it when its draw names none.</summary>
    void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, RoomEffectActorKind kindLeftOpen);
}
