namespace SuperMetroid.Core.Game;

/// <summary>Per-part initialization policy for Kraid's post-growth lint attacks.</summary>
internal static class KraidLintInitializationDefinitions
{
    /// <summary>$A7:AE2F selects native enemy offset $0080: the top lint.</summary>
    internal const int TopSlot = 2;
    /// <summary>$A7:AE38 selects native enemy offset $00C0: the middle lint.</summary>
    internal const int MiddleSlot = 3;
    /// <summary>$A7:AE41 selects native enemy offset $0100: the bottom lint.</summary>
    internal const int BottomSlot = 4;

    /// <summary>
    /// KraidLint_InitialFunctionTimers at $A7:A916/A918/A91A supply the top,
    /// middle and bottom delays. $A7:AE32-AE47 enables each named part separately;
    /// $A7:B923-B93E aligns it while its timer counts down to LintProduce.
    /// These cases express independent per-part policies, not a calculated sequence.
    /// </summary>
    internal static ushort InitialDelayForSlot(int slot) => slot switch
    {
        TopSlot => 288,
        MiddleSlot => 160,
        BottomSlot => 64,
        _ => throw new IndexOutOfRangeException(),
    };
}
