using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Confines the S-DSP register layout — register numbers, voice strides, field masks and raw
/// register reads/writes — to approved boundaries (#631). Raw DSP APIs are declared
/// <c>[DspRegisterAccess]</c>; only declarations inside a <c>[DspRegisterBoundary("reason")]</c> may use them.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DspRegisterAccessAnalyzer() : LayoutBoundaryAnalyzer(AccessAttributeName, BoundaryAttributeName, RawAccessRule, RawImplementationRule)
{
    /// <summary>Raw S-DSP register access outside an approved boundary.</summary>
    public const string RawAccessId = "SME6310";

    /// <summary>A raw S-DSP contract implemented outside an approved boundary.</summary>
    public const string RawImplementationId = "SME6311";

    /// <summary>Marks a raw S-DSP register API.</summary>
    public const string AccessAttributeName = "DspRegisterAccessAttribute";

    /// <summary>Marks an approved S-DSP register boundary.</summary>
    public const string BoundaryAttributeName = "DspRegisterBoundaryAttribute";

    /// <summary>SME6310 warning for use of raw S-DSP register APIs outside a declared audio boundary.</summary>
    private static readonly DiagnosticDescriptor RawAccessRule = new(
        RawAccessId,
        "Raw S-DSP register access outside an audio boundary",
        "'{0}' is raw S-DSP register access; use the typed global or per-voice DSP controls",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Register numbers, voice strides, masks and register encodings belong to the DSP implementation, " +
            "its typed controls, native-quirk adapters and exact register tests, which declare [DspRegisterBoundary].");

    /// <summary>SME6311 warning for implementing a raw S-DSP contract outside an approved boundary.</summary>
    private static readonly DiagnosticDescriptor RawImplementationRule = new(
        RawImplementationId,
        "Raw S-DSP contract implemented outside an audio boundary",
        "{0} implements raw S-DSP contract '{1}'; declare it an approved [DspRegisterBoundary] or use the typed controls",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only the DSP implementation and exact register test doubles may implement raw S-DSP contracts.");
}
