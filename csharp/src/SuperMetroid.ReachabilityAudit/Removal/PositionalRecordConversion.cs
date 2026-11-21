using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Turns a positional record whose generated properties are all dead, but whose base call reads some of its parameters,
/// into a record without a parameter list (#2599): its constructor takes the parameters the base call reads, under
/// constructor-parameter names, and passes them on as <c>: base(...)</c>, so retiring the properties leaves no read of a
/// parameter that is gone. The cleanup of 0ea41f339b deleted <c>KraidColours.BandShade</c>'s parameter <c>Shade</c>
/// whole, and its base call <c>: BodyShown(Shade)</c> no longer compiled (CS0103). The record's <c>param</c>
/// documentation of a kept parameter moves to the constructor; that of every other parameter goes with it.
/// </summary>
internal static class PositionalRecordConversion
{
    /// <summary>The file's text with every conversion applied.</summary>
    internal static string Apply(string text, CSharpParseOptions options, IReadOnlyList<RecordConversion> conversions, string path)
    {
        foreach (RecordConversion conversion in conversions)
            text = Convert(text, options, conversion, path);
        return text;
    }

    /// <summary>The file's text with one record converted.</summary>
    private static string Convert(string text, CSharpParseOptions options, RecordConversion conversion, string path)
    {
        SyntaxNode root = CSharpSyntaxTree.ParseText(text, options).GetRoot();
        var found = root.DescendantNodes().OfType<RecordDeclarationSyntax>().Where(record => record.Identifier.Text == conversion.Record &&
            record.ParameterList is { } list && list.Parameters.Select(parameter => parameter.Identifier.Text).SequenceEqual(conversion.Parameters)).ToList();
        if (found.Count != 1)
            throw new InvalidOperationException($"{path}: expected one record {conversion.Record}({string.Join(", ", conversion.Parameters)}) to convert, found {found.Count}.");
        RecordDeclarationSyntax record = found[0];
        SourceText source = SourceText.From(text);
        string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        TextLine keywordLine = source.Lines.GetLineFromPosition(record.Keyword.SpanStart);
        string indent = Indentation(keywordLine.ToString());
        string inner = indent + "    ";
        PrimaryConstructorBaseTypeSyntax baseCall = record.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>().SingleOrDefault()
            ?? throw new InvalidOperationException($"{path}: record {conversion.Record} has no base call to keep its parameters for.");
        var names = conversion.Parameters.Where(conversion.Kept.Contains).ToDictionary(name => name, ParameterName, StringComparer.Ordinal);

        var changes = new List<TextChange>();
        var documentation = new List<string>();
        foreach (XmlElementSyntax element in Documentation(record))
        {
            string name = element.StartTag.Attributes.OfType<XmlNameAttributeSyntax>().First().Identifier.Identifier.ValueText;
            if (!conversion.Parameters.Contains(name))
                continue;
            TextLine first = source.Lines.GetLineFromPosition(element.SpanStart);
            TextLine last = source.Lines.GetLineFromPosition(element.Span.End);
            bool alone = source.ToString(TextSpan.FromBounds(first.Start, element.SpanStart)).Trim() == "///" &&
                source.ToString(TextSpan.FromBounds(element.Span.End, last.End)).Trim().Length == 0;
            changes.Add(new TextChange(alone ? TextSpan.FromBounds(first.Start, last.EndIncludingLineBreak) : element.Span, ""));
            if (names.TryGetValue(name, out string? renamed))
                documentation.AddRange(Relined(element.ToString().Replace($"name=\"{name}\"", $"name=\"{renamed}\"", StringComparison.Ordinal), inner));
        }
        changes.Add(new TextChange(record.ParameterList!.Span, ""));
        changes.Add(new TextChange(baseCall.ArgumentList.Span, ""));

        var parameters = record.ParameterList.Parameters.Where(parameter => names.ContainsKey(parameter.Identifier.Text))
            .Select(parameter => Parameter(parameter, names[parameter.Identifier.Text]));
        string access = record.Modifiers.Any(SyntaxKind.AbstractKeyword) ? "protected" : "public";
        string constructor =
            $"{inner}/// <summary>Creates the record from what its base takes.</summary>{newline}" +
            string.Concat(documentation.Select(line => line + newline)) +
            $"{inner}{access} {record.Identifier.Text}({string.Join(", ", parameters)}){newline}" +
            $"{inner}    : base{Arguments(baseCall.ArgumentList, names)}{newline}" +
            $"{inner}{{{newline}{inner}}}{newline}";
        if (record.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
            changes.Add(new TextChange(record.SemicolonToken.Span, $"{newline}{indent}{{{newline}{constructor}{indent}}}"));
        else
            changes.Add(new TextChange(new TextSpan(source.Lines.GetLineFromPosition(record.OpenBraceToken.SpanStart).EndIncludingLineBreak, 0),
                constructor + (record.Members.Count > 0 ? newline : "")));
        return source.WithChanges(changes).ToString();
    }

    /// <summary>The <c>param</c> elements of a record's own documentation.</summary>
    private static IEnumerable<XmlElementSyntax> Documentation(RecordDeclarationSyntax record) =>
        record.GetLeadingTrivia().Select(trivia => trivia.GetStructure()).OfType<DocumentationCommentTriviaSyntax>()
            .SelectMany(comment => comment.Content.OfType<XmlElementSyntax>())
            .Where(element => element.StartTag.Name.ToString() == "param" && element.StartTag.Attributes.OfType<XmlNameAttributeSyntax>().Any());

    /// <summary>A documentation element's lines, each a documentation line at <paramref name="indent"/>.</summary>
    private static IEnumerable<string> Relined(string element, string indent) =>
        element.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
            .Select(line => line.TrimStart())
            .Select(line => line.StartsWith("///", StringComparison.Ordinal) ? line[3..].TrimStart() : line)
            .Select(line => $"{indent}/// {line}");

    /// <summary>A kept positional parameter as a constructor parameter: without attributes aimed at the property or its
    /// field, which a constructor parameter cannot carry, and under its constructor-parameter name.</summary>
    private static string Parameter(ParameterSyntax parameter, string name)
    {
        var attributes = parameter.AttributeLists.Where(list => list.Target?.Identifier.ValueText is not ("property" or "field"))
            .Select(list => list.ToString() + " ");
        string modifiers = string.Concat(parameter.Modifiers.Select(modifier => modifier.Text + " "));
        return string.Concat(attributes) + modifiers + parameter.Type + " " + name + (parameter.Default is { } value ? " " + value : "");
    }

    /// <summary>The base call's arguments with every read of a kept parameter under its new name.</summary>
    private static string Arguments(ArgumentListSyntax arguments, Dictionary<string, string> names)
    {
        SourceText text = SourceText.From(arguments.ToString());
        int start = arguments.SpanStart;
        var renames = arguments.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Where(identifier => names.ContainsKey(identifier.Identifier.Text) &&
                !(identifier.Parent is MemberAccessExpressionSyntax access && access.Name == identifier) &&
                identifier.Parent is not NameColonSyntax)
            .Select(identifier => new TextChange(new TextSpan(identifier.Identifier.SpanStart - start, identifier.Identifier.Span.Length),
                names[identifier.Identifier.Text]));
        return text.WithChanges(renames).ToString();
    }

    /// <summary>A positional parameter's name as a constructor parameter: its first letter lower-case, escaped when that
    /// is a keyword.</summary>
    private static string ParameterName(string name)
    {
        string lowered = char.ToLowerInvariant(name[0]) + name[1..];
        return SyntaxFacts.GetKeywordKind(lowered) != SyntaxKind.None ? "@" + lowered : lowered;
    }

    /// <summary>The white space a line starts with.</summary>
    private static string Indentation(string line) => line[..(line.Length - line.TrimStart().Length)];
}
