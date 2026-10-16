using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Ratchets species identity tests outside their species' owner files (#1312). A comparison or pattern against a named
/// <c>EnemyDefinitionId</c>, or a call to another folder's <c>Is…</c> definition predicate, belongs in the folder of the
/// factory that owns the definition; the shared enemy loop, collision, interpreter and drawing ask the bound entity
/// instead. The empty-slot and respawn-placeholder tests are occupancy, not species identity. A file's reviewed count in
/// <c>species-identity-baseline.txt</c> may only fall.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpeciesIdentityAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A species identity test outside the species' owner files.</summary>
    public const string ForeignIdentityId = "SME6392";

    /// <summary>A reviewed count that the file no longer reaches.</summary>
    public const string StaleCountId = "SME6393";

    /// <summary>The baseline file that activates the rule.</summary>
    internal const string BaselineFileName = "species-identity-baseline.txt";

    /// <summary>The definition type whose named members are species identities.</summary>
    private const string DefinitionTypeName = "EnemyDefinitionId";

    /// <summary>The factory interface whose Definitions name a species' owned definitions.</summary>
    private const string FactoryInterfaceName = "IEnemyEntityFactory";

    /// <summary>Definition members that denote occupancy rather than a species.</summary>
    private static readonly ImmutableHashSet<string> OccupancyMembers = ImmutableHashSet.Create(StringComparer.Ordinal, "Respawn", "None");

    private static readonly DiagnosticDescriptor ForeignIdentityRule = new(
        ForeignIdentityId,
        "Species identity test outside its owner",
        "{0}; ask the bound entity (slot.Entity) through a participation interface, or move the test into the species' folder",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Shared enemy code dispatches through the occupant's entity. A test of which species occupies a slot " +
            "belongs with the factory that owns the definition (#1312).");

    private static readonly DiagnosticDescriptor StaleCountRule = new(
        StaleCountId,
        "Species-identity baseline is stale",
        "{0} now holds {1} species identity test(s) but the baseline allows {2}; lower the baseline so the ratchet keeps the gain",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ForeignIdentityRule, StaleCountRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            AdditionalText? file = start.Options.AdditionalFiles.FirstOrDefault(additional =>
                string.Equals(Path.GetFileName(additional.Path), BaselineFileName, StringComparison.OrdinalIgnoreCase));
            if (file is null)
                return;
            SourceText content = file.GetText(start.CancellationToken)
                ?? throw new InvalidOperationException($"{file.Path}: the species-identity baseline could not be read.");
            CartridgeEncodingBaseline baseline = CartridgeEncodingBaseline.Parse(file.Path, content.ToString());
            Dictionary<string, List<string>> owners = OwnerDirectories(start.Compilation, start.CancellationToken);
            start.RegisterSemanticModelAction(model => AnalyzeTree(model, baseline, owners));
        });
    }

    /// <summary>Each owned definition member's owner directories, read from the factories' Definitions lists.</summary>
    private static Dictionary<string, List<string>> OwnerDirectories(Compilation compilation, CancellationToken cancellationToken)
    {
        var owners = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            string? directory = Path.GetDirectoryName(tree.FilePath);
            if (directory is null)
                continue;
            foreach (TypeDeclarationSyntax type in tree.GetRoot(cancellationToken).DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (type.BaseList?.Types.Any(baseType => baseType.Type.ToString() == FactoryInterfaceName) != true)
                    continue;
                foreach (PropertyDeclarationSyntax property in type.Members.OfType<PropertyDeclarationSyntax>()
                             .Where(property => property.Identifier.Text == "Definitions"))
                {
                    if (property.Initializer is not { } initializer)
                        continue;
                    foreach (MemberAccessExpressionSyntax member in initializer.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                                 .Where(member => member.Expression.ToString() == DefinitionTypeName))
                    {
                        string name = member.Name.Identifier.Text;
                        if (!owners.TryGetValue(name, out List<string>? directories))
                            owners[name] = directories = [];
                        directories.Add(Normalize(directory));
                    }
                }
            }
        }
        return owners;
    }

    /// <summary>Counts and reports the tree's foreign species identity tests against its reviewed baseline.</summary>
    private static void AnalyzeTree(SemanticModelAnalysisContext context, CartridgeEncodingBaseline baseline,
        Dictionary<string, List<string>> owners)
    {
        SyntaxTree tree = context.SemanticModel.SyntaxTree;
        if (CartridgeEncodingScanner.IsBuildGenerated(tree.FilePath))
            return;
        string? path = CartridgeEncodingScanner.RepositoryRelative(baseline.RepositoryRoot, tree.FilePath);
        string? directory = Path.GetDirectoryName(tree.FilePath);
        if (path is null || directory is null)
            return;
        directory = Normalize(directory);

        var found = new List<(Location Location, string What)>();
        foreach (SyntaxNode node in tree.GetRoot(context.CancellationToken).DescendantNodes())
        {
            switch (node)
            {
                case MemberAccessExpressionSyntax member when member.Expression.ToString() == DefinitionTypeName &&
                    IsIdentityTest(member) && !OccupancyMembers.Contains(member.Name.Identifier.Text) &&
                    !Owns(owners, member.Name.Identifier.Text, directory):
                    found.Add((member.GetLocation(), $"'{member}' tested outside its species' folder"));
                    break;
                case InvocationExpressionSyntax invocation when IsForeignDefinitionPredicate(invocation, context.SemanticModel,
                    directory, context.CancellationToken):
                    found.Add((invocation.GetLocation(), $"'{invocation.Expression}' is another species' definition predicate"));
                    break;
            }
        }

        int allowed = baseline.Allowed(path, ForeignIdentityId);
        if (allowed == 0)
        {
            foreach ((Location location, string what) in found)
                context.ReportDiagnostic(Diagnostic.Create(ForeignIdentityRule, location, what));
        }
        else if (found.Count > allowed)
        {
            context.ReportDiagnostic(Diagnostic.Create(ForeignIdentityRule, found[0].Location,
                $"{path} holds {found.Count} species identity tests but its reviewed baseline allows {allowed}"));
        }
        else if (found.Count < allowed)
        {
            context.ReportDiagnostic(Diagnostic.Create(StaleCountRule, Location.Create(tree, new TextSpan(0, 0)), path, found.Count, allowed));
        }
    }

    /// <summary>Whether a definition member is the compared side of ==/!=, a pattern constant or a switch label.</summary>
    private static bool IsIdentityTest(MemberAccessExpressionSyntax member)
    {
        SyntaxNode? parent = member.Parent;
        while (parent is ParenthesizedExpressionSyntax)
            parent = parent.Parent;
        return parent switch
        {
            BinaryExpressionSyntax binary => binary.IsKind(SyntaxKind.EqualsExpression) || binary.IsKind(SyntaxKind.NotEqualsExpression) ||
                binary.IsKind(SyntaxKind.IsExpression),
            ConstantPatternSyntax => true,
            CaseSwitchLabelSyntax => true,
            _ => false,
        };
    }

    /// <summary>Whether <paramref name="directory"/> lies in one of the member's owner directories.</summary>
    private static bool Owns(Dictionary<string, List<string>> owners, string member, string directory) =>
        owners.TryGetValue(member, out List<string>? directories) &&
        directories.Any(owner => directory.Equals(owner, StringComparison.OrdinalIgnoreCase) ||
            directory.StartsWith(owner + "/", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Whether the invocation calls a static bool <c>Is…</c> predicate over one definition that another folder declares:
    /// a species' identity test used outside that species.
    /// </summary>
    private static bool IsForeignDefinitionPredicate(InvocationExpressionSyntax invocation, SemanticModel model, string directory,
        CancellationToken cancellationToken)
    {
        if (model.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol method ||
            !method.IsStatic || !method.Name.StartsWith("Is", StringComparison.Ordinal) ||
            method.ReturnType.SpecialType != SpecialType.System_Boolean || method.Parameters.Length != 1 ||
            method.Parameters[0].Type.Name != DefinitionTypeName)
        {
            return false;
        }
        string? declared = method.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree.FilePath;
        string? declaredDirectory = declared is null ? null : Path.GetDirectoryName(declared);
        if (declaredDirectory is null)
            return false;
        declaredDirectory = Normalize(declaredDirectory);
        return !directory.Equals(declaredDirectory, StringComparison.OrdinalIgnoreCase) &&
            !directory.StartsWith(declaredDirectory + "/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>A directory with forward slashes and no trailing separator.</summary>
    private static string Normalize(string directory) => directory.Replace('\\', '/').TrimEnd('/');
}
