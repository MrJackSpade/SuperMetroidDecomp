namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid's route from the ceiling to Samus after it releases Mother Brain (#1952).</summary>
internal static class BabyMetroidRouteDefinitions
{
    /// <summary>What every route leg's gradual acceleration divides the remaining distance by: sixteen, its gentlest pull.</summary>
    internal const int AccelerationDivisor = 16;

    /// <summary>
    /// The route's legs, in the order the Baby flies them: each steers to its waypoint and reverses off screen with its
    /// own wrong-way speed. The first five reverse by a sixteenth of a pixel an update and the last three, closing on
    /// Samus, by a thirty-second. Reaching the last waypoint ends the route.
    /// </summary>
    internal static IReadOnlyList<BabyMetroidRouteLeg> Legs { get; } =
    [
        new(new(160, 120), SignedEightEight.FromParts(0, 16)),
        new(new(304, 122), SignedEightEight.FromParts(0, 16)),
        new(new(192, 64), SignedEightEight.FromParts(0, 16)),
        new(new(192, 112), SignedEightEight.FromParts(0, 16)),
        new(new(224, 128), SignedEightEight.FromParts(0, 16)),
        new(new(205, 144), SignedEightEight.FromParts(0, 8)),
        new(new(204, 160), SignedEightEight.FromParts(0, 8)),
        new(new(203, 176), SignedEightEight.FromParts(0, 8)),
    ];
}
