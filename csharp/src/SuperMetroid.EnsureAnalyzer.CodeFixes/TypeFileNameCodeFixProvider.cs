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
/// Names a file after the one type it declares, <c>Type.cs</c> in the same folder: the asynchronous cleanup's fix for
/// SME6401 (#1667), applied at every finding (#2599). The cleanup's runner records the rename as a deleted file and a
/// created one with the same text, which Git records as a rename, and retargets every <c>.projitems</c> and
/// <c>&lt;Compile&gt;</c> item (its <c>Link</c> included) that named the old path. A name that differs only in case is
/// the same file on a file system that ignores case, and is renamed all the same. When another file has the name
/// already, the type goes into it beside that file's code (<see cref="TypeMoves.AppendAsync"/>), which deletes nothing;
/// the file it leaves is deleted when nothing else is left in it, and the build or the next run of the file-layout rules
/// reports what the two files' code now shares. No compiled code changes.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TypeFileNameCodeFixProvider)), Shared]
public sealed class TypeFileNameCodeFixProvider : DocumentFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [TypeFileNameAnalyzer.Id];

    /// <inheritdoc />
    internal override string Title => "Name the file after its type";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        if (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax root || document.FilePath is not { } path)
            throw new InvalidOperationException($"{document.Name} is not a C# file on disk.");
        ImmutableArray<MemberDeclarationSyntax> types = OneTypePerFileAnalyzer.TopLevelTypes(root);
        if (types.Length != 1)
            throw new InvalidOperationException($"{path}: {TypeFileNameAnalyzer.Id} is reported for a file that declares {types.Length} top-level types, not one.");
        MemberDeclarationSyntax declaration = types[0];
        string target = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, OneTypePerFileAnalyzer.NameOf(declaration) + ".cs");
        if (string.Equals(target, path, StringComparison.Ordinal))
            throw new InvalidOperationException($"{path}: {TypeFileNameAnalyzer.Id} is reported for a file already named after its type.");
        SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        Solution solution = document.Project.Solution;
        bool sameFile = string.Equals(target, path, StringComparison.OrdinalIgnoreCase);
        if (!sameFile && TypeMoves.DocumentAt(solution, target) is { } existing)
            return await MoveIntoAsync(document, root, text, declaration, existing, cancellationToken).ConfigureAwait(false);
        if (!sameFile && File.Exists(target))
            throw new InvalidOperationException($"{target} exists but no project of the solution compiles it, so {path} cannot take its name.");
        return solution.RemoveDocument(document.Id)
            .AddDocument(DocumentId.CreateNewId(document.Project.Id), Path.GetFileName(target), text, document.Folders, target);
    }

    /// <summary>Puts the file's type into <paramref name="existing"/>, the file named after it, and deletes the file it
    /// leaves when nothing else is left in it.</summary>
    private static async Task<Solution> MoveIntoAsync(Document document, CompilationUnitSyntax root, SourceText text, MemberDeclarationSyntax declaration,
        Document existing, CancellationToken cancellationToken)
    {
        string newline = TypeMoves.Newline(root);
        var edits = new SolutionEdits(document.Project.Solution);
        SourceEdits own = await edits.ForAsync(root.SyntaxTree, cancellationToken).ConfigureAwait(false);
        string block = TypeMoves.Block(root, text, own, declaration, newline);
        await TypeMoves.AppendAsync(edits, existing, root, declaration, block, newline, cancellationToken).ConfigureAwait(false);
        bool alone = root.AttributeLists.Count == 0 && !root.Members.OfType<GlobalStatementSyntax>().Any() &&
            !root.Usings.Any(directive => !directive.GlobalKeyword.IsKind(SyntaxKind.None));
        if (alone)
            edits.Delete(root.SyntaxTree);
        else
            own.RemoveMoved(declaration);
        return edits.Apply();
    }
}
