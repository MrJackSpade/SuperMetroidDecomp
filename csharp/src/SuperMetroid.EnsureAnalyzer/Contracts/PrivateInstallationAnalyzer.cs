using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A private installation in verification states its reason (SME6455, #1487): in a verification assembly, a call to
/// <c>GameAssetInstaller.Install</c>, <c>EnsureInstalled</c> or <c>OpenOrRepair</c>, or to a static <c>Extract</c> of an
/// import-pipeline component (a type in a <c>SuperMetroid.AssetExtraction</c> namespace, such as
/// <c>EnemyTileArtworkFiles.Extract</c>), builds an installation of its own from the ROM. Verification reads the shared
/// <c>RepositoryInstallation.Installation</c>, or writes into a <c>RepositoryInstallation.CopyTo</c> copy; a member that
/// must install anyway carries <c>[PrivateInstallation("reason")]</c> with a non-empty reason. The call's owner is the
/// innermost method, constructor or property around it; lambdas and local functions belong to the member that contains
/// them. One diagnostic per call or method reference; there is no allowance list.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrivateInstallationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A verification member builds an installation of its own without stating why.</summary>
    public const string PrivateInstallationId = "SME6455";

    /// <summary>The suffix every verification assembly's name ends with.</summary>
    private const string VerificationSuffix = "Verification";

    /// <summary>The import pipeline's root namespace, whose component extractors build installation files.</summary>
    private const string ExtractionNamespace = "SuperMetroid.AssetExtraction";

    /// <summary>The installer type whose entry points build a whole installation.</summary>
    private const string InstallerType = "GameAssetInstaller";

    /// <summary>The name of a component extractor's entry point.</summary>
    private const string ExtractMethod = "Extract";

    /// <summary>The stating attribute's type name.</summary>
    private const string AttributeName = "PrivateInstallationAttribute";

    /// <summary>The installer's entry points that create, open-or-repair, or reinstall an installation.</summary>
    private static readonly ImmutableHashSet<string> InstallerMethods = ["Install", "EnsureInstalled", "OpenOrRepair"];

    private static readonly DiagnosticDescriptor Rule = new(
        PrivateInstallationId,
        "Verification builds an installation of its own",
        "'{0}' builds an installation of its own; read RepositoryInstallation.Installation, write into RepositoryInstallation.CopyTo, or state why with [PrivateInstallation(\"reason\")]",
        "Performance",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A full install from the ROM takes about 140 s and allocates over 30 GB, and a component extraction costs " +
            "seconds and gigabytes of its own. Verification reads the shared repository installation, or writes into a copy of " +
            "it; a member that must install anyway, because it writes into the installation or exercises the installer, states " +
            "why with a non-empty [PrivateInstallation] reason (#1487).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName?.EndsWith(VerificationSuffix, StringComparison.Ordinal) != true)
                return;
            start.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            start.RegisterOperationAction(AnalyzeMethodReference, OperationKind.MethodReference);
        });
    }

    /// <summary>Reports a call that builds an installation from a member that states no reason.</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context) =>
        Analyze(context, ((IInvocationOperation)context.Operation).TargetMethod);

    /// <summary>Reports a method group that builds an installation, passed on from a member that states no reason.</summary>
    private static void AnalyzeMethodReference(OperationAnalysisContext context) =>
        Analyze(context, ((IMethodReferenceOperation)context.Operation).Method);

    /// <summary>Reports <paramref name="target"/> when it builds an installation and its owner states no reason.</summary>
    private static void Analyze(OperationAnalysisContext context, IMethodSymbol target)
    {
        IMethodSymbol method = (target.ReducedFrom ?? target).OriginalDefinition;
        if (!BuildsInstallation(method) || StatesReason(Owner(context.ContainingSymbol)))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(),
            $"{method.ContainingType.Name}.{method.Name}"));
    }

    /// <summary>
    /// Whether <paramref name="method"/> is one of the installer's entry points, or a static <c>Extract</c> of a type in
    /// the import pipeline's namespaces.
    /// </summary>
    private static bool BuildsInstallation(IMethodSymbol method)
    {
        if (method.ContainingType is not { } type || !InExtractionNamespace(type.ContainingNamespace))
            return false;
        return (type.Name == InstallerType && InstallerMethods.Contains(method.Name)) ||
            (method.IsStatic && method.Name == ExtractMethod);
    }

    /// <summary>Whether <paramref name="space"/> is <c>SuperMetroid.AssetExtraction</c> or a namespace within it.</summary>
    private static bool InExtractionNamespace(INamespaceSymbol? space)
    {
        if (space is null || space.IsGlobalNamespace)
            return false;
        string name = space.ToDisplayString();
        return name == ExtractionNamespace || name.StartsWith(ExtractionNamespace + ".", StringComparison.Ordinal);
    }

    /// <summary>
    /// The member a call belongs to: lambdas and local functions give way to the member around them, and a property
    /// accessor to its property.
    /// </summary>
    private static ISymbol Owner(ISymbol symbol)
    {
        ISymbol owner = symbol;
        while (owner is IMethodSymbol { MethodKind: MethodKind.AnonymousFunction or MethodKind.LocalFunction } nested &&
               nested.ContainingSymbol is { } outer)
            owner = outer;
        return owner is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } ? property : owner;
    }

    /// <summary>Whether <paramref name="owner"/> carries <c>[PrivateInstallation]</c> with a non-empty reason.</summary>
    private static bool StatesReason(ISymbol owner) =>
        owner.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.Name == AttributeName &&
            attribute.ConstructorArguments.Length == 1 &&
            attribute.ConstructorArguments[0].Value is string reason &&
            !string.IsNullOrWhiteSpace(reason));
}
