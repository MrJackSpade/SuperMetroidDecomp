namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's route from the ceiling to Samus: its legs in the order the Baby flies them, each leg's place on the
/// route giving the leg after it (#2470).
/// </summary>
internal static partial class BabyMetroidRoute
{
    /// <summary>The leg the Baby starts the route on once it reaches the ceiling: the route's first.</summary>
    internal static ListStep<BabyMetroidRouteLeg> FirstLeg => ListStep<BabyMetroidRouteLeg>.FirstOf(BabyMetroidRouteDefinitions.Legs);
}
