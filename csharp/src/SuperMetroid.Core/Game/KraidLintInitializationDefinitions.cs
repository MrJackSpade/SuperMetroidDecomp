namespace SuperMetroid.Core.Game;

/// <summary>Kraid's three lint parts, valued by their enemy slot index.</summary>
internal enum KraidLintPart
{
    /// <summary>$A7:AE2F selects native enemy offset $0080: the top lint.</summary>
    Top = 2,
    /// <summary>$A7:AE38 selects native enemy offset $00C0: the middle lint.</summary>
    Middle = 3,
    /// <summary>$A7:AE41 selects native enemy offset $0100: the bottom lint.</summary>
    Bottom = 4,
}

/// <summary>Per-part initialization policy for Kraid's post-growth lint attacks.</summary>
internal static class KraidLintInitializationDefinitions
{
    /// <summary>
    /// KraidLint_InitialFunctionTimers at $A7:A916/A918/A91A supply the top,
    /// middle and bottom delays. $A7:AE32-AE47 enables each named part separately;
    /// $A7:B923-B93E aligns it while its timer counts down to LintProduce.
    /// These cases express independent per-part policies, not a calculated sequence.
    /// </summary>
    internal static ushort InitialDelay(KraidLintPart part) => part switch
    {
        KraidLintPart.Top => 288,
        KraidLintPart.Middle => 160,
        KraidLintPart.Bottom => 64,
        _ => throw new InvalidOperationException($"Undefined KraidLintPart {part}."),
    };
}
