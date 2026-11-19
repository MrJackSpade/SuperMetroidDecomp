using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// What a catalog may not hold (#1452): every <c>if</c>, <c>switch</c> statement or expression, conditional <c>?:</c>,
/// null-coalescing or null-conditional operator, loop, <c>goto</c>, pattern or type test, non-constant comparison, query
/// <c>where</c> clause and LINQ predicate, and every body (a method, constructor, accessor, operator, local function or
/// lambda) with statements beyond a single <c>return</c>. SME6451 reports each one in a catalog file, and SME6472 reads a
/// member without any as data: what a catalog would accept decides nothing.
/// </summary>
internal static class CatalogDecisions
{
    /// <summary>Methods that take a predicate and decide which elements of a sequence count.</summary>
    private static readonly ImmutableHashSet<string> PredicateMethods =
    [
        "Where", "Any", "All", "First", "FirstOrDefault", "Single", "SingleOrDefault", "Last", "LastOrDefault",
        "Count", "LongCount", "TakeWhile", "SkipWhile", "Find", "FindIndex", "FindLast", "FindLastIndex", "FindAll",
        "Exists", "TrueForAll", "RemoveAll",
    ];

    /// <summary>The decision syntax a catalog may not hold, each with how a diagnostic names it.</summary>
    private static readonly ImmutableDictionary<SyntaxKind, string> Branches = new Dictionary<SyntaxKind, string>
    {
        [SyntaxKind.IfStatement] = "an 'if'",
        [SyntaxKind.SwitchStatement] = "a 'switch' statement",
        [SyntaxKind.SwitchExpression] = "a 'switch' expression",
        [SyntaxKind.ConditionalExpression] = "a conditional '?:'",
        [SyntaxKind.CoalesceExpression] = "a null-coalescing '??'",
        [SyntaxKind.CoalesceAssignmentExpression] = "a null-coalescing '??'",
        [SyntaxKind.ConditionalAccessExpression] = "a null-conditional access",
        [SyntaxKind.ForStatement] = "a loop",
        [SyntaxKind.ForEachStatement] = "a loop",
        [SyntaxKind.ForEachVariableStatement] = "a loop",
        [SyntaxKind.WhileStatement] = "a loop",
        [SyntaxKind.DoStatement] = "a loop",
        [SyntaxKind.GotoStatement] = "a 'goto'",
        [SyntaxKind.GotoCaseStatement] = "a 'goto'",
        [SyntaxKind.GotoDefaultStatement] = "a 'goto'",
        [SyntaxKind.IsPatternExpression] = "a pattern test",
        [SyntaxKind.IsExpression] = "a pattern test",
        [SyntaxKind.WhereClause] = "a query predicate",
    }.ToImmutableDictionary();

    /// <summary>The comparisons and logical tests a catalog may hold only when their value is a compile-time constant.</summary>
    private static readonly ImmutableHashSet<SyntaxKind> Comparisons =
    [
        SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.LessThanExpression,
        SyntaxKind.LessThanOrEqualExpression, SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression,
        SyntaxKind.LogicalAndExpression, SyntaxKind.LogicalOrExpression,
    ];

    /// <summary>Every syntax kind <see cref="Describe"/> can report.</summary>
    internal static ImmutableArray<SyntaxKind> Kinds { get; } =
        [.. Branches.Keys, .. Comparisons, SyntaxKind.InvocationExpression, SyntaxKind.Block];

    /// <summary>
    /// The decision or procedure <paramref name="node"/> is, or none: a branch, loop, jump, test or query predicate, a
    /// comparison whose value is not a compile-time constant, a call that filters or searches a sequence with a
    /// predicate, or a body with statements beyond a single <c>return</c>.
    /// </summary>
    internal static CatalogDecision? Describe(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
    {
        if (Branches.TryGetValue(node.Kind(), out string? branch))
            return new CatalogDecision(node.GetFirstToken().GetLocation(), branch);
        return node switch
        {
            BinaryExpressionSyntax binary when Comparisons.Contains(binary.Kind()) &&
                !model.GetConstantValue(binary, cancellationToken).HasValue =>
                new CatalogDecision(binary.OperatorToken.GetLocation(), $"a comparison '{binary.OperatorToken.Text}'"),
            InvocationExpressionSyntax invocation => Predicate(invocation, model, cancellationToken),
            BlockSyntax block when IsProcedure(block) =>
                new CatalogDecision(block.OpenBraceToken.GetLocation(), "a body with statements"),
            _ => null,
        };
    }

    /// <summary>Whether anything within <paramref name="node"/>, itself included, is a decision or a procedure.</summary>
    internal static bool Decides(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken) =>
        node.DescendantNodesAndSelf().Any(descendant => Describe(descendant, model, cancellationToken) is not null);

    /// <summary>The predicate <paramref name="invocation"/> passes to a method that filters or searches a sequence, or none.</summary>
    private static CatalogDecision? Predicate(InvocationExpressionSyntax invocation, SemanticModel model, CancellationToken cancellationToken)
    {
        string? name = invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
            MemberBindingExpressionSyntax binding => binding.Name.Identifier.Text,
            _ => null,
        };
        if (name is null || !PredicateMethods.Contains(name) ||
            !invocation.ArgumentList.Arguments.Any(argument => IsPredicate(model, argument, cancellationToken)))
        {
            return null;
        }
        return new CatalogDecision(invocation.ArgumentList.GetLocation(), $"a predicate passed to '{name}'");
    }

    /// <summary>
    /// Whether <paramref name="argument"/> is a predicate: it converts to a delegate that answers yes or no. A lookup
    /// that shares a predicate method's name but takes a key, such as a name, decides nothing.
    /// </summary>
    private static bool IsPredicate(SemanticModel model, ArgumentSyntax argument, CancellationToken cancellationToken) =>
        model.GetTypeInfo(argument.Expression, cancellationToken).ConvertedType is
            INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod.ReturnType.SpecialType: SpecialType.System_Boolean };

    /// <summary>
    /// Whether <paramref name="block"/> is the body of a method, constructor, accessor, operator, local function or
    /// lambda with statements beyond a single <c>return</c>: a procedure rather than a value.
    /// </summary>
    private static bool IsProcedure(BlockSyntax block) =>
        block.Parent is BaseMethodDeclarationSyntax or AccessorDeclarationSyntax or LocalFunctionStatementSyntax or
            AnonymousFunctionExpressionSyntax &&
        block.Statements.Count != 0 &&
        !(block.Statements.Count == 1 && block.Statements[0] is ReturnStatementSyntax);
}
