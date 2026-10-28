using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports, in Core, any field, property, record component or method that holds or returns an integer named as a tile
/// transfer, upload or destination (SME6445, #1406). Under pre-assembled artwork a behaviour that used to queue a tile
/// upload selects a frame of a sheet or a part image instead, and publishes the selection through the display model; an
/// integer transfer, upload or destination is the hardware's video-memory copy. See <c>docs/frame-selection.md</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TransferWordAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A member holds a transfer, upload or destination as an integer.</summary>
    public const string TransferWordId = "SME6445";

    /// <summary>
    /// The camel-case words naming a copy into video memory, mapped to the word reported. Matched whole and without regard
    /// to case, so <c>Transference</c> and <c>Uploader</c> are not reported.
    /// </summary>
    internal static readonly ImmutableDictionary<string, string> TransferWords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Transfer"] = "Transfer",
        ["Transfers"] = "Transfer",
        ["Transferred"] = "Transfer",
        ["Transferring"] = "Transfer",
        ["Upload"] = "Upload",
        ["Uploads"] = "Upload",
        ["Uploaded"] = "Upload",
        ["Uploading"] = "Upload",
        ["Destination"] = "Destination",
        ["Destinations"] = "Destination",
    }.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>The integral types a video-memory word, count or offset is held in.</summary>
    private static readonly ImmutableHashSet<SpecialType> IntegralTypes =
    [
        SpecialType.System_Byte, SpecialType.System_SByte, SpecialType.System_Int16, SpecialType.System_UInt16,
        SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64,
    ];

    /// <summary>The generic sequences of one element type, and <see cref="Nullable{T}"/>, whose element is what they hold.</summary>
    private static readonly ImmutableHashSet<string> Sequences =
    [
        "Nullable", "Span", "ReadOnlySpan", "Memory", "ReadOnlyMemory", "ImmutableArray", "List", "IList", "IReadOnlyList",
        "ICollection", "IReadOnlyCollection", "IEnumerable", "ImmutableList", "ImmutableHashSet", "FrozenSet", "HashSet",
        "ISet", "IReadOnlySet", "Queue", "Stack",
    ];

    /// <summary>The generic dictionaries, whose values are what they hold.</summary>
    private static readonly ImmutableHashSet<string> Dictionaries =
        ["Dictionary", "IDictionary", "IReadOnlyDictionary", "ImmutableDictionary", "FrozenDictionary", "SortedDictionary"];

    private static readonly DiagnosticDescriptor Rule = new(
        TransferWordId,
        "A member holds a transfer, upload or destination as an integer",
        "'{0}' holds a tile {1} as an integer; select a frame of a sheet or a part image and publish it through the display model instead",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Under pre-assembled artwork, every tile upload a behaviour performed is \"show frame N of this sheet\" or " +
            "\"show this part image\". The behaviour selects the frame or part at the update it used to queue the transfer, " +
            "and the selection is published through the display model. No Core member or record component holds an integer " +
            "named as a transfer, an upload or a destination (#1406, docs/frame-selection.md).");

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
            start.RegisterSymbolAction(AnalyzeMember, SymbolKind.Field, SymbolKind.Property, SymbolKind.Method);
        });
    }

    /// <summary>
    /// The transfer word a member named <paramref name="name"/> of <paramref name="type"/> holds as an integer, or null when
    /// its name has none of <see cref="TransferWords"/> or it holds no integers.
    /// </summary>
    internal static string? Violation(string name, ITypeSymbol type)
    {
        if (!HoldsIntegers(type, depth: 0))
            return null;
        foreach (string word in CoreVocabularyAnalyzer.Words(name))
        {
            if (TransferWords.TryGetValue(word, out string? reported))
                return reported;
        }
        return null;
    }

    /// <summary>Reports a field, property, record component or ordinary method that holds a transfer word as an integer.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol.IsOverride || symbol.Locations.FirstOrDefault(location => location.IsInSource) is not { } location)
            return;
        ITypeSymbol? type = symbol switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            IMethodSymbol { MethodKind: MethodKind.Ordinary } method => method.ReturnType,
            _ => null,
        };
        if (type is not null && Violation(symbol.Name, type) is { } word)
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, word.ToLowerInvariant()));
    }

    /// <summary>
    /// Whether <paramref name="type"/> is an integral type, or an array, sequence, nullable or dictionary value of one,
    /// looking through at most a few levels of nesting.
    /// </summary>
    private static bool HoldsIntegers(ITypeSymbol type, int depth)
    {
        if (IntegralTypes.Contains(type.SpecialType))
            return true;
        if (depth >= 3)
            return false;
        return type switch
        {
            IArrayTypeSymbol array => HoldsIntegers(array.ElementType, depth + 1),
            INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } named when Sequences.Contains(named.Name) =>
                HoldsIntegers(named.TypeArguments[0], depth + 1),
            INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 2 } named when Dictionaries.Contains(named.Name) =>
                HoldsIntegers(named.TypeArguments[1], depth + 1),
            _ => false,
        };
    }
}
