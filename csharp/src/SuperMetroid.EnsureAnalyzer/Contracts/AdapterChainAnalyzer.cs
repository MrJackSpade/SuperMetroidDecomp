using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #624: conversions whose only purpose is to reach another operation. A parameterless <c>ToArray()</c> that feeds a
/// LINQ operator copies a sequence the operator could read directly, and a span's <c>ToArray()</c> before a LINQ
/// operator copies what a <c>ReadOnlySpanQueries</c> query reads in place. A parameterless <c>AsSpan()</c> before an
/// array's <c>SequenceEqual</c> adapts what C# 14 already binds to the span comparison. A <c>ToArray()</c> stored,
/// passed, sliced or compared through another overload keeps an ownership purpose and is not reported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AdapterChainAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A materialization whose only purpose is to reach the operator that follows it.</summary>
    public const string MaterializationId = "SME6390";

    /// <summary>A parameterless <c>AsSpan()</c> whose only purpose is to reach an array's <c>SequenceEqual</c>.</summary>
    public const string SpanAdapterId = "SME6391";

    /// <summary>The span-query type whose members replace a span's <c>ToArray()</c> bridge.</summary>
    public const string SpanQueriesMetadataName = "SuperMetroid.Core.ReadOnlySpanQueries";

    /// <summary>Reports a copy made only to reach the next operator.</summary>
    private static readonly DiagnosticDescriptor MaterializationRule = new(
        MaterializationId,
        "Materialization only to reach an operator",
        "'{0}.ToArray()' only feeds '{1}'; {2}",
        "Collections",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Call the operator on the source, or the named span query that reads it in place (#624).");

    /// <summary>Reports a span adapter before an array comparison.</summary>
    private static readonly DiagnosticDescriptor SpanAdapterRule = new(
        SpanAdapterId,
        "Span adapter only to reach SequenceEqual",
        "'{0}.AsSpan()' only adapts the array for SequenceEqual; compare the array directly",
        "Collections",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An array binds to the span SequenceEqual directly; the parameterless AsSpan() adds nothing (#624).");

    /// <summary>Span queries by the LINQ operator name they replace.</summary>
    private static readonly ImmutableDictionary<string, string> SpanQueryFor = new Dictionary<string, string>
    {
        ["Any"] = "Any", ["All"] = "All", ["Count"] = "Count", ["First"] = "First", ["Single"] = "Single", ["Sum"] = "Sum",
        ["Select"] = "SelectToArray", ["Where"] = "WhereToArray", ["ToDictionary"] = "ToDictionary", ["Concat"] = "ConcatToArray",
    }.ToImmutableDictionary();

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MaterializationRule, SpanAdapterRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
    }

    /// <summary>Classifies one parameterless ToArray()/AsSpan() call by the operator it feeds.</summary>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var adapter = (InvocationExpressionSyntax)context.Node;
        if (adapter is not { Expression: MemberAccessExpressionSyntax access, ArgumentList.Arguments.Count: 0 } ||
            access.Name.Identifier.Text is not ("ToArray" or "AsSpan") ||
            adapter.Parent is not MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax next } nextAccess ||
            nextAccess.Expression != adapter)
            return;
        SemanticModel model = context.SemanticModel;
        if (model.GetSymbolInfo(adapter, context.CancellationToken).Symbol is not IMethodSymbol adapterMethod ||
            model.GetSymbolInfo(next, context.CancellationToken).Symbol is not IMethodSymbol nextMethod)
            return;
        string receiver = Shorten(access.Expression.ToString());
        string operatorName = nextAccess.Name.Identifier.Text;
        if (access.Name.Identifier.Text == "AsSpan")
        {
            if (operatorName == "SequenceEqual" && next.ArgumentList.Arguments.Count == 1 &&
                model.GetTypeInfo(access.Expression, context.CancellationToken).Type is IArrayTypeSymbol)
            {
                context.ReportDiagnostic(Diagnostic.Create(SpanAdapterRule, access.Name.GetLocation(), receiver));
            }
            return;
        }
        if (!IsEnumerable(nextMethod) && !IsSpanQuery(nextMethod))
            return;
        string adapterOwner = (adapterMethod.ReducedFrom ?? adapterMethod).ContainingType.ToDisplayString();
        if (adapterOwner == "System.Linq.Enumerable" ||
            IsSequence(model.GetTypeInfo(access.Expression, context.CancellationToken).Type))
        {
            context.ReportDiagnostic(Diagnostic.Create(MaterializationRule, access.Name.GetLocation(), receiver, operatorName,
                "call it on the source sequence"));
            return;
        }
        // A span or memory copied to an array only for LINQ: report only where a span query takes its place.
        string? receiverType = (adapterMethod.ContainingType.OriginalDefinition).ToDisplayString();
        bool span = receiverType is "System.ReadOnlySpan<T>" or "System.Span<T>";
        bool memory = receiverType is "System.ReadOnlyMemory<T>" or "System.Memory<T>";
        if ((span || memory) && SpanQueryFor.TryGetValue(operatorName, out string? query) &&
            context.Compilation.GetTypeByMetadataName(SpanQueriesMetadataName) is { } queries &&
            HasQuery(queries, query))
        {
            context.ReportDiagnostic(Diagnostic.Create(MaterializationRule, access.Name.GetLocation(), receiver, operatorName,
                $"use the span query '{(memory ? ".Span." : "")}{query}'"));
        }
    }

    /// <summary>Whether <paramref name="method"/> is a <c>System.Linq.Enumerable</c> operator.</summary>
    private static bool IsEnumerable(IMethodSymbol method) =>
        (method.ReducedFrom ?? method).ContainingType.ToDisplayString() == "System.Linq.Enumerable";

    /// <summary>Whether <paramref name="type"/> is an enumerable collection that LINQ reads directly.</summary>
    public static bool IsSequence(ITypeSymbol? type) =>
        type is not null && (type.ToDisplayString() is var name && !name.StartsWith("System.IO.", StringComparison.Ordinal)) &&
        (type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
         type.AllInterfaces.Any(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T));

    /// <summary>Whether <paramref name="method"/> is one of the <c>ReadOnlySpanQueries</c> queries.</summary>
    private static bool IsSpanQuery(IMethodSymbol method)
    {
        for (INamedTypeSymbol? type = (method.ReducedFrom ?? method).ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.ToDisplayString() == SpanQueriesMetadataName)
                return true;
        }
        return false;
    }

    /// <summary>Whether the span-query type declares <paramref name="name"/>, directly or in an extension block.</summary>
    private static bool HasQuery(INamedTypeSymbol queries, string name) =>
        queries.GetMembers(name).Length > 0 ||
        queries.GetTypeMembers().Any(block => block.GetMembers(name).Length > 0);

    /// <summary>The receiver text for a message, at most 60 characters.</summary>
    private static string Shorten(string text) => text.Length > 60 ? text.Substring(0, 57) + "..." : text;
}
