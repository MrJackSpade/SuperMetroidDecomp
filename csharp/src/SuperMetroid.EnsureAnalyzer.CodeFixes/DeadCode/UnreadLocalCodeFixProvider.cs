using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// CS0219, a local assigned but never read, enforced by the asynchronous cleanup (#1657). The compiler reports it only
/// when every value assigned is a constant, so the local and every assignment to it go and no work is skipped; a write
/// whose value could have an effect keeps it as <c>_ = value;</c> (<see cref="Writes"/>).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnreadLocalCodeFixProvider)), Shared]
public sealed class UnreadLocalCodeFixProvider : DocumentFixProvider
{
    /// <summary>The compiler's assigned-but-never-used warning.</summary>
    public const string Id = "CS0219";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [Id];

    /// <inheritdoc />
    internal override string Title => "Remove the unread local";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no syntax tree.");
        SemanticModel model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no semantic model.");
        Solution solution = document.Project.Solution;
        var edits = new SolutionEdits(solution);
        var declarators = new List<VariableDeclaratorSyntax>();
        var sites = new Dictionary<SyntaxNode, Diagnostic>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent is not VariableDeclaratorSyntax declarator ||
                model.GetDeclaredSymbol(declarator, cancellationToken) is not ILocalSymbol local)
                throw new CleanupFailure(diagnostic, "the finding is not at a local's declaration");
            if (declarator.Initializer is { } initializer && !SideEffects.IsFree(model.GetOperation(initializer.Value, cancellationToken)))
                throw new CleanupFailure(diagnostic, $"the initializer of '{local.Name}' could have an effect");
            var references = await SymbolFinder.FindReferencesAsync(local, solution, ImmutableHashSet.Create(document), cancellationToken).ConfigureAwait(false);
            await Writes.RemoveAsync(edits, local, [.. references.SelectMany(found => found.Locations)], diagnostic, cancellationToken).ConfigureAwait(false);
            declarators.Add(declarator);
            sites[declarator] = diagnostic;
        }
        VariableDeclarations.Remove(await edits.ForAsync(root.SyntaxTree, cancellationToken).ConfigureAwait(false), declarators, node => sites[node]);
        return edits.Apply();
    }
}
