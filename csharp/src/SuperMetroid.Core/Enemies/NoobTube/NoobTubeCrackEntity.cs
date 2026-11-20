namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// The glass tube's crack enemy, which the glass tunnel's population places already deleted, and the enemy pass clears
/// before any update. Its initialization's only effect in the game was the crack colours every tube piece draws in, which
/// the pieces' own shading carries (#1512), so it takes no part in setup (#2571). Its placement stays because it is observable (#2375): it is the population's
/// only enemy, so without it the room would place none, draw the tube's cracks and shards from the standard artwork
/// instead of its own, keep the previous room's death quota and enemy count, and leave a pending wall-jump shake for a
/// later room.
/// </summary>
/// <param name="place">The place the crack occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NoobTubeCrackEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyEntity;
