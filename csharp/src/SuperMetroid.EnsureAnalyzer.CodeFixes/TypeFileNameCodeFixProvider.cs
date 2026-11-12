using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>
/// Renames a file to the name SME6401 expects for the one type it declares, <c>Type.cs</c> in the same folder: the
/// asynchronous cleanup's fix (#1667). The cleanup's runner records it as a deleted file and a created one with the same
/// text, which Git records as a rename, and retargets every <c>.projitems</c> and <c>&lt;Compile&gt;</c> item (its
/// <c>Link</c> included) that named the old path. No compiled code changes. A file whose expected name is taken fails
/// the fix.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeFileNameCodeFixProvider)), Shared]
public sealed class TypeFileNameCodeFixProvider : DocumentFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [TypeFileNameAnalyzer.Id];

    /// <inheritdoc />
    internal override string Title => "Name the file after its type";

    /// <inheritdoc />
    internal override async Task<bool> CanFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        SyntaxNode? found = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        return found is CompilationUnitSyntax root && Refusal(document, root) is null;
    }

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        if (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax root)
            throw new InvalidOperationException($"{document.Name} is not a C# file.");
        if (Refusal(document, root) is { } reason)
            throw new CleanupFailure(diagnostics[0], reason);
        string target = Target(document, root);
        SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        return document.Project.Solution.RemoveDocument(document.Id)
            .AddDocument(DocumentId.CreateNewId(document.Project.Id), Path.GetFileName(target), text, document.Folders, target);
    }

    /// <summary>The path the file should have: its type's name in the same folder.</summary>
    private static string Target(Document document, CompilationUnitSyntax root) =>
        Path.Combine(Path.GetDirectoryName(document.FilePath) ?? string.Empty,
            OneTypePerFileAnalyzer.NameOf(OneTypePerFileAnalyzer.TopLevelTypes(root)[0]) + ".cs");

    /// <summary>Why the file cannot be renamed, or null when it can.</summary>
    private static string? Refusal(Document document, CompilationUnitSyntax root)
    {
        if (document.FilePath is null)
            return "the document has no file";
        if (OneTypePerFileAnalyzer.TopLevelTypes(root).Length != 1)
            return "the file does not declare exactly one top-level type";
        string target = Target(document, root);
        if (File.Exists(target) || document.Project.Solution.GetDocumentIdsWithFilePath(target).Any())
            return $"{Path.GetFileName(target)} exists already";
        return null;
    }
}
