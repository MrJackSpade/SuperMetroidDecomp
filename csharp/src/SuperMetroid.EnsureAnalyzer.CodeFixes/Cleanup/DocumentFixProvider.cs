using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>
/// A code fix the asynchronous cleanup applies (tools/cleanup/README.md): one change for all of a document's findings at
/// once, which may change other documents too. The fix is offered for every finding and always applied (#2599): a
/// finding whose fix needs a choice the fix cannot make is fixed all the same, and where that breaks the code the build
/// fails at the site. A fix never skips a finding and never leaves it to be recorded as unfixed; a finding the fix cannot
/// even find (the analyzer and the fix disagree about where it is) fails the fix with the exception that names it.
/// </summary>
public abstract class DocumentFixProvider : CodeFixProvider
{
    /// <summary>The title of the fix, also its equivalence key.</summary>
    internal abstract string Title { get; }

    /// <inheritdoc />
    public sealed override FixAllProvider GetFixAllProvider() => new DocumentFixAll(this);

    /// <inheritdoc />
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(
                CodeAction.Create(Title, token => FixAsync(context.Document, ImmutableArray.Create(diagnostic), token), equivalenceKey: Title),
                diagnostic);
        }
        return Task.CompletedTask;
    }

    /// <summary>The solution with every given finding of <paramref name="document"/> resolved.</summary>
    internal abstract Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken);
}
