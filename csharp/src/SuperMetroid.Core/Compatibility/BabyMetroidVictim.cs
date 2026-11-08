namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemy the Baby Metroid feeds on in its Tourian room (#1542): whichever enemy the room places directly after
/// it, not an enemy it has found or been given. It latches onto that enemy, drains it, flashes it and hands it to its
/// corpse once drained, so the enemy placed after the Baby is the one that dies, whatever it is. In the retail room the
/// Sidehopper is placed directly after the Baby, so it is the Sidehopper that is drained.
/// </summary>
/// <remarks>
/// Consulted during the Baby Metroid's own update, each time it reaches its victim: when it latches on, while it drains
/// and shakes it, and when it hands the drained victim to its corpse. Evidence: the Baby's drain and corpse handoff
/// address the victim as the enemy record directly after its own, without testing what that record holds; the retail
/// Tourian population places the Sidehopper there. Asking past the final enemy is refused.
/// </remarks>
[CompatibilityService("the Baby Metroid feeds on whichever enemy its room places directly after it, which in the retail room is the Sidehopper")]
public static class BabyMetroidVictim
{
    /// <summary>The enemy placed directly after the Baby Metroid.</summary>
    /// <typeparam name="TSlot">An enemy slot.</typeparam>
    /// <param name="slots">Every enemy slot, in placement order.</param>
    /// <param name="babyMetroid">The Baby Metroid's zero-based place in <paramref name="slots"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The Baby Metroid is not in a place, or is in the final place.</exception>
    public static TSlot Of<TSlot>(IReadOnlyList<TSlot> slots, int babyMetroid)
    {
        Ensure.NotNull(slots);
        int victim = babyMetroid + 1;
        if (babyMetroid < 0 || victim >= slots.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(babyMetroid), babyMetroid,
                $"The Baby Metroid in place {babyMetroid} has no enemy placed after it among {slots.Count}.");
        }
        return slots[victim];
    }
}
