using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using SuperMetroid.EnsureAnalyzer.Contracts;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Rewrites recognized argument guards (SME6201) to their <c>Ensure</c> operation and removes a manual
/// parameter name from an <c>Ensure</c> call (SME6202) (#677). A fix is offered only when the value's type
/// fits the operation exactly: reference types for <c>NotNull</c>, <see cref="int"/> for the numeric checks
/// and span-convertible values for <c>LengthEqual</c>. Exception type and parameter name are preserved;
/// other shapes stay diagnostic-only.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnsureUsageCodeFixProvider)), Shared]
public sealed class EnsureUsageCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [EnsureUsageAnalyzer.GuardId, EnsureUsageAnalyzer.RedundantArgumentId];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        SemanticModel? model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null || model is null)
            return;
        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            SyntaxNode node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            NodeReplacement? replacement = diagnostic.Id == EnsureUsageAnalyzer.RedundantArgumentId
                ? RemoveManualName(node)
                : GuardReplacement(node, model, context.CancellationToken);
            if (replacement is not { } change)
                continue;
            context.RegisterCodeFix(
                CodeAction.Create(
                    diagnostic.Id == EnsureUsageAnalyzer.RedundantArgumentId ? "Let Ensure capture the value expression" : "Use Ensure",
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(change.Old,
                        change.New.WithTriviaFrom(change.Old).WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation)))),
                    equivalenceKey: diagnostic.Id),
                diagnostic);
        }
    }

    /// <summary>Removes a supplied name argument so Ensure captures the checked expression itself.</summary>
    private static NodeReplacement? RemoveManualName(SyntaxNode node)
    {
        if (node.FirstAncestorOrSelf<ArgumentSyntax>() is not { Parent: ArgumentListSyntax list } argument)
            return null;
        return new NodeReplacement(list, list.WithArguments(list.Arguments.Remove(argument)));
    }

    /// <summary>Recognizes supported throw helpers, coalescing throws, and if guards with exact operand types.</summary>
    private static NodeReplacement? GuardReplacement(SyntaxNode node, SemanticModel model, CancellationToken token)
    {
        // ArgumentNullException.ThrowIfNull(value) and ArgumentOutOfRangeException.ThrowIfNegative(value).
        if (node.FirstAncestorOrSelf<InvocationExpressionSyntax>() is { ArgumentList.Arguments.Count: 1 } helper &&
            model.GetSymbolInfo(helper, token).Symbol is IMethodSymbol method)
        {
            ExpressionSyntax value = helper.ArgumentList.Arguments[0].Expression;
            string? operation = new EnsureCallKey(method.ContainingType.ToDisplayString(), method.Name) switch
            {
                ("System.ArgumentNullException", "ThrowIfNull") when IsReference(model, value, token) => "NotNull",
                ("System.ArgumentOutOfRangeException", "ThrowIfNegative") when IsInt32(model, value, token) => "AtLeastZero",
                _ => null,
            };
            return operation is null ? null : new NodeReplacement(helper, EnsureCall(model, helper, operation, value));
        }

        // value ?? throw new ArgumentNullException(nameof(value)).
        if (node.FirstAncestorOrSelf<BinaryExpressionSyntax>(binary => binary.IsKind(SyntaxKind.CoalesceExpression)) is { } coalesce &&
            IsReference(model, coalesce.Left, token))
        {
            return new NodeReplacement(coalesce, EnsureCall(model, coalesce, "NotNull", coalesce.Left));
        }

        // if (guard) throw new Argument...Exception(..., nameof(value));
        if (node.FirstAncestorOrSelf<IfStatementSyntax>() is not { Else: null } statement)
            return null;
        ExpressionSyntax condition = statement.Condition is ParenthesizedExpressionSyntax parenthesized
            ? parenthesized.Expression : statement.Condition;
        InvocationExpressionSyntax? call = condition switch
        {
            IsPatternExpressionSyntax { Pattern: ConstantPatternSyntax { Expression: LiteralExpressionSyntax literal } } isNull
                when literal.IsKind(SyntaxKind.NullLiteralExpression) && IsReference(model, isNull.Expression, token) =>
                EnsureCall(model, statement, "NotNull", isNull.Expression),
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.EqualsExpression, Right: LiteralExpressionSyntax nullLiteral } equals
                when nullLiteral.IsKind(SyntaxKind.NullLiteralExpression) && IsReference(model, equals.Left, token) =>
                EnsureCall(model, statement, "NotNull", equals.Left),
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LessThanExpression, Right: LiteralExpressionSyntax { Token.ValueText: "0" } } negative
                when IsInt32(model, negative.Left, token) =>
                EnsureCall(model, statement, "AtLeastZero", negative.Left),
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.NotEqualsExpression, Left: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Length" } length } unequal
                when IsInt32(model, unequal.Right, token) && ConvertsToSpan(model, length.Expression, token) =>
                EnsureCall(model, statement, "LengthEqual", length.Expression, unequal.Right),
            BinaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.LogicalOrExpression,
                Left: BinaryExpressionSyntax { RawKind: (int)SyntaxKind.LessThanExpression } low,
                Right: BinaryExpressionSyntax { RawKind: (int)SyntaxKind.GreaterThanExpression } high,
            } when low.Left.IsEquivalentTo(high.Left) && IsInt32(model, low.Left, token) &&
                   IsInt32(model, low.Right, token) && IsInt32(model, high.Right, token) =>
                EnsureCall(model, statement, "BetweenInclusive", low.Right, high.Right, low.Left),
            _ => null,
        };
        if (call is null)
            return null;
        return new NodeReplacement(statement, SyntaxFactory.ExpressionStatement(call));
    }

    /// <summary>Creates the chosen Ensure call, qualifying its type when the short name is out of scope.</summary>
    private static InvocationExpressionSyntax EnsureCall(
        SemanticModel model, SyntaxNode site, string operation, params ExpressionSyntax[] arguments)
    {
        // Use the short name where it already binds to Ensure; otherwise qualify it.
        bool bound = model.LookupNamespacesAndTypes(site.SpanStart, name: "Ensure")
            .Any(symbol => symbol.ToDisplayString() == "SuperMetroid.Core.Ensure");
        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.ParseExpression($"{(bound ? "Ensure" : "SuperMetroid.Core.Ensure")}.{operation}"),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                arguments.Select(argument => SyntaxFactory.Argument(argument.WithoutTrivia())))));
    }

    /// <summary>Accepts concrete reference values for a null guard without inventing generic constraints.</summary>
    private static bool IsReference(SemanticModel model, ExpressionSyntax value, CancellationToken token) =>
        model.GetTypeInfo(value, token).Type is { IsReferenceType: true, TypeKind: not TypeKind.TypeParameter };

    /// <summary>Requires an exact Int32 value for numeric Ensure operations.</summary>
    private static bool IsInt32(SemanticModel model, ExpressionSyntax value, CancellationToken token) =>
        model.GetTypeInfo(value, token).Type?.SpecialType == SpecialType.System_Int32;

    /// <summary>Accepts one-dimensional arrays, strings, and span types for LengthEqual conversion.</summary>
    private static bool ConvertsToSpan(SemanticModel model, ExpressionSyntax value, CancellationToken token) =>
        model.GetTypeInfo(value, token).Type switch
        {
            IArrayTypeSymbol { Rank: 1 } => true,
            { SpecialType: SpecialType.System_String } => true,
            INamedTypeSymbol { Name: "Span" or "ReadOnlySpan", ContainingNamespace.Name: "System" } => true,
            _ => false,
        };
}
