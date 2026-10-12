using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// Confines the SNES CPU A-bus layout to approved boundaries (#629). Raw address-space reads and
/// writes, raw ROM/WRAM/SRAM buffers and address-bound cell primitives are declared
/// <c>[CpuBusAccess]</c>; only declarations inside a <c>[CpuBusBoundary("reason")]</c> may use them.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CpuBusAccessAnalyzer() : LayoutBoundaryAnalyzer(AccessAttributeName, BoundaryAttributeName, RawAccessRule, RawImplementationRule)
{
    /// <summary>Raw CPU-bus access outside an approved boundary.</summary>
    public const string RawAccessId = "SME6290";

    /// <summary>A raw CPU-bus contract implemented outside an approved boundary.</summary>
    public const string RawImplementationId = "SME6291";

    /// <summary>Marks a raw CPU-bus API.</summary>
    public const string AccessAttributeName = "CpuBusAccessAttribute";

    /// <summary>Marks an approved CPU-bus boundary.</summary>
    public const string BoundaryAttributeName = "CpuBusBoundaryAttribute";

    private static readonly DiagnosticDescriptor RawAccessRule = new(
        RawAccessId,
        "Raw CPU-bus access outside a memory boundary",
        "'{0}' is raw CPU-bus access; use a typed, domain-named memory view",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Addresses, banks, widths, byte order and encodings belong to typed domain memory views, the " +
            "address-space implementation, native-layout codecs and exact bus tests, which declare [CpuBusBoundary].");

    private static readonly DiagnosticDescriptor RawImplementationRule = new(
        RawImplementationId,
        "Raw CPU-bus contract implemented outside a memory boundary",
        "{0} implements raw CPU-bus contract '{1}'; declare it an approved [CpuBusBoundary] or use a typed memory view",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only the address-space implementation and exact bus test doubles may implement raw CPU-bus contracts.");
}
