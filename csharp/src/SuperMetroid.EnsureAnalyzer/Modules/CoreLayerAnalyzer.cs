using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Modules;

/// <summary>
/// #626 Core layering (SME6404): inside the production Core assembly, code in a module may reference types of its
/// own layer or a lower one (Foundation, Simulation, Presentation, Session, from <c>csharp/module-map.json</c>), so
/// hardware and shared infrastructure never reach into gameplay and the simulation never reaches into the frame loop
/// or the frontend. Tooling and verification mirrors of Core are not production code and are not layered, and
/// documentation crefs are descriptions rather than dependencies.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CoreLayerAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core reference to a module of a higher layer.</summary>
    public const string Id = "SME6404";

    /// <summary>A Core module that the module map assigns to no layer.</summary>
    public const string UnlayeredId = "SME6406";

    /// <summary>The production assembly the layering applies to.</summary>
    public const string CoreAssembly = ModuleMap.CoreNamespace;

    private static readonly DiagnosticDescriptor Rule = new(
        Id, "Core module depends only on its own or a lower layer",
        "'{0}' ({1} module {2}) is referenced from {3} module {4}; a Core module may depend only on its own or a lower layer",
        "Architecture", DiagnosticSeverity.Error, isEnabledByDefault: true,
        description: "Core modules form layers; a lower layer never references a higher one (#626).");

    private static readonly DiagnosticDescriptor UnlayeredRule = new(
        UnlayeredId, "Core module has a layer",
        "Core module '{0}' is not assigned a layer in csharp/module-map.json",
        "Architecture", DiagnosticSeverity.Error, isEnabledByDefault: true,
        description: "Every top-level Core module belongs to one layer, so its dependencies are checked (#626).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, UnlayeredRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreAssembly || ModuleMap.From(start.Options, start.CancellationToken) is not { } map)
                return;
            start.RegisterSyntaxNodeAction(node => Analyze(node, map), SyntaxKind.IdentifierName, SyntaxKind.GenericName);
            start.RegisterSyntaxNodeAction(node => AnalyzeNamespace(node, map), SyntaxKind.NamespaceDeclaration, SyntaxKind.FileScopedNamespaceDeclaration);
        });
    }

    private static void Analyze(SyntaxNodeAnalysisContext context, ModuleMap map)
    {
        var name = (SimpleNameSyntax)context.Node;
        // Documentation references (crefs) describe collaborators without depending on them.
        if (name.IsPartOfStructuredTrivia() || name.Parent is BaseNamespaceDeclarationSyntax || name.FirstAncestorOrSelf<UsingDirectiveSyntax>() is not null ||
            name.Parent is QualifiedNameSyntax { Parent: BaseNamespaceDeclarationSyntax })
            return;
        string? source = context.ContainingSymbol?.ContainingNamespace?.ToDisplayString() ??
            name.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>()?.Name.ToString();
        int? sourceLayer = map.CoreLayerOf(source);
        if (sourceLayer is null)
            return;
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol;
        INamedTypeSymbol? type = symbol switch
        {
            INamedTypeSymbol named => named,
            IMethodSymbol { MethodKind: MethodKind.Constructor } constructor => constructor.ContainingType,
            IMethodSymbol { ReducedFrom: not null } extension => extension.ContainingType,
            IMethodSymbol { IsExtensionMethod: true } extension => extension.ContainingType,
            _ => symbol?.ContainingType is { IsExtension: true } block ? block.ContainingType : null,
        };
        if (type is null || !SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, context.Compilation.Assembly))
            return;
        string? target = type.ContainingNamespace?.ToDisplayString();
        int? targetLayer = map.CoreLayerOf(target);
        if (targetLayer is null || targetLayer <= sourceLayer)
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, name.GetLocation(), type.Name,
            map.CoreLayerNames[targetLayer.Value], ModuleName(target), map.CoreLayerNames[sourceLayer.Value], ModuleName(source)));
    }

    private static void AnalyzeNamespace(SyntaxNodeAnalysisContext context, ModuleMap map)
    {
        var declaration = (BaseNamespaceDeclarationSyntax)context.Node;
        string ns = declaration.Name.ToString();
        if (ns.StartsWith(CoreAssembly + ".", StringComparison.Ordinal) && map.CoreLayerOf(ns) is null)
            context.ReportDiagnostic(Diagnostic.Create(UnlayeredRule, declaration.Name.GetLocation(), ModuleName(ns)));
    }

    private static string ModuleName(string? ns)
    {
        string[] parts = (ns ?? "").Split('.');
        return parts.Length >= 3 ? parts[2] : "(Core root)";
    }
}
