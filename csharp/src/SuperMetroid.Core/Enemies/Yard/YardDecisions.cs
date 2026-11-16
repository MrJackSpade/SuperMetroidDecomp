namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The random field a Yard reads as it hides.</summary>
internal static class YardDecisions
{
    /// <summary>
    /// The random choice that keeps a Yard no longer hiding in its closed shell for another update (#2180): taken by an odd
    /// sample.
    /// </summary>
    internal static readonly RandomSampleField HoldShellClosedAgain = new(0, 1);
}
