using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.OldTourianWall;

namespace SuperMetroid.Core.World;

/// <summary>The room-object-triggered spawns over the species that own them.</summary>
/// <param name="noobTube">The n00b tube.</param>
/// <param name="eyeDoor">The eye door.</param>
/// <param name="oldTourianWall">The old Tourian escape-shaft wall.</param>
/// <param name="draygon">Draygon.</param>
/// <param name="motherBrain">Mother Brain.</param>
/// <param name="bombTorizo">Bomb Torizo.</param>
/// <param name="escapeExplosions">The escape explosions.</param>
internal sealed class EnemyObjectSpawns(
    NoobTubeBehavior noobTube,
    EyeDoorBehavior eyeDoor,
    OldTourianEscapeShaftWallBehavior oldTourianWall,
    DraygonBehavior draygon,
    MotherBrainBehavior motherBrain,
    TorizoBehavior bombTorizo,
    EscapeExplosionsBehavior escapeExplosions) : IEnemyObjectSpawns
{
    /// <inheritdoc/>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        noobTube.SpawnNoobTubeProjectile(request);

    /// <inheritdoc/>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        eyeDoor.SpawnEyeDoorProjectile(request);

    /// <inheritdoc/>
    public void SpawnEyeDoorAttack(EyeDoorAttackRequest request, SharedGameState system) =>
        eyeDoor.SpawnEyeDoorAttack(request, system);

    /// <inheritdoc/>
    public void SpawnOldTourianEscapeShaftWallExplosion() => oldTourianWall.SpawnOldTourianEscapeShaftWallExplosion();

    /// <inheritdoc/>
    public void DisableDraygonCannon(DraygonWallCannon cannon) => draygon.DisableDraygonCannon(cannon);

    /// <inheritdoc/>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request) => motherBrain.Projectiles.SpawnMotherBrainGlassProjectile(request);

    /// <inheritdoc/>
    public EnemyProjectile? SpawnBombTorizoStatueBreakingProjectile(BombTorizoStatueProjectileRequest request) =>
        bombTorizo.Attacks.SpawnBombTorizoStatueBreakingProjectile(request);

    /// <inheritdoc/>
    public void SpawnEscapeExplosion(Position16<RoomPixels> position, int random, RoomEffectActorKind kindLeftOpen) =>
        escapeExplosions.SpawnEscapeExplosion(position, random, kindLeftOpen);
}
