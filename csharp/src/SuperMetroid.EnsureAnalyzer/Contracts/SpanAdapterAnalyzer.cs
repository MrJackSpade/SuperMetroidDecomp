using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #624: a parameterless <c>AsSpan()</c> before an array's one-argument <c>SequenceEqual</c> adapts what C# 14 already
/// binds to the span comparison. The asynchronous cleanup enforces this rule with its code fix (tools/cleanup/README.md),
/// so it has an analyzer of its own: the build skips an analyzer only when every rule it reports is off.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpanAdapterAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A parameterless <c>AsSpan()</c> whose only purpose is to reach an array's <c>SequenceEqual</c>.</summary>
    public const string SpanAdapterId = "SME6391";

    /// <summary>Reports a span adapter before an array comparison.</summary>
    private static readonly DiagnosticDescriptor SpanAdapterRule = new(
        SpanAdapterId,
        "Span adapter only to reach SequenceEqual",
        "'{0}.AsSpan()' only adapts the array for SequenceEqual; compare the array directly",
        "Collections",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An array binds to the span SequenceEqual directly; the parameterless AsSpan() adds nothing (#624).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [SpanAdapterRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    /// <summary>Reports a parameterless AsSpan() on an array whose only use is a one-argument SequenceEqual.</summary>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var adapter = (InvocationExpressionSyntax)context.Node;
        if (adapter is not { Expression: MemberAccessExpressionSyntax access, ArgumentList.Arguments.Count: 0 } ||
            access.Name.Identifier.Text != "AsSpan" ||
            adapter.Parent is not MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax next } nextAccess ||
            nextAccess.Expression != adapter ||
            nextAccess.Name.Identifier.Text != "SequenceEqual" || next.ArgumentList.Arguments.Count != 1)
            return;
        SemanticModel model = context.SemanticModel;
        if (model.GetSymbolInfo(adapter, context.CancellationToken).Symbol is not IMethodSymbol ||
            model.GetSymbolInfo(next, context.CancellationToken).Symbol is not IMethodSymbol ||
            model.GetTypeInfo(access.Expression, context.CancellationToken).Type is not IArrayTypeSymbol)
            return;
        context.ReportDiagnostic(Diagnostic.Create(SpanAdapterRule, access.Name.GetLocation(),
            AdapterChainAnalyzer.Shorten(access.Expression.ToString())));
    }
}
