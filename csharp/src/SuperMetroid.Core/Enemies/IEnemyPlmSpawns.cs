using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy-side effects room PLMs and room effects trigger.</summary>
internal interface IEnemyPlmSpawns
{
    /// <summary>Spawns a n00b-tube projectile.</summary>
    void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request, int roomWidthInBlocks);

    /// <summary>Spawns an eye-door projectile.</summary>
    void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request, int roomWidthInBlocks, Bank80SystemState system);

    /// <summary>Spawns the old Tourian escape-shaft wall's explosion.</summary>
    void SpawnOldTourianEscapeShaftWallExplosion();

    /// <summary>Disables the Draygon cannon a PLM identifies.</summary>
    void DisableDraygonCannon(DraygonCannonControlWord word);

    /// <summary>Spawns a projectile Mother Brain's glass bytecode requested.</summary>
    void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request);

    /// <summary>Spawns a fragment of Bomb Torizo's breaking statue.</summary>
    RoomEnemyProjectileSlot? SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request);

    /// <summary>Spawns one escape explosion.</summary>
    void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, ushort inheritedX);
}
