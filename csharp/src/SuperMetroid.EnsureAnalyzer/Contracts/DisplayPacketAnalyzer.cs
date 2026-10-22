using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports, in Core, any member of a type the published display packet reaches that holds raw memory or is named with
/// a register or memory word (SME6443, #1403). The packet describes what the player sees: placed images, tile layers,
/// text, solid colours and effect descriptions. Memory images and register values are how the hardware drew them.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisplayPacketAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A member reachable from the published display packet holds raw memory or names a register or memory word.</summary>
    public const string DisplayPacketId = "SME6443";

    /// <summary>The types a frame is published as; everything their members reach is in the packet.</summary>
    internal static readonly ImmutableHashSet<string> PacketTypes = ["RenderFrameSnapshot", "DisplayPacket"];

    /// <summary>Camel-case words naming the hardware's memories, their words, or its registers.</summary>
    internal static readonly ImmutableHashSet<string> HardwareWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "Vram", "Cgram", "Oam", "Wram", "Memory", "Register", "Registers", "Word", "Words", "Obsel", "ObjectSelection");

    /// <summary>The elements whose arrays and spans are raw memory: bytes and 16-bit words.</summary>
    private static readonly ImmutableHashSet<SpecialType> RawElements =
        [SpecialType.System_Byte, SpecialType.System_SByte, SpecialType.System_UInt16, SpecialType.System_Int16];

    /// <summary>Splits an identifier into its camel-case words.</summary>
    private static readonly Regex CamelWords = new("[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+", RegexOptions.CultureInvariant);

    private static readonly DiagnosticDescriptor Rule = new(
        DisplayPacketId,
        "The published display packet carries memory or registers",
        "'{0}' {1} in the published display packet; describe what is shown, not the memory or registers that showed it",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each accepted frame publishes one immutable display packet: ordered layers of placed images, tile layers " +
            "with scroll, text, solid colours and effect descriptions. It carries no memory image, no register value and no " +
            "member named for either (#1403).");

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
            var reachable = new Lazy<ImmutableHashSet<INamedTypeSymbol>>(() =>
                PublishedTypeReachability.Reachable(start.Compilation, core, PacketTypes));
            start.RegisterSymbolAction(symbolContext => AnalyzeMember(symbolContext, reachable.Value), SymbolKind.Field, SymbolKind.Property);
        });
    }

    /// <summary>What a member does that the rule forbids, or null when it describes what is shown.</summary>
    internal static string? Violation(string name, ITypeSymbol type)
    {
        if (IsRawMemory(type))
            return "holds raw memory";
        return CamelWords.Matches(name.TrimStart('_')).Cast<Match>().Any(word => HardwareWords.Contains(word.Value))
            ? "is named for a register or memory word"
            : null;
    }

    /// <summary>Reports a member of a type the packet reaches.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context, ImmutableHashSet<INamedTypeSymbol> reachable)
    {
        ISymbol symbol = context.Symbol;
        if (PublishedTypeReachability.ReachedMemberType(symbol, reachable, out Location? location) is { } type &&
            Violation(symbol.Name, type) is { } what)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, symbol.Name, what));
        }
    }

    /// <summary>Whether <paramref name="type"/> is an array, span, memory or single-element collection of bytes or 16-bit words.</summary>
    private static bool IsRawMemory(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol array => RawElements.Contains(array.ElementType.SpecialType),
        INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } named => RawElements.Contains(named.TypeArguments[0].SpecialType),
        _ => false,
    };
}
