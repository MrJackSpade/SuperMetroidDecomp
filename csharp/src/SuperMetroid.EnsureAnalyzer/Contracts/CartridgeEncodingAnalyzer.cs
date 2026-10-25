using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports cartridge encoding in repository code (#641, #223, #1323, #1324) at every site: raw hexadecimal literals,
/// address-valued pointer dispatch, opaque word arrays, raw storage-width operations, hand-written countdowns,
/// [Flags] primitive casts and narrow storage members. There is no baseline; an intrinsic encoded value carries a
/// same-line '// cartridge-encoding: allow(Class) - reason' review and an encoding owner carries [EncodingOwner].
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CartridgeEncodingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A raw hexadecimal literal in functional code.</summary>
    public const string RawHexId = "SME6340";

    /// <summary>A primitive compared with, offset from or switched over an address-valued code constant.</summary>
    public const string PointerDispatchId = "SME6341";

    /// <summary>An integral array or collection literal holding hexadecimal words.</summary>
    public const string OpaqueWordArrayId = "SME6342";

    /// <summary>A raw storage-width or layout shift or mask in functional code (#223).</summary>
    public const string RawWidthId = "SME6345";

    /// <summary>A hand-written sign-bit test on a countdown (#1324).</summary>
    public const string CountdownSignTestId = "SME6348";

    /// <summary>A hand-written wrapping self-decrement of narrow storage (#1324).</summary>
    public const string CountdownSelfDecrementId = "SME6349";

    /// <summary>A <c>[Flags]</c> enum value cast to its storage primitive in functional code (#1323).</summary>
    public const string FlagsPrimitiveCastId = "SME6346";

    /// <summary>A narrow integral declaration holding gameplay state in Core (#1323, #1419).</summary>
    public const string NarrowStorageMemberId = "SME6347";

    /// <summary>Guidance appended to each encoding diagnostic so users can find the reviewed exception policy.</summary>
    private const string Guide = "see docs/cartridge-encoding.md";

    /// <summary>Warns when functional code adds an unreviewed hexadecimal literal.</summary>
    private static readonly DiagnosticDescriptor RawHexRule = new(
        RawHexId,
        "Raw hexadecimal literal in functional code",
        "{0}; model the value as a quantity, enum, flags, structured definition or catalog member ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Functional code must not gain hexadecimal literals. Decimal quantities, proven enums, verified " +
            "[Flags], structured definitions and named catalogs replace them; an intrinsic encoded value carries a " +
            "same-line '// cartridge-encoding: allow(Class) - reason' review.");

    /// <summary>Warns when primitive values dispatch on native code or instruction addresses.</summary>
    private static readonly DiagnosticDescriptor PointerDispatchRule = new(
        PointerDispatchId,
        "Address-based dispatch on a primitive",
        "{0}; dispatch on a typed state, command or definition instead of a cartridge address ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Comparing, offsetting or switching a primitive against address-valued code or instruction " +
            "constants interprets cartridge pointers at run time. Decode at the boundary and dispatch on semantic types.");

    /// <summary>Warns when integral collections encode opaque hexadecimal instruction or table words.</summary>
    private static readonly DiagnosticDescriptor OpaqueWordArrayRule = new(
        OpaqueWordArrayId,
        "Opaque word array",
        "{0}; replace the encoded words with typed definitions ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Integral arrays of hexadecimal words carry encoded instructions or packed tables. Express them " +
            "as records, timelines or state transitions with named fields.");

    /// <summary>Warns on raw lane shifts and masks that should use the owning cartridge encoding type.</summary>
    private static readonly DiagnosticDescriptor RawWidthRule = new(
        RawWidthId,
        "Raw storage-width or layout operation",
        "{0}; name what it means: a byte lane (SnesLittleEndian), an 8.8 or 16.16 fixed-point part (SignedEightEight, " +
            "SignedSixteenSixteen, FixedCoordinate), a bank and offset (SnesAddress), a packed field, a screen or tile " +
            "conversion, or a fraction scale; the same shift can mean any of these, so choose the actual one ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A shift by 8, 16 or 24 or a byte/word/bank mask does not say why the width exists. Call a named " +
            "operation on the type that owns the encoding ([EncodingOwner]) instead; an intrinsic exception carries a " +
            "same-line '// cartridge-encoding: allow(Class) - reason' review.");

    private static readonly DiagnosticDescriptor CountdownSignTestRule = new(
        CountdownSignTestId,
        "Hand-written countdown sign test",
        "{0}; store the timer as WrappingCountdown and test the CountdownStep its Decrement returns " +
            "(IsNegative, IsNonNegative, IsZeroOrNegative) or the countdown's IsNegative ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A countdown's '& 0x8000', '& 0x80', comparison with $8000 or $80, or '(short)t < 0' restates the " +
            "65C816 N flag by hand. WrappingCountdown and CountdownStep name the native branch instead.");

    private static readonly DiagnosticDescriptor CountdownSelfDecrementRule = new(
        CountdownSelfDecrementId,
        "Hand-written wrapping decrement",
        "{0}; use WrappingCountdown.Decrement (DEC then BPL/BEQ/BMI), DecrementToZero (LDA, BEQ, DEC) or " +
            "WrappingCounter.DecrementLowByte (8-bit DEC of a packed low byte) ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "'x = unchecked((ushort)(x - 1))', its byte form and '--' on ushort or byte storage hide the native " +
            "wraparound and leave the branch to a hand-written sign or zero test. The countdown type keeps the exact word.");

    private static readonly DiagnosticDescriptor FlagsPrimitiveCastRule = new(
        FlagsPrimitiveCastId,
        "[Flags] enum cast to a primitive word",
        "{0}; keep the typed flags and convert only at the encoding boundary, with the type's ToNativeWord ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Gameplay state holds [Flags] enums as their type. Casting one to ushort or byte moves the bits back " +
            "into an untyped word; only codecs and [EncodingOwner] types convert to the native word.");

    private static readonly DiagnosticDescriptor NarrowStorageMemberRule = new(
        NarrowStorageMemberId,
        "Narrow CPU-word storage in functional code",
        "{0}; store the domain type (an int quantity, bool, enum, flags, nullable value or logical index) and convert at " +
            "the codec, or make a load-bearing wrap explicit ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A byte, sbyte, short or ushort field, property, parameter or local holds gameplay state at a cartridge " +
            "register width. Widen quantities whose wrap does not matter; a wrap the game observes lives in a named " +
            "wrapping stub of the Compatibility module, and a width an encoding needs in its [EncodingOwner] codec " +
            "(#1323, #1419, docs/cartridge-encoding.md).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [RawHexRule, PointerDispatchRule, OpaqueWordArrayRule, RawWidthRule, CountdownSignTestRule, CountdownSelfDecrementRule,
            FlagsPrimitiveCastRule, NarrowStorageMemberRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (!InScope(start.Compilation.AssemblyName))
                return;
            start.RegisterSemanticModelAction(AnalyzeTree);
        });
    }

    /// <summary>
    /// Every repository assembly is in scope except the analyzer projects, which compile the detector's own rules.
    /// </summary>
    internal static bool InScope(string? assemblyName) =>
        assemblyName is not null && assemblyName.StartsWith(RepositoryOwnership.AssemblyPrefix, StringComparison.Ordinal) &&
        !assemblyName.StartsWith("SuperMetroid.EnsureAnalyzer", StringComparison.Ordinal);

    /// <summary>Scans a compiled source tree and reports every encoding occurrence where it is.</summary>
    private static void AnalyzeTree(SemanticModelAnalysisContext context)
    {
        if (CartridgeEncodingScanner.IsBuildGenerated(context.SemanticModel.SyntaxTree.FilePath))
            return;
        foreach (CartridgeEncodingOccurrence occurrence in CartridgeEncodingScanner.Scan(context.SemanticModel, context.CancellationToken))
        {
            DiagnosticDescriptor? descriptor = CartridgeEncodingScanner.RuleId(occurrence.Rule) switch
            {
                RawHexId => RawHexRule,
                PointerDispatchId => PointerDispatchRule,
                OpaqueWordArrayId => OpaqueWordArrayRule,
                RawWidthId => RawWidthRule,
                CountdownSignTestId => CountdownSignTestRule,
                CountdownSelfDecrementId => CountdownSelfDecrementRule,
                FlagsPrimitiveCastId => FlagsPrimitiveCastRule,
                NarrowStorageMemberId => NarrowStorageMemberRule,
                null => null,
                var id => throw new InvalidOperationException($"Unmapped cartridge-encoding rule id {id}."),
            };
            if (descriptor is not null)
                context.ReportDiagnostic(Diagnostic.Create(descriptor, occurrence.Location,
                    $"{occurrence.Class} ({occurrence.Basis})", Guide));
        }
    }
}
