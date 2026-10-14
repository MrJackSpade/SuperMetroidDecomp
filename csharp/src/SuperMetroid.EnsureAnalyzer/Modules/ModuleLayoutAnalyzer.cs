using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Modules;

/// <summary>
/// #626 module layout: every compiled source file lies in a module registered in <c>csharp/module-map.json</c>
/// (SME6403), a file declaring a type uses the namespace its module path implies (SME6402), and a verification file
/// sits in the module of the domain it verifies or a declared shared module (SME6405). Generated output under
/// <c>obj</c> or <c>bin</c> is not source and is not checked.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ModuleLayoutAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A namespace that differs from the one the module path implies.</summary>
    public const string NamespaceId = "SME6402";

    /// <summary>A compiled source file outside every registered module.</summary>
    public const string UnregisteredId = "SME6403";

    /// <summary>A verification file outside the domain modules and declared shared modules.</summary>
    public const string DomainId = "SME6405";

    private static readonly DiagnosticDescriptor NamespaceRule = new(
        NamespaceId, "Namespace matches the module path",
        "'{0}' declares namespace '{1}'; its path in module '{2}' implies '{3}'",
        "Architecture", DiagnosticSeverity.Error, isEnabledByDefault: true,
        description: "A file's namespace is its module's namespace followed by its subdirectories (#626).");

    private static readonly DiagnosticDescriptor UnregisteredRule = new(
        UnregisteredId, "Source file in a registered module",
        "'{0}' is not inside a module registered in csharp/module-map.json; place it in its module or register the module",
        "Architecture", DiagnosticSeverity.Error, isEnabledByDefault: true,
        description: "Every maintained source file belongs to a registered module (#626).");

    private static readonly DiagnosticDescriptor DomainRule = new(
        DomainId, "Verification file in a domain module",
        "'{0}' is in '{1}', which is not a {2} module or a declared shared module ({3}); place it with the domain it exercises",
        "Architecture", DiagnosticSeverity.Error, isEnabledByDefault: true,
        description: "Verification code lives in the module of the domain it exercises (#626).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [NamespaceRule, UnregisteredRule, DomainRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            ModuleMap? map = ModuleMap.From(start.Options, start.CancellationToken);
            if (map is null)
                return;
            start.RegisterSyntaxTreeAction(tree => Analyze(tree, map));
        });
    }

    private static void Analyze(SyntaxTreeAnalysisContext context, ModuleMap map)
    {
        string? relative = map.Relative(context.Tree.FilePath);
        if (relative is null || relative.Contains("/obj/") || relative.Contains("/bin/") || context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
            return;
        ModuleMap.Module? module = map.ModuleOf(relative);
        if (module is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(UnregisteredRule, Location.Create(context.Tree, default), relative));
            return;
        }
        if (module.Role == "fixture")
            return;
        ImmutableArray<MemberDeclarationSyntax> types = OneTypePerFileAnalyzer.TopLevelTypes(root);
        if (types.Length == 0)
            return;
        BaseNamespaceDeclarationSyntax? declared = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        string actual = declared?.Name.ToString() ?? "";
        string expected = ModuleMap.ExpectedNamespace(module, relative);
        Location location = declared?.Name.GetLocation() ?? IdentifierLocation(types[0]);
        string name = OneTypePerFileAnalyzer.NameOf(types[0]);
        if (actual != expected)
        {
            context.ReportDiagnostic(Diagnostic.Create(NamespaceRule, location, name,
                actual.Length == 0 ? "(global)" : actual, module.Path, expected.Length == 0 ? "(global)" : expected));
        }
        if (module.DomainOf is { } domain)
        {
            string[] directories = ModuleMap.Subdirectories(module, relative);
            string top = directories.Length == 0 ? "" : directories[0];
            if (!map.CoreLayers.ContainsKey(top) && !module.SharedModules.Contains(top))
            {
                context.ReportDiagnostic(Diagnostic.Create(DomainRule, IdentifierLocation(types[0]), name,
                    directories.Length == 0 ? module.Path : module.Path + "/" + top, domain, string.Join(", ", module.SharedModules)));
            }
        }
    }

    private static Location IdentifierLocation(MemberDeclarationSyntax declaration) => declaration switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier.GetLocation(),
        DelegateDeclarationSyntax @delegate => @delegate.Identifier.GetLocation(),
        _ => declaration.GetLocation(),
    };
}
