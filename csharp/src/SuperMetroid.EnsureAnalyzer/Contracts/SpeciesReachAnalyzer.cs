using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Keeps species behind the enemy system's capability interfaces (#1315). Outside the enemy-owning Core modules, code
/// reaches the room's enemies through <c>RoomEnemySystem</c>'s capabilities (<c>Elevator</c>, <c>Ceres</c>,
/// <c>PlmSpawns</c>, <c>DisplaySources</c> and the rest), never through a <c>*Behavior</c> or <c>*Encounter</c> property.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpeciesReachAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A species property of the enemy system read outside the enemy-owning modules.</summary>
    public const string Id = "SME6407";

    /// <summary>The enemy system whose species properties are guarded.</summary>
    internal const string EnemySystemMetadataName = "SuperMetroid.Core.World.RoomEnemySystem";

    /// <summary>The Core modules that own species and may reach them directly.</summary>
    private static readonly ImmutableHashSet<string> OwningModules = ImmutableHashSet.Create(StringComparer.Ordinal, "Enemies", "Bosses");

    /// <summary>The path segment that precedes a Core file's module folder.</summary>
    private const string CoreDirectory = "/" + CoreLayerAnalyzer.CoreAssembly + "/";

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "Species reached outside the enemy domain",
        "'{0}' reaches a species from module '{1}'; use a capability interface of the enemy system",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The runtime, rooms, frontend and presentation read and command species through the enemy system's " +
            "capability interfaces, so a species' members stay its own (#1315).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly ||
                start.Compilation.GetTypeByMetadataName(EnemySystemMetadataName) is not { } enemySystem)
            {
                return;
            }
            start.RegisterOperationAction(operation => Analyze(operation, enemySystem), OperationKind.PropertyReference);
        });
    }

    /// <summary>Reports a species property read from a file outside the owning modules.</summary>
    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol enemySystem)
    {
        var reference = (IPropertyReferenceOperation)context.Operation;
        IPropertySymbol property = reference.Property;
        if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, enemySystem) || !IsSpecies(property.Type))
            return;
        string module = ModuleOf(reference.Syntax.SyntaxTree.FilePath);
        if (OwningModules.Contains(module))
            return;
        // The enemy system composes the species it holds; its own members build and wire them.
        if (SymbolEqualityComparer.Default.Equals(context.ContainingSymbol.ContainingType, enemySystem))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, reference.Syntax.GetLocation(), property.Name, module));
    }

    /// <summary>Whether the property's type is a species behavior or encounter.</summary>
    private static bool IsSpecies(ITypeSymbol type) =>
        type.TypeKind == TypeKind.Class &&
        (type.Name.EndsWith("Behavior", StringComparison.Ordinal) || type.Name.EndsWith("Encounter", StringComparison.Ordinal));

    /// <summary>The Core module folder that holds <paramref name="filePath"/>, or empty for a file at the project root.</summary>
    private static string ModuleOf(string filePath)
    {
        string path = filePath.Replace('\\', '/');
        int core = path.LastIndexOf(CoreDirectory, StringComparison.OrdinalIgnoreCase);
        if (core < 0)
            return string.Empty;
        string rest = path.Substring(core + CoreDirectory.Length);
        int slash = rest.IndexOf('/');
        return slash < 0 ? string.Empty : rest.Substring(0, slash);
    }
}
