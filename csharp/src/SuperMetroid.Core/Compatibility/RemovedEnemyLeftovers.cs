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
/// again; a Norfair Pipe Bug formation does not rise while one of its Gamets is killed and not yet respawned; and the hit
/// that destroys a destroyable shutter still sets it off, finding the destroyed shutter at rest. An enemy placed in the
/// place afterwards is read as itself.
/// </summary>
/// <remarks>
/// One per enemy place, for the room, created with the room's places. Told of each removal as it happens: a deletion
/// when the enemy pass empties a place whose enemy deleted itself, a kill when the death empties its place. Consulted
/// whenever an enemy reads a neighbour's species state: a flame on every update, after its parent's health test; Evir
/// parts whenever they read their body's facing or animation and an Evir body whenever it reads its spit; a fingernail
/// once per launch; a formation's leader on every update it waits, once per member; and a destroyable shutter's reaction
/// after a hit's damage. Evidence: the followers' routines, the Evir arms' and spit's routines, the fingernail launch, the
/// formation leader and the destroyable shutter's shot reaction read the other enemy's record, or their own after the
/// common shot damage, without testing whether an enemy still occupies it; a deletion clears only the record's identity,
/// while a kill clears the whole record, which a respawned enemy's setup writes again.
/// </remarks>
/// <typeparam name="TStates">The species states one enemy owns.</typeparam>
[CompatibilityService("an enemy removed from its place leaves its state there for its neighbours: as it left it after a deletion, cleared after a kill")]
public sealed class RemovedEnemyLeftovers<TStates> where TStates : class
{
    /// <summary>The states the place's last removed enemy left; null when a kill cleared them or no enemy has left the place.</summary>
    private TStates? _left;

    /// <summary>The place's enemy is deleted without being killed: its <paramref name="states"/> stay as it left them.</summary>
    /// <param name="states">The deleted enemy's species states.</param>
    internal void Deleted(TStates states) => _left = Ensure.NotNull(states);

    /// <summary>The place's enemy is killed: the kill clears what it leaves.</summary>
    internal void Killed() => _left = null;

    /// <summary>
    /// The species states a neighbour finds in the place: <paramref name="occupant"/>'s while an enemy holds it, otherwise
    /// what the last removed enemy left, or null once a kill cleared it.
    /// </summary>
    /// <param name="occupant">The states of the enemy holding the place, or null while it holds none.</param>
    public TStates? Found(TStates? occupant) => occupant ?? _left;
}
