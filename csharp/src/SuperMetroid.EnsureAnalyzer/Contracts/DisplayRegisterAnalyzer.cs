using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports, in <c>SuperMetroid.Core</c>, every field or property that stores a PPU register (SME6449, #1407): one whose
/// name or summary names a display register (<c>BG*SC</c>, <c>BG*NBA</c>, <c>BGMODE</c>, <c>TM</c>/<c>TS</c>, the window
/// registers <c>W12SEL</c> through <c>TSW</c>, <c>COLDATA</c>, <c>CGADSUB</c>, <c>CGWSEL</c>, <c>INIDISP</c>, the
/// <c>M7*</c> registers), or a byte cache named for registers. Gameplay publishes typed scene state at the frame-end
/// latch; register shadows are the hardware's encoding of that scene (docs/display-scene.md).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisplayRegisterAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core field or property stores a PPU register.</summary>
    public const string DisplayRegisterId = "SME6449";

    /// <summary>The display registers, by their mnemonics.</summary>
    internal static readonly ImmutableArray<string> Registers =
    [
        "BG1SC", "BG2SC", "BG3SC", "BG4SC", "BG12NBA", "BG34NBA", "BGMODE", "TM", "TS", "W12SEL", "W34SEL", "WOBJSEL",
        "WH0", "WH1", "WH2", "WH3", "WBGLOG", "WOBJLOG", "TMW", "TSW", "COLDATA", "CGADSUB", "CGWSEL", "INIDISP",
        "M7SEL", "M7A", "M7B", "M7C", "M7D", "M7X", "M7Y",
    ];

    /// <summary>A register mnemonic written as one upper-case token in documentation.</summary>
    private static readonly Regex DocumentedRegister = new(@"(?<![A-Za-z0-9$])(" + string.Join("|", Registers) + @")(?![A-Za-z0-9])",
        RegexOptions.CultureInvariant);

    /// <summary>The words that name registers, which a byte cache named with them holds.</summary>
    private static readonly ImmutableHashSet<string> RegisterWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "Register", "Registers");

    private static readonly DiagnosticDescriptor Rule = new(
        DisplayRegisterId,
        "Core stores a display register",
        "'{0}' {1}; publish the scene it describes (camera, layers, windows, blending, per-line effects, brightness) instead",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Gameplay publishes typed scene state at the frame-end latch, and the backend renders it. PPU register " +
            "shadows, window and colour-math registers and register-indexed byte caches are the hardware's encoding of " +
            "that scene (#1407, docs/display-scene.md).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName == CoreLayerAnalyzer.CoreAssembly)
                start.RegisterSymbolAction(AnalyzeMember, SymbolKind.Field, SymbolKind.Property);
        });
    }

    /// <summary>The register the identifier <paramref name="name"/> spells in one camel-case word or a run of up to three, or null.</summary>
    internal static string? NamedRegister(string name)
    {
        string[] words = CoreVocabularyAnalyzer.Words(name).ToArray();
        for (int start = 0; start < words.Length; start++)
        {
            string spelled = "";
            for (int end = start; end < words.Length && end < start + 3; end++)
            {
                spelled += words[end];
                foreach (string register in Registers)
                {
                    if (string.Equals(spelled, register, StringComparison.OrdinalIgnoreCase))
                        return register;
                }
            }
        }
        return null;
    }

    /// <summary>Reports a field or property that names a register, documents one, or caches register bytes.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol.IsOverride || symbol.Locations.FirstOrDefault(location => location.IsInSource) is not { } location)
            return;
        if (NamedRegister(symbol.Name) is { } named)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, $"names the display register {named}"));
            return;
        }
        string? summary = symbol.GetDocumentationCommentXml(cancellationToken: context.CancellationToken);
        if (summary is not null && DocumentedRegister.Match(summary) is { Success: true } documented)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, $"documents the display register {documented.Value}"));
            return;
        }
        ITypeSymbol? type = symbol switch { IFieldSymbol field => field.Type, IPropertySymbol property => property.Type, _ => null };
        if (type is not null && IsByteCache(type) && NamesRegisters(symbol))
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, "is a byte cache indexed by register offset"));
    }

    /// <summary>Whether <paramref name="type"/> is a buffer of bytes: an array, span or memory of them.</summary>
    private static bool IsByteCache(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol array => array.ElementType.SpecialType == SpecialType.System_Byte,
        INamedTypeSymbol { IsGenericType: true } generic => generic.Name is "Span" or "ReadOnlySpan" or "Memory" or "ReadOnlyMemory" &&
            generic.TypeArguments[0].SpecialType == SpecialType.System_Byte,
        _ => false,
    };

    /// <summary>Whether the member or the type that declares it is named for registers.</summary>
    private static bool NamesRegisters(ISymbol symbol) =>
        CoreVocabularyAnalyzer.Words(symbol.Name).Any(RegisterWords.Contains) ||
        symbol.ContainingType is { } owner && CoreVocabularyAnalyzer.Words(owner.Name).Any(RegisterWords.Contains);
}
