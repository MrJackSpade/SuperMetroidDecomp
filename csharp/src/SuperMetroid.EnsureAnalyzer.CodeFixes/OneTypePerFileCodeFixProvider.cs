using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Moves a reported top-level type into a sibling file named after it (#626), carrying the file's using directives
/// and the type's namespace, attributes and documentation. The fix is offered only when that move is unambiguous: the
/// file has no preprocessor directives, no top-level statements and no file-local types, and no sibling already uses
/// the name. Anything else needs manual placement under the module map.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OneTypePerFileCodeFixProvider)), Shared]
public sealed class OneTypePerFileCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [OneTypePerFileAnalyzer.Id];

    /// <inheritdoc />
    public override FixAllProvider? GetFixAllProvider() => null;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? found = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (found is not CompilationUnitSyntax root || context.Document.FilePath is not { } path)
            return;
        if (root.DescendantTrivia().Any(trivia => trivia.IsDirective) || root.Members.OfType<GlobalStatementSyntax>().Any())
            return;
        ImmutableArray<MemberDeclarationSyntax> types = OneTypePerFileAnalyzer.TopLevelTypes(root);
        if (types.Any(type => type.Modifiers.Any(SyntaxKind.FileKeyword)))
            return;
        string directory = Path.GetDirectoryName(path) ?? string.Empty;
        foreach (Diagnostic diagnostic in context.Diagnostics)
        {
            MemberDeclarationSyntax? declaration = types.FirstOrDefault(type => type.Span.Contains(diagnostic.Location.SourceSpan));
            if (declaration is null || declaration.Parent is BaseNamespaceDeclarationSyntax { Parent: BaseNamespaceDeclarationSyntax })
                continue;
            string name = OneTypePerFileAnalyzer.NameOf(declaration);
            string target = Path.Combine(directory, name + ".cs");
            if (types.Count(type => OneTypePerFileAnalyzer.NameOf(type) == name) > 1 || File.Exists(target) ||
                context.Document.Project.Documents.Any(document => string.Equals(document.FilePath, target, StringComparison.OrdinalIgnoreCase)))
                continue;
            context.RegisterCodeFix(
                CodeAction.Create($"Move '{name}' to {name}.cs",
                    token => Task.FromResult(Move(context.Document, root, declaration, name, target)),
                    equivalenceKey: nameof(OneTypePerFileCodeFixProvider)),
                diagnostic);
        }
    }

    /// <summary>Adds the sibling file holding <paramref name="declaration"/> and removes it from the original.</summary>
    private static Solution Move(Document document, CompilationUnitSyntax root, MemberDeclarationSyntax declaration, string name, string target)
    {
        string newline = root.DescendantTrivia().FirstOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)).ToString() is { Length: > 0 } eol ? eol : "\n";
        var text = new System.Text.StringBuilder();
        foreach (UsingDirectiveSyntax directive in root.Usings)
            text.Append(directive.WithoutTrivia().ToFullString()).Append(newline);
        if (root.Usings.Count > 0)
            text.Append(newline);
        if (declaration.Parent is BaseNamespaceDeclarationSyntax ns)
            text.Append("namespace ").Append(ns.Name.ToString()).Append(';').Append(newline).Append(newline);
        string body = declaration.WithoutLeadingTrivia().ToFullString().TrimEnd();
        string leading = Dedent(declaration.GetLeadingTrivia().ToFullString(), declaration.Parent is NamespaceDeclarationSyntax).TrimStart('\r', '\n');
        if (declaration.Parent is NamespaceDeclarationSyntax)
            body = Dedent(body, blockScoped: true);
        text.Append(leading).Append(body).Append(newline);

        CompilationUnitSyntax remaining = root.RemoveNode(declaration, SyntaxRemoveOptions.KeepNoTrivia) ??
            throw new InvalidOperationException($"Removing '{name}' left no compilation unit.");
        Solution solution = document.WithSyntaxRoot(remaining).Project.Solution;
        DocumentId added = DocumentId.CreateNewId(document.Project.Id);
        string content = text.ToString().Replace("\r\n", "\n").Replace("\n", newline);
        return solution.AddDocument(added, name + ".cs", SourceText.From(content), document.Folders, target);
    }

    /// <summary>Removes one level of block-namespace indentation from every line.</summary>
    private static string Dedent(string text, bool blockScoped)
    {
        if (!blockScoped)
            return text;
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        return string.Join("\n", lines.Select(line => line.StartsWith("    ", StringComparison.Ordinal) ? line.Substring(4) : line));
    }
}
