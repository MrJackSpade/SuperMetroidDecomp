using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>
/// A code fix the asynchronous cleanup applies (tools/cleanup/README.md): one change for all of a document's findings at
/// once, which may change other documents too. A finding the fix cannot resolve without changing behaviour or making a
/// choice fails the fix with <see cref="CleanupFailure"/>, naming the site; it is never skipped.
/// </summary>
public abstract class DocumentFixProvider : CodeFixProvider
{
    /// <summary>The title of the fix, also its equivalence key.</summary>
    internal abstract string Title { get; }

    /// <inheritdoc />
    public sealed override FixAllProvider GetFixAllProvider() => new DocumentFixAll(this);

    /// <inheritdoc />
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            if (!await CanFixAsync(context.Document, diagnostic, context.CancellationToken).ConfigureAwait(false))
                continue;
            context.RegisterCodeFix(
                CodeAction.Create(Title, token => FixAsync(context.Document, ImmutableArray.Create(diagnostic), token), equivalenceKey: Title),
                diagnostic);
        }
    }

    /// <summary>Whether the fix is offered for <paramref name="diagnostic"/>: every finding, unless a fix tells the
    /// shapes it cannot resolve apart cheaply. A finding not offered fails the cleanup's run, naming the site.</summary>
    internal virtual Task<bool> CanFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken) => Task.FromResult(true);

    /// <summary>The solution with every given finding of <paramref name="document"/> resolved.</summary>
    internal abstract Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken);
}
