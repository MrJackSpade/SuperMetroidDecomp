using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Per-part initialization policy for Kraid's post-growth lint attacks.</summary>
internal static class KraidLintInitializationDefinitions
{
    /// <summary>
    /// The lints' initial phase timers supply the top,
    /// middle and bottom delays. Each lint's initializer enables its named part separately;
    /// the lint's alignment step aligns it while its timer counts down to LintProduce.
    /// These cases express independent per-part policies, not a calculated sequence.
    /// </summary>
    internal static readonly FrozenDictionary<KraidLintPart, int> InitialDelays = new Dictionary<KraidLintPart, int>
    {
        [KraidLintPart.Top] = 288,
        [KraidLintPart.Middle] = 160,
        [KraidLintPart.Bottom] = 64,
    }.ToFrozenDictionary();
}
