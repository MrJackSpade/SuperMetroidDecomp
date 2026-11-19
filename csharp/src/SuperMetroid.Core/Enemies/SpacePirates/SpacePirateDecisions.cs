namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The random chances the ninja and wall Space Pirates read to choose their moves.</summary>
internal static class SpacePirateDecisions
{
    /// <summary>
    /// Whether a ninja Space Pirate about to divekick draws again (#2510): taken by a sample divisible by four, one draw in
    /// four, so it draws until one is not. Every accepted draw starts the same divekick.
    /// </summary>
    internal static readonly RandomChance DivekickDrawnAgain = RandomChance.Lowest(1, new RandomSampleField(0, 2));

    /// <summary>Whether a wall Space Pirate climbs up rather than down (#2479): taken by an odd sample, one climb in two.</summary>
    internal static readonly RandomChance ClimbsUp = RandomChance.Highest(1, new RandomSampleField(0, 1));
}
