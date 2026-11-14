using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.World;

/// <summary>The enemy-side effects room objects and room effects trigger.</summary>
internal interface IEnemyObjectSpawns
{
    /// <summary>Spawns a n00b-tube projectile.</summary>
    void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request);

    /// <summary>Spawns an eye-door projectile.</summary>
    void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request, SharedGameState system);

    /// <summary>Spawns the old Tourian escape-shaft wall's explosion.</summary>
    void SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Disables the Draygon cannon a room object identifies.</summary>
    void DisableDraygonCannon(DraygonWallCannon cannon);

    /// <summary>Spawns a projectile Mother Brain's glass animation requested.</summary>
    void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request);

    /// <summary>Spawns a fragment of Bomb Torizo's breaking statue.</summary>
    EnemyProjectile? SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request);

    /// <summary>Spawns one escape explosion, of the kind its place gives it when its draw names none.</summary>
    void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, RoomSpriteObjectKind kindLeftOpen);
}
