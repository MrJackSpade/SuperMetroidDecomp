namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The random chance a Sidehopper takes to choose its next hop.</summary>
internal static partial class HopperDecisions
{
    /// <summary>Whether a Sidehopper prepares a big hop rather than a small one (#2479): one hop in two.</summary>
    internal static readonly RandomChance HopsBig = RandomChance.Highest(1, RandomSampleField.OneOf(2));
}
