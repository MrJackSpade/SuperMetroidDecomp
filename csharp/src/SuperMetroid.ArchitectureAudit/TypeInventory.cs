using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.EnsureAnalyzer.SourceLayout;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// Inventories every type declared in the repository's source. Each type gets a disposition from its shape;
/// structural findings — an owner constructed with its creator, a service locator, a context bag, an oversized
/// class — are flags that the type itself must justify with <c>[ArchitectureReview]</c> or fix.
/// </summary>
internal static class TypeInventory
{
    /// <summary>The per-site justification attribute, matched by name so every assembly's declaration counts.</summary>
    internal const string ReviewAttributeName = "ArchitectureReviewAttribute";

    /// <summary>A product class with more hand-written lines than this is reviewed as a possible god class.</summary>
    internal const int LargeClassLines = 1500;

    /// <summary>A context-bag candidate: this many service-typed members on a type that only carries them.</summary>
    internal const int ContextBagMembers = 8;

    /// <summary>A dependencies record with more members than this hands its consumer more than it should reach (#1362).</summary>
    internal const int WideDependenciesMembers = 12;

    /// <summary>Finding key for an owner that stores a collaborator constructed with itself.</summary>
    internal const string BackReference = "back-reference";
    /// <summary>Finding key for a generic service lookup API.</summary>
    internal const string ServiceLocator = "service-locator";
    /// <summary>Finding key for a behavior-free carrier of many service references.</summary>
    internal const string ContextBag = "context-bag";
    /// <summary>Finding key for a product class exceeding the source-line review threshold.</summary>
    internal const string Large = "large";
    /// <summary>Finding key for a <c>*Dependencies</c> record with more than <see cref="WideDependenciesMembers"/> members.</summary>
    internal const string WideDependencies = "wide-dependencies";

    /// <summary>Collects source types across projects and describes each type's scope, disposition, and flags.</summary>
    public static IReadOnlyList<TypeEntry> Collect(LoadedSolution solution)
    {
        var entries = new Dictionary<string, TypeInventoryEntry>(StringComparer.Ordinal);
        foreach (var (project, compilation) in solution.Projects)
        {
            foreach (INamedTypeSymbol type in AllTypes(compilation.Assembly.GlobalNamespace))
            {
                var files = type.DeclaringSyntaxReferences
                    .Select(reference => reference.SyntaxTree.FilePath)
                    .Where(path => IsRepositorySource(solution.RepositoryRoot, path))
                    .Select(path => Relative(solution.RepositoryRoot, path))
                    .ToList();
                if (files.Count == 0)
                    continue;
                string key = Key(type);
                if (!entries.TryGetValue(key, out var entry))
                    entries[key] = entry = new TypeInventoryEntry(type, new SortedSet<string>(StringComparer.Ordinal), new SortedSet<string>(StringComparer.Ordinal), compilation);
                entry.Assemblies.Add(project.AssemblyName);
                foreach (string file in files)
                    entry.Files.Add(file);
            }
        }
        var backReferences = BackReferencedOwners(entries.Values.Select(entry => new CompiledType(entry.Symbol, entry.Compilation)));
        var parameterObjects = ConstructorParameterObjects(entries.Values.Select(entry => entry.Symbol));
        return entries
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => Describe(pair.Key, pair.Value.Symbol, pair.Value.Assemblies, pair.Value.Files, backReferences, parameterObjects))
            .ToList();
    }

    /// <summary>Classifies one type and records structural findings from its source declarations.</summary>
    private static TypeEntry Describe(string key, INamedTypeSymbol type, IReadOnlyCollection<string> assemblies,
        IReadOnlyCollection<string> files, Dictionary<string, string> backReferences, HashSet<string> parameterObjects)
    {
        string scope = Scope(assemblies, files);
        string kind = Kind(type);
        var declarations = type.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax()).OfType<TypeDeclarationSyntax>().ToList();
        bool isPartial = declarations.Any(declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword));
        var handWritten = declarations.Where(declaration => !GeneratedSource.IsGenerated(declaration.SyntaxTree, CancellationToken.None)).ToList();
        string partial = !isPartial ? "none"
            : handWritten.Count == 1 && handWritten.Count < declarations.Count ? "generated-boundary"
            : handWritten.Count == 0 ? "generated"
            : "split";
        var flags = new List<string>();
        if (backReferences.TryGetValue(key, out string? creator))
            flags.Add($"{BackReference}:{creator}");
        if (IsServiceLocator(type))
            flags.Add(ServiceLocator);
        // A constructor's own parameter object (one constructor takes it; nothing stores or passes it on) groups
        // one owner's collaborators; it is not a bag handed around.
        bool parameterObject = parameterObjects.Contains(key);
        if (IsContextBag(type) && !parameterObject)
            flags.Add(ContextBag);
        if (scope == "product" && type.TypeKind == TypeKind.Class &&
            handWritten.Sum(declaration => declaration.GetLocation().GetLineSpan().EndLinePosition.Line -
                                           declaration.GetLocation().GetLineSpan().StartLinePosition.Line + 1) > LargeClassLines)
            flags.Add(Large);
        if (IsWideDependencies(type))
            flags.Add(WideDependencies);
        var reviews = type.GetAttributes()
            .Where(attribute => attribute.AttributeClass?.Name == ReviewAttributeName && attribute.ConstructorArguments.Length == 2)
            .Select(attribute => new ArchitectureReview(
                attribute.ConstructorArguments[0].Value as string ?? "",
                attribute.ConstructorArguments[1].Value as string ?? ""))
            .ToList();
        return new TypeEntry(key, kind, scope, [.. assemblies], [.. files], partial,
            parameterObject ? "constructor-parameters" : Disposition(type, kind, partial), flags, reviews);
    }

    /// <summary>Assigns a type's structural role from its kind, members, and generated boundary.</summary>
    private static string Disposition(INamedTypeSymbol type, string kind, string partial)
    {
        if (partial is "generated-boundary" or "generated")
            return "generated-boundary";
        return kind switch
        {
            "enum" or "struct" or "record struct" => "value",
            "interface" or "delegate" => "contract",
            "record" => "record",
            "static class" => type.GetMembers().All(member =>
                    member is IFieldSymbol or IPropertySymbol or INamedTypeSymbol or IMethodSymbol { MethodKind: not MethodKind.Ordinary })
                ? "definition-catalog"
                : "static-functions",
            _ => type.GetMembers().OfType<IFieldSymbol>().Any(field => !field.IsStatic && !field.IsConst)
                ? "owner"
                : "stateless",
        };
    }

    /// <summary>
    /// Types used as the parameter of exactly one constructor and never as a field, property or any other parameter:
    /// one owner's grouped collaborators.
    /// </summary>
    private static HashSet<string> ConstructorParameterObjects(IEnumerable<INamedTypeSymbol> types)
    {
        var constructorUses = new Dictionary<string, int>(StringComparer.Ordinal);
        var otherUses = new HashSet<string>(StringComparer.Ordinal);
        foreach (INamedTypeSymbol type in types.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default))
        {
            foreach (ISymbol member in type.GetMembers())
            {
                switch (member)
                {
                    case IMethodSymbol method:
                        foreach (IParameterSymbol parameter in method.Parameters)
                        {
                            if (parameter.Type is not INamedTypeSymbol parameterType)
                                continue;
                            string key = Key(parameterType);
                            if (method.MethodKind == MethodKind.Constructor &&
                                !SymbolEqualityComparer.Default.Equals(parameterType, type))
                                constructorUses[key] = constructorUses.GetValueOrDefault(key) + 1;
                            else if (!SymbolEqualityComparer.Default.Equals(parameterType, type))
                                otherUses.Add(key);
                        }
                        break;
                    case IFieldSymbol { IsImplicitlyDeclared: false } field when field.Type is INamedTypeSymbol fieldType &&
                                                                               !SymbolEqualityComparer.Default.Equals(fieldType, type):
                        otherUses.Add(Key(fieldType));
                        break;
                    case IPropertySymbol property when property.Type is INamedTypeSymbol propertyType &&
                                                       !SymbolEqualityComparer.Default.Equals(propertyType, type):
                        otherUses.Add(Key(propertyType));
                        break;
                    default:
                        break;
                }
            }
        }
        return constructorUses.Where(pair => pair.Value == 1 && !otherUses.Contains(pair.Key)).Select(pair => pair.Key)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>Owners whose constructor receives the instance that creates and stores them.</summary>
    private static Dictionary<string, string> BackReferencedOwners(IEnumerable<CompiledType> types)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (creator, compilation) in types)
        {
            foreach (SyntaxReference reference in creator.DeclaringSyntaxReferences)
            {
                SyntaxNode declaration = reference.GetSyntax();
                SemanticModel model = compilation.GetSemanticModel(declaration.SyntaxTree);
                foreach (BaseObjectCreationExpressionSyntax creation in declaration.DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>())
                {
                    if (creation.ArgumentList is not { } arguments ||
                        !arguments.Arguments.Any(argument => argument.Expression is ThisExpressionSyntax) ||
                        !IsStoredByCreator(creation, model, creator))
                        continue;
                    if (model.GetSymbolInfo(creation).Symbol is IMethodSymbol { ContainingType: { } owner } &&
                        SymbolEqualityComparer.Default.Equals(owner.ContainingAssembly, creator.ContainingAssembly))
                        result.TryAdd(Key(owner), Key(creator));
                }
            }
        }
        return result;
    }

    /// <summary>Checks whether a construction is assigned to the creating type's instance field or property.</summary>
    private static bool IsStoredByCreator(SyntaxNode creation, SemanticModel model, INamedTypeSymbol creator)
    {
        SyntaxNode? parent = creation.Parent;
        while (parent is ParenthesizedExpressionSyntax or CastExpressionSyntax)
            parent = parent.Parent;
        ISymbol? target = parent switch
        {
            AssignmentExpressionSyntax assignment => model.GetSymbolInfo(assignment.Left).Symbol,
            EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax variable } => model.GetDeclaredSymbol(variable),
            EqualsValueClauseSyntax { Parent: PropertyDeclarationSyntax property } => model.GetDeclaredSymbol(property),
            _ => null,
        };
        return target is IFieldSymbol or IPropertySymbol &&
               SymbolEqualityComparer.Default.Equals(target.ContainingType, creator) && !target.IsStatic;
    }

    /// <summary>Detects IServiceProvider or a zero-argument generic service lookup method.</summary>
    private static bool IsServiceLocator(INamedTypeSymbol type) =>
        type.AllInterfaces.Any(contract => contract.ToDisplayString() == "System.IServiceProvider") ||
        type.GetMembers().OfType<IMethodSymbol>().Any(method =>
            method.MethodKind == MethodKind.Ordinary && method.TypeParameters.Length == 1 && method.Parameters.Length == 0 &&
            SymbolEqualityComparer.Default.Equals(method.ReturnType, method.TypeParameters[0]) &&
            method.Name is "Get" or "GetService" or "GetRequiredService" or "Resolve" or "Locate");

    /// <summary>Detects a behavior-free class carrying at least the configured number of project services.</summary>
    private static bool IsContextBag(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Class || type.IsStatic)
            return false;
        var members = type.GetMembers();
        var services = members.Where(member => member switch
        {
            IPropertySymbol { IsStatic: false } property => IsService(property.Type),
            IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false } field => IsService(field.Type),
            _ => false,
        }).Count();
        bool behavior = members.OfType<IMethodSymbol>().Any(method => method.MethodKind == MethodKind.Ordinary && !method.IsImplicitlyDeclared);
        return services >= ContextBagMembers && !behavior;
    }

    /// <summary>
    /// A record named <c>*Dependencies</c>, the collaborators one consumer is constructed with, holding more than
    /// <see cref="WideDependenciesMembers"/> instance properties.
    /// </summary>
    private static bool IsWideDependencies(INamedTypeSymbol type) =>
        type.IsRecord && type.Name.EndsWith("Dependencies", StringComparison.Ordinal) &&
        type.GetMembers().OfType<IPropertySymbol>().Count(property => !property.IsStatic && !property.IsImplicitlyDeclared) >
            WideDependenciesMembers;

    /// <summary>Identifies project interface or class references counted as context-bag services.</summary>
    private static bool IsService(ITypeSymbol type) =>
        type.TypeKind is TypeKind.Interface or TypeKind.Class && type.SpecialType == SpecialType.None &&
        type.ContainingNamespace?.ToDisplayString().StartsWith("SuperMetroid", StringComparison.Ordinal) == true;

    /// <summary>Names the Roslyn type kind, distinguishing records and static classes.</summary>
    private static string Kind(INamedTypeSymbol type) => type.TypeKind switch
    {
        TypeKind.Enum => "enum",
        TypeKind.Interface => "interface",
        TypeKind.Delegate => "delegate",
        TypeKind.Struct => type.IsRecord ? "record struct" : "struct",
        TypeKind.Unknown or
            TypeKind.Array or
            TypeKind.Class or
            TypeKind.Dynamic or
            TypeKind.Error or
            TypeKind.Module or
            TypeKind.Pointer or
            TypeKind.TypeParameter or
            TypeKind.Submission or
            TypeKind.FunctionPointer or
            TypeKind.Extension => type.IsRecord ? "record" : type.IsStatic ? "static class" : "class",
        _ => type.IsRecord ? "record" : type.IsStatic ? "static class" : "class",
    };

    /// <summary>Classifies declarations as product, tool, or test from their files and assemblies.</summary>
    private static string Scope(IReadOnlyCollection<string> assemblies, IReadOnlyCollection<string> files)
    {
        if (files.Any(file => file.StartsWith("csharp/test-support/", StringComparison.Ordinal)) ||
            assemblies.All(assembly => assembly.EndsWith("Verification", StringComparison.Ordinal)))
            return "test";
        if (assemblies.All(assembly => assembly is "SuperMetroid.DebugRunner" or "SuperMetroid.Tooling" or "SuperMetroid.AssetExtractor"
                or "SuperMetroid.RoomViewer" or "SuperMetroid.EnsureAnalyzer" or "SuperMetroid.EnsureAnalyzer.CodeFixes"
            || assembly.EndsWith("Audit", StringComparison.Ordinal)))
            return "tool";
        return "product";
    }

    /// <summary>Walks namespaces and nested declarations in an assembly.</summary>
    private static IEnumerable<INamedTypeSymbol> AllTypes(INamespaceSymbol root)
    {
        foreach (INamespaceOrTypeSymbol member in root.GetMembers())
        {
            if (member is INamespaceSymbol nested)
            {
                foreach (INamedTypeSymbol type in AllTypes(nested))
                    yield return type;
            }
            else if (member is INamedTypeSymbol type)
            {
                foreach (INamedTypeSymbol inner in WithNested(type))
                    yield return inner;
            }
        }
    }

    /// <summary>Yields a declared type and all of its non-implicit nested types.</summary>
    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        if (!type.IsImplicitlyDeclared)
            yield return type;
        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
            foreach (INamedTypeSymbol inner in WithNested(nested))
                yield return inner;
    }

    /// <summary>Creates an assembly-independent fully qualified key for a source type.</summary>
    private static string Key(INamedTypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "", StringComparison.Ordinal);

    /// <summary>Excludes generated output and temporary probes from the repository source inventory.</summary>
    private static bool IsRepositorySource(string root, string path) =>
        path.Length != 0 && Path.GetFullPath(path).StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase) &&
        !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
        !path.Contains($"{Path.DirectorySeparatorChar}test-temp{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    /// <summary>Returns a slash-normalized repository-relative source path.</summary>
    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');
}
