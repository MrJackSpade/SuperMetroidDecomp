using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Confines the SNES PPU layout — register numbers, VRAM word/byte addresses, CGRAM slots, OAM
/// low/high tables and their backing buffers — to approved boundaries (#630). Raw PPU APIs are
/// declared <c>[PpuLayoutAccess]</c>; only declarations inside a <c>[PpuLayoutBoundary("reason")]</c>
/// may use them.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PpuLayoutAccessAnalyzer() : LayoutBoundaryAnalyzer(AccessAttributeName, BoundaryAttributeName, RawAccessRule, RawImplementationRule)
{
    /// <summary>Raw PPU layout access outside an approved boundary.</summary>
    public const string RawAccessId = "SME6300";

    /// <summary>A raw PPU contract implemented outside an approved boundary.</summary>
    public const string RawImplementationId = "SME6301";

    /// <summary>Marks a raw PPU layout API.</summary>
    public const string AccessAttributeName = "PpuLayoutAccessAttribute";

    /// <summary>Marks an approved PPU layout boundary.</summary>
    public const string BoundaryAttributeName = "PpuLayoutBoundaryAttribute";

    /// <summary>Warns when a caller accesses raw PPU layout outside an approved rendering or codec boundary.</summary>
    private static readonly DiagnosticDescriptor RawAccessRule = new(
        RawAccessId,
        "Raw PPU layout access outside a rendering boundary",
        "'{0}' is raw PPU layout access; use a typed, domain-named rendering view",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Register numbers, VRAM addresses, CGRAM slots, OAM tables and their encodings belong to typed " +
            "rendering views, the PPU backend, snapshot/DMA codecs and exact hardware tests, which declare [PpuLayoutBoundary].");

    /// <summary>Warns when a raw PPU contract is implemented outside an approved backend or hardware test double.</summary>
    private static readonly DiagnosticDescriptor RawImplementationRule = new(
        RawImplementationId,
        "Raw PPU contract implemented outside a rendering boundary",
        "{0} implements raw PPU contract '{1}'; declare it an approved [PpuLayoutBoundary] or use a typed rendering view",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only the PPU backend and exact hardware test doubles may implement raw PPU contracts.");
}
