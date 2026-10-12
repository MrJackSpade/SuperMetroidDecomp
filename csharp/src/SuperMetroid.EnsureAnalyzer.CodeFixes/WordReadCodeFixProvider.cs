using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Replaces a word rebuilt from two byte reads (SME6350) with its canonical <c>Read…Word</c> operation (#223).
/// The fix is offered only when the address progression is unambiguous; a bank OR'd with an unwrapped
/// <c>offset + 1</c> needs a deliberate Linear or FixedBank choice.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(WordReadCodeFixProvider)), Shared]
public sealed class WordReadCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [WordReadAnalyzer.ReconstructedWordId];

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
            WordReadMatch? match = node.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()
                .Select(binary => WordReadMatcher.Match(binary, model))
                .FirstOrDefault(candidate => candidate is not null && candidate.Expression.Span == diagnostic.Location.SourceSpan);
            if (match is null || Replacement(match) is not { } replacement)
                continue;
            context.RegisterCodeFix(
                CodeAction.Create($"Use {match.CanonicalMethod}",
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(match.Expression,
                        replacement.WithTriviaFrom(match.Expression).WithAdditionalAnnotations(Formatter.Annotation)))),
                    equivalenceKey: match.CanonicalMethod),
                diagnostic);
        }
    }

    /// <summary>The canonical read for <paramref name="match"/>, OR'd with any other operands; null when it needs review.</summary>
    public static ExpressionSyntax? Replacement(WordReadMatch match)
    {
        if (match.CanonicalMethod is not { } method)
            return null;
        ExpressionSyntax receiver = match.ViaAddressSpace
            ? SyntaxFactory.ParseExpression($"CartridgeImportSource.Require({match.Receiver.WithoutTrivia()})")
            : match.Receiver.WithoutTrivia();
        ExpressionSyntax word = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                receiver, SyntaxFactory.IdentifierName(method)),
            SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(match.Address.WithoutTrivia()))));
        if (match.OtherOperands.IsEmpty)
            return word;
        ExpressionSyntax combined = match.OtherOperands.Select(operand => operand.WithoutTrivia())
            .Aggregate((left, right) => SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, left, right));
        return SyntaxFactory.ParenthesizedExpression(SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, combined, word));
    }
}
