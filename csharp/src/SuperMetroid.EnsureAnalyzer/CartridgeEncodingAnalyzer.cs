using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// Ratchets cartridge encoding in production code (#641): a file may not gain raw hexadecimal
/// literals, address-valued pointer dispatch or opaque word arrays beyond its reviewed baseline,
/// and a file absent from the baseline may hold none. Active only in projects given
/// <c>cartridge-encoding-baseline.txt</c> as an additional file.
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

    /// <summary>A file holds fewer occurrences than its baseline: lower the reviewed count.</summary>
    public const string StaleCountId = "SME6343";

    /// <summary>A baseline entry names a file this project no longer compiles.</summary>
    public const string StaleFileId = "SME6344";

    private const string Guide = "see docs/cartridge-encoding.md";

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

    private static readonly DiagnosticDescriptor PointerDispatchRule = new(
        PointerDispatchId,
        "Address-based dispatch on a primitive",
        "{0}; dispatch on a typed state, command or definition instead of a cartridge address ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Comparing, offsetting or switching a primitive against address-valued code or instruction " +
            "constants interprets cartridge pointers at run time. Decode at the boundary and dispatch on semantic types.");

    private static readonly DiagnosticDescriptor OpaqueWordArrayRule = new(
        OpaqueWordArrayId,
        "Opaque word array",
        "{0}; replace the encoded words with typed definitions ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Integral arrays of hexadecimal words carry encoded instructions or packed tables. Express them " +
            "as records, timelines or state transitions with named fields.");

    private static readonly DiagnosticDescriptor RawWidthRule = new(
        RawWidthId,
        "Raw storage-width or layout operation",
        "{0}; name what it means: a byte lane (SnesLittleEndian), an 8.8 or 16.16 fixed-point part (SnesSignedEightEight, " +
            "SnesSignedSixteenSixteen, SnesFixedPosition), a bank and offset (SnesAddress), a packed field, a screen or tile " +
            "conversion, or a fraction scale; the same shift can mean any of these, so choose the actual one ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A shift by 8, 16 or 24 or a byte/word/bank mask does not say why the width exists. Call a named " +
            "operation on the type that owns the encoding ([SnesEncodingOwner]) instead; an intrinsic exception carries a " +
            "same-line '// cartridge-encoding: allow(Class) - reason' review.");

    private static readonly DiagnosticDescriptor CountdownSignTestRule = new(
        CountdownSignTestId,
        "Hand-written countdown sign test",
        "{0}; store the timer as NativeCountdown and test the NativeWordCounterStep its Decrement returns " +
            "(IsNegative, IsNonNegative, IsZeroOrNegative) or the countdown's IsNegative ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A countdown's '& 0x8000', '& 0x80', comparison with $8000 or $80, or '(short)t < 0' restates the " +
            "65C816 N flag by hand. NativeCountdown and NativeWordCounterStep name the native branch instead.");

    private static readonly DiagnosticDescriptor CountdownSelfDecrementRule = new(
        CountdownSelfDecrementId,
        "Hand-written wrapping decrement",
        "{0}; use NativeCountdown.Decrement (DEC then BPL/BEQ/BMI), DecrementToZero (LDA, BEQ, DEC) or " +
            "NativeWordCounter.DecrementLowByte (8-bit DEC of a packed low byte) ({1})",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "'x = unchecked((ushort)(x - 1))', its byte form and '--' on ushort or byte storage hide the native " +
            "wraparound and leave the branch to a hand-written sign or zero test. The countdown type keeps the exact word.");

    private static readonly DiagnosticDescriptor StaleCountRule = new(
        StaleCountId,
        "Cartridge-encoding baseline is stale",
        "{0} now holds {1} {2} occurrence(s) but the baseline allows {3}; lower the baseline so the ratchet keeps the gain",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor StaleFileRule = new(
        StaleFileId,
        "Cartridge-encoding baseline names a missing file",
        "The baseline lists {0} for {1}, which this project no longer compiles; remove the entry",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [RawHexRule, PointerDispatchRule, OpaqueWordArrayRule, RawWidthRule, CountdownSignTestRule, CountdownSelfDecrementRule,
            StaleCountRule, StaleFileRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            AdditionalText? file = start.Options.AdditionalFiles.FirstOrDefault(additional =>
                string.Equals(Path.GetFileName(additional.Path), CartridgeEncodingScanner.BaselineFileName,
                    StringComparison.OrdinalIgnoreCase));
            if (file is null)
                return;
            SourceText content = file.GetText(start.CancellationToken)
                ?? throw new InvalidOperationException($"{file.Path}: the cartridge-encoding baseline could not be read.");
            CartridgeEncodingBaseline baseline = CartridgeEncodingBaseline.Parse(file.Path, content.ToString());
            var compiled = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);

            start.RegisterSemanticModelAction(model => AnalyzeTree(model, baseline, compiled));
            start.RegisterCompilationEndAction(end =>
            {
                string projectPrefix = $"csharp/src/{end.Compilation.AssemblyName}/";
                foreach (var entry in baseline.Counts.Keys.Where(key =>
                             key.Path.StartsWith(projectPrefix, StringComparison.Ordinal) && !compiled.ContainsKey(key.Path)))
                {
                    end.ReportDiagnostic(Diagnostic.Create(StaleFileRule, Location.None, entry.Rule, entry.Path));
                }
            });
        });
    }

    private static void AnalyzeTree(
        SemanticModelAnalysisContext context,
        CartridgeEncodingBaseline baseline,
        ConcurrentDictionary<string, bool> compiled)
    {
        string filePath = context.SemanticModel.SyntaxTree.FilePath;
        if (CartridgeEncodingScanner.IsBuildGenerated(filePath))
            return;
        string? path = CartridgeEncodingScanner.RepositoryRelative(baseline.RepositoryRoot, filePath);
        if (path is null)
            return;
        compiled[path] = true;

        var occurrences = CartridgeEncodingScanner.Scan(context.SemanticModel, context.CancellationToken);
        foreach (var group in occurrences.GroupBy(occurrence => occurrence.Rule))
        {
            DiagnosticDescriptor? descriptor = CartridgeEncodingScanner.RuleId(group.Key) switch
            {
                RawHexId => RawHexRule,
                PointerDispatchId => PointerDispatchRule,
                OpaqueWordArrayId => OpaqueWordArrayRule,
                RawWidthId => RawWidthRule,
                CountdownSignTestId => CountdownSignTestRule,
                CountdownSelfDecrementId => CountdownSelfDecrementRule,
                null => null,
                var id => throw new InvalidOperationException($"Unmapped cartridge-encoding rule id {id}."),
            };
            if (descriptor is null)
                continue;
            var found = group.OrderBy(occurrence => occurrence.Location.SourceSpan.Start).ToList();
            int allowed = baseline.Allowed(path, descriptor.Id);
            if (allowed == 0)
            {
                // A file without a reviewed count is held to none: every occurrence is reported where it is.
                foreach (var occurrence in found)
                {
                    context.ReportDiagnostic(Diagnostic.Create(descriptor, occurrence.Location,
                        $"{occurrence.Class} ({occurrence.Basis})", Guide));
                }
            }
            else if (found.Count > allowed)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, found[0].Location,
                    $"{path} holds {found.Count} occurrences but its reviewed baseline allows {allowed}", Guide));
            }
        }

        foreach (string rule in CartridgeEncodingScanner.RatchetedRuleIds)
        {
            int allowed = baseline.Allowed(path, rule);
            if (allowed == 0)
                continue;
            int count = occurrences.Count(occurrence => CartridgeEncodingScanner.RuleId(occurrence.Rule) == rule);
            if (count < allowed)
            {
                context.ReportDiagnostic(Diagnostic.Create(StaleCountRule,
                    Location.Create(context.SemanticModel.SyntaxTree, new TextSpan(0, 0)), path, count, rule, allowed));
            }
        }
    }
}
