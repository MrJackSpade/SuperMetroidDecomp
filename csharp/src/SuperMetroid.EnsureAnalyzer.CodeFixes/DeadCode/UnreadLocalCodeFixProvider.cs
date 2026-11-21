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
/// whose value could have an effect keeps it as <c>_ = value;</c> (<see cref="Writes"/>), and so does an initializer,
/// before the declaration it leaves.
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
        var unread = new UnreadParameters(solution, edits);
        SourceEdits own = await edits.ForAsync(root.SyntaxTree, cancellationToken).ConfigureAwait(false);
        foreach (Diagnostic diagnostic in diagnostics)
        {
            if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent is not VariableDeclaratorSyntax declarator ||
                model.GetDeclaredSymbol(declarator, cancellationToken) is not ILocalSymbol local)
                throw new InvalidOperationException($"{document.FilePath}: {diagnostic.Id} is reported at {diagnostic.Location.SourceSpan}, where no local is declared.");
            var references = await SymbolFinder.FindReferencesAsync(local, solution, ImmutableHashSet.Create(document), cancellationToken).ConfigureAwait(false);
            await Writes.RemoveAsync(edits, [.. references.SelectMany(found => found.Locations)], unread, cancellationToken).ConfigureAwait(false);
            if (declarator.Initializer is { } initializer && !SideEffects.IsFree(model.GetOperation(initializer.Value, cancellationToken)))
            {
                if (KeepEvaluation(own, declarator, initializer.Value, local, model))
                    continue;
            }
            else if (declarator.Initializer is { } removed)
                unread.Removing(removed, model);
            declarators.Add(declarator);
        }
        VariableDeclarations.Remove(own, declarators);
        await unread.RemoveAsync(cancellationToken).ConfigureAwait(false);
        return edits.Apply();
    }

    /// <summary>Keeps the evaluation of an initializer that could have an effect as <c>_ = value;</c>: in place of the
    /// declaration when the local is its only variable (true: the declaration is taken care of), or before it.</summary>
    private static bool KeepEvaluation(SourceEdits edits, VariableDeclaratorSyntax declarator, ExpressionSyntax value, ILocalSymbol local, SemanticModel model)
    {
        if (declarator.Parent is not VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax statement } declaration)
            throw new InvalidOperationException($"{declarator.SyntaxTree.FilePath}: the local '{local.Name}' at {declarator.Span} is not declared by a statement.");
        string kept = $"_ = {InitializerEvaluation.Render(new KeptInitializer(local, value, declaration.Type, model))};";
        if (declaration.Variables.Count == 1)
        {
            edits.Replace(statement.Span, kept);
            return true;
        }
        edits.Insert(statement.SpanStart, kept + " ");
        return false;
    }
}
