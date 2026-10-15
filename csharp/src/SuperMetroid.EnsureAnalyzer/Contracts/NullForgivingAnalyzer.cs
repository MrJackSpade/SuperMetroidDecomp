using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #623: the postfix null-forgiving operator (<c>expression!</c>) switches nullable analysis off without a runtime
/// check, so a wrong assumption surfaces later as an opaque <see cref="System.NullReferenceException"/>. Repository
/// code expresses non-nullness through honest contracts instead: non-null return types, checked required-value
/// operations, constructor initialization or explicit lifecycle state. There is no allowlist or baseline; generated
/// repository code is analyzed too, so a generator that emits the operator is fixed at its template.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullForgivingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A postfix nullable-suppression expression.</summary>
    public const string Id = "SME6380";

    /// <summary>Reports each postfix nullable suppression at its operator.</summary>
    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "Null-forgiving operator",
        "'{0}!' suppresses nullable analysis; express the contract instead (non-null type, checked required-value operation, initialization or explicit state)",
        "Nullability",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The null-forgiving operator hides whether a value can be null and defers the failure (#623).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Report, SyntaxKind.SuppressNullableWarningExpression);
    }

    /// <summary>Reports the suppressed operand at the operator token.</summary>
    private static void Report(SyntaxNodeAnalysisContext context)
    {
        var suppression = (Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax)context.Node;
        string operand = suppression.Operand.ToString();
        if (operand.Length > 60)
            operand = operand.Substring(0, 57) + "...";
        context.ReportDiagnostic(Diagnostic.Create(Rule, suppression.OperatorToken.GetLocation(), operand));
    }
}
