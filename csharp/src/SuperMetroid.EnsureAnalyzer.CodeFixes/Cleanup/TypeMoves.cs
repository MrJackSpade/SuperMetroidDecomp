using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

/// <summary>
/// Moving a top-level type to the file named after it, for the file-layout fixes (SME6400, SME6401): which file a type
/// belongs in, the text that carries it with what it was compiled under, a new file for it, and adding it to a file that
/// exists already, which deletes nothing.
/// </summary>
internal static class TypeMoves
{
    /// <summary>Whether a type belongs in the file of <paramref name="stem"/>: SME6401's name, or the type's name with
    /// another case, which a file system that ignores case cannot hold beside it.</summary>
    internal static bool NamesFile(string stem, MemberDeclarationSyntax declaration) =>
        TypeFileNameAnalyzer.NamesType(stem, declaration) ||
        string.Equals(stem, OneTypePerFileAnalyzer.NameOf(declaration), StringComparison.OrdinalIgnoreCase);

    /// <summary>The document of the solution at <paramref name="path"/>, compared without case, or null.</summary>
    internal static Document? DocumentAt(Solution solution, string path) =>
        solution.Projects.SelectMany(project => project.Documents)
            .FirstOrDefault(document => string.Equals(document.FilePath, path, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The text that carries <paramref name="declaration"/> to another file: the directives that set the nullable context
    /// and suppressed warnings it was compiled under, then the type with the comments directly above it, out of its block
    /// namespaces' indentation, wrapped in the <c>#if</c> branches that hold it.
    /// </summary>
    internal static string Block(CompilationUnitSyntax root, SourceText text, SourceEdits edits, MemberDeclarationSyntax declaration, string newline)
    {
        int depth = declaration.Ancestors().OfType<NamespaceDeclarationSyntax>().Count();
        int firstLine = text.Lines.GetLineFromPosition(edits.AttachedStart(declaration)).LineNumber;
        string body = text.ToString(TextSpan.FromBounds(text.Lines[firstLine].Start, declaration.Span.End));
        string dedented = Dedent(body, firstLine, Directives.TokenLines(text, declaration), depth, newline);
        var lines = Directives.State(root, declaration.SpanStart);
        lines.Add(Directives.Wrap(dedented, Directives.EnclosingConditions(root, declaration.SpanStart), newline));
        return string.Join(newline, lines);
    }

    /// <summary>The namespace a type is declared in, or the empty string.</summary>
    internal static string Namespace(MemberDeclarationSyntax declaration) =>
        string.Join(".", declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(ns => ns.Name.ToString()));

    /// <summary>The non-global using directives in scope at <paramref name="declaration"/>: the file's and its namespaces'.</summary>
    internal static List<string> Usings(CompilationUnitSyntax root, MemberDeclarationSyntax declaration) =>
        [.. root.Usings.Concat(declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().SelectMany(ns => ns.Usings))
            .Where(directive => directive.GlobalKeyword.IsKind(SyntaxKind.None))
            .Select(directive => directive.WithoutTrivia().ToFullString())];

    /// <summary>
    /// Adds a moved type to the existing file <paramref name="target"/>: the source's extern aliases, symbols and usings it
    /// lacks go before the file's own, and the type follows the file's code, inside the file's file-scoped namespace when
    /// it is the type's, or in a namespace block of its own, which a file with a file-scoped namespace cannot hold, so the
    /// build fails there.
    /// </summary>
    internal static async Task AppendAsync(SolutionEdits edits, Document target, CompilationUnitSyntax source, MemberDeclarationSyntax declaration,
        string block, string newline, CancellationToken cancellationToken)
    {
        SyntaxTree tree = await target.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{target.FilePath} has no syntax tree.");
        var root = (CompilationUnitSyntax)await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        SourceEdits targetEdits = await edits.ForAsync(tree, cancellationToken).ConfigureAwait(false);
        SourceText text = targetEdits.Text;
        // Symbols precede every token of a file, extern aliases precede its usings, and its usings precede its code.
        var presentSymbols = new HashSet<string>(Directives.Symbols(root, text.Length));
        var symbols = Directives.Symbols(source, declaration.SpanStart).Where(symbol => !presentSymbols.Contains(symbol)).ToList();
        if (symbols.Count > 0)
            targetEdits.Insert(0, string.Concat(symbols.Select(line => line + newline)));
        var presentExterns = new HashSet<string>(root.Externs.Select(directive => directive.WithoutTrivia().ToFullString()));
        var externs = source.Externs.Select(directive => directive.WithoutTrivia().ToFullString()).Where(directive => !presentExterns.Contains(directive)).ToList();
        SyntaxNode? firstImport = (SyntaxNode?)root.Externs.FirstOrDefault() ?? root.Usings.FirstOrDefault();
        if (externs.Count > 0)
        {
            targetEdits.Insert(firstImport is null ? 0 : text.Lines.GetLineFromPosition(firstImport.SpanStart).Start,
                string.Concat(externs.Select(line => line + newline)) + (firstImport is null && root.Usings.Count == 0 ? newline : ""));
        }
        var presentUsings = new HashSet<string>(root.Usings.Concat(root.Members.OfType<BaseNamespaceDeclarationSyntax>().SelectMany(ns => ns.Usings))
            .Select(directive => directive.WithoutTrivia().ToFullString()));
        var usings = Usings(source, declaration).Where(directive => !presentUsings.Contains(directive)).ToList();
        if (usings.Count > 0)
        {
            string inserted = string.Concat(usings.Select(line => line + newline));
            if (root.Usings.Count > 0)
                targetEdits.Insert(text.Lines.GetLineFromPosition(root.Usings[root.Usings.Count - 1].Span.End).EndIncludingLineBreak, inserted);
            else
                targetEdits.Insert(firstImport is null ? 0 : text.Lines.GetLineFromPosition(root.Externs[root.Externs.Count - 1].Span.End).EndIncludingLineBreak,
                    inserted + newline);
        }
        string name = Namespace(declaration);
        bool sameNamespace = root.Members.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault() is { } fileScoped
            ? fileScoped.Name.ToString() == name
            : name.Length == 0;
        string appended = sameNamespace
            ? block
            : "namespace " + name + newline + "{" + newline + Indent(block, newline) + newline + "}";
        string separator = text.Length > 0 && !text.ToString().EndsWith("\n", StringComparison.Ordinal) ? newline + newline : newline;
        targetEdits.Insert(text.Length, separator + appended + newline);
    }

    /// <summary>Indents every non-empty line one level.</summary>
    internal static string Indent(string lines, string newline) =>
        string.Join(newline, lines.Replace("\r\n", "\n").Split('\n').Select(line => line.Length > 0 ? "    " + line : line));

    /// <summary>
    /// Removes <paramref name="levels"/> levels of block-namespace indentation from every line of <paramref name="lines"/>,
    /// which starts at line <paramref name="firstLine"/> of its file, but those a token spanning lines holds.
    /// </summary>
    internal static string Dedent(string lines, int firstLine, HashSet<int> kept, int levels, string newline)
    {
        string indent = new(' ', 4 * levels);
        string[] split = lines.Replace("\r\n", "\n").Split('\n');
        return string.Join(newline, split.Select((line, index) =>
            !kept.Contains(firstLine + index) && line.StartsWith(indent, StringComparison.Ordinal) ? line.Substring(indent.Length) : line));
    }

    /// <summary>The file's line break.</summary>
    internal static string Newline(CompilationUnitSyntax root) =>
        root.DescendantTrivia().FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)).ToString() is { Length: > 0 } eol ? eol : "\n";

    /// <summary>A file the fix creates: its head from the first type moved into it, and every type moved into it.</summary>
    internal sealed class NewFile
    {
        private readonly string _newline;
        private readonly string _head;
        private readonly string _namespace;
        private readonly List<string> _blocks = [];

        /// <summary>Starts the file of <paramref name="declaration"/>, moved out of <paramref name="root"/>.</summary>
        internal NewFile(CompilationUnitSyntax root, MemberDeclarationSyntax declaration, string block, string newline)
        {
            _newline = newline;
            _namespace = Namespace(declaration);
            var head = new System.Text.StringBuilder();
            var top = Directives.Symbols(root, declaration.SpanStart)
                .Concat(root.Externs.Select(directive => directive.WithoutTrivia().ToFullString()))
                .Concat(root.Usings.Where(directive => directive.GlobalKeyword.IsKind(SyntaxKind.None)).Select(directive => directive.WithoutTrivia().ToFullString()))
                .ToList();
            foreach (string line in top)
                head.Append(line).Append(newline);
            if (top.Count > 0)
                head.Append(newline);
            if (_namespace.Length > 0)
            {
                head.Append("namespace ").Append(_namespace).Append(';').Append(newline).Append(newline);
                var inner = declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().SelectMany(ns => ns.Usings).ToList();
                foreach (UsingDirectiveSyntax directive in inner)
                    head.Append(directive.WithoutTrivia().ToFullString()).Append(newline);
                if (inner.Count > 0)
                    head.Append(newline);
            }
            _head = head.ToString();
            _blocks.Add(block);
        }

        /// <summary>Adds another type moved into the file; one of another namespace goes in a namespace block of its own,
        /// which a file with a file-scoped namespace cannot hold, so the build fails there.</summary>
        internal void Add(string block, string @namespace) =>
            _blocks.Add(@namespace == _namespace ? block : "namespace " + @namespace + _newline + "{" + _newline + Indent(block, _newline) + _newline + "}");

        /// <summary>The file's text.</summary>
        internal string Text() => (_head + string.Join(_newline + _newline, _blocks) + _newline).Replace("\r\n", "\n").Replace("\n", _newline);
    }
}
