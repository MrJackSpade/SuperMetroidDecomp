namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The random gate of Mother Brain's low-X forward walk.</summary>
public static class MotherBrainForwardWalkDefinitions
{
    /// <summary>
    /// The low-X forward walk: one random number in 64, from the top of the range, starts a forward step (#1619).
    /// </summary>
    public static readonly RandomChance ForwardWalk = RandomChance.Highest(64, new RandomSampleField(0, 12));
}
