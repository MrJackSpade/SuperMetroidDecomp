namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The random gate of Mother Brain's low-X forward walk.</summary>
public static partial class MotherBrainForwardWalkDefinitions
{
    /// <summary>
    /// The low-X forward walk: one sample in 64 starts a forward step (#1619).
    /// </summary>
    public static readonly RandomChance ForwardWalk = RandomChance.Highest(64, RandomSampleField.OneOf(4096));
}
