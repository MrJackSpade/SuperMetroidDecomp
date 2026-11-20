using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>One exploding normal bomb overlapping one enemy in the ordinary bomb pass.</summary>
/// <param name="Occupant">The enemy whose selected shot reaction runs, which a killing hit kills.</param>
/// <param name="Bomb">The exploding bomb, already marked with its hit direction.</param>
/// <param name="OrdinaryProjectiles">Samus's beams and missiles (reactions read the first shot's aim).</param>
/// <param name="Samus">Samus.</param>
/// <param name="SelectedShot">The named reaction of the selected hitbox (the definition's on the radius path), if named.</param>
/// <param name="UsesFrameHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="Camera">The camera's position as the bomb pass runs.</param>
/// <remarks>The stages around common damage receive it as an <see cref="EnemyBombHitContext"/>, which adds the sampled health.</remarks>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal record EnemyBombContext(
    RoomEnemyOccupant Occupant,
    SamusBomb Bomb,
    SamusProjectileSystem OrdinaryProjectiles,
    SamusState Samus,
    EnemyReaction? SelectedShot,
    bool UsesFrameHitboxes,
    Position16<RoomPixels> Camera)
{
    /// <summary>The place the struck occupant was spawned in, which the reaction reads and writes.</summary>
    internal RoomEnemyPlace Enemy => Occupant.Place;

    /// <summary>
    /// The species states of the occupant the bomb struck, taken as the pass finds the overlap (#1543). A killing hit
    /// removes the occupant from its place before <see cref="EnemyBombReaction.AfterCommon"/> runs; a reaction that still
    /// acts on the killed enemy reaches its states here.
    /// </summary>
    internal required EnemyOccupantStates StruckOccupant { get; init; }
}
