using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Keeps enemy state out of generic slot words (#1325). The common enemy slot declares no multipurpose words
/// (<c>VariableA</c>-<c>F</c>, <c>Parameter1</c>/<c>2</c> or others named that way): each family owns its state as typed
/// fields, and the authored spawn parameters stay in the slot's spawn record.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnemySlotWordAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A multipurpose word declared on the common enemy slot.</summary>
    public const string Id = "SME6408";

    /// <summary>The common enemy slot whose members are guarded.</summary>
    internal const string EnemySlotMetadataName = "SuperMetroid.Core.Enemies.RoomEnemySlot";

    /// <summary>The names the cartridge's multipurpose enemy words take: a lettered variable or a numbered parameter.</summary>
    private static readonly Regex MultipurposeWordName = new("^(Variable[A-Z]|Parameter[0-9]+)$", RegexOptions.CultureInvariant);

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "Multipurpose enemy slot word",
        "'{0}' is a multipurpose word on the common enemy slot; keep it as typed state of the family that owns it",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Enemy families own their state as typed fields; the common slot holds no words whose meaning " +
            "depends on the occupant, and authored spawn parameters are read from the spawn record (#1325).");

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
                start.Compilation.GetTypeByMetadataName(EnemySlotMetadataName) is not { } enemySlot)
            {
                return;
            }
            start.RegisterSymbolAction(symbol => Analyze(symbol, enemySlot), SymbolKind.Field, SymbolKind.Property);
        });
    }

    /// <summary>Reports a field or property of the enemy slot named as a multipurpose word.</summary>
    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol enemySlot)
    {
        ISymbol member = context.Symbol;
        if (member.IsImplicitlyDeclared || !SymbolEqualityComparer.Default.Equals(member.ContainingType, enemySlot) ||
            !MultipurposeWordName.IsMatch(member.Name))
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations[0], member.Name));
    }
}
