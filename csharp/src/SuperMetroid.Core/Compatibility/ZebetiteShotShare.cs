namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemy a shot on a Zebetite shares its health and flash with (#1542): the other half of a linked pair, and for
/// a Zebetite with no linked half the first enemy placed in its room. In Tourian that first enemy is Mother Brain's
/// body, so shooting a single Zebetite gives her body the barrier's health and hurt flash.
/// </summary>
/// <remarks>
/// Consulted by the Zebetite's shot reaction after the common shot damage, once per shot. Evidence: the reaction copies
/// health and flash to the enemy record its link value names, and the spawn records and a kill leave that value naming
/// the first record; only a linked pair's spawn writes it.
/// </remarks>
[CompatibilityService("a shot on a Zebetite with no linked half shares its health and flash with the first enemy placed in its room, which in Tourian is Mother Brain's body")]
public static class ZebetiteShotShare
{
    /// <summary>The enemy the struck Zebetite shares its health and flash with.</summary>
    /// <typeparam name="TSlot">An enemy slot.</typeparam>
    /// <param name="slots">Every enemy slot, in placement order.</param>
    /// <param name="linkedHalf">The struck Zebetite's linked half, or null when it has none.</param>
    /// <exception cref="InvalidOperationException">The room places no enemy.</exception>
    public static TSlot Recipient<TSlot>(IReadOnlyList<TSlot> slots, TSlot? linkedHalf) where TSlot : class
    {
        Ensure.NotNull(slots);
        if (linkedHalf is not null)
            return linkedHalf;
        return slots.Count != 0
            ? slots[0]
            : throw new InvalidOperationException("A Zebetite with no linked half was shot in a room that places no enemy.");
    }
}
