using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>
/// The text changes a cleanup fix makes to one document, applied together. A removed node takes its whole lines when it
/// stands on lines of its own, with the comments and documentation directly above it; one blank line beside it goes too
/// where two would otherwise meet, or where one would be left against an opening or closing brace or the end of the file. A node sharing a line
/// with other code is removed with the spaces after it. Changes that overlap, and removals that would delete a
/// preprocessor directive, fail.
/// </summary>
internal sealed class SourceEdits
{
    private readonly SourceText _text;
    private readonly string _path;
    private readonly List<TextChange> _changes = [];
    private readonly List<TextSpan> _removals = [];
    private readonly List<TextSpan> _directiveLines = [];

    /// <summary>Starts the edits of the document holding <paramref name="text"/> at <paramref name="path"/>.</summary>
    internal SourceEdits(SourceText text, string path)
    {
        _text = text;
        _path = path;
    }

    /// <summary>The document's text before the edits.</summary>
    internal SourceText Text => _text;

    /// <summary>Replaces <paramref name="span"/> with <paramref name="replacement"/>.</summary>
    internal void Replace(TextSpan span, string replacement) => _changes.Add(new TextChange(span, replacement));

    /// <summary>Inserts <paramref name="inserted"/> at <paramref name="position"/>.</summary>
    internal void Insert(int position, string inserted) => _changes.Add(new TextChange(new TextSpan(position, 0), inserted));

    /// <summary>Removes <paramref name="node"/>, with its lines when it stands on lines of its own.</summary>
    internal void Remove(SyntaxNode node) => RemoveRange(node, node);

    /// <summary>Removes <paramref name="node"/> as <see cref="Remove"/> does, but leaves the comments and
    /// documentation above it, which belong to what follows (an attribute list's member).</summary>
    internal void RemoveKeepingComments(SyntaxNode node) => RemoveRange(node, node, attached: false);

    /// <summary>Removes the line of a preprocessor directive.</summary>
    internal void RemoveDirectiveLine(SyntaxNode directive) =>
        _directiveLines.Add(_text.Lines.GetLineFromPosition(directive.SpanStart).SpanIncludingLineBreak);

    /// <summary>Removes <paramref name="first"/> through <paramref name="last"/>, siblings in source order, with the
    /// comments directly above <paramref name="first"/> unless <paramref name="attached"/> is false.</summary>
    internal void RemoveRange(SyntaxNode first, SyntaxNode last, bool attached = true)
    {
        int start = attached ? AttachedStart(first) : first.SpanStart;
        int end = last.Span.End;
        TextLine startLine = _text.Lines.GetLineFromPosition(start);
        TextLine endLine = _text.Lines.GetLineFromPosition(end);
        SyntaxToken previous = first.GetFirstToken().GetPreviousToken();
        SyntaxToken next = last.GetLastToken().GetNextToken();
        bool aloneAtStart = previous.RawKind == 0 || previous.Span.End <= startLine.Start;
        bool aloneAtEnd = next.RawKind == 0 || next.SpanStart >= endLine.EndIncludingLineBreak || next.Span.Length == 0;
        if (aloneAtStart && aloneAtEnd)
        {
            _removals.Add(TextSpan.FromBounds(startLine.Start, endLine.EndIncludingLineBreak));
            return;
        }
        start = first.SpanStart;
        int after = end;
        while (after < endLine.End && char.IsWhiteSpace(_text[after]))
            after++;
        if (after == endLine.End)
        {
            while (start > startLine.Start && char.IsWhiteSpace(_text[start - 1]))
                start--;
            _removals.Add(TextSpan.FromBounds(start, end));
            return;
        }
        _removals.Add(TextSpan.FromBounds(start, after));
    }

    /// <summary>The document's text with every edit applied.</summary>
    internal SourceText Apply()
    {
        var changes = new List<TextChange>(_changes);
        changes.AddRange(Blocks().Select(span => new TextChange(span, string.Empty)));
        changes = [.. changes.OrderBy(change => change.Span.Start).ThenBy(change => change.Span.End)];
        for (int index = 1; index < changes.Count; index++)
        {
            if (changes[index].Span.Start < changes[index - 1].Span.End)
                throw new InvalidOperationException($"{_path}: two cleanup edits overlap at {changes[index].Span.Start}.");
        }
        foreach (TextSpan removal in _removals)
        {
            if (_text.ToString(removal).Split('\n').Any(line => line.TrimStart().StartsWith("#", StringComparison.Ordinal)))
                throw new InvalidOperationException($"{_path}: a cleanup edit would delete a preprocessor directive at {removal.Start}.");
        }
        return _text.WithChanges(changes);
    }

    /// <summary>The removals, merged where they touch, each widened by the blank line that would otherwise be left
    /// doubled or against a brace.</summary>
    private List<TextSpan> Blocks()
    {
        var merged = new List<TextSpan>();
        foreach (TextSpan removal in _removals.Concat(_directiveLines).OrderBy(span => span.Start))
        {
            if (merged.Count > 0 && removal.Start <= merged[merged.Count - 1].End)
                merged[merged.Count - 1] = TextSpan.FromBounds(merged[merged.Count - 1].Start, Math.Max(merged[merged.Count - 1].End, removal.End));
            else
                merged.Add(removal);
        }
        var blocks = new List<TextSpan>();
        int previousEnd = 0;
        foreach (TextSpan block in merged)
        {
            TextSpan widened = WholeLines(block) ? Widen(block, previousEnd) : block;
            blocks.Add(widened);
            previousEnd = widened.End;
        }
        return blocks;
    }

    /// <summary>Whether a removal covers whole lines.</summary>
    private bool WholeLines(TextSpan span) =>
        _text.Lines.GetLineFromPosition(span.Start).Start == span.Start &&
        (span.End == _text.Length || _text.Lines.GetLineFromPosition(span.End).Start == span.End);

    /// <summary>A whole-line removal with the blank line before or after it that would otherwise be left doubled or
    /// against a brace.</summary>
    private TextSpan Widen(TextSpan block, int previousEnd)
    {
        int first = _text.Lines.GetLineFromPosition(block.Start).LineNumber;
        int last = _text.Lines.GetLineFromPosition(Math.Max(block.Start, block.End - 1)).LineNumber;
        bool blankBefore = first > 0 && IsBlank(first - 1);
        bool blankAfter = last + 1 < _text.Lines.Count && IsBlank(last + 1) && _text.Lines[last + 1].Start < _text.Length;
        bool openBefore = first == 0 || _text.Lines[first - 1].ToString().TrimEnd().EndsWith("{", StringComparison.Ordinal);
        bool closeAfter = last + 1 >= _text.Lines.Count || _text.Lines[last + 1].Start == _text.Length ||
            _text.Lines[last + 1].ToString().TrimStart().StartsWith("}", StringComparison.Ordinal);
        if (blankBefore && (blankAfter || closeAfter) && _text.Lines[first - 1].Start >= previousEnd)
            return TextSpan.FromBounds(_text.Lines[first - 1].Start, block.End);
        if (blankAfter && openBefore)
            return TextSpan.FromBounds(block.Start, _text.Lines[last + 1].EndIncludingLineBreak);
        return block;
    }

    /// <summary>Whether a line holds only white space.</summary>
    private bool IsBlank(int line) => string.IsNullOrWhiteSpace(_text.Lines[line].ToString());

    /// <summary>Where a node's removal starts: the comment or documentation lines directly above it in its leading
    /// trivia, or the node itself.</summary>
    internal int AttachedStart(SyntaxNode node)
    {
        int start = node.SpanStart;
        int line = _text.Lines.GetLineFromPosition(start).LineNumber;
        while (line > 0)
        {
            TextLine above = _text.Lines[line - 1];
            string content = above.ToString().Trim();
            if (above.Start < node.FullSpan.Start || content.Length == 0 ||
                !(content.StartsWith("//", StringComparison.Ordinal) || content.StartsWith("/*", StringComparison.Ordinal) ||
                  content.StartsWith("*", StringComparison.Ordinal)))
                break;
            line--;
            start = above.Start + (above.ToString().Length - above.ToString().TrimStart().Length);
        }
        return start;
    }
}
