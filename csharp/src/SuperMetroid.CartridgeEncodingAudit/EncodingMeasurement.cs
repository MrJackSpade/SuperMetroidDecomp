using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.CartridgeEncodingAudit;

/// <summary>The scan of every audited project's authored sources, each file once.</summary>
internal sealed class EncodingMeasurement(IReadOnlyList<EncodedFile> files)
{
    /// <summary>The audited files in path order.</summary>
    public IReadOnlyList<EncodedFile> Files { get; } = files;

    /// <summary>Measures the projects that carry the ratchet baseline as an additional file.</summary>
    public static async Task<EncodingMeasurement> MeasureAsync(LoadedSolution solution)
    {
        var files = new Dictionary<string, EncodedFile>(StringComparer.Ordinal);
        foreach (var (project, compilation) in solution.Projects)
        {
            if (!project.AdditionalDocuments.Any(document => string.Equals(
                    Path.GetFileName(document.FilePath), CartridgeEncodingScanner.BaselineFileName,
                    StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }
            foreach (Document document in project.Documents)
            {
                string? filePath = document.FilePath;
                if (filePath is null || CartridgeEncodingScanner.IsBuildGenerated(filePath))
                    continue;
                string? path = CartridgeEncodingScanner.RepositoryRelative(solution.RepositoryRoot, filePath);
                if (path is null || files.ContainsKey(path))
                    continue;
                SyntaxTree tree = await document.GetSyntaxTreeAsync()
                    ?? throw new InvalidOperationException($"{path} has no syntax tree.");
                SemanticModel model = compilation.GetSemanticModel(tree);
                var occurrences = CartridgeEncodingScanner.Scan(model, CancellationToken.None);
                files.Add(path, new EncodedFile(path, project.Name, occurrences,
                    AddressInterpretingTypes(tree, occurrences), StorageWidthMembers(tree)));
            }
            Console.WriteLine($"Scanned {project.Name}");
        }
        return new EncodingMeasurement(files.Values.OrderBy(file => file.Path, StringComparer.Ordinal).ToList());
    }

    /// <summary>The ratcheted (path, rule) counts the baseline records.</summary>
    public IEnumerable<KeyValuePair<(string Path, string Rule), int>> RatchetCounts() =>
        Files.SelectMany(file => file.Occurrences
            .Select(occurrence => CartridgeEncodingScanner.RuleId(occurrence.Rule))
            .OfType<string>()
            .GroupBy(rule => rule)
            .Select(group => new KeyValuePair<(string, string), int>((file.Path, group.Key), group.Count())));

    /// <summary>Repository totals by rule.</summary>
    public string Summary() => string.Join(Environment.NewLine,
        Files.SelectMany(file => file.Occurrences).GroupBy(occurrence => occurrence.Rule).OrderBy(group => group.Key)
            .Select(group => $"{group.Key}: {group.Count()}"));

    /// <summary>Names types containing pointer-dispatch occurrences so the audit can identify address interpreters.</summary>
    private static List<string> AddressInterpretingTypes(
        SyntaxTree tree,
        IReadOnlyList<CartridgeEncodingOccurrence> occurrences)
    {
        SyntaxNode root = tree.GetRoot();
        return occurrences.Where(occurrence => occurrence.Rule == CartridgeEncodingRule.PointerDispatch)
            .Select(occurrence => root.FindToken(occurrence.Location.SourceSpan.Start).Parent?
                .AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Counts nonconstant narrow fields and properties outside catalog files.</summary>
    private static int StorageWidthMembers(SyntaxTree tree)
    {
        if (CartridgeEncodingScanner.IsCatalogFile(tree.FilePath))
            return 0;
        static bool Narrow(TypeSyntax type) => type is PredefinedTypeSyntax predefined &&
            predefined.Keyword.Kind() is SyntaxKind.UShortKeyword or SyntaxKind.ByteKeyword or SyntaxKind.ShortKeyword or
                SyntaxKind.SByteKeyword;
        SyntaxNode root = tree.GetRoot();
        return root.DescendantNodes().OfType<PropertyDeclarationSyntax>().Count(property => Narrow(property.Type)) +
            root.DescendantNodes().OfType<FieldDeclarationSyntax>()
                .Where(field => !field.Modifiers.Any(SyntaxKind.ConstKeyword) && Narrow(field.Declaration.Type))
                .Sum(field => field.Declaration.Variables.Count);
    }
}
