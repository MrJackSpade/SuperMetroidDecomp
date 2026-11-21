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
/// Moves top-level types out of a file that declares several (#626), the asynchronous cleanup's fix for SME6400 (#1666),
/// at every finding (#2599): a fix is always applied, and breaks the code where it must. The type the file is named
/// after stays (SME6401's name, compared without case, since a file system that ignores case holds one such file); every
/// other reported type moves, and when the reported type is the one the file is named after, the file's first type moves
/// in its place. A moved type goes to a sibling file named as SME6401 expects (<c>Pair&lt;T&gt;</c> goes to
/// <c>Pair.cs</c>) with the source file's extern aliases and using directives (the cleanup's IDE0005 pass removes the
/// unneeded ones; global usings stay where they are), the type's namespace as a file-scoped namespace with the usings
/// declared inside it, and the comments and documentation directly above the type with its attributes. It keeps what it
/// was compiled under: the <c>#define</c>s, <c>#nullable</c> and <c>#pragma warning</c> directives before it, and the
/// <c>#if</c> branches it lies in, which wrap it; directives inside it move with it, and lines inside a token spanning
/// lines (a verbatim or raw string) are not re-indented. When that file exists already, the type is added to it, which
/// deletes nothing: its usings join the file's, and it follows the file's code, in the file's namespace when it is the
/// same, so the build or the next run of the rule reports what the two files' code now shares. A type that belongs in
/// the very file it is in beside another that does too (<c>Pair</c> and <c>Pair&lt;T&gt;</c> in <c>Pair.cs</c>) has no
/// file to go to: an <c>#error</c> above it fails the build there, naming both. A file-local type moves like any other,
/// and the references across the files fail the build. The cleanup's runner adds a new file to every project that
/// compiles the source file, and beside it in an explicit <c>.projitems</c> or <c>&lt;Compile&gt;</c> list.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OneTypePerFileCodeFixProvider)), Shared]
public sealed class OneTypePerFileCodeFixProvider : DocumentFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [OneTypePerFileAnalyzer.Id];

    /// <inheritdoc />
    internal override string Title => "Move the type to its own file";

    /// <inheritdoc />
    internal override async Task<Solution> FixAsync(Document document, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
    {
        if (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is not CompilationUnitSyntax root || document.FilePath is not { } path)
            throw new InvalidOperationException($"{document.Name} is not a C# file on disk.");
        SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        string newline = TypeMoves.Newline(root);
        string stem = Path.GetFileNameWithoutExtension(path);
        string directory = Path.GetDirectoryName(path) ?? string.Empty;
        ImmutableArray<MemberDeclarationSyntax> types = OneTypePerFileAnalyzer.TopLevelTypes(root);
        bool statements = root.Members.OfType<GlobalStatementSyntax>().Any();
        Solution solution = document.Project.Solution;
        var edits = new SolutionEdits(solution);
        SourceEdits own = await edits.ForAsync(root.SyntaxTree, cancellationToken).ConfigureAwait(false);
        var created = new Dictionary<string, TypeMoves.NewFile>(StringComparer.OrdinalIgnoreCase);
        var moved = new HashSet<MemberDeclarationSyntax>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            MemberDeclarationSyntax reported = types.FirstOrDefault(type => type.Span.Contains(diagnostic.Location.SourceSpan))
                ?? throw new InvalidOperationException($"{path}: the {diagnostic.Id} finding at {diagnostic.Location.SourceSpan} names no top-level type declaration.");
            MemberDeclarationSyntax declaration = !statements && TypeMoves.NamesFile(stem, reported) ? types[0] : reported;
            string name = OneTypePerFileAnalyzer.NameOf(declaration);
            if (TypeMoves.NamesFile(stem, declaration))
            {
                // Both types belong in this file, so neither can leave it: the build fails here until one is renamed.
                own.Insert(text.Lines.GetLineFromPosition(own.AttachedStart(reported)).Start,
                    $"#error {OneTypePerFileAnalyzer.Id}: '{Shown(reported)}' and '{Shown(declaration)}' both belong in " +
                    $"{Path.GetFileName(path)}; one of them needs another name{newline}");
                continue;
            }
            if (!moved.Add(declaration))
                continue;
            string block = TypeMoves.Block(root, text, own, declaration, newline);
            string target = Path.Combine(directory, name + ".cs");
            if (created.TryGetValue(target, out TypeMoves.NewFile? pending))
                pending.Add(block, TypeMoves.Namespace(declaration));
            else if (TypeMoves.DocumentAt(solution, target) is { } existing)
                await TypeMoves.AppendAsync(edits, existing, root, declaration, block, newline, cancellationToken).ConfigureAwait(false);
            else if (File.Exists(target))
                throw new InvalidOperationException($"{target} exists but no project of the solution compiles it, so {name} cannot be added to it.");
            else
                created[target] = new TypeMoves.NewFile(root, declaration, block, newline);
            own.RemoveMoved(declaration);
        }
        solution = edits.Apply();
        foreach (KeyValuePair<string, TypeMoves.NewFile> file in created)
        {
            solution = solution.AddDocument(DocumentId.CreateNewId(document.Project.Id), Path.GetFileName(file.Key),
                SourceText.From(file.Value.Text()), document.Folders, file.Key);
        }
        return solution;
    }

    /// <summary>A type's name with its type parameters, as the <c>#error</c> names it.</summary>
    private static string Shown(MemberDeclarationSyntax declaration) => declaration switch
    {
        TypeDeclarationSyntax type => type.Identifier.ValueText + type.TypeParameterList,
        DelegateDeclarationSyntax @delegate => @delegate.Identifier.ValueText + @delegate.TypeParameterList,
        _ => OneTypePerFileAnalyzer.NameOf(declaration),
    };
}
