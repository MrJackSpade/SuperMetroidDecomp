using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Suppressions;

/// <summary>
/// IDE0079, a suppression that suppresses nothing, enforced by the asynchronous cleanup (#1672). The cleanup's runner
/// finds the sites (tools/cleanup/CodeFixRunner.cs); a finding is at a <c>#pragma warning disable</c> code, with the
/// matching <c>restore</c> code as its additional location, or at a <c>[SuppressMessage]</c> attribute. A pragma loses
/// the unnecessary codes, and the whole directive line when none is left; the matching restore loses the same codes.
/// An attribute is removed from its list, and the list's line when it was the list's only attribute.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnnecessarySuppressionCodeFixProvider)), Shared]
public sealed class UnnecessarySuppressionCodeFixProvider : DocumentFixProvider
{
    /// <summary>The unnecessary-suppression rule.</summary>
    public const string Id = "IDE0079";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [Id];

    /// <inheritdoc />
    internal override string Title => "Remove the unnecessary suppression";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{document.FilePath} has no syntax tree.");
        var edits = new SourceEdits(await document.GetTextAsync(cancellationToken).ConfigureAwait(false), document.FilePath ?? document.Name);
        var codes = new Dictionary<PragmaWarningDirectiveTriviaSyntax, HashSet<ExpressionSyntax>>();
        var attributes = new Dictionary<AttributeListSyntax, HashSet<AttributeSyntax>>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            foreach (Location location in new[] { diagnostic.Location }.Concat(diagnostic.AdditionalLocations))
            {
                SyntaxNode node = root.FindNode(location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
                if (node.FirstAncestorOrSelf<ExpressionSyntax>() is { Parent: PragmaWarningDirectiveTriviaSyntax pragma } code)
                    Add(codes, pragma, code);
                else if (node.FirstAncestorOrSelf<AttributeSyntax>() is { Parent: AttributeListSyntax list } attribute)
                    Add(attributes, list, attribute);
                else
                    throw new CleanupFailure(diagnostic, $"the finding is at a {node.Kind()}, not a pragma code or a suppression attribute");
            }
        }
        foreach (var entry in codes)
        {
            var kept = entry.Key.ErrorCodes.Where(code => !entry.Value.Contains(code)).ToList();
            if (kept.Count == 0)
                edits.RemoveDirectiveLine(entry.Key);
            else
                edits.Replace(ListSpan(entry.Key.ErrorCodes), string.Join(", ", kept.Select(code => code.ToString())));
        }
        foreach (var entry in attributes)
        {
            var kept = entry.Key.Attributes.Where(attribute => !entry.Value.Contains(attribute)).ToList();
            if (kept.Count == 0)
                edits.RemoveKeepingComments(entry.Key);
            else
                edits.Replace(ListSpan(entry.Key.Attributes), string.Join(", ", kept.Select(attribute => attribute.ToString())));
        }
        return document.WithText(edits.Apply()).Project.Solution;
    }

    /// <summary>Adds <paramref name="item"/> to the set kept for <paramref name="key"/>.</summary>
    private static void Add<TKey, TItem>(Dictionary<TKey, HashSet<TItem>> sets, TKey key, TItem item)
        where TKey : notnull
    {
        if (!sets.TryGetValue(key, out HashSet<TItem>? set))
            sets[key] = set = [];
        set.Add(item);
    }

    /// <summary>The span from a separated list's first element to its last.</summary>
    private static TextSpan ListSpan<TNode>(SeparatedSyntaxList<TNode> list)
        where TNode : SyntaxNode =>
        TextSpan.FromBounds(list[0].SpanStart, list[list.Count - 1].Span.End);
}
