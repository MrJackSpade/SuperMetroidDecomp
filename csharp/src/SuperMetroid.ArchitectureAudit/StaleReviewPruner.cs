using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// Deletes stale <c>[ArchitectureReview]</c> attributes from the source: the asynchronous cleanup's fix for a review
/// whose finding is gone. Each attribute is found through its own syntax reference. An attribute list that held only
/// stale reviews is removed whole, with its line when nothing else shares it; otherwise only the stale attributes and
/// their separators go. Every other byte of the file, its byte-order mark and line endings included, is kept.
/// <para>
/// Every edit is computed and validated before any file is written: the file on disk must be the text the solution
/// was loaded from, the edits must not overlap, and the edited file must parse with no new error and hold exactly the
/// removed reviews fewer. If any edit fails, nothing is written.
/// </para>
/// </summary>
internal static class StaleReviewPruner
{
    /// <summary>The attribute's name as source may write it, with or without the <c>Attribute</c> suffix.</summary>
    private static readonly string[] ReviewNames = ["ArchitectureReview", TypeInventory.ReviewAttributeName];

    /// <summary>The UTF-8 byte-order mark a source file may start with.</summary>
    private static readonly byte[] ByteOrderMark = [0xEF, 0xBB, 0xBF];

    /// <summary>Removes every stale review, printing one line per review and a summary; returns the exit code.</summary>
    public static int Prune(List<StaleReview> stale, string repositoryRoot)
    {
        var rewrites = new List<FileRewrite>();
        foreach (var file in stale.GroupBy(review => Path.GetFullPath(review.Review.Site.SyntaxTree.FilePath), StringComparer.OrdinalIgnoreCase)
                     .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            rewrites.Add(Rewrite(file.Key, [.. file]));
        foreach (FileRewrite rewrite in rewrites)
            File.WriteAllBytes(rewrite.Path, rewrite.Bytes);
        foreach (FileRewrite rewrite in rewrites)
        {
            string relative = Path.GetRelativePath(repositoryRoot, rewrite.Path).Replace('\\', '/');
            foreach (StaleReview review in rewrite.Removed)
                Console.WriteLine($"Removed stale review: {review.Type} \"{review.Review.Finding}\" in {relative}");
        }
        Console.WriteLine($"Removed {rewrites.Sum(rewrite => rewrite.Removed.Count)} stale reviews in {rewrites.Count} files.");
        return 0;
    }

    /// <summary>Computes and validates one file's new bytes without writing them.</summary>
    private static FileRewrite Rewrite(string path, List<StaleReview> reviews)
    {
        byte[] bytes = File.ReadAllBytes(path);
        bool bom = bytes is [0xEF, 0xBB, 0xBF, ..];
        string text = new UTF8Encoding(false, true).GetString(bytes, bom ? ByteOrderMark.Length : 0, bytes.Length - (bom ? ByteOrderMark.Length : 0));
        foreach (SyntaxTree tree in reviews.Select(review => review.Review.Site.SyntaxTree).Distinct())
        {
            if (tree.GetText().ToString() != text)
                throw new InvalidOperationException($"{path} differs from the text the solution was loaded from; rerun the prune on an unchanged tree.");
        }

        var attributes = new List<AttributeSyntax>();
        foreach (StaleReview review in reviews)
        {
            if (review.Review.Site.GetSyntax() is not AttributeSyntax attribute || !IsReview(attribute))
                throw new InvalidOperationException($"{path}: the review of {review.Type} does not point at an [ArchitectureReview] attribute.");
            if (attributes.Any(known => known.Span == attribute.Span))
                throw new InvalidOperationException($"{path}: two stale reviews point at the same attribute at {attribute.GetLocation().GetLineSpan()}.");
            attributes.Add(attribute);
        }

        var changes = new List<TextSpan>();
        foreach (var group in attributes.GroupBy(attribute => ListOf(path, attribute).Span))
        {
            AttributeListSyntax list = ListOf(path, group.First());
            var removed = group.Select(attribute => attribute.Span).ToHashSet();
            if (removed.Count == list.Attributes.Count)
                changes.Add(WholeList(text, list));
            else
                changes.AddRange(StaleRuns(list, removed));
        }
        changes.Sort((left, right) => left.Start.CompareTo(right.Start));
        for (int index = 1; index < changes.Count; index++)
        {
            if (changes[index].Start < changes[index - 1].End)
                throw new InvalidOperationException($"{path}: the removals at {changes[index - 1]} and {changes[index]} overlap.");
        }

        var edited = new StringBuilder(text);
        for (int index = changes.Count - 1; index >= 0; index--)
            edited.Remove(changes[index].Start, changes[index].Length);
        string result = edited.ToString();
        Validate(path, text, result, reviews.Count, (CSharpParseOptions)reviews[0].Review.Site.SyntaxTree.Options);

        byte[] encoded = new UTF8Encoding(false, true).GetBytes(result);
        byte[] written = bom ? [.. ByteOrderMark, .. encoded] : encoded;
        return new FileRewrite(path, written, reviews);
    }

    /// <summary>The attribute list holding a review attribute.</summary>
    private static AttributeListSyntax ListOf(string path, AttributeSyntax attribute) =>
        attribute.Parent as AttributeListSyntax ??
        throw new InvalidOperationException($"{path}: an [ArchitectureReview] at {attribute.GetLocation().GetLineSpan()} is not in an attribute list.");

    /// <summary>
    /// The span that removes a whole attribute list: its lines with their line break when nothing else shares them,
    /// otherwise the list with the spaces that separate it from what remains on its line.
    /// </summary>
    private static TextSpan WholeList(string text, AttributeListSyntax list)
    {
        int lineStart = list.SpanStart;
        while (lineStart > 0 && text[lineStart - 1] is ' ' or '\t')
            lineStart--;
        bool aloneBefore = lineStart == 0 || text[lineStart - 1] == '\n';
        int lineEnd = list.Span.End;
        while (lineEnd < text.Length && text[lineEnd] is ' ' or '\t')
            lineEnd++;
        bool aloneAfter = lineEnd == text.Length || text[lineEnd] is '\r' or '\n';
        if (aloneBefore && aloneAfter)
        {
            if (lineEnd < text.Length && text[lineEnd] == '\r')
                lineEnd++;
            if (lineEnd < text.Length && text[lineEnd] == '\n')
                lineEnd++;
            return TextSpan.FromBounds(lineStart, lineEnd);
        }
        return aloneAfter ? TextSpan.FromBounds(lineStart, list.Span.End) : TextSpan.FromBounds(list.SpanStart, lineEnd);
    }

    /// <summary>
    /// The spans that remove each run of consecutive stale attributes from a list that keeps others: a leading run up
    /// to the first kept attribute, any later run from the separator before it.
    /// </summary>
    private static IEnumerable<TextSpan> StaleRuns(AttributeListSyntax list, HashSet<TextSpan> removed)
    {
        SeparatedSyntaxList<AttributeSyntax> attributes = list.Attributes;
        int index = 0;
        while (index < attributes.Count)
        {
            if (!removed.Contains(attributes[index].Span))
            {
                index++;
                continue;
            }
            int first = index;
            while (index < attributes.Count && removed.Contains(attributes[index].Span))
                index++;
            int last = index - 1;
            yield return first == 0
                ? TextSpan.FromBounds(attributes[first].SpanStart, attributes[last + 1].SpanStart)
                : TextSpan.FromBounds(attributes.GetSeparator(first - 1).SpanStart, attributes[last].Span.End);
        }
    }

    /// <summary>Fails unless the edited text parses with no new error and holds exactly the removed reviews fewer.</summary>
    private static void Validate(string path, string original, string edited, int removed, CSharpParseOptions options)
    {
        SyntaxTree before = CSharpSyntaxTree.ParseText(original, options, path);
        SyntaxTree after = CSharpSyntaxTree.ParseText(edited, options, path);
        int errorsBefore = before.GetDiagnostics().Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        var errorsAfter = after.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();
        if (errorsAfter.Count != errorsBefore)
            throw new InvalidOperationException($"{path}: removing its stale reviews leaves a file that does not parse:\n" +
                string.Join('\n', errorsAfter));
        int reviewsBefore = Reviews(before).Count();
        int reviewsAfter = Reviews(after).Count();
        if (reviewsAfter != reviewsBefore - removed)
            throw new InvalidOperationException($"{path}: removing {removed} stale reviews left {reviewsAfter} of {reviewsBefore}.");
        int typesBefore = before.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Count();
        int typesAfter = after.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Count();
        if (typesAfter != typesBefore)
            throw new InvalidOperationException($"{path}: removing its stale reviews changed its type declarations from {typesBefore} to {typesAfter}.");
    }

    /// <summary>The review attributes a parsed file declares.</summary>
    private static IEnumerable<AttributeSyntax> Reviews(SyntaxTree tree) =>
        tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().Where(IsReview);

    /// <summary>Whether an attribute names <c>ArchitectureReview</c>, however qualified.</summary>
    private static bool IsReview(AttributeSyntax attribute)
    {
        SimpleNameSyntax name = attribute.Name switch
        {
            QualifiedNameSyntax qualified => qualified.Right,
            AliasQualifiedNameSyntax alias => alias.Name,
            SimpleNameSyntax simple => simple,
            _ => throw new InvalidOperationException($"Unexpected attribute name {attribute.Name} at {attribute.GetLocation().GetLineSpan()}."),
        };
        return ReviewNames.Contains(name.Identifier.ValueText, StringComparer.Ordinal);
    }

    /// <summary>One file's validated new content and the reviews it no longer holds.</summary>
    /// <param name="Path">The file.</param>
    /// <param name="Bytes">Its new bytes, with its byte-order mark when it had one.</param>
    /// <param name="Removed">The stale reviews removed from it.</param>
    private sealed record FileRewrite(string Path, byte[] Bytes, List<StaleReview> Removed);
}
