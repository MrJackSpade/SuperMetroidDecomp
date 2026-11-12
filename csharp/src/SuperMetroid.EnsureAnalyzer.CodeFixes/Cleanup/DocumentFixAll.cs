using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>Fixes every finding of one document in a single change, through the provider's own fix.</summary>
internal sealed class DocumentFixAll : FixAllProvider
{
    private readonly DocumentFixProvider _provider;

    /// <summary>Creates the fix-all of <paramref name="provider"/>.</summary>
    internal DocumentFixAll(DocumentFixProvider provider) => _provider = provider;

    /// <inheritdoc />
    public override IEnumerable<FixAllScope> GetSupportedFixAllScopes() => [FixAllScope.Document];

    /// <inheritdoc />
    public override async Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
    {
        if (fixAllContext.Scope != FixAllScope.Document || fixAllContext.Document is not { } document)
            throw new InvalidOperationException($"{_provider.GetType().Name} fixes one document at a time, not {fixAllContext.Scope}.");
        var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false);
        return CodeAction.Create(_provider.Title, token => _provider.FixAsync(document, diagnostics, token), fixAllContext.CodeActionEquivalenceKey);
    }
}
