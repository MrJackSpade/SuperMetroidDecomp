using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>The text edits a cleanup fix makes across the documents of a solution, applied together.</summary>
internal sealed class SolutionEdits
{
    private readonly Solution _solution;
    private readonly Dictionary<DocumentId, SourceEdits> _byDocument = new();
    private readonly HashSet<DocumentId> _deleted = [];

    /// <summary>Starts the edits of <paramref name="solution"/>.</summary>
    internal SolutionEdits(Solution solution) => _solution = solution;

    /// <summary>The solution before the edits.</summary>
    internal Solution Solution => _solution;

    /// <summary>The edits of the document holding <paramref name="tree"/>.</summary>
    internal async Task<SourceEdits> ForAsync(SyntaxTree tree, CancellationToken cancellationToken)
    {
        Document document = _solution.GetDocument(tree)
            ?? throw new InvalidOperationException($"{tree.FilePath} is not a document of the solution.");
        if (!_byDocument.TryGetValue(document.Id, out SourceEdits? edits))
        {
            edits = new SourceEdits(await tree.GetTextAsync(cancellationToken).ConfigureAwait(false), document.FilePath ?? document.Name);
            _byDocument[document.Id] = edits;
        }
        return edits;
    }

    /// <summary>Deletes the document holding <paramref name="tree"/>; its edits are not applied.</summary>
    internal void Delete(SyntaxTree tree) =>
        _deleted.Add(_solution.GetDocumentId(tree) ?? throw new InvalidOperationException($"{tree.FilePath} is not a document of the solution."));

    /// <summary>The solution with every document's edits applied and the deleted documents removed.</summary>
    internal Solution Apply()
    {
        Solution solution = _solution;
        foreach (KeyValuePair<DocumentId, SourceEdits> entry in _byDocument)
        {
            if (!_deleted.Contains(entry.Key))
                solution = solution.WithDocumentText(entry.Key, entry.Value.Apply());
        }
        foreach (DocumentId deleted in _deleted)
            solution = solution.RemoveDocument(deleted);
        return solution;
    }
}
