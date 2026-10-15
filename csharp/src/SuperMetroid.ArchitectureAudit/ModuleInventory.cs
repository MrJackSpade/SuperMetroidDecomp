using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// #626 module inventory: every maintained source file with its module, role, namespace, compiling assemblies,
/// Core layer and the other modules whose source types it references, plus each module's aggregate dependencies and
/// the dependency cycles among Core modules. The checked-in <c>docs/module-inventory.json</c> must match it, and every
/// cycle must be exactly one of the map's <c>acceptedCycles</c>, each of which must still occur.
/// </summary>
internal static class ModuleInventory
{
    /// <summary>Builds the inventory document from every compiled repository source file and reports unaccepted or stale cycles into <paramref name="problems"/>.</summary>
    public static object Collect(LoadedSolution solution, List<string> problems)
    {
        string root = solution.RepositoryRoot;
        string mapText = File.ReadAllText(Path.Combine(root, "csharp", ModuleMap.FileName));
        ModuleMap map = ModuleMap.Parse(mapText, root);
        var files = new SortedDictionary<string, FileEntry>(StringComparer.Ordinal);
        foreach (var (project, compilation) in solution.Projects)
        {
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                if (map.Relative(tree.FilePath) is not { } relative || relative.Contains("/obj/", StringComparison.Ordinal) ||
                    relative.StartsWith("test-temp/", StringComparison.Ordinal))
                    continue;
                if (!files.TryGetValue(relative, out FileEntry? entry))
                    files[relative] = entry = Describe(map, relative, tree, compilation);
                entry.Assemblies.Add(project.AssemblyName);
            }
        }
        var modules = files.Values
            .GroupBy(file => file.Module, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new
            {
                module = group.Key,
                role = group.First().Role,
                files = group.Count(),
                dependsOn = group.SelectMany(file => file.DependsOn).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            })
            .ToList();
        List<List<string>> cycles = CoreCycles(files.Values);
        CheckCycles(cycles, AcceptedCycles(mapText), problems);
        return new
        {
            issue = 626,
            regenerate = "dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write",
            coreLayers = map.CoreLayerNames,
            coreCycles = cycles,
            modules,
            files = files.Values.Select(file => new
            {
                path = file.Path,
                module = file.Module,
                role = file.Role,
                @namespace = file.Namespace,
                layer = file.Layer,
                assemblies = file.Assemblies,
                dependsOn = file.DependsOn.Count == 0 ? null : file.DependsOn,
            }).ToList(),
        };
    }

    /// <summary>Classifies one file and resolves the modules of the repository types it references.</summary>
    private static FileEntry Describe(ModuleMap map, string relative, SyntaxTree tree, Compilation compilation)
    {
        ModuleMap.Module module = map.ModuleOf(relative) ??
            throw new InvalidDataException($"{relative} is compiled but lies in no module of csharp/module-map.json.");
        var root = (CompilationUnitSyntax)tree.GetRoot();
        string? ns = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
        string self = ModuleKey(map, relative, ns);
        int? layer = map.CoreLayerOf(ns);
        SemanticModel model = compilation.GetSemanticModel(tree);
        var dependsOn = new SortedSet<string>(StringComparer.Ordinal);
        foreach (SimpleNameSyntax name in root.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (name.IsPartOfStructuredTrivia() || name.FirstAncestorOrSelf<UsingDirectiveSyntax>() is not null ||
                name.FirstAncestorOrSelf<BaseNamespaceDeclarationSyntax>() is { } declaration && declaration.Name.Span.Contains(name.Span))
                continue;
            ISymbol? symbol = model.GetSymbolInfo(name).Symbol;
            INamedTypeSymbol? type = symbol as INamedTypeSymbol ?? symbol?.ContainingType;
            if (type is null)
                continue;
            foreach (SyntaxReference reference in type.OriginalDefinition.DeclaringSyntaxReferences)
            {
                if (map.Relative(reference.SyntaxTree.FilePath) is not { } target || target.Contains("/obj/", StringComparison.Ordinal))
                    continue;
                string? targetNamespace = type.OriginalDefinition.ContainingNamespace?.ToDisplayString();
                string key = ModuleKey(map, target, targetNamespace);
                if (key != self)
                    dependsOn.Add(key);
                break;
            }
        }
        return new FileEntry(relative, self, module.Role, ns, layer is { } index ? map.CoreLayerNames[index] : null,
            new SortedSet<string>(StringComparer.Ordinal), dependsOn);
    }

    /// <summary>
    /// The module a file belongs to: its registered module, refined to the top-level domain module inside the
    /// production Core project.
    /// </summary>
    private static string ModuleKey(ModuleMap map, string relative, string? ns)
    {
        ModuleMap.Module module = map.ModuleOf(relative) ?? throw new InvalidDataException($"{relative} lies in no registered module.");
        if (module.Namespace == ModuleMap.CoreNamespace && module.Role == "production")
        {
            string[] parts = (ns ?? "").Split('.');
            return parts.Length >= 3 ? module.Path + "/" + parts[2] : module.Path;
        }
        return module.Path;
    }

    /// <summary>The module sets listed under the map's <c>acceptedCycles</c>, each sorted.</summary>
    private static List<string[]> AcceptedCycles(string mapText)
    {
        using JsonDocument document = JsonDocument.Parse(mapText);
        if (!document.RootElement.TryGetProperty("acceptedCycles", out JsonElement accepted))
            return [];
        return accepted.EnumerateArray().Select(cycle => cycle.GetProperty("modules").EnumerateArray()
            .Select(module => module.GetString() ?? throw new InvalidDataException("An accepted cycle lists a null module."))
            .Order(StringComparer.Ordinal).ToArray()).ToList();
    }

    /// <summary>Requires every found cycle to be accepted exactly and every accepted cycle to still occur.</summary>
    private static void CheckCycles(List<List<string>> found, List<string[]> accepted, List<string> problems)
    {
        foreach (List<string> cycle in found.Where(cycle => !accepted.Any(entry => entry.SequenceEqual(cycle, StringComparer.Ordinal))))
            problems.Add($"Core modules form an unaccepted dependency cycle: {string.Join(", ", cycle)}. Break it, or list it with its tracking issues under acceptedCycles in csharp/module-map.json.");
        foreach (string[] entry in accepted.Where(entry => !found.Any(cycle => cycle.SequenceEqual(entry, StringComparer.Ordinal))))
            problems.Add($"Stale accepted cycle in csharp/module-map.json: {string.Join(", ", entry)} no longer occurs exactly; update or remove it.");
    }

    /// <summary>The strongly connected groups (size above one) of the Core module dependency graph.</summary>
    private static List<List<string>> CoreCycles(IEnumerable<FileEntry> files)
    {
        const string corePrefix = "csharp/src/SuperMetroid.Core/";
        var edges = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (FileEntry file in files.Where(file => file.Module.StartsWith(corePrefix, StringComparison.Ordinal)))
        {
            if (!edges.TryGetValue(file.Module, out SortedSet<string>? targets))
                edges[file.Module] = targets = new SortedSet<string>(StringComparer.Ordinal);
            foreach (string target in file.DependsOn.Where(target => target.StartsWith(corePrefix, StringComparison.Ordinal)))
                targets.Add(target);
        }
        var index = new Dictionary<string, int>(StringComparer.Ordinal);
        var low = new Dictionary<string, int>(StringComparer.Ordinal);
        var stack = new Stack<string>();
        var onStack = new HashSet<string>(StringComparer.Ordinal);
        var groups = new List<List<string>>();
        int next = 0;
        void Connect(string node)
        {
            index[node] = low[node] = next++;
            stack.Push(node);
            onStack.Add(node);
            foreach (string target in edges.TryGetValue(node, out SortedSet<string>? targets) ? targets : [])
            {
                if (!index.ContainsKey(target))
                {
                    Connect(target);
                    low[node] = Math.Min(low[node], low[target]);
                }
                else if (onStack.Contains(target))
                {
                    low[node] = Math.Min(low[node], index[target]);
                }
            }
            if (low[node] != index[node])
                return;
            var group = new List<string>();
            string member;
            do
            {
                member = stack.Pop();
                onStack.Remove(member);
                group.Add(member[corePrefix.Length..]);
            }
            while (member != node);
            if (group.Count > 1)
                groups.Add(group.Order(StringComparer.Ordinal).ToList());
        }
        foreach (string node in edges.Keys)
        {
            if (!index.ContainsKey(node))
                Connect(node);
        }
        return groups.OrderBy(group => group[0], StringComparer.Ordinal).ToList();
    }

    /// <summary>One inventoried source file.</summary>
    private sealed record FileEntry(string Path, string Module, string Role, string? Namespace, string? Layer,
        SortedSet<string> Assemblies, SortedSet<string> DependsOn);
}
