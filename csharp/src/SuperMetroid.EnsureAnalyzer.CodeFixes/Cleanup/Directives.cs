using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>
/// What a cleanup fix that moves code to another place needs to carry with it so the code compiles as it did: the
/// conditional branches it lies in, the directives that set the state it was compiled in, and the lines a re-indent must
/// leave alone because a token spanning lines holds them.
/// </summary>
internal static class Directives
{
    /// <summary>
    /// The conditions of the <c>#if</c> branches that hold <paramref name="position"/>, outermost first, each as one
    /// preprocessor expression: a branch after others of its group is taken only when none of them is, so an <c>#elif</c>
    /// or <c>#else</c> branch's condition names the earlier ones negated.
    /// </summary>
    internal static List<string> EnclosingConditions(SyntaxNode root, int position)
    {
        var groups = new List<(List<string> Earlier, string? Current)>();
        foreach (DirectiveTriviaSyntax directive in DirectivesBefore(root, position))
        {
            if (directive is IfDirectiveTriviaSyntax open)
                groups.Add(([], open.Condition.ToString()));
            else if (groups.Count == 0)
                continue;
            else if (directive is ElifDirectiveTriviaSyntax elif)
                groups[groups.Count - 1] = (Earlier(groups[groups.Count - 1]), elif.Condition.ToString());
            else if (directive is ElseDirectiveTriviaSyntax)
                groups[groups.Count - 1] = (Earlier(groups[groups.Count - 1]), null);
            else if (directive is EndIfDirectiveTriviaSyntax)
                groups.RemoveAt(groups.Count - 1);
        }
        return [.. groups.Select(group => Condition(group.Earlier, group.Current))];
    }

    /// <summary>
    /// The <c>#define</c> and <c>#undef</c> directives before <paramref name="position"/>, as lines, which a new file
    /// must start with to compile its code under the same symbols.
    /// </summary>
    internal static List<string> Symbols(SyntaxNode root, int position) =>
        [.. DirectivesBefore(root, position).Where(directive => directive is DefineDirectiveTriviaSyntax or UndefDirectiveTriviaSyntax)
            .Select(directive => directive.ToString().Trim())];

    /// <summary>
    /// The <c>#nullable</c> and <c>#pragma warning</c> directives before <paramref name="position"/>, as lines in order.
    /// Played from a file's start they set the nullable context and the suppressed warnings that held at the position.
    /// </summary>
    internal static List<string> State(SyntaxNode root, int position) =>
        [.. DirectivesBefore(root, position).Where(directive => directive is NullableDirectiveTriviaSyntax or PragmaWarningDirectiveTriviaSyntax)
            .Select(directive => directive.ToString().Trim())];

    /// <summary>Wraps <paramref name="lines"/> in an <c>#if</c> for each condition, outermost first.</summary>
    internal static string Wrap(string lines, List<string> conditions, string newline)
    {
        string wrapped = lines;
        for (int index = conditions.Count - 1; index >= 0; index--)
            wrapped = "#if " + conditions[index] + newline + wrapped + newline + "#endif";
        return wrapped;
    }

    /// <summary>
    /// The numbers of the lines of <paramref name="text"/> inside <paramref name="node"/> that start within a token
    /// spanning lines (a verbatim or raw string, an interpolated string): their leading white space is the token's text,
    /// so a re-indent must leave them as they are.
    /// </summary>
    internal static HashSet<int> TokenLines(SourceText text, SyntaxNode node)
    {
        var lines = new HashSet<int>();
        void Add(TextSpan span)
        {
            int first = text.Lines.GetLineFromPosition(span.Start).LineNumber;
            int last = text.Lines.GetLineFromPosition(span.End).LineNumber;
            for (int line = first + 1; line <= last; line++)
                lines.Add(line);
        }
        foreach (SyntaxToken token in node.DescendantTokens())
        {
            if (token.Span.Length > 0 && token.ToString().Contains("\n"))
                Add(token.Span);
        }
        foreach (InterpolatedStringExpressionSyntax interpolated in node.DescendantNodesAndSelf().OfType<InterpolatedStringExpressionSyntax>())
        {
            if (interpolated.ToString().Contains("\n"))
                Add(interpolated.Span);
        }
        return lines;
    }

    /// <summary>Every directive of the file that starts before <paramref name="position"/>, in order.</summary>
    private static IEnumerable<DirectiveTriviaSyntax> DirectivesBefore(SyntaxNode root, int position) =>
        root.DescendantTrivia(descendIntoTrivia: true)
            .Where(trivia => trivia.IsDirective && trivia.SpanStart < position)
            .Select(trivia => trivia.GetStructure()).OfType<DirectiveTriviaSyntax>();

    /// <summary>A group's earlier conditions with its current one added.</summary>
    private static List<string> Earlier((List<string> Earlier, string? Current) group) =>
        group.Current is null ? group.Earlier : [.. group.Earlier, group.Current];

    /// <summary>One branch's condition: none of the earlier taken, and its own.</summary>
    private static string Condition(List<string> earlier, string? current)
    {
        var parts = earlier.Select(condition => $"!({condition})").ToList();
        if (current is not null)
            parts.Add(earlier.Count == 0 ? current : $"({current})");
        return string.Join(" && ", parts);
    }
}
