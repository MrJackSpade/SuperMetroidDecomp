namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Per-part initialization policy for Kraid's post-growth lint attacks.</summary>
internal static class KraidLintInitializationDefinitions
{
    /// <summary>
    /// The updates of the lints' first phase waits once Kraid grows, the top's, the middle's and the bottom's: each lint is
    /// produced on its wait's last update (<see cref="KraidLintPhase.Produce"/>).
    /// Each lint's delay is its own, top latest and bottom soonest.
    /// </summary>
    internal static readonly IdentityTable<KraidLintPart, int> InitialDelays = new(new Dictionary<KraidLintPart, int>
    {
        [KraidLintPart.Top] = 288,
        [KraidLintPart.Middle] = 160,
        [KraidLintPart.Bottom] = 64,
    });
}
