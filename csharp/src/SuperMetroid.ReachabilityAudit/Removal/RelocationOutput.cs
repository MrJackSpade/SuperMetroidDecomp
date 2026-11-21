using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The relocation's last step: renders each moved type declaration, with the reference edits inside it, into the file of
/// the same path under the target (merging into a file an earlier run wrote), applies the edits outside moved declarations
/// in place, removes the moved declarations and deletes the files left without a type.
/// </summary>
internal static class RelocationOutput
{
    /// <summary>Renders the moved declarations into their target files and writes every changed source file.</summary>
    /// <param name="moves">The declarations that move.</param>
    /// <param name="edits">The reference edits, by tree, to which the removal of each moved declaration is added.</param>
    /// <param name="trees">Every repository tree, once.</param>
    /// <param name="targetDirectory">The directory moved code goes into.</param>
    /// <param name="identity">Gives each source file its repository-relative path.</param>
    public static PlacedFiles Place(IReadOnlyList<RelocationMove> moves, Dictionary<SyntaxTree, List<TextEdit>> edits, IReadOnlyList<ProjectTree> trees,
        string targetDirectory, SymbolIdentity identity)
    {
        var compilationOf = trees.ToDictionary(entry => entry.Tree, entry => entry.Compilation);
        // A source project's global usings do not follow its code; each moved declaration takes those it names.
        var globalNamespaces = trees.GroupBy(entry => entry.Compilation).ToDictionary(group => group.Key, group =>
            (IReadOnlySet<string>)group.SelectMany(entry => entry.Tree.GetCompilationUnitRoot().Usings)
                .Where(u => u.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) && u.Alias is null && u.StaticKeyword.IsKind(SyntaxKind.None) && u.Name is not null)
                .Select(u => u.Name!.ToString()).ToHashSet(StringComparer.Ordinal));
        var files = new SortedDictionary<string, TargetFile>(StringComparer.Ordinal);
        foreach (var move in moves)
        {
            string path = Path.Combine(targetDirectory, TargetRelativePath(identity.Relative(move.Tree.FilePath)));
            if (!files.TryGetValue(path, out var file))
                files[path] = file = new TargetFile([.. move.Tree.GetCompilationUnitRoot().Usings.Select(u => u.ToString())], Namespace(move.Tree));
            foreach (string name in GlobalNamespacesNamed(move, globalNamespaces[compilationOf[move.Tree]]))
                file.RequireUsing($"using {name};");
            file.Types.Add(Rewritten(move.Tree, move.Node.FullSpan, edits));
        }

        var movedSpans = moves.GroupBy(move => move.Tree).ToDictionary(group => group.Key, group => group.Select(move => move.Node.FullSpan).ToList());
        foreach (var move in moves)
        {
            if (!edits.TryGetValue(move.Tree, out var list))
                edits[move.Tree] = list = [];
            list.Add(new TextEdit(move.Node.FullSpan, ""));
        }
        int deleted = 0;
        foreach (var (tree, list) in edits)
        {
            string path = Path.GetFullPath(tree.FilePath);
            string output = InPlace(tree, list, movedSpans.TryGetValue(tree, out var moved) ? moved : []);
            var root = CSharpSyntaxTree.ParseText(output).GetRoot();
            if (root.DescendantNodes().Any(node => node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax))
                File.WriteAllText(path, output, new UTF8Encoding(HasByteOrderMark(path)));
            else
            {
                File.Delete(path);
                deleted++;
            }
        }
        foreach (var (path, file) in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                throw new InvalidOperationException($"{path} has no containing directory."));
            // A later run adds to the files an earlier run created; a merge keeps the existing file's byte-order mark, and
            // new files get one.
            bool existing = File.Exists(path);
            File.WriteAllText(path, existing ? file.MergeInto(File.ReadAllText(path)) : file.Render(),
                new UTF8Encoding(!existing || HasByteOrderMark(path)));
        }
        return new PlacedFiles(files.Count, deleted);
    }

    /// <summary>
    /// A source tree's text with its edits applied in place: every edit outside the moved declarations, and the removal of
    /// each moved declaration (an empty edit of exactly its span).
    /// </summary>
    /// <param name="tree">The tree.</param>
    /// <param name="edits">The tree's edits, removals included.</param>
    /// <param name="movedSpans">The full spans of the declarations that move out of the tree.</param>
    /// <exception cref="InvalidOperationException">Two applied edits overlap.</exception>
    private static string InPlace(SyntaxTree tree, IEnumerable<TextEdit> edits, IReadOnlyList<TextSpan> movedSpans)
    {
        var applied = edits.Where(e => e.Text == "" && movedSpans.Contains(e.Span) || !movedSpans.Any(s => s.Contains(e.Span)))
            .Distinct().OrderByDescending(e => e.Span.Start).ThenByDescending(e => e.Span.End).ToList();
        var text = new StringBuilder(tree.GetText().ToString());
        int floor = int.MaxValue;
        foreach (var (span, replacement) in applied)
        {
            if (span.End > floor)
                throw new InvalidOperationException($"Overlapping relocation edits in {Path.GetFullPath(tree.FilePath)} at {span}.");
            text.Remove(span.Start, span.Length).Insert(span.Start, replacement);
            floor = span.Start;
        }
        return text.ToString();
    }

    /// <summary>
    /// csharp/src/SuperMetroid.Core/Game/X.cs maps to Game/X.cs under the target, which stands for the source project.
    /// </summary>
    /// <exception cref="InvalidOperationException">The path is not under a project directory.</exception>
    private static string TargetRelativePath(string repositoryRelative)
    {
        string[] parts = repositoryRelative.Split('/');
        int project = Array.FindIndex(parts, p => p.StartsWith("SuperMetroid.", StringComparison.Ordinal));
        if (project < 0) throw new InvalidOperationException($"{repositoryRelative} is not under a project directory.");
        return Path.Combine(parts[(project + 1)..]);
    }

    /// <summary>Whether a source file starts with a UTF-8 byte-order mark, which rewriting it keeps.</summary>
    private static bool HasByteOrderMark(string path) => File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];

    /// <summary>Applies reference edits inside a moved declaration from the end to preserve original spans.</summary>
    private static string Rewritten(SyntaxTree tree, TextSpan span, Dictionary<SyntaxTree, List<TextEdit>> edits)
    {
        var text = new StringBuilder(tree.GetText().ToString(span));
        if (edits.TryGetValue(tree, out var list))
            foreach (var (editSpan, replacement) in list.Where(e => span.Contains(e.Span) && e.Span != span).Distinct()
                .OrderByDescending(e => e.Span.Start))
                text.Remove(editSpan.Start - span.Start, editSpan.Length).Insert(editSpan.Start - span.Start, replacement);
        return text.ToString();
    }

    /// <summary>
    /// The source project's global-using namespaces whose types, attributes or extension methods the moved declaration
    /// names without qualification, other than those its own namespace already encloses.
    /// </summary>
    private static IEnumerable<string> GlobalNamespacesNamed(RelocationMove move, IReadOnlySet<string> globalNamespaces)
    {
        if (globalNamespaces.Count == 0)
            yield break;
        string own = Namespace(move.Tree);
        var named = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var name in move.Node.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (name.Parent is QualifiedNameSyntax qualified && qualified.Right == name ||
                name.Parent is MemberAccessExpressionSyntax access && access.Name == name && access.Expression is not (ThisExpressionSyntax or BaseExpressionSyntax) &&
                move.Model.GetSymbolInfo(name).Symbol is not IMethodSymbol { IsExtensionMethod: true } and not IMethodSymbol { ReducedFrom: not null })
                continue;
            INamedTypeSymbol? type = move.Model.GetSymbolInfo(name).Symbol switch
            {
                INamedTypeSymbol typeSymbol => typeSymbol,
                IMethodSymbol { MethodKind: MethodKind.Constructor } constructor => constructor.ContainingType,
                IMethodSymbol { ReducedFrom: { } extension } => extension.ContainingType,
                IMethodSymbol { IsExtensionMethod: true } extension => extension.ContainingType,
                _ => null,
            };
            while (type?.ContainingType is { } outer)
                type = outer;
            if (type?.ContainingNamespace is not { IsGlobalNamespace: false } containing)
                continue;
            string ns = containing.ToDisplayString();
            if (globalNamespaces.Contains(ns) && own != ns && !own.StartsWith(ns + ".", StringComparison.Ordinal))
                named.Add(ns);
        }
        foreach (string ns in named)
            yield return ns;
    }

    /// <summary>Retains a moved declaration's source namespace.</summary>
    private static string Namespace(SyntaxTree tree) =>
        tree.GetCompilationUnitRoot().Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";

    /// <summary>Accumulates the moved types for one destination source file.</summary>
    /// <param name="usings">The using directives of the file the types leave.</param>
    /// <param name="ns">The namespace the types keep.</param>
    private sealed class TargetFile(List<string> usings, string ns)
    {
        /// <summary>The moved type declarations, in the order they were found.</summary>
        public List<string> Types { get; } = [];

        /// <summary>Adds a using directive the moved code needs, once.</summary>
        public void RequireUsing(string directive)
        {
            if (!usings.Contains(directive))
                usings.Add(directive);
        }

        /// <summary>Adds this file's usings and types to an existing target file.</summary>
        public string MergeInto(string existing)
        {
            var root = CSharpSyntaxTree.ParseText(existing).GetCompilationUnitRoot();
            var presentUsings = root.Usings.Select(u => u.ToString()).ToHashSet(StringComparer.Ordinal);
            string newUsings = string.Concat(usings.Where(u => !presentUsings.Contains(u)).Select(u => u + "\r\n"));
            var output = new StringBuilder(existing);
            foreach (string type in Types)
                output.Append("\r\n").Append(type.Trim('\r', '\n')).Append("\r\n");
            if (newUsings.Length > 0)
                output.Insert(root.Usings.Count > 0 ? root.Usings[^1].FullSpan.End : 0, newUsings);
            return output.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        /// <summary>Writes the using directives, the namespace and the moved types into a new target file.</summary>
        public string Render()
        {
            var output = new StringBuilder();
            foreach (string u in usings) output.Append(u).Append("\r\n");
            if (usings.Count > 0) output.Append("\r\n");
            if (ns.Length > 0) output.Append("namespace ").Append(ns).Append(";\r\n");
            foreach (string type in Types)
                output.Append("\r\n").Append(type.Trim('\r', '\n')).Append("\r\n");
            return output.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }
    }
}
