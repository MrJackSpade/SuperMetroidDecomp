namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemies a composite enemy's follow-up to a hit reaches (#1500): the enemies placed directly after the part
/// that was hit, whichever part that is. A hit on a Dragon passes its hurt and freeze timers to the next enemy, or
/// deletes it when the hit kills; a hit on an Evir freezes or deletes the next two, and so does a hit on any Magdollite
/// part, skipping a recipient that would lie past the room's final place. Touches and shots reach only the
/// body, whose followers are its own wing, arms and spit, but a power bomb also hits the intangible trailing parts, so
/// its follow-up reaches the enemies placed after them: the next Dragon's body, a Geruta, a Sova, a Hibashi or a
/// Squeept after a Dragon wing, the next Evir's body after an Evir's arms, and the enemies listed after a Magdollite
/// after its rising body or tracking overlay, which a kill deletes: the Acid Statue room's Holtz and its follower after
/// its Magdollite, and in the Magdollite Tunnel the next Magdollite's head and body, or the two Multiviolas after the
/// last one.
/// </summary>
/// <remarks>
/// Consulted when each Dragon, Evir and Magdollite part is placed, which keeps the recipients it is given; its follow-up after the
/// common touch, shot or power-bomb reaction reaches them, once per hit (#1542). The recipients are places, so a
/// recipient that has died or been deleted is still the one reached. The arms and spit of an Evir whose body such a
/// follow-up deleted live on, following that body's last facing and animation where it was deleted (#1543).
/// Evidence: the follow-ups at <c>$A2:E7DA</c>, <c>$A8:8B16</c> and <c>$A8:B410</c> write the records one and two slots
/// after the struck enemy's slot without testing which part was struck, and the power-bomb sweep at <c>$A0:A306</c> skips
/// no intangible enemy. A Magdollite's rising body and tracking overlay are intangible parts of twenty health that a
/// power bomb's full damage kills (<c>$A0:A597</c>), so a kill's deletion reaches past the Magdollite in both retail
/// rooms that list one: the Acid Statue room lists its Holtz pair after its Magdollite, and the Magdollite Tunnel its
/// three Magdollites in a row followed by two Multiviolas. In every retail Dragon and Evir population the trailing parts
/// lie within the enemy slots: the last Dragon wing occupies slot 18 and the last Evir arms slot 7, so the recipients
/// never pass the final slot, and the room's own lookup refuses a part placed where they would; the last retail
/// Magdollite part occupies slot 8. The follow-up check compares a Dragon's and an Evir's recipients, and every retail
/// Magdollite part's, with the slots one and two after the part.
/// </remarks>
[CompatibilityService("a composite enemy's follow-up to a hit reaches the enemies placed after the part that was hit, so a power bomb hitting a trailing part carries it to the next enemies")]
public static class FollowUpAfterStruckPart
{
    /// <summary>The <paramref name="reach"/> enemies placed directly after a Dragon, Evir or Magdollite part, nearest first.</summary>
    /// <typeparam name="TEnemy">An enemy place.</typeparam>
    /// <param name="listedAfterPart">The enemy the room lists a given number of places, one or more, after the part.</param>
    /// <param name="reach">How many following enemies the part's follow-up reaches, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reach"/> is less than one.</exception>
    public static IReadOnlyList<TEnemy> Recipients<TEnemy>(Func<int, TEnemy> listedAfterPart, int reach)
    {
        Ensure.NotNull(listedAfterPart);
        _ = Ensure.BetweenInclusive(1, int.MaxValue, reach);
        return [.. Enumerable.Range(1, reach).Select(listedAfterPart)];
    }
}
