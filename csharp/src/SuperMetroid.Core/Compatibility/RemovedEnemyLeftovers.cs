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
/// <typeparam name="TEnemy">The enemies a place holds.</typeparam>
[CompatibilityService("an enemy removed from its place leaves its state there for its neighbours: as it left it after a deletion, cleared after a kill")]
public sealed partial class RemovedEnemyLeftovers<TEnemy>
    where TEnemy : class
{
    /// <summary>The enemy the place's last deletion left there as it left, or null when its last removal was a kill or none.</summary>
    private TEnemy? _deleted;

    /// <summary>Whether the place's last removal was a kill, whose cleared record a neighbour may write to.</summary>
    private bool _leftByKill;

    /// <summary>The record the last kill cleared, made empty when a neighbour first reaches it; null until then.</summary>
    private object? _clearedRecord;

    /// <summary>The place's enemy is deleted without being killed: <paramref name="enemy"/> stays as it left.</summary>
    /// <param name="enemy">The deleted enemy, with the state it holds.</param>
    internal void Deleted(TEnemy enemy)
    {
        _deleted = Ensure.NotNull(enemy);
        _leftByKill = false;
        _clearedRecord = null;
    }

    /// <summary>The place's enemy is killed: the kill clears what it leaves, so neighbours find no enemy there.</summary>
    internal void Killed()
    {
        _deleted = null;
        _leftByKill = true;
        _clearedRecord = null;
    }

    /// <summary>
    /// The enemy a neighbour finds in the place: <paramref name="held"/> while an enemy holds it, otherwise the one the last
    /// deletion left; null once a kill cleared the place or while no enemy has left it.
    /// </summary>
    /// <param name="held">The enemy holding the place, or null while it holds none.</param>
    internal TEnemy? Found(TEnemy? held) => held ?? _deleted;

    /// <summary>
    /// The record a kill cleared, while no enemy holds the place and its last removal was a kill: a neighbour that writes to
    /// the place writes to this <typeparamref name="TRecord"/>, made empty when first reached, and reads back what it wrote
    /// until an enemy is placed in the place again; otherwise null.
    /// </summary>
    /// <param name="held">Whether an enemy holds the place.</param>
    /// <typeparam name="TRecord">The record the neighbour reads and writes, empty as a kill leaves it.</typeparam>
    internal TRecord? ClearedByKill<TRecord>(bool held) where TRecord : class, new()
    {
        if (held || !_leftByKill)
            return null;
        if (_clearedRecord is TRecord record)
            return record;
        var emptied = new TRecord();
        _clearedRecord = emptied;
        return emptied;
    }
}
