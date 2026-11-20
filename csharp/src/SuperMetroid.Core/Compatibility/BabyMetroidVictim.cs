namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemy the Baby Metroid feeds on in its Tourian room (#1542): whichever enemy the room places directly after
/// it, not an enemy it has found or been given. It latches onto that enemy, drains it, flashes it and hands it to its
/// corpse once drained, so the enemy placed after the Baby is the one that dies, whatever it is. In the retail room the
/// Sidehopper is placed directly after the Baby, so it is the Sidehopper that is drained.
/// </summary>
/// <remarks>
/// Consulted during the Baby Metroid's own update, each time it reaches its victim: when it latches on, while it drains
/// and shakes it, and when it hands the drained victim to its corpse; and when the victim is placed, which asks whether
/// the Baby placed before it has already fed (<see cref="FeederOf"/>). Evidence: the Baby's drain and corpse handoff
/// address the victim as the enemy record directly after its own, without testing what that record holds; the retail
/// Tourian population places the Sidehopper there. Asking past the final enemy is refused.
/// </remarks>
[CompatibilityService("the Baby Metroid feeds on whichever enemy its room places directly after it, which in the retail room is the Sidehopper")]
public static class BabyMetroidVictim
{
    /// <summary>The enemy placed directly after the Baby Metroid.</summary>
    /// <typeparam name="TPlace">An enemy place.</typeparam>
    /// <param name="places">Every enemy place, in placement order.</param>
    /// <param name="babyMetroid">The Baby Metroid's place, one of <paramref name="places"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The Baby Metroid is not in a place, or is in the final place.</exception>
    public static TPlace Of<TPlace>(IReadOnlyList<TPlace> places, TPlace babyMetroid) where TPlace : class
    {
        int victim = PlaceOf(places, babyMetroid, nameof(babyMetroid)) + 1;
        if (victim >= places.Count)
            throw new ArgumentOutOfRangeException(nameof(babyMetroid), babyMetroid, $"The Baby Metroid in the final place has no enemy placed after it.");
        return places[victim];
    }

    /// <summary>
    /// The enemy that feeds on the enemy at <paramref name="victim"/> when it is a Baby Metroid: the enemy placed directly
    /// before it, by the rule <see cref="Of"/> states from the Baby's side; null for the enemy placed first, which no enemy
    /// is placed before. Whether that enemy is a Baby is its caller's question.
    /// </summary>
    /// <typeparam name="TPlace">An enemy place.</typeparam>
    /// <param name="places">Every enemy place, in placement order.</param>
    /// <param name="victim">The victim's place, one of <paramref name="places"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The victim is not in a place.</exception>
    public static TPlace? FeederOf<TPlace>(IReadOnlyList<TPlace> places, TPlace victim) where TPlace : class
    {
        int place = PlaceOf(places, victim, nameof(victim));
        return place == 0 ? null : places[place - 1];
    }

    /// <summary>Where <paramref name="enemy"/> lies among <paramref name="places"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="enemy"/> is none of the places.</exception>
    private static int PlaceOf<TPlace>(IReadOnlyList<TPlace> places, TPlace enemy, string name) where TPlace : class
    {
        Ensure.NotNull(places);
        Ensure.NotNull(enemy);
        for (int place = 0; place < places.Count; place++)
        {
            if (ReferenceEquals(places[place], enemy))
                return place;
        }
        throw new ArgumentOutOfRangeException(name, enemy, $"The enemy is not among the {places.Count} places.");
    }
}
