using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Ratchets CPU-bus threading (#1320). SME6290 flags raw bus member use; this rule flags taking, storing or exposing a
/// bus at all: a parameter, field or property (record positional parameters included) whose type is a
/// <c>[CpuBusAccess]</c> interface, declared outside a <c>[CpuBusBoundary]</c>. Every declaration in Core is reported
/// where it is.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CpuBusDeclarationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A CPU-bus-typed declaration outside an approved boundary.</summary>
    public const string DeclarationId = "SME6292";

    private static readonly DiagnosticDescriptor DeclarationRule = new(
        DeclarationId,
        "CPU-bus declaration outside a memory boundary",
        "{0}; pass the owned state or narrow service the code uses, or declare the import in a [CpuBusBoundary]",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Gameplay code receives the owned state it reads. Only import and codec boundaries, address-space " +
            "implementations and exact bus tests take, store or expose a CPU-bus interface (#1320).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DeclarationRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            // Core owns gameplay; the rule scopes to it by assembly name, identically in the build and the IDE.
            if (start.Compilation.AssemblyName != "SuperMetroid.Core")
                return;
            start.RegisterSemanticModelAction(AnalyzeTree);
        });
    }

    /// <summary>Reports every CPU-bus declaration in the tree outside a boundary.</summary>
    private static void AnalyzeTree(SemanticModelAnalysisContext context)
    {
        SyntaxTree tree = context.SemanticModel.SyntaxTree;
        if (CartridgeEncodingScanner.IsBuildGenerated(tree.FilePath))
            return;

        var found = new List<(Location Location, string What)>();
        foreach (SyntaxNode node in tree.GetRoot(context.CancellationToken).DescendantNodes())
        {
            (ISymbol? symbol, ITypeSymbol? type, string kind) = node switch
            {
                ParameterSyntax parameter when context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken) is IParameterSymbol p =>
                    (p, p.Type, "parameter"),
                VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax } declarator when
                    context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken) is IFieldSymbol f => (f, f.Type, "field"),
                PropertyDeclarationSyntax property when context.SemanticModel.GetDeclaredSymbol(property, context.CancellationToken) is IPropertySymbol p =>
                    (p, p.Type, "property"),
                _ => ((ISymbol?)null, (ITypeSymbol?)null, ""),
            };
            if (symbol is null || type is null || !IsBusInterface(type) ||
                LayoutBoundaries.InBoundary(symbol, CpuBusAccessAnalyzer.BoundaryAttributeName, CpuBusAccessAnalyzer.AccessAttributeName))
                continue;
            found.Add((symbol.Locations[0], $"{kind} '{symbol.Name}' is typed as CPU-bus interface '{type.WithNullableAnnotation(NullableAnnotation.None).Name}'"));
        }

        foreach ((Location location, string what) in found)
            context.ReportDiagnostic(Diagnostic.Create(DeclarationRule, location, what));
    }

    /// <summary>Whether <paramref name="type"/> is an interface marked <c>[CpuBusAccess]</c>.</summary>
    private static bool IsBusInterface(ITypeSymbol type) =>
        type is INamedTypeSymbol { TypeKind: TypeKind.Interface } named &&
        LayoutBoundaries.HasAttribute(named.OriginalDefinition, CpuBusAccessAnalyzer.AccessAttributeName);
}
