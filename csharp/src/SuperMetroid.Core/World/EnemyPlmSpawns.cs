using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.OldTourianWall;

namespace SuperMetroid.Core.World;

/// <summary>The PLM-triggered spawns over the species that own them.</summary>
/// <param name="noobTube">The n00b tube.</param>
/// <param name="eyeDoor">The eye door.</param>
/// <param name="oldTourianWall">The old Tourian escape-shaft wall.</param>
/// <param name="draygon">Draygon.</param>
/// <param name="motherBrain">Mother Brain.</param>
/// <param name="bombTorizo">Bomb Torizo.</param>
/// <param name="escapeExplosions">The escape explosions.</param>
internal sealed class EnemyPlmSpawns(
    NoobTubeBehavior noobTube,
    EyeDoorBehavior eyeDoor,
    OldTourianEscapeShaftWallBehavior oldTourianWall,
    DraygonBehavior draygon,
    MotherBrainBehavior motherBrain,
    BombTorizoBehavior bombTorizo,
    EscapeExplosionsBehavior escapeExplosions) : IEnemyPlmSpawns
{
    /// <inheritdoc/>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request, int roomWidthInBlocks) =>
        noobTube.SpawnNoobTubeProjectile(request, roomWidthInBlocks);

    /// <inheritdoc/>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request, int roomWidthInBlocks, Bank80SystemState system) =>
        eyeDoor.SpawnEyeDoorProjectile(request, roomWidthInBlocks, system);

    /// <inheritdoc/>
    public void SpawnOldTourianEscapeShaftWallExplosion() => oldTourianWall.SpawnOldTourianEscapeShaftWallExplosion();

    /// <inheritdoc/>
    public void DisableDraygonCannon(DraygonCannonControlWord word) => draygon.DisableDraygonCannon(word);

    /// <inheritdoc/>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request) => motherBrain.Projectiles.SpawnMotherBrainGlassProjectile(request);

    /// <inheritdoc/>
    public RoomEnemyProjectileSlot? SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request) =>
        bombTorizo.Attacks.SpawnBombTorizoStatueBreakingProjectile(request);

    /// <inheritdoc/>
    public void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, ushort inheritedX) =>
        escapeExplosions.SpawnEscapeExplosion(position, random, inheritedX);
}
