using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports, in Core, any field or property that holds a palette layout (SME6444, #1405): colours keyed by an integer
/// slot, or colours named as a palette. Images are pre-coloured; a colour effect is typed state owned by what drives
/// it, and the colours an effect steps through live in a domain-named catalog. Slots are the hardware's layout of
/// colour memory.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PaletteLayoutAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A member holds colours laid out as palette memory.</summary>
    public const string PaletteLayoutId = "SME6444";

    /// <summary>The colour types: a hardware colour word, a display colour and an asset document's colour.</summary>
    internal static readonly ImmutableHashSet<string> ColourTypes = ["RgbColor", "RgbaPixel", "PaletteRgb5"];

    /// <summary>Camel-case words naming a palette.</summary>
    internal static readonly ImmutableHashSet<string> PaletteWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "Palette", "Palettes");

    /// <summary>The generic sequences of one element type.</summary>
    private static readonly ImmutableHashSet<string> Sequences =
    [
        "Span", "ReadOnlySpan", "Memory", "ReadOnlyMemory", "ImmutableArray", "List", "IList", "IReadOnlyList",
        "ICollection", "IReadOnlyCollection", "IEnumerable", "ImmutableList", "FrozenSet", "HashSet",
    ];

    /// <summary>The generic dictionaries.</summary>
    private static readonly ImmutableHashSet<string> Dictionaries =
        ["Dictionary", "IDictionary", "IReadOnlyDictionary", "ImmutableDictionary", "FrozenDictionary", "SortedDictionary"];

    /// <summary>The integer types a slot is numbered with.</summary>
    private static readonly ImmutableHashSet<SpecialType> SlotTypes =
    [
        SpecialType.System_Byte, SpecialType.System_SByte, SpecialType.System_Int16, SpecialType.System_UInt16,
        SpecialType.System_Int32, SpecialType.System_UInt32,
    ];

    /// <summary>Splits an identifier into its camel-case words.</summary>
    private static readonly Regex CamelWords = new("[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+", RegexOptions.CultureInvariant);

    private static readonly DiagnosticDescriptor Rule = new(
        PaletteLayoutId,
        "A member holds colours laid out as palette memory",
        "'{0}' {1}; images are pre-coloured, so hold the effect's typed state or a domain-named colour list instead",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Under pre-coloured images a palette is the hardware's layout of colour memory. A colour effect (a flash, a " +
            "fade, a cycle, a tint) is typed state owned by the entity or system that drives it, and the colours it needs " +
            "live in a domain-named catalog. No Core member keys colours by an integer slot or holds colours named as a " +
            "palette (#1405).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterSymbolAction(AnalyzeMember, SymbolKind.Field, SymbolKind.Property);
        });
    }

    /// <summary>How a member of <paramref name="type"/> named <paramref name="name"/> lays colours out as palette memory, or null.</summary>
    internal static string? Violation(string name, ITypeSymbol type)
    {
        if (IsSlotKeyed(type))
            return "keys colours by an integer slot";
        if (Element(type) is { } colour && IsColour(colour) && CamelWords.Matches(name.TrimStart('_')).Cast<Match>().Any(word => PaletteWords.Contains(word.Value)))
            return "holds colours named as a palette";
        return null;
    }

    /// <summary>Reports a field or property that lays colours out as palette memory.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared)
            return;
        ITypeSymbol? type = symbol switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null,
        };
        if (type is not null && Violation(symbol.Name, type) is { } what)
            context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, what));
    }

    /// <summary>Whether <paramref name="type"/> is a colour or a sequence of colours.</summary>
    private static bool IsColours(ITypeSymbol type) =>
        IsColour(type) || (Element(type) is { } element && IsColour(element));

    /// <summary>Whether <paramref name="type"/> is one of the colour types.</summary>
    private static bool IsColour(ITypeSymbol type) => ColourTypes.Contains(type.Name);

    /// <summary>The element of a one-dimensional array or a sequence, or null for any other type.</summary>
    private static ITypeSymbol? Element(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol { Rank: 1 } array => array.ElementType,
        INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } named when Sequences.Contains(named.Name) => named.TypeArguments[0],
        _ => null,
    };

    /// <summary>Whether <paramref name="type"/> is a dictionary from an integer slot to colours.</summary>
    private static bool IsSlotKeyed(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 2 } named &&
        Dictionaries.Contains(named.Name) &&
        SlotTypes.Contains(named.TypeArguments[0].SpecialType) &&
        IsColours(named.TypeArguments[1]);
}
