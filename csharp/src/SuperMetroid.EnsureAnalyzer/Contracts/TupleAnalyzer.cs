using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1399: a tuple has no name, no documentation, no invariants and no place for behaviour, so a value that travels as one is
/// a domain value nobody defined. This reports every tuple type and tuple literal written in source, every use of
/// <c>System.Tuple</c> or <c>System.ValueTuple</c> by name, and every call to an external API that returns a tuple. A
/// deconstruction target is not reported: deconstructing a declared type is fine, and deconstructing a tuple is reported
/// where the tuple is made. Only an external API whose signature requires a tuple may be justified, at that one site.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TupleAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A tuple stands where a declared type belongs.</summary>
    public const string TupleId = "SME6437";

    /// <summary>Reports a tuple type, a tuple literal or a tuple-returning external call.</summary>
    private static readonly DiagnosticDescriptor Rule = new(
        TupleId,
        "Tuple instead of a declared type",
        "{0} is a tuple; declare a named type for it or use an existing domain type",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Tuples are not used: a value made of several parts is a declared record, struct or class with a domain " +
            "name and documentation, or an existing domain type. Only an external API whose signature requires a tuple may " +
            "be justified, at that site (#1399).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeTupleType, SyntaxKind.TupleType);
        context.RegisterSyntaxNodeAction(AnalyzeTupleLiteral, SyntaxKind.TupleExpression);
        context.RegisterSyntaxNodeAction(AnalyzeTupleName, SyntaxKind.GenericName, SyntaxKind.IdentifierName);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    /// <summary>Reports a tuple type written in source, once for the outermost tuple.</summary>
    private static void AnalyzeTupleType(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.Parent is TupleElementSyntax)
            return;
        Report(context, context.Node);
    }

    /// <summary>Reports a tuple literal that is not a deconstruction target.</summary>
    private static void AnalyzeTupleLiteral(SyntaxNodeAnalysisContext context)
    {
        var tuple = (TupleExpressionSyntax)context.Node;
        if (tuple.Parent is ArgumentSyntax { Parent: TupleExpressionSyntax })
            return;
        if (IsDeconstructionTarget(tuple))
            return;
        Report(context, tuple);
    }

    /// <summary>Reports <c>System.Tuple</c> or <c>System.ValueTuple</c> named in source.</summary>
    private static void AnalyzeTupleName(SyntaxNodeAnalysisContext context)
    {
        var name = (SimpleNameSyntax)context.Node;
        if (name.Identifier.ValueText is not ("Tuple" or "ValueTuple"))
            return;
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol;
        INamedTypeSymbol? type = symbol as INamedTypeSymbol ?? symbol?.ContainingType;
        if (type is { ContainingNamespace: { Name: "System", ContainingNamespace.IsGlobalNamespace: true } } &&
            type.Name is "Tuple" or "ValueTuple")
        {
            Report(context, name.Parent is QualifiedNameSyntax parent ? parent : name);
        }
    }

    /// <summary>
    /// Reports a call to an external method whose declared return type is a tuple. A generic method that returns a tuple
    /// only because source code supplied one as a type argument (a LINQ projection) is reported where that tuple is written.
    /// </summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (method.Locations.Any(location => location.IsInSource) || !ContainsTuple(method.OriginalDefinition.ReturnType))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), $"'{method.Name}' returns a value that"));
    }

    /// <summary>Whether <paramref name="tuple"/> receives a deconstruction rather than making a tuple.</summary>
    private static bool IsDeconstructionTarget(TupleExpressionSyntax tuple)
    {
        SyntaxNode current = tuple;
        while (current.Parent is ArgumentSyntax { Parent: TupleExpressionSyntax outer })
            current = outer;
        return current.Parent switch
        {
            AssignmentExpressionSyntax assignment => assignment.Left == current,
            ForEachVariableStatementSyntax loop => loop.Variable == current,
            _ => false,
        };
    }

    /// <summary>Whether <paramref name="type"/> is a tuple or carries one as an element or type argument.</summary>
    private static bool ContainsTuple(ITypeSymbol type) => type switch
    {
        INamedTypeSymbol { IsTupleType: true } => true,
        IArrayTypeSymbol array => ContainsTuple(array.ElementType),
        INamedTypeSymbol named => named.TypeArguments.Any(ContainsTuple),
        _ => false,
    };

    /// <summary>Reports <paramref name="node"/>, quoting it.</summary>
    private static void Report(SyntaxNodeAnalysisContext context, SyntaxNode node)
    {
        string text = node.ToString();
        if (text.Length > 60)
            text = text.Substring(0, 57) + "...";
        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), $"'{text}'"));
    }
}
