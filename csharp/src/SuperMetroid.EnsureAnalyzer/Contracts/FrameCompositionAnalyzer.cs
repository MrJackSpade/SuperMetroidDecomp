using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports, in Core, any member reachable from the draw entries the backend receives that composes a frame from
/// characters or selects a colour from a palette row (SME6442, #1392). A drawn thing is a pre-assembled, pre-coloured
/// image selected by identity; character pages and palette rows are the hardware's way of building one.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FrameCompositionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A member reachable from the draw entries holds characters or selects a palette row.</summary>
    public const string FrameCompositionId = "SME6442";

    /// <summary>The types the backend receives a frame through; everything their members reach is a draw path.</summary>
    internal static readonly ImmutableHashSet<string> DrawEntryTypes = ["DisplayPacket", "SpriteDraw"];

    /// <summary>Character pages and sheets: the hardware's tile memory, from which frames are composed.</summary>
    internal static readonly ImmutableHashSet<string> CharacterPageTypes =
    [
        "SpriteArtwork", "ObjCharacterPage", "ObjCharacterTablePage", "ObjCharacterComposition", "ObjCharacterImage",
        "SpriteCharacterPlacement", "SpriteCharacters", "StagedObjCharacters", "GraphicsSetTileCharacters", "AssetObjCharacters",
        "RoomCharacterAtlas", "BgCharacterGrid", "SnesCharacterGrid",
    ];

    /// <summary>Palette rows: the hardware's colour memory, from which a part or tile selects its colours.</summary>
    internal static readonly ImmutableHashSet<string> PaletteRowTypes =
        ["CgramPalette", "CgramPalettes", "SnesCgram", "SnesObjPalettes", "IPaletteColors"];

    /// <summary>Member names that select a character by number or carry the character memory backgrounds draw from.</summary>
    internal static readonly ImmutableHashSet<string> CharacterNames =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "Character", "Characters", "Vram");

    /// <summary>Member names that select a palette row by number or carry the colour memory.</summary>
    internal static readonly ImmutableHashSet<string> PaletteRowNames =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "Palette", "PaletteRow", "PaletteRows", "Cgram");

    private static readonly DiagnosticDescriptor Rule = new(
        FrameCompositionId,
        "A draw path composes a frame from characters or palette rows",
        "'{0}' {1} on a draw path; draw a pre-assembled, pre-coloured image selected by identity instead",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Extraction pre-assembles and pre-colours every piece of artwork under its standard palette; the " +
            "simulation selects a frame, part or tile by identity and places it, and colour effects are shader parameters. " +
            "A draw path never composes a frame from characters or selects a colour from a palette row (#1392).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            IAssemblySymbol core = start.Compilation.Assembly;
            var reachable = new Lazy<ImmutableHashSet<INamedTypeSymbol>>(() => PublishedTypeReachability.Reachable(start.Compilation, core, DrawEntryTypes));
            start.RegisterSymbolAction(symbolContext => AnalyzeMember(symbolContext, reachable.Value),
                SymbolKind.Field, SymbolKind.Property);
        });
    }

    /// <summary>What a member does that the rule forbids, or null when it neither holds characters nor selects a palette row.</summary>
    internal static string? Composition(string name, ITypeSymbol type)
    {
        if (Mentions(type, CharacterPageTypes))
            return "holds a character page";
        if (Mentions(type, PaletteRowTypes))
            return "holds palette rows";
        // A name selects characters or a palette row only when it holds raw numbers: an identity of the same name names
        // a finished image or colour set, which is the destination.
        if (!IsRawData(type))
            return null;
        string bare = name.TrimStart('_');
        if (CharacterNames.Contains(bare))
            return "selects characters";
        return PaletteRowNames.Contains(bare) || bare.EndsWith("PaletteRow", StringComparison.Ordinal) ||
            bare.EndsWith("PaletteRows", StringComparison.Ordinal)
            ? "selects a palette row"
            : null;
    }

    /// <summary>Reports a member of a type the draw entries reach.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context, ImmutableHashSet<INamedTypeSymbol> reachable)
    {
        ISymbol symbol = context.Symbol;
        if (PublishedTypeReachability.ReachedMemberType(symbol, reachable, out Location? location) is { } type &&
            Composition(symbol.Name, type) is { } what)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, what));
        }
    }

    /// <summary>Colour words, which hold the hardware's packed colours.</summary>
    internal static readonly ImmutableHashSet<string> RawColourTypes = ["RgbColor"];

    /// <summary>Whether <paramref name="type"/> holds raw numbers: an integer, a colour word, or a collection of them.</summary>
    private static bool IsRawData(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
            return IsRawData(array.ElementType);
        if (type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or
            SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32)
        {
            return true;
        }
        if (type is not INamedTypeSymbol named)
            return false;
        if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            return IsRawData(named.TypeArguments[0]);
        if (RawColourTypes.Contains(named.Name))
            return true;
        return named.IsGenericType && named.TypeArguments.Length == 1 && IsRawData(named.TypeArguments[0]);
    }

    /// <summary>Whether <paramref name="type"/>, its elements or its type arguments name one of <paramref name="names"/>.</summary>
    private static bool Mentions(ITypeSymbol type, ImmutableHashSet<string> names) => type switch
    {
        IArrayTypeSymbol array => Mentions(array.ElementType, names),
        INamedTypeSymbol named => names.Contains(named.Name) || named.TypeArguments.Any(argument => Mentions(argument, names)),
        _ => false,
    };
}
