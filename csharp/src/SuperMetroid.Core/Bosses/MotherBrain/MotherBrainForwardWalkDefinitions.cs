namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The random gate of Mother Brain's low-X forward walk.</summary>
public static class MotherBrainForwardWalkDefinitions
{
    /// <summary>
    /// The low-X forward walk: the top 64 of the 4096 rolls of the sample's bits 0-11, 4032 and over, start a forward step
    /// (#1619).
    /// </summary>
    public static readonly RandomChance ForwardWalk = RandomChance.Highest(64, new RandomSampleField(0, 12));
}
