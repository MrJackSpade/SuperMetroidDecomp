using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// Confines the SPC700 APU memory layout — raw RAM indexing and spans, RAM addresses, port numbers and
/// sequence-pointer arithmetic — to approved boundaries (#632). Raw DSP APIs are declared
/// <c>[SpcMemoryAccess]</c>; only declarations inside a <c>[SpcMemoryBoundary("reason")]</c> may use them.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpcMemoryAccessAnalyzer() : LayoutBoundaryAnalyzer(AccessAttributeName, BoundaryAttributeName, RawAccessRule, RawImplementationRule)
{
    /// <summary>Raw SPC700 memory access outside an approved boundary.</summary>
    public const string RawAccessId = "SME6320";

    /// <summary>A raw SPC700 memory contract implemented outside an approved boundary.</summary>
    public const string RawImplementationId = "SME6321";

    /// <summary>Marks a raw SPC700 memory API.</summary>
    public const string AccessAttributeName = "SpcMemoryAccessAttribute";

    /// <summary>Marks an approved SPC700 memory boundary.</summary>
    public const string BoundaryAttributeName = "SpcMemoryBoundaryAttribute";

    /// <summary>Stores raw access rule.</summary>
    private static readonly DiagnosticDescriptor RawAccessRule = new(
        RawAccessId,
        "Raw SPC700 memory access outside an audio boundary",
        "'{0}' is raw SPC700 memory access; use the typed audio-driver memory views",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "APU RAM addresses, layouts, ports and encodings belong to the APU RAM implementation, " +
            "typed driver-memory views, definition codecs and exact APU memory tests, which declare [SpcMemoryBoundary].");

    /// <summary>Stores raw implementation rule.</summary>
    private static readonly DiagnosticDescriptor RawImplementationRule = new(
        RawImplementationId,
        "Raw SPC700 memory contract implemented outside an audio boundary",
        "{0} implements raw SPC700 memory contract '{1}'; declare it an approved [SpcMemoryBoundary] or use the typed driver-memory views",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only the APU RAM implementation and exact APU memory test doubles may implement raw SPC700 memory contracts.");
}
