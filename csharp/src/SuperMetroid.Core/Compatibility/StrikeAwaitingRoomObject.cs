namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A shot's strike on a resident room object's block, which the object answers at its own update rather than as the shot
/// lands (#1554, #2484, #2562): the last strike waits for the object, a later strike replacing it, until the object takes it.
/// A door takes it only once it takes shots, so a shot that struck it before then still opens it, or is refused with its dud
/// sound, at that later update; the n00b tube takes it only while it waits for a Power Bomb; Draygon's wall cannon takes a
/// missile's strike once its wake is armed.
/// </summary>
/// <remarks>
/// <para>
/// Consulted at the struck object's update, in the room objects' step, which runs after the projectiles' step in which the
/// shot lands; so the object's sound and its change of course come at its update, after the update's earlier sounds.
/// Lifetime: the struck object's; each shootable room object owns one.
/// </para>
/// <para>
/// Evidence: the game's projectile collision with a block records the block's room object as struck, and the object's own
/// instruction list reads and clears that record when it runs; a door's list reads it only once the door takes shots, and a
/// later collision overwrites the record (#1554, #2484).
/// </para>
/// </remarks>
/// <typeparam name="THit">What a strike tells the object: the family of the projectile that struck it.</typeparam>
public sealed partial class StrikeAwaitingRoomObject<THit>
    where THit : struct
{
    /// <summary>The strike the object has still to take, or null for none.</summary>
    public THit? Awaiting { get; private set; }

    /// <summary>A shot struck the object's block: its strike waits for the object, replacing any earlier one.</summary>
    public void Land(THit hit) => Awaiting = hit;

    /// <summary>The object takes the strike waiting for it, if any, which then no longer waits.</summary>
    public THit? Take()
    {
        THit? hit = Awaiting;
        Awaiting = null;
        return hit;
    }
}
