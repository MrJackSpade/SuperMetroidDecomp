using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Moves each reported top-level type into a sibling file named after it (#626), the asynchronous cleanup's fix for
/// SME6400 (#1666). The new file is named as SME6401 expects (<c>Pair&lt;T&gt;</c> goes to <c>Pair.cs</c>) and takes the
/// source file's using directives (the cleanup's IDE0005 pass removes the unneeded ones; global usings stay where they
/// are), the type's namespace as a file-scoped namespace with the usings declared inside it, and the comments and
/// documentation directly above the type with its attributes. The cleanup's runner adds the new file to every project
/// that compiles the source file, and beside it in an explicit <c>.projitems</c> or <c>&lt;Compile&gt;</c> list. No
/// compiled code changes. The fix is offered only when the move is unambiguous: the file has no preprocessor
/// directives, extern aliases or top-level statements, the type is not file-local, no other type of the file has its
/// name, no file of that name exists, and no string spanning lines would be re-indented out of a namespace block. Anything else needs manual placement under the module map.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OneTypePerFileCodeFixProvider)), Shared]
public sealed class OneTypePerFileCodeFixProvider : DocumentFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [OneTypePerFileAnalyzer.Id];

    /// <inheritdoc />
    internal override string Title => "Move the type to its own file";

    /// <inheritdoc />
    internal override async Task<bool> CanFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        SyntaxNode? found = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        return found is CompilationUnitSyntax root && Refusal(document, root, diagnostic) is null;
    }

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        if (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax root || document.FilePath is not { } path)
            throw new InvalidOperationException($"{document.Name} is not a C# file on disk.");
        SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var edits = new SourceEdits(text, path);
        var moved = new List<(string Name, string Content)>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            if (Refusal(document, root, diagnostic) is { } reason)
                throw new CleanupFailure(diagnostic, reason);
            MemberDeclarationSyntax declaration = Declaration(root, diagnostic)!;
            moved.Add((OneTypePerFileAnalyzer.NameOf(declaration), NewFile(root, text, edits, declaration)));
            edits.Remove(declaration);
        }
        Solution solution = document.WithText(edits.Apply()).Project.Solution;
        string directory = Path.GetDirectoryName(path) ?? string.Empty;
        foreach ((string name, string content) in moved)
        {
            DocumentId added = DocumentId.CreateNewId(document.Project.Id);
            solution = solution.AddDocument(added, name + ".cs", SourceText.From(content), document.Folders, Path.Combine(directory, name + ".cs"));
        }
        return solution;
    }

    /// <summary>The reported top-level type declaration.</summary>
    private static MemberDeclarationSyntax? Declaration(CompilationUnitSyntax root, Diagnostic diagnostic) =>
        OneTypePerFileAnalyzer.TopLevelTypes(root).FirstOrDefault(type => type.Span.Contains(diagnostic.Location.SourceSpan));

    /// <summary>Why the reported type cannot be moved unambiguously, or null when it can.</summary>
    private static string? Refusal(Document document, CompilationUnitSyntax root, Diagnostic diagnostic)
    {
        if (document.FilePath is not { } path)
            return "the document has no file";
        if (root.DescendantTrivia().Any(trivia => trivia.IsDirective))
            return "the file has preprocessor directives";
        if (root.Externs.Count > 0)
            return "the file has extern aliases";
        if (root.Members.OfType<GlobalStatementSyntax>().Any())
            return "the file has top-level statements";
        ImmutableArray<MemberDeclarationSyntax> types = OneTypePerFileAnalyzer.TopLevelTypes(root);
        if (Declaration(root, diagnostic) is not { } declaration)
            return "no top-level type holds the finding";
        if (declaration.Modifiers.Any(SyntaxKind.FileKeyword))
            return "the type is file-local";
        if (declaration.Ancestors().OfType<NamespaceDeclarationSyntax>().Any() &&
            declaration.DescendantTokens().Any(token => token.ToString().Contains("\n")))
            return "the type holds text spanning lines, which taking it out of its namespace block would re-indent";
        string name = OneTypePerFileAnalyzer.NameOf(declaration);
        string target = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, name + ".cs");
        if (types.Count(type => OneTypePerFileAnalyzer.NameOf(type) == name) > 1)
            return $"another type of the file is also named '{name}'";
        if (File.Exists(target) || document.Project.Documents.Any(other => string.Equals(other.FilePath, target, StringComparison.OrdinalIgnoreCase)))
            return $"{name}.cs exists already";
        return null;
    }

    /// <summary>The text of the new file holding <paramref name="declaration"/>.</summary>
    private static string NewFile(CompilationUnitSyntax root, SourceText text, SourceEdits edits, MemberDeclarationSyntax declaration)
    {
        string newline = root.DescendantTrivia().FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)).ToString() is { Length: > 0 } eol ? eol : "\n";
        var namespaces = declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().ToList();
        int depth = namespaces.Count(ns => ns is NamespaceDeclarationSyntax);
        var content = new System.Text.StringBuilder();
        var usings = root.Usings.Where(directive => directive.GlobalKeyword.IsKind(SyntaxKind.None)).ToList();
        foreach (UsingDirectiveSyntax directive in usings)
            content.Append(directive.WithoutTrivia().ToFullString()).Append(newline);
        if (usings.Count > 0)
            content.Append(newline);
        if (namespaces.Count > 0)
        {
            content.Append("namespace ").Append(string.Join(".", namespaces.Select(ns => ns.Name.ToString()))).Append(';').Append(newline).Append(newline);
            var inner = namespaces.SelectMany(ns => ns.Usings).ToList();
            foreach (UsingDirectiveSyntax directive in inner)
                content.Append(directive.WithoutTrivia().ToFullString()).Append(newline);
            if (inner.Count > 0)
                content.Append(newline);
        }
        int start = text.Lines.GetLineFromPosition(edits.AttachedStart(declaration)).Start;
        string leading = text.ToString(TextSpan.FromBounds(start, declaration.SpanStart));
        string body = text.ToString(declaration.Span);
        content.Append(Dedent(leading + body, depth)).Append(newline);
        return content.ToString().Replace("\r\n", "\n").Replace("\n", newline);
    }

    /// <summary>Removes <paramref name="levels"/> levels of block-namespace indentation from every line.</summary>
    private static string Dedent(string text, int levels)
    {
        string indent = new(' ', 4 * levels);
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        return string.Join("\n", lines.Select(line => line.StartsWith(indent, StringComparison.Ordinal) ? line.Substring(indent.Length) : line));
    }
}
