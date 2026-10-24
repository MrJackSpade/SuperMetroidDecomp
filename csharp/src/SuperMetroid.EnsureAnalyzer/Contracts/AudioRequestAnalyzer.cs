using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports, in Core, any audio request that carries an integral value, a port, a bank or an address (SME6446, #1410).
/// A request asks the audio layer for something in the game's terms: play a music track, stop the music, queue a sound
/// in a library, change the music data set. What it names is a domain identity decoded once at export; an integer on a
/// request is how a port write, an upload address or a raw request byte travels.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AudioRequestAnalyzer : DiagnosticAnalyzer
{
    /// <summary>An audio request carries an integral value, a port, a bank or an address.</summary>
    public const string AudioRequestId = "SME6446";

    /// <summary>The last camel-case word of a request type's name.</summary>
    internal static readonly ImmutableHashSet<string> RequestWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "Request", "Command");

    /// <summary>Camel-case words that make a request outside the Audio namespace an audio request.</summary>
    internal static readonly ImmutableHashSet<string> AudioWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "Audio", "Music", "Sound", "Sounds");

    /// <summary>Camel-case words naming a type that stands for a port, an upload bank or an address.</summary>
    internal static readonly ImmutableHashSet<string> HardwareWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "Port", "Ports", "Bank", "Banks", "Address", "Addresses");

    /// <summary>The namespace segment of the audio module.</summary>
    private const string AudioSegment = "Audio";

    /// <summary>The integral numeric types.</summary>
    private static readonly ImmutableHashSet<SpecialType> IntegralTypes =
    [
        SpecialType.System_Byte, SpecialType.System_SByte, SpecialType.System_Int16, SpecialType.System_UInt16,
        SpecialType.System_Int32, SpecialType.System_UInt32, SpecialType.System_Int64, SpecialType.System_UInt64,
        SpecialType.System_IntPtr, SpecialType.System_UIntPtr,
    ];

    /// <summary>Splits an identifier into its camel-case words.</summary>
    private static readonly Regex CamelWords = new("[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+", RegexOptions.CultureInvariant);

    private static readonly DiagnosticDescriptor Rule = new(
        AudioRequestId,
        "An audio request carries a port, an address or a raw value",
        "'{0}' carries {1}; an audio request names what it asks for (a music track, a sound, a music data set) by its domain identity",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Core asks the audio layer for things in the game's terms: play a music track, stop the music, queue a sound " +
            "in a library, change the music data set. Identities are names decoded once at export. No audio request (a type " +
            "named for a request or command in the audio module, or named for music, sound or audio elsewhere, and every " +
            "type nested in or derived from one) carries an integral value or a value standing for a port, an upload bank or " +
            "an address (#1410).");

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
            start.RegisterSymbolAction(AnalyzeMember, SymbolKind.Field, SymbolKind.Property);
        });
    }

    /// <summary>What a member of type <paramref name="type"/> carries that an audio request may not, or null.</summary>
    internal static string? Violation(ITypeSymbol type)
    {
        ITypeSymbol carried = Element(type) ?? type;
        if (IsIntegral(carried))
            return "an integral value";
        if (carried.TypeKind == TypeKind.Enum && Words(carried.Name).FirstOrDefault(HardwareWords.Contains) is { } word)
            return word.StartsWith("Port", StringComparison.OrdinalIgnoreCase) ? "a port"
                : word.StartsWith("Bank", StringComparison.OrdinalIgnoreCase) ? "a bank"
                : "an address";
        return null;
    }

    /// <summary>Whether <paramref name="type"/> is an audio request, or nested in or derived from one.</summary>
    internal static bool IsAudioRequest(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? container = type; container is not null; container = container.ContainingType)
        {
            for (INamedTypeSymbol? candidate = container; candidate is not null; candidate = candidate.BaseType)
            {
                if (NamesAudioRequest(candidate))
                    return true;
            }
        }
        return false;
    }

    /// <summary>Reports an instance field or property of an audio request that carries a forbidden value.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol symbol = context.Symbol;
        if (symbol.IsStatic || symbol.IsOverride || symbol.ContainingType is not { } owner)
            return;
        ITypeSymbol? type = symbol switch
        {
            IFieldSymbol { IsImplicitlyDeclared: false, AssociatedSymbol: null } field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null,
        };
        if (type is not null && IsAudioRequest(owner) && Violation(type) is { } what)
            context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, what));
    }

    /// <summary>Whether <paramref name="type"/>'s own name and namespace make it an audio request.</summary>
    private static bool NamesAudioRequest(INamedTypeSymbol type)
    {
        string[] words = Words(type.Name);
        if (words.Length == 0 || !RequestWords.Contains(words[words.Length - 1]))
            return false;
        return InAudioNamespace(type.ContainingNamespace) || words.Any(AudioWords.Contains);
    }

    /// <summary>Whether <paramref name="space"/> or one of its parents is named <c>Audio</c>.</summary>
    private static bool InAudioNamespace(INamespaceSymbol? space)
    {
        for (; space is { IsGlobalNamespace: false }; space = space.ContainingNamespace)
        {
            if (space.Name == AudioSegment)
                return true;
        }
        return false;
    }

    /// <summary>Whether <paramref name="type"/> is an integral numeric type or a nullable one.</summary>
    private static bool IsIntegral(ITypeSymbol type) =>
        IntegralTypes.Contains(type.SpecialType) ||
        (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable &&
         IntegralTypes.Contains(nullable.TypeArguments[0].SpecialType));

    /// <summary>The element of a one-dimensional array or a single-argument generic collection, or null.</summary>
    private static ITypeSymbol? Element(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol { Rank: 1 } array => array.ElementType,
        INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } named
            when named.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T => named.TypeArguments[0],
        _ => null,
    };

    /// <summary>The camel-case words of <paramref name="identifier"/>.</summary>
    private static string[] Words(string identifier) =>
        [.. CamelWords.Matches(identifier.TrimStart('_')).Cast<Match>().Select(match => match.Value)];
}
