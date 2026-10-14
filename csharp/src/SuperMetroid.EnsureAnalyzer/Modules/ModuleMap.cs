using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Modules;

/// <summary>
/// The repository module map (#626), <c>csharp/module-map.json</c>, read from the build's additional files: every
/// maintained source file's module, the namespace its path implies, and Core's dependency layers.
/// </summary>
public sealed class ModuleMap
{
    /// <summary>Namespace and assembly name of the production Core project, the only project divided into layers.</summary>
    public const string CoreNamespace = "SuperMetroid.Core";

    /// <summary>File name of the map among a compilation's additional files.</summary>
    public const string FileName = "module-map.json";

    /// <summary>The module of a file: its repository-relative root, namespace root, role and mirror rules.</summary>
    public sealed record Module(
        string Path,
        string Namespace,
        string Role,
        ImmutableDictionary<string, string> Mirrors,
        string? DomainOf,
        ImmutableArray<string> SharedModules,
        ImmutableArray<string> References);

    /// <summary>Repository root: the directory above the one holding the map.</summary>
    public string RepositoryRoot { get; }

    /// <summary>Every registered module, longest path first so the most specific module wins.</summary>
    public ImmutableArray<Module> Modules { get; }

    /// <summary>Core top-level module name to its layer index (0 = lowest).</summary>
    public ImmutableDictionary<string, int> CoreLayers { get; }

    /// <summary>Core layer names in order.</summary>
    public ImmutableArray<string> CoreLayerNames { get; }

    /// <summary>Holds a parsed map; use <see cref="Parse"/> or <see cref="From"/>.</summary>
    private ModuleMap(string repositoryRoot, ImmutableArray<Module> modules, ImmutableDictionary<string, int> coreLayers, ImmutableArray<string> coreLayerNames)
    {
        RepositoryRoot = repositoryRoot;
        Modules = modules;
        CoreLayers = coreLayers;
        CoreLayerNames = coreLayerNames;
    }

    /// <summary>The map among <paramref name="options"/>' additional files, or null when the compilation carries none.</summary>
    public static ModuleMap? From(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        AdditionalText? file = options.AdditionalFiles.FirstOrDefault(text =>
            string.Equals(System.IO.Path.GetFileName(text.Path), FileName, StringComparison.OrdinalIgnoreCase));
        if (file is null)
            return null;
        string text = file.GetText(cancellationToken)?.ToString() ??
            throw new InvalidOperationException($"{file.Path} could not be read.");
        string mapDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(file.Path)) ??
            throw new InvalidOperationException($"{file.Path} has no directory.");
        string root = System.IO.Path.GetDirectoryName(mapDirectory) ?? throw new InvalidOperationException($"{mapDirectory} has no parent.");
        return Parse(text, root);
    }

    /// <summary>Parses map JSON whose repository root is <paramref name="repositoryRoot"/>.</summary>
    public static ModuleMap Parse(string json, string repositoryRoot)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement rootElement = document.RootElement;
        var modules = ImmutableArray.CreateBuilder<Module>();
        foreach (JsonElement module in Required(rootElement, "modules").EnumerateArray())
        {
            var mirrors = ImmutableDictionary.CreateBuilder<string, string>(StringComparer.Ordinal);
            if (module.TryGetProperty("mirrors", out JsonElement mirrorElement))
            {
                foreach (JsonProperty mirror in mirrorElement.EnumerateObject())
                    mirrors[mirror.Name] = mirror.Value.GetString() ?? throw new InvalidDataException($"Mirror {mirror.Name} has no namespace.");
            }
            modules.Add(new Module(
                Text(module, "path").TrimEnd('/'),
                Text(module, "namespace"),
                Text(module, "role"),
                mirrors.ToImmutable(),
                module.TryGetProperty("domainOf", out JsonElement domain) ? domain.GetString() : null,
                Strings(module, "sharedModules"),
                Strings(module, "references")));
        }
        var layers = ImmutableDictionary.CreateBuilder<string, int>(StringComparer.Ordinal);
        var names = ImmutableArray.CreateBuilder<string>();
        int index = 0;
        foreach (JsonElement layer in Required(rootElement, "coreLayers").EnumerateArray())
        {
            names.Add(Text(layer, "name"));
            foreach (JsonElement module in Required(layer, "modules").EnumerateArray())
                layers[module.GetString() ?? throw new InvalidDataException("A core layer lists a null module.")] = index;
            index++;
        }
        return new ModuleMap(
            System.IO.Path.GetFullPath(repositoryRoot),
            modules.OrderByDescending(module => module.Path.Length).ToImmutableArray(),
            layers.ToImmutable(),
            names.ToImmutable());
    }

    /// <summary>The repository-relative path of <paramref name="filePath"/> with '/' separators, or null outside the repository.</summary>
    public string? Relative(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.Path.IsPathRooted(filePath))
            return null;
        string full = System.IO.Path.GetFullPath(filePath);
        string root = RepositoryRoot.TrimEnd('\\', '/') + System.IO.Path.DirectorySeparatorChar;
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            return null;
        return full.Substring(root.Length).Replace('\\', '/');
    }

    /// <summary>The module owning a repository-relative path, or null when none is registered.</summary>
    public Module? ModuleOf(string relativePath) =>
        Modules.FirstOrDefault(module => relativePath.StartsWith(module.Path + "/", StringComparison.Ordinal));

    /// <summary>The subdirectories of <paramref name="relativePath"/> below its module root.</summary>
    public static string[] Subdirectories(Module module, string relativePath)
    {
        string inside = relativePath.Substring(module.Path.Length + 1);
        string[] parts = inside.Split('/');
        return parts.Take(parts.Length - 1).ToArray();
    }

    /// <summary>The namespace a typed file at <paramref name="relativePath"/> must declare.</summary>
    public static string ExpectedNamespace(Module module, string relativePath)
    {
        string[] directories = Subdirectories(module, relativePath);
        if (directories.Length > 0 && module.Mirrors.TryGetValue(directories[0], out string? mirrored))
            return Join(mirrored, directories.Skip(1));
        return Join(module.Namespace, directories);
    }

    /// <summary>The Core layer of a namespace inside SuperMetroid.Core, or null outside Core.</summary>
    public int? CoreLayerOf(string? ns)
    {
        if (ns is null || !(ns == CoreNamespace || ns.StartsWith(CoreNamespace + ".", StringComparison.Ordinal)))
            return null;
        string[] parts = ns.Split('.');
        if (parts.Length < 3)
            return 0;
        return CoreLayers.TryGetValue(parts[2], out int layer) ? layer : null;
    }

    /// <summary>Appends <paramref name="directories"/> to a namespace root, dot-separated.</summary>
    private static string Join(string root, IEnumerable<string> directories)
    {
        string tail = string.Join(".", directories);
        return tail.Length == 0 ? root : root.Length == 0 ? tail : root + "." + tail;
    }

    /// <summary>A required JSON property, rejecting a map that lacks it.</summary>
    private static JsonElement Required(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value) ? value : throw new InvalidDataException($"The module map lacks '{name}'.");

    /// <summary>A required non-null string property.</summary>
    private static string Text(JsonElement element, string name) =>
        Required(element, name).GetString() ?? throw new InvalidDataException($"The module map's '{name}' is null.");

    /// <summary>An optional string-array property, empty when absent.</summary>
    private static ImmutableArray<string> Strings(JsonElement element, string name) =>
        element.TryGetProperty(name, out JsonElement value)
            ? value.EnumerateArray().Select(item => item.GetString() ?? throw new InvalidDataException($"'{name}' lists a null entry.")).ToImmutableArray()
            : ImmutableArray<string>.Empty;
}
