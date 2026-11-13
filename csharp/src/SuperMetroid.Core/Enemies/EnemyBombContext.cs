using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>One exploding normal bomb overlapping one enemy in the ordinary bomb pass.</summary>
/// <param name="Enemy">The enemy whose selected shot callback runs.</param>
/// <param name="Bomb">The exploding bomb slot, already marked with its hit direction.</param>
/// <param name="OrdinaryProjectiles">Samus's beam/missile slots (callbacks read slot zero's aim).</param>
/// <param name="Samus">Samus, when the pass has one.</param>
/// <param name="SelectedShotAi">The named callback of the selected hitbox (the definition's on the radius path), if named.</param>
/// <param name="UsesFrameHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HealthBefore">Enemy health before common bomb damage (set for the common stages).</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct EnemyBombContext(
    RoomEnemySlot Enemy,
    SamusBombProjectileSlot Bomb,
    SamusProjectileSystem OrdinaryProjectiles,
    SamusState? Samus,
    EnemyReaction? SelectedShotAi,
    bool UsesFrameHitboxes,
    int HealthBefore = 0)
{
    /// <summary>
    /// The species states of the occupant the bomb struck, taken as the pass finds the overlap (#1543). A killing hit
    /// removes the occupant from its place before <see cref="EnemyBombReaction.AfterCommon"/> runs; a tail that still
    /// acts on the killed enemy reaches its states here.
    /// </summary>
    internal required EnemyOccupantStates StruckOccupant { get; init; }
}
