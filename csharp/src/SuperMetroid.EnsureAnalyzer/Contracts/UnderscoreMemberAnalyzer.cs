using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Keeps underscore-prefixed state private (#1328). An underscore names an implementation detail of its type; another
/// type reaches that state through a named operation or a member named as part of the type's surface, never the field.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnderscoreMemberAnalyzer : DiagnosticAnalyzer
{
    /// <summary>An underscore-prefixed field or property that is not private.</summary>
    public const string UnderscoreMemberId = "SME6354";

    private static readonly DiagnosticDescriptor Rule = new(
        UnderscoreMemberId,
        "Underscore member is not private",
        "'{0}' is {1}; an underscore-prefixed member is private, so other types use a named operation or a member without the underscore",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Underscore-prefixed fields and properties are private implementation state (#1328).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.Field, SymbolKind.Property);
    }

    /// <summary>Reports a non-private field or property whose name starts with an underscore.</summary>
    private static void Analyze(SymbolAnalysisContext context)
    {
        ISymbol member = context.Symbol;
        if (member.IsImplicitlyDeclared || !member.Name.StartsWith("_", StringComparison.Ordinal) ||
            member.DeclaredAccessibility is Accessibility.Private or Accessibility.ProtectedAndInternal)
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations[0], member.Name,
            member.DeclaredAccessibility.ToString().ToLowerInvariant()));
    }
}
