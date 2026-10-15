using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.EnsureAnalyzer.Contracts;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Removes an adapter whose removal is unambiguous (#624): a sequence's <c>ToArray()</c> that only feeds a LINQ
/// operator (SME6390), and a parameterless <c>AsSpan()</c> before an array's <c>SequenceEqual</c> (SME6391). A span's
/// <c>ToArray()</c> bridge changes the operator to a span query and is left to the author.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AdapterChainCodeFixProvider)), Shared]
public sealed class AdapterChainCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [AdapterChainAnalyzer.MaterializationId, AdapterChainAnalyzer.SpanAdapterId];

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
            if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is not SimpleNameSyntax name ||
                name.Parent is not MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax adapter } access)
                continue;
            bool linqCopy = diagnostic.Id == AdapterChainAnalyzer.MaterializationId &&
                AdapterChainAnalyzer.IsSequence(model.GetTypeInfo(access.Expression, context.CancellationToken).Type);
            if (diagnostic.Id == AdapterChainAnalyzer.MaterializationId && !linqCopy)
                continue;
            string title = linqCopy ? "Remove the ToArray() copy" : "Remove the AsSpan() adapter";
            context.RegisterCodeFix(
                CodeAction.Create(title,
                    _ => Task.FromResult(context.Document.WithSyntaxRoot(
                        root.ReplaceNode(adapter, access.Expression.WithTriviaFrom(adapter)))),
                    equivalenceKey: title),
                diagnostic);
        }
    }
}
