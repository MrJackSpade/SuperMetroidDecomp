using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Per-part initialization policy for Kraid's post-growth lint attacks.</summary>
internal static class KraidLintInitializationDefinitions
{
    /// <summary>The enemy slot each lint part occupies in Kraid's room.</summary>
    internal static readonly FrozenDictionary<KraidLintPart, int> Slot = new Dictionary<KraidLintPart, int>
    {
        [KraidLintPart.Top] = 2,
        [KraidLintPart.Middle] = 3,
        [KraidLintPart.Bottom] = 4,
    }.ToFrozenDictionary();

    /// <summary>
    /// KraidLint_InitialFunctionTimers at $A7:A916/A918/A91A supply the top,
    /// middle and bottom delays. $A7:AE32-AE47 enables each named part separately;
    /// $A7:B923-B93E aligns it while its timer counts down to LintProduce.
    /// These cases express independent per-part policies, not a calculated sequence.
    /// </summary>
    internal static readonly FrozenDictionary<KraidLintPart, ushort> InitialDelays = new Dictionary<KraidLintPart, ushort>
    {
        [KraidLintPart.Top] = 288,
        [KraidLintPart.Middle] = 160,
        [KraidLintPart.Bottom] = 64,
    }.ToFrozenDictionary();
}
