using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>One exploding normal bomb overlapping one enemy in the ordinary bomb pass.</summary>
/// <param name="Enemy">The enemy whose selected shot callback runs.</param>
/// <param name="Bomb">The exploding bomb slot, already marked with direction bit $10.</param>
/// <param name="OrdinaryProjectiles">Samus's beam/missile slots (callbacks read slot zero's aim).</param>
/// <param name="Samus">Samus, when the pass has one.</param>
/// <param name="SelectedShotAi">The named callback of the selected hitbox (the header's on the radius path), if named.</param>
/// <param name="UsesExtendedHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HealthBefore">Enemy health before common bomb damage (set for the common stages).</param>
internal readonly record struct EnemyBombContext(
    RoomEnemySlot Enemy,
    SamusBombProjectileSlot Bomb,
    SamusProjectileSystem OrdinaryProjectiles,
    SamusState? Samus,
    EnemyInteractionCallback? SelectedShotAi,
    bool UsesExtendedHitboxes,
    int HealthBefore = 0);
