using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Keeps PPU memory out of frontend state (#1358): a frontend screen publishes its tilemaps, palette and sprite entries,
/// and a capture codec composes the memory image, so no class in a frontend namespace may hold VRAM, CGRAM or OAM in a
/// field, property or primary-constructor parameter. Transient rendering views are structs and pass; a codec's local
/// scratch memory is not state and passes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FrontendPpuMemoryAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Frontend state holding PPU memory.</summary>
    public const string OwnedMemoryId = "SME6302";

    /// <summary>The namespace whose classes may not hold PPU memory.</summary>
    private const string FrontendNamespace = "SuperMetroid.Core.Frontend";

    /// <summary>The namespace declaring the PPU memory types.</summary>
    private const string HardwareNamespace = "SuperMetroid.Core.Hardware";

    /// <summary>The PPU memory types a frontend may not hold.</summary>
    private static readonly ImmutableHashSet<string> MemoryTypes = ["SnesVram", "SnesCgram", "OamBuffer"];

    /// <summary>Warns when a frontend class holds PPU memory.</summary>
    private static readonly DiagnosticDescriptor OwnedMemoryRule = new(
        OwnedMemoryId,
        "Frontend state holds PPU memory",
        "{0} holds {1} in '{2}'; publish tilemaps, a ScreenPalette and ScreenSprites, and let a capture codec compose the image",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Frontend screens publish what is on screen; only capture codecs build PPU memory, as local scratch.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [OwnedMemoryRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMember, SymbolKind.Field, SymbolKind.Property);
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    /// <summary>Reports a frontend class field or property typed as PPU memory.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol member = context.Symbol;
        if (member.IsImplicitlyDeclared || member.IsStatic || !IsFrontendClass(member.ContainingType))
            return;
        ITypeSymbol type = member switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => throw new InvalidOperationException($"Unexpected symbol kind {member.Kind}."),
        };
        if (IsMemoryType(type))
            context.ReportDiagnostic(Diagnostic.Create(OwnedMemoryRule, member.Locations[0], member.ContainingType.Name, type.Name, member.Name));
    }

    /// <summary>Reports a frontend class primary-constructor parameter typed as PPU memory, which the class captures.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!IsFrontendClass(type))
            return;
        foreach (IMethodSymbol constructor in type.InstanceConstructors)
        {
            if (!constructor.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax(context.CancellationToken) is Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax))
                continue;
            foreach (IParameterSymbol parameter in constructor.Parameters)
            {
                if (IsMemoryType(parameter.Type))
                    context.ReportDiagnostic(Diagnostic.Create(OwnedMemoryRule, parameter.Locations[0], type.Name, parameter.Type.Name, parameter.Name));
            }
        }
    }

    /// <summary>Whether <paramref name="type"/> is a class declared in a frontend namespace.</summary>
    private static bool IsFrontendClass(INamedTypeSymbol? type) =>
        type is { TypeKind: TypeKind.Class } && IsWithin(type.ContainingNamespace, FrontendNamespace);

    /// <summary>Whether <paramref name="type"/> is one of the PPU memory types.</summary>
    private static bool IsMemoryType(ITypeSymbol type) =>
        type is INamedTypeSymbol named && MemoryTypes.Contains(named.Name) &&
        named.ContainingNamespace.ToDisplayString() == HardwareNamespace;

    /// <summary>Whether <paramref name="space"/> is <paramref name="root"/> or nested inside it.</summary>
    private static bool IsWithin(INamespaceSymbol? space, string root)
    {
        string name = space?.ToDisplayString() ?? string.Empty;
        return name == root || name.StartsWith(root + ".", StringComparison.Ordinal);
    }
}
