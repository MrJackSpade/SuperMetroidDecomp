namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid's route from the ceiling to Samus: its legs in the order the Baby flies them.</summary>
internal static class BabyMetroidRoute
{
    /// <summary>The leg the Baby starts the route on once it reaches the ceiling.</summary>
    internal static BabyMetroidRouteLeg FirstLeg => BabyMetroidRouteDefinitions.Legs[0];

    /// <summary>The leg the Baby flies after <paramref name="leg"/>, or null when <paramref name="leg"/> is the last.</summary>
    /// <exception cref="InvalidDataException"><paramref name="leg"/> is not one of the route's legs.</exception>
    internal static BabyMetroidRouteLeg? LegAfter(BabyMetroidRouteLeg leg)
    {
        IReadOnlyList<BabyMetroidRouteLeg> legs = BabyMetroidRouteDefinitions.Legs;
        for (int place = 0; place < legs.Count; place++)
        {
            if (legs[place] == leg)
                return place + 1 < legs.Count ? legs[place + 1] : null;
        }
        throw new InvalidDataException($"Baby route leg {leg} is not one of the route's legs.");
    }
}
