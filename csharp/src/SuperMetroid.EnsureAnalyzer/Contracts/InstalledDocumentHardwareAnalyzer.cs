using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Installed content carries no hardware data (SME6463, #2564): in <c>SuperMetroid.AssetExtraction</c>, a JSON document
/// holds pictures, purpose-named colours written as <c>#rrggbb</c> and placements, never the cartridge's representation
/// of them. A document is a type the import hands to <c>System.Text.Json.JsonSerializer</c> or
/// <c>JsonAssetDocument.Read</c> to read or write, directly or through a generic helper of the same assembly whose type
/// argument reaches one, together with every type of the same assembly its serialized properties hold. Each such document
/// is reported where it is declared for each property, at any depth, that holds:
/// <list type="bullet">
/// <item>a colour as red, green and blue channel levels (<c>PaletteRgb5</c>, a gradient stop's levels), the cartridge's
/// five-bit colour encoding;</item>
/// <item>a palette row number, an integer named as a palette, and the priority of a tile or sprite that has one;</item>
/// <item>a palette index, an integer named as an ink;</item>
/// <item>a native sixteen-bit word, a <c>ushort</c>, as a packed colour, tile or attribute word is.</item>
/// </list>
/// A document only the import reads, which the extraction writes to a component's import data and the installer deletes
/// before it publishes, is no installed content: it is justified where it is declared by naming the import data that
/// holds it.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InstalledDocumentHardwareAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A JSON document holds the cartridge's representation of a colour, a tile or a sprite.</summary>
    public const string InstalledHardwareDataId = "SME6463";

    /// <summary>The import pipeline, which writes every installed document.</summary>
    public const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The serializer's methods that read or write a document of their type argument.</summary>
    private static readonly ImmutableHashSet<string> SerializerMethods =
    [
        "Serialize", "SerializeAsync", "SerializeToUtf8Bytes", "SerializeToDocument", "SerializeToElement", "SerializeToNode",
        "Deserialize", "DeserializeAsync", "DeserializeAsyncEnumerable",
    ];

    /// <summary>The import's own document reader, which validates and deserializes its type argument.</summary>
    private const string AssetDocumentReader = "JsonAssetDocument";

    /// <summary>The colour channels a colour held as levels names.</summary>
    private static readonly ImmutableArray<string> ChannelNames = ["Red", "Green", "Blue"];

    /// <summary>Splits an identifier into its camel-case words.</summary>
    private static readonly Regex CamelWords = new("[A-Z]+(?![a-z])|[A-Z]?[a-z]+|[0-9]+", RegexOptions.CultureInvariant);

    private static readonly DiagnosticDescriptor Rule = new(
        InstalledHardwareDataId,
        "A JSON document holds hardware data",
        "Document '{0}' holds {1} at '{2}'; an installed document holds pictures, purpose-named colours written as #rrggbb " +
            "and placements, so convert it, or justify a document only the import reads by naming the import data that holds it",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Installed content is what the game and its player work with (#2564): pre-coloured pictures, colours " +
            "named by what they colour and written as ordinary colours, and placements. A palette row, a tile's or sprite's " +
            "priority, a palette index, a native word or a colour in the cartridge's five-bit channels is the hardware's " +
            "representation, which stays in the extraction codec that decodes it once. Documents only the import reads, " +
            "deleted before the installer publishes, are justified at their declaration by naming the import data that holds them.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != ExtractionAssembly)
                return;
            IAssemblySymbol assembly = start.Compilation.Assembly;
            var documents = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);
            var helpers = new ConcurrentDictionary<IMethodSymbol, ConcurrentBag<int>>(SymbolEqualityComparer.Default);
            var helperCalls = new ConcurrentBag<IMethodSymbol>();
            start.RegisterOperationAction(operation =>
            {
                var invocation = (IInvocationOperation)operation.Operation;
                IMethodSymbol method = invocation.TargetMethod;
                if (IsSerializer(method))
                {
                    foreach (ITypeSymbol argument in method.TypeArguments)
                    {
                        if (argument is ITypeParameterSymbol { DeclaringMethod: { } declaring } parameter)
                            helpers.GetOrAdd(declaring.OriginalDefinition, _ => []).Add(parameter.Ordinal);
                        else
                            AddDocuments(argument, assembly, documents);
                    }
                }
                else if (method.IsGenericMethod && SymbolEqualityComparer.Default.Equals(method.ContainingAssembly, assembly))
                {
                    helperCalls.Add(method);
                }
            }, OperationKind.Invocation);
            start.RegisterCompilationEndAction(end =>
            {
                foreach (IMethodSymbol call in helperCalls)
                {
                    if (!helpers.TryGetValue(call.OriginalDefinition, out ConcurrentBag<int>? ordinals))
                        continue;
                    foreach (int ordinal in ordinals.Distinct())
                        AddDocuments(call.TypeArguments[ordinal], assembly, documents);
                }
                foreach (INamedTypeSymbol document in documents.Keys)
                {
                    if (document.Locations.FirstOrDefault(location => location.IsInSource) is not { } declared)
                        continue;
                    foreach (Finding finding in Findings(document, assembly))
                        end.ReportDiagnostic(Diagnostic.Create(Rule, declared, document.Name, finding.What, finding.Path));
                }
            });
        });
    }

    /// <summary>Whether <paramref name="method"/> reads or writes a JSON document of its type argument.</summary>
    private static bool IsSerializer(IMethodSymbol method) =>
        method.IsGenericMethod && method.ContainingType is { } type &&
        ((type.Name == "JsonSerializer" && type.ContainingNamespace?.ToDisplayString() == "System.Text.Json" && SerializerMethods.Contains(method.Name)) ||
         (type.Name == AssetDocumentReader && method.Name == "Read"));

    /// <summary>Adds each type of <paramref name="assembly"/> that <paramref name="type"/> is or holds as an element or type argument.</summary>
    private static void AddDocuments(ITypeSymbol type, IAssemblySymbol assembly, ConcurrentDictionary<INamedTypeSymbol, byte> documents)
    {
        foreach (INamedTypeSymbol held in Held(type))
        {
            if (SymbolEqualityComparer.Default.Equals(held.ContainingAssembly, assembly))
                documents.TryAdd(held.OriginalDefinition, 0);
        }
    }

    /// <summary>The named types <paramref name="type"/> is or holds through arrays, nullable values and type arguments.</summary>
    private static IEnumerable<INamedTypeSymbol> Held(ITypeSymbol type)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                foreach (INamedTypeSymbol element in Held(array.ElementType))
                    yield return element;
                break;
            case INamedTypeSymbol named:
                if (named.IsGenericType)
                {
                    foreach (ITypeSymbol argument in named.TypeArguments)
                    {
                        foreach (INamedTypeSymbol held in Held(argument))
                            yield return held;
                    }
                }
                if (named.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                    yield return named;
                break;
        }
    }

    /// <summary>
    /// What <paramref name="document"/> holds of the hardware's representation, each at the path of serialized properties that
    /// reaches it, walking every type of <paramref name="assembly"/> it holds once.
    /// </summary>
    private static List<Finding> Findings(INamedTypeSymbol document, IAssemblySymbol assembly)
    {
        var findings = new List<Finding>();
        var walked = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default) { document };
        var pending = new Queue<(INamedTypeSymbol Type, string Path)>();
        pending.Enqueue((document, ""));
        while (pending.Count > 0)
        {
            (INamedTypeSymbol type, string path) = pending.Dequeue();
            IPropertySymbol[] properties = [.. SerializedProperties(type)];
            bool hasPaletteRow = properties.Any(property => IsPaletteRow(property));
            foreach (IPropertySymbol property in properties)
            {
                string at = path.Length == 0 ? property.Name : path + "." + property.Name;
                if (What(property, hasPaletteRow, assembly) is { } what)
                    findings.Add(new Finding(what, at));
                foreach (INamedTypeSymbol held in Held(property.Type))
                {
                    if (SymbolEqualityComparer.Default.Equals(held.ContainingAssembly, assembly) && walked.Add(held.OriginalDefinition))
                        pending.Enqueue((held.OriginalDefinition, at));
                }
            }
        }
        return findings;
    }

    /// <summary>The hardware representation <paramref name="property"/> holds, or null.</summary>
    private static string? What(IPropertySymbol property, bool hasPaletteRow, IAssemblySymbol assembly)
    {
        if (Held(property.Type).Any(held => SymbolEqualityComparer.Default.Equals(held.ContainingAssembly, assembly) && IsChannelColour(held)))
            return "a colour as red, green and blue channel levels";
        if (IsPaletteRow(property))
            return "a palette row number";
        if (hasPaletteRow && Words(property.Name).Contains("Priority") && IsIntegerOrBool(property.Type))
            return "a tile's or sprite's priority";
        if (Words(property.Name).Any(word => word is "Ink" or "Inks") && IsInteger(property.Type))
            return "a palette index";
        if (IsWord(property.Type))
            return "a native sixteen-bit word";
        return null;
    }

    /// <summary>Whether <paramref name="property"/> holds a palette row: an integer named as a palette.</summary>
    private static bool IsPaletteRow(IPropertySymbol property) =>
        Words(property.Name).Any(word => word is "Palette" or "Palettes") && IsInteger(property.Type);

    /// <summary>Whether <paramref name="type"/> holds a colour as its three channel levels.</summary>
    private static bool IsChannelColour(INamedTypeSymbol type)
    {
        IPropertySymbol[] properties = [.. SerializedProperties(type)];
        return ChannelNames.All(channel => properties.Any(property => property.Name == channel && IsInteger(property.Type)));
    }

    /// <summary>The camel-case words of <paramref name="name"/>.</summary>
    private static IEnumerable<string> Words(string name) =>
        CamelWords.Matches(name).Cast<Match>().Select(match => match.Value);

    /// <summary>The properties the serializer reads and writes on <paramref name="type"/> and its bases: public instance properties with a getter that it does not always ignore.</summary>
    private static IEnumerable<IPropertySymbol> SerializedProperties(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            foreach (IPropertySymbol property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (!property.IsStatic && !property.IsIndexer && property.DeclaredAccessibility == Accessibility.Public &&
                    property.GetMethod is { DeclaredAccessibility: Accessibility.Public } && !AlwaysIgnored(property) &&
                    !(property.IsImplicitlyDeclared && property.Name == "EqualityContract"))
                {
                    yield return property;
                }
            }
        }
    }

    /// <summary>Whether <paramref name="property"/> carries <c>[JsonIgnore]</c> without a condition, or with <c>Always</c>.</summary>
    private static bool AlwaysIgnored(IPropertySymbol property) =>
        property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "JsonIgnoreAttribute" &&
            attribute.NamedArguments.All(argument => argument.Key != "Condition" || argument.Value.Value is JsonIgnoreAlways));

    /// <summary>The value of <c>JsonIgnoreCondition.Always</c>, the condition that ignores a property whatever it holds.</summary>
    private const int JsonIgnoreAlways = 1;

    /// <summary>Whether <paramref name="type"/> is an integer, or an array or nullable of integers.</summary>
    private static bool IsInteger(ITypeSymbol type) => Scalar(type) is
    {
        SpecialType: SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or
            SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64,
    };

    /// <summary>Whether <paramref name="type"/> is an integer or a flag, or an array or nullable of them.</summary>
    private static bool IsIntegerOrBool(ITypeSymbol type) => IsInteger(type) || Scalar(type).SpecialType == SpecialType.System_Boolean;

    /// <summary>Whether <paramref name="type"/> is a sixteen-bit word, or an array or nullable of them.</summary>
    private static bool IsWord(ITypeSymbol type) => Scalar(type).SpecialType == SpecialType.System_UInt16;

    /// <summary><paramref name="type"/> with arrays and nullable values removed.</summary>
    private static ITypeSymbol Scalar(ITypeSymbol type) => type switch
    {
        IArrayTypeSymbol array => Scalar(array.ElementType),
        INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable => Scalar(nullable.TypeArguments[0]),
        _ => type,
    };

    /// <summary>One hardware representation a document holds.</summary>
    /// <param name="What">What it holds.</param>
    /// <param name="Path">The serialized properties that reach it from the document.</param>
    private sealed record Finding(string What, string Path);
}
