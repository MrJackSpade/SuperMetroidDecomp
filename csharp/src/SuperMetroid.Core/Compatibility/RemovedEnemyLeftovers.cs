using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What an enemy removed from its place leaves there for the enemies that read it (#2130): a deleted enemy leaves its
/// state as it left it, and a killed enemy leaves it cleared. Neighbours read an enemy's place without testing whether it
/// still holds that enemy, so they go on reading what it left. A Squeept's or a Geruta's flame whose parent was deleted
/// without being killed, as the follow-up of a power bomb killing a Dragon listed before the parent deletes it, keeps
/// following the parent's last rise or animation; an Evir's arms and spit, which run out the update a hit kills their
/// body after it, read the killed body as facing left, with no animation, and an Evir body reads a killed spit as not
/// growing back, while a deleted body or spit is read as it was; a Kraid fingernail launching after its sibling was killed
/// reads the sibling as stopped and as having taken no alternate spawn, until the sibling's respawn sets it up at rest
/// again; a Norfair Pipe Bug formation does not rise while one of its Gamets is killed and not yet respawned; the hit
/// that destroys a destroyable shutter still sets it off, finding the destroyed shutter at rest; and the rest of a
/// Shaktool chain runs out the update a hit kills one of its segments in (#1543, #2143), orbiting, orienting and reversing
/// against the killed segment emptied, with no orbit, angular speed, orientation or motion, and reading back what it
/// writes there that update, though the kill deletes the chain; the motion and heading the chain shares are its own. An enemy placed in the place
/// afterwards is read as itself.
/// </summary>
/// <remarks>
/// One per enemy place, for the room, created with the room's places. Told of each removal as it happens: a deletion
/// when the enemy pass empties a place whose enemy deleted itself, a kill when the death empties its place. Consulted
/// whenever an enemy reads a neighbour's species state: a flame on every update, after its parent's health test; Evir
/// parts whenever they read their body's facing or animation and an Evir body whenever it reads its spit; a fingernail
/// once per launch; a formation's leader on every update it waits, once per member; a destroyable shutter's reaction
/// after a hit's damage; and a Shaktool segment whenever it reaches another segment of its chain. Evidence: the followers'
/// routines, the Evir arms' and spit's routines, the fingernail launch, the formation leader, the destroyable shutter's
/// shot reaction and the Shaktool chain's routines read the other enemy's record, or their own after the common shot
/// damage, without testing whether an enemy still occupies it; a deletion clears only the record's identity, while a
/// kill clears the whole record, which a respawned enemy's setup writes again, and which the Shaktool chain writes to.
/// </remarks>
[CompatibilityService("an enemy removed from its place leaves its state there for its neighbours: as it left it after a deletion, cleared after a kill")]
internal sealed class RemovedEnemyLeftovers
{
    /// <summary>
    /// What the place's last removed enemy left: the deleted enemy as it left, the record a kill cleared, or nothing while
    /// no enemy has left the place.
    /// </summary>
    private EnemyPlaceFinding _left = new EnemyPlaceFinding.Nothing();

    /// <summary>
    /// The place's <paramref name="enemy"/> is deleted without being killed: its state stays as it left it. An occupant
    /// deleted before its species built its enemy leaves no state.
    /// </summary>
    /// <param name="enemy">The deleted enemy, holding its state, or null when its species had not built it.</param>
    internal void Deleted(IEnemyEntity? enemy) =>
        _left = enemy is null ? new EnemyPlaceFinding.Nothing() : new EnemyPlaceFinding.Enemy(enemy);

    /// <summary>The place's enemy is killed: the kill clears what it leaves, to a record that holds no state.</summary>
    internal void Killed() => _left = new EnemyPlaceFinding.Cleared(new ClearedEnemyRecord());

    /// <summary>
    /// What a neighbour finds in the place: <paramref name="occupant"/> while an enemy holds it, otherwise what the last
    /// removed enemy left.
    /// </summary>
    /// <param name="occupant">The enemy holding the place, or null while it holds none.</param>
    internal EnemyPlaceFinding Found(IEnemyEntity? occupant) => occupant is null ? _left : new EnemyPlaceFinding.Enemy(occupant);
}
