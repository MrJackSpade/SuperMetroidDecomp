using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Ratchets CPU-bus threading (#1320). SME6290 flags raw bus member use; this rule flags taking, storing or exposing a
/// bus at all: a parameter, field or property (record positional parameters included) whose type is a
/// <c>[CpuBusAccess]</c> interface, declared outside a <c>[CpuBusBoundary]</c>. A file's reviewed count in
/// <c>cpu-bus-declaration-baseline.txt</c> may only fall; the baseline activates the rule.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CpuBusDeclarationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A CPU-bus-typed declaration outside an approved boundary.</summary>
    public const string DeclarationId = "SME6292";

    /// <summary>A reviewed count that the file no longer reaches.</summary>
    public const string StaleCountId = "SME6293";

    /// <summary>The baseline file that activates the rule.</summary>
    internal const string BaselineFileName = "cpu-bus-declaration-baseline.txt";

    private static readonly DiagnosticDescriptor DeclarationRule = new(
        DeclarationId,
        "CPU-bus declaration outside a memory boundary",
        "{0}; pass the owned state or narrow service the code uses, or declare the import in a [CpuBusBoundary]",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Gameplay code receives the owned state it reads. Only import and codec boundaries, address-space " +
            "implementations and exact bus tests take, store or expose a CPU-bus interface (#1320).");

    private static readonly DiagnosticDescriptor StaleCountRule = new(
        StaleCountId,
        "CPU-bus declaration baseline is stale",
        "{0} now holds {1} CPU-bus declaration(s) but the baseline allows {2}; lower the baseline so the ratchet keeps the gain",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DeclarationRule, StaleCountRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            AdditionalText? file = start.Options.AdditionalFiles.FirstOrDefault(additional =>
                string.Equals(Path.GetFileName(additional.Path), BaselineFileName, StringComparison.OrdinalIgnoreCase));
            if (file is null)
                return;
            SourceText content = file.GetText(start.CancellationToken)
                ?? throw new InvalidOperationException($"{file.Path}: the CPU-bus declaration baseline could not be read.");
            CartridgeEncodingBaseline baseline = CartridgeEncodingBaseline.Parse(file.Path, content.ToString());
            start.RegisterSemanticModelAction(model => AnalyzeTree(model, baseline));
        });
    }

    /// <summary>Counts and reports the tree's CPU-bus declarations against its reviewed baseline.</summary>
    private static void AnalyzeTree(SemanticModelAnalysisContext context, CartridgeEncodingBaseline baseline)
    {
        SyntaxTree tree = context.SemanticModel.SyntaxTree;
        if (CartridgeEncodingScanner.IsBuildGenerated(tree.FilePath))
            return;
        string? path = CartridgeEncodingScanner.RepositoryRelative(baseline.RepositoryRoot, tree.FilePath);
        if (path is null)
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

        int allowed = baseline.Allowed(path, DeclarationId);
        if (allowed == 0)
        {
            foreach ((Location location, string what) in found)
                context.ReportDiagnostic(Diagnostic.Create(DeclarationRule, location, what));
        }
        else if (found.Count > allowed)
        {
            context.ReportDiagnostic(Diagnostic.Create(DeclarationRule, found[0].Location,
                $"{path} holds {found.Count} CPU-bus declarations but its reviewed baseline allows {allowed}"));
        }
        else if (found.Count < allowed)
        {
            context.ReportDiagnostic(Diagnostic.Create(StaleCountRule, Location.Create(tree, new TextSpan(0, 0)), path, found.Count, allowed));
        }
    }

    /// <summary>Whether <paramref name="type"/> is an interface marked <c>[CpuBusAccess]</c>.</summary>
    private static bool IsBusInterface(ITypeSymbol type) =>
        type is INamedTypeSymbol { TypeKind: TypeKind.Interface } named &&
        LayoutBoundaries.HasAttribute(named.OriginalDefinition, CpuBusAccessAnalyzer.AccessAttributeName);
}
