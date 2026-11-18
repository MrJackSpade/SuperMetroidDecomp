namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The random field and chance the ninja and wall Space Pirates read to choose their moves.</summary>
internal static class SpacePirateDecisions
{
    /// <summary>
    /// The random field a ninja Space Pirate draws until it is nonzero before its divekick. Each of
    /// the three accepted values selects the same divekick.
    /// </summary>
    internal static readonly RandomSampleField DivekickChoice = new(0, 2);

    /// <summary>Whether a wall Space Pirate climbs up rather than down (#2479): taken by an odd sample, one climb in two.</summary>
    internal static readonly RandomChance ClimbsUp = RandomChance.Highest(1, new RandomSampleField(0, 1));
}
