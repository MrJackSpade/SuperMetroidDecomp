namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The random chances the ninja and wall Space Pirates read to choose their moves.</summary>
internal static class SpacePirateDecisions
{
    /// <summary>
    /// Whether a ninja Space Pirate about to divekick draws again (#2510): one draw in four is drawn again, so it draws
    /// until one is kept. Every kept draw starts the same divekick.
    /// </summary>
    internal static readonly RandomChance DivekickDrawnAgain = RandomChance.Lowest(1, RandomSampleField.OneOf(4));

    /// <summary>Whether a wall Space Pirate climbs up rather than down (#2479): one climb in two.</summary>
    internal static readonly RandomChance ClimbsUp = RandomChance.Highest(1, RandomSampleField.OneOf(2));
}
