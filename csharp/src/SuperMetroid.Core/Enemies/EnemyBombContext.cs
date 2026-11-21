using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>One exploding normal bomb overlapping one enemy in the ordinary bomb pass.</summary>
/// <param name="Occupant">The enemy whose selected shot reaction runs, which a killing hit kills.</param>
/// <param name="PlacedBomb">The exploding bomb, holding its place, already marked with its hit direction.</param>
/// <param name="OrdinaryProjectiles">Samus's beams and missiles (reactions read the first shot's aim).</param>
/// <param name="Samus">Samus.</param>
/// <param name="SelectedShot">The shot reaction that runs: the selected hitbox's, or the definition's on the radius path.</param>
/// <param name="UsesFrameHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="Camera">The camera's position as the bomb pass runs.</param>
/// <remarks>The stages around common damage receive it as an <see cref="EnemyBombHitContext"/>, which adds the sampled health.</remarks>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
[ArchitectureReview("context-bag", "One bomb's hit on one enemy (#2558): every member is a fact of that hit (the struck occupant, the bomb holding its place, the reaction it selects, the pass's camera) that the hit's reaction stages read while the hit runs.")]
internal partial record EnemyBombContext(
    RoomEnemyOccupant Occupant,
    SamusBombOccupant PlacedBomb,
    SamusProjectileSystem OrdinaryProjectiles,
    SamusState Samus,
    EnemyReaction SelectedShot,
    bool UsesFrameHitboxes,
    Position16<RoomPixels> Camera)
{
    /// <summary>The place the struck occupant was spawned in, which the reaction reads and writes.</summary>
    internal RoomEnemyPlace Enemy => Occupant.Place;

    /// <summary>The place the exploding bomb holds, with its position, fuse and animation.</summary>
    internal SamusBomb Bomb => PlacedBomb.Bomb;

    /// <summary>
    /// The species states of the occupant the bomb struck, taken as the pass finds the overlap (#1543). A killing hit
    /// removes the occupant from its place before <see cref="EnemyBombReaction.AfterCommon"/> runs; a reaction that still
    /// acts on the killed enemy reaches its states here.
    /// </summary>
    internal required EnemyOccupantStates StruckOccupant { get; init; }
}
