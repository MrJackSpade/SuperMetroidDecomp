namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The random chance a Yard takes as it hides.</summary>
internal static partial class YardDecisions
{
    /// <summary>
    /// Whether a Yard no longer hiding keeps its closed shell for another update (#2180, #2479): one update in
    /// two.
    /// </summary>
    internal static readonly RandomChance HoldsShellClosedAgain = RandomChance.Highest(1, RandomSampleField.OneOf(2));
}
