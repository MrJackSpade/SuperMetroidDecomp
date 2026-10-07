using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Deletes the declarations of the selected finding categories from repository source. Every
/// part of a partial declaration is removed; a field declaration only when all of its variables
/// are removed. Positional record parameters change constructor signatures and are reported for
/// manual edits instead. Removed instance fields, including auto-property backing fields, are
/// listed so persisted debugger-state layouts can retire them.
/// </summary>
internal static class SymbolRemover
{
    public static void Remove(LoadedSolution solution, ReachabilityResult result, IReadOnlySet<string> categories,
        string? pathPrefix, string retiredFieldsPath)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var targets = ReachabilityFindings.Classify(result)
            .Where(f => categories.Contains(f.Category) && (pathPrefix is null || f.Declaration.File.StartsWith(pathPrefix, StringComparison.Ordinal)))
            .Select(f => f.Declaration.Key).ToHashSet();
        var retired = new SortedSet<string>(StringComparer.Ordinal);
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int removedNodes = 0, deletedFiles = 0;
        foreach (var (_, compilation) in solution.Projects)
        {
            foreach (var tree in compilation.SyntaxTrees.Where(identity.IsRepositorySource))
            {
                string path = Path.GetFullPath(tree.FilePath);
                if (!processed.Add(path))
                    continue;
                var model = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();
                var removals = new List<SyntaxNode>();
                foreach (var node in root.DescendantNodes())
                {
                    if (SymbolIdentity.Normalize(DeclarationCollector.DeclaredSymbol(model, node)) is not { } symbol
                        || identity.Key(symbol) is not { } key || !targets.Contains(key))
                        continue;
                    if (node is ParameterSyntax)
                    {
                        Console.WriteLine($"SKIP positional record property {symbol.ToDisplayString()} ({identity.Relative(path)})");
                        continue;
                    }
                    removals.Add(RemovalNode(node, model, identity, targets));
                    if (RetiredField(symbol) is { } field)
                        retired.Add($"{RuntimeName(field.ContainingType)}\t{field.Name}");
                }
                var outermost = removals.Distinct().Where(n => !removals.Any(other => other != n && other.Span.Contains(n.Span) && other.Contains(n))).ToList();
                if (outermost.Count == 0)
                    continue;
                var edited = root.RemoveNodes(outermost, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepDirectives)!;
                removedNodes += outermost.Count;
                if (edited is CompilationUnitSyntax unit && !unit.DescendantNodes().OfType<MemberDeclarationSyntax>()
                        .Any(m => m is not BaseNamespaceDeclarationSyntax))
                {
                    File.Delete(path);
                    deletedFiles++;
                    continue;
                }
                bool bom = File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];
                File.WriteAllText(path, edited.ToFullString(), new UTF8Encoding(bom));
            }
        }
        File.WriteAllLines(retiredFieldsPath, retired, new UTF8Encoding(false));
        Console.WriteLine($"Removed {removedNodes} declarations of {targets.Count} findings; deleted {deletedFiles} emptied files; " +
            $"{retired.Count} retired instance fields written to {retiredFieldsPath}.");
    }

    private static SyntaxNode RemovalNode(SyntaxNode node, SemanticModel model, SymbolIdentity identity, HashSet<string> targets)
    {
        if (node is not VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } || declaration.Parent is not BaseFieldDeclarationSyntax field)
            return node;
        bool allRemoved = declaration.Variables.All(v => identity.Key(model.GetDeclaredSymbol(v)) is { } key && targets.Contains(key));
        return allRemoved ? field : node;
    }

    /// <summary>The reflection full name (nested types joined by '+'), as persisted layouts record it.</summary>
    private static string RuntimeName(INamedTypeSymbol type) => type.ContainingType is { } outer
        ? RuntimeName(outer) + "+" + type.MetadataName
        : type.ContainingNamespace.IsGlobalNamespace ? type.MetadataName : type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;

    /// <summary>The instance field a removed declaration takes out of persisted object layouts, if any.</summary>
    private static IFieldSymbol? RetiredField(ISymbol symbol) => symbol switch
    {
        IFieldSymbol { IsStatic: false, IsConst: false, ContainingType.TypeKind: TypeKind.Class or TypeKind.Struct } field => field,
        IPropertySymbol { IsStatic: false } property => property.ContainingType.GetMembers().OfType<IFieldSymbol>()
            .FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property)),
        _ => null,
    };
}
