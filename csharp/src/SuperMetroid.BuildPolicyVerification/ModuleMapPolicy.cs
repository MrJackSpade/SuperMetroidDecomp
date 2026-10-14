using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace SuperMetroid.BuildPolicyVerification;

/// <summary>
/// The #626 checks that the analyzers cannot make: every maintained project and source root is registered in
/// <c>csharp/module-map.json</c>, each project's references and shared-source imports match the map exactly (and
/// production projects reference only production projects), and no build output lives under a source root outside
/// each project's own ignored <c>bin</c> and <c>obj</c>.
/// </summary>
internal static class ModuleMapPolicy
{
    /// <summary>Source roots whose immediate subdirectories are modules.</summary>
    private static readonly string[] SourceRoots = ["csharp/src", "csharp/test-support", "csharp/tools"];

    /// <summary>Runs every module-map policy check against the repository at <paramref name="root"/>.</summary>
    internal static void Verify(string root)
    {
        Module[] modules = Load(root);
        var findings = new List<string>();
        VerifyRegistered(root, modules, findings);
        VerifyReferences(root, modules, findings);
        VerifyNoBuildOutput(root, modules, findings);
        if (findings.Count != 0)
            throw new InvalidDataException("The module map is violated:" + Environment.NewLine + string.Join(Environment.NewLine, findings));
        Console.WriteLine($"Module map: {modules.Length} registered modules; project references, shared-source imports and source roots agree with it.");
    }

    /// <summary>Requires every source-root directory to be a registered module and every registered module to exist.</summary>
    private static void VerifyRegistered(string root, Module[] modules, List<string> findings)
    {
        var registered = modules.Select(module => module.Path).ToHashSet(StringComparer.Ordinal);
        foreach (string sourceRoot in SourceRoots)
        {
            foreach (string directory in Directory.EnumerateDirectories(Path.Combine(root, sourceRoot)))
            {
                string path = sourceRoot + "/" + Path.GetFileName(directory);
                if (!registered.Contains(path))
                    findings.Add($"{path} is not a module registered in csharp/module-map.json");
            }
        }
        foreach (Module module in modules)
        {
            if (!Directory.Exists(Path.Combine(root, module.Path)))
                findings.Add($"registered module {module.Path} does not exist");
        }
    }

    /// <summary>Compares each project's references and shared-source imports with the map.</summary>
    private static void VerifyReferences(string root, Module[] modules, List<string> findings)
    {
        Dictionary<string, Module> byProject = modules
            .Where(module => module.References is not null)
            .ToDictionary(module => Path.GetFileName(module.Path), StringComparer.Ordinal);
        foreach (Module module in byProject.Values)
        {
            if (module.References is not { } declared)
                continue;
            string name = Path.GetFileName(module.Path);
            string projectFile = Path.Combine(root, module.Path, name + ".csproj");
            if (!File.Exists(projectFile))
            {
                findings.Add($"{module.Path} declares project references but has no {name}.csproj");
                continue;
            }
            XDocument project = XDocument.Load(projectFile);
            string[] references = Includes(project, "ProjectReference", "Include", ".csproj");
            string[] imports = Includes(project, "Import", "Project", ".projitems");
            Compare(findings, module.Path, "project references", references, declared);
            Compare(findings, module.Path, "shared-source imports", imports, module.Support);
            if (module.Role == "production")
            {
                foreach (string reference in references.Where(reference => byProject.TryGetValue(reference, out Module? target) && target.Role != "production"))
                    findings.Add($"{module.Path} is production code but references {reference} ({byProject[reference].Role})");
            }
            foreach (string reference in references.Where(reference => !byProject.ContainsKey(reference)))
                findings.Add($"{module.Path} references unregistered project {reference}");
        }
    }

    /// <summary>
    /// Rejects ignored output under a source root other than a project's own <c>bin</c>/<c>obj</c> and IDE user
    /// files, and tracked build artifacts anywhere under a source root.
    /// </summary>
    private static void VerifyNoBuildOutput(string root, Module[] modules, List<string> findings)
    {
        string[] roots = modules.Where(module => module.Role != "fixture").Select(module => module.Path).ToArray();
        foreach (string ignored in Git(root, ["ls-files", "--others", "--ignored", "--exclude-standard", "--directory", "--", .. roots]))
        {
            string path = ignored.TrimEnd('/');
            string? owner = roots.FirstOrDefault(module => path.StartsWith(module + "/", StringComparison.Ordinal));
            string inside = owner is null ? path : path[(owner.Length + 1)..];
            if (inside is "bin" or "obj" || inside.EndsWith(".csproj.user", StringComparison.Ordinal))
                continue;
            findings.Add($"{ignored} is build or tool output inside a source root; write it under the project's bin/obj or outside csharp/");
        }
        foreach (string tracked in Git(root, ["ls-files", "--", .. roots]))
        {
            string extension = Path.GetExtension(tracked);
            if (extension is ".dll" or ".pdb" or ".exe" or ".nupkg" || tracked.Contains("/bin/", StringComparison.Ordinal) ||
                tracked.Contains("/obj/", StringComparison.Ordinal))
                findings.Add($"{tracked} is a committed build artifact inside a source root");
        }
    }

    /// <summary>Reports the entries of <paramref name="expected"/> missing from <paramref name="actual"/> and the undeclared extras.</summary>
    private static void Compare(List<string> findings, string module, string kind, string[] actual, string[] expected)
    {
        string[] missing = expected.Except(actual, StringComparer.Ordinal).ToArray();
        string[] extra = actual.Except(expected, StringComparer.Ordinal).ToArray();
        if (missing.Length != 0)
            findings.Add($"{module} lacks the {kind} the map declares: {string.Join(", ", missing)}");
        if (extra.Length != 0)
            findings.Add($"{module} has {kind} the map does not declare: {string.Join(", ", extra)}");
    }

    /// <summary>The file stems of a project's <paramref name="element"/> items whose <paramref name="attribute"/> ends in <paramref name="extension"/>.</summary>
    private static string[] Includes(XDocument project, string element, string attribute, string extension) =>
        project.Descendants(element)
            .Select(node => (string?)node.Attribute(attribute))
            .OfType<string>()
            .Where(path => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            .Select(path => Path.GetFileNameWithoutExtension(path.Replace('\\', '/')))
            .ToArray();

    /// <summary>Reads the registered modules from <c>csharp/module-map.json</c>.</summary>
    private static Module[] Load(string root)
    {
        using JsonDocument map = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "csharp", "module-map.json")));
        return map.RootElement.GetProperty("modules").EnumerateArray().Select(module => new Module(
            module.GetProperty("path").GetString() ?? throw new InvalidDataException("A module has a null path."),
            module.GetProperty("role").GetString() ?? throw new InvalidDataException("A module has a null role."),
            module.TryGetProperty("references", out JsonElement references) ? Strings(references) : null,
            module.TryGetProperty("support", out JsonElement support) ? Strings(support) : [])).ToArray();
    }

    /// <summary>The strings of a JSON array, rejecting null entries.</summary>
    private static string[] Strings(JsonElement array) =>
        array.EnumerateArray().Select(item => item.GetString() ?? throw new InvalidDataException("A module list has a null entry.")).ToArray();

    /// <summary>Runs git in the repository and returns its output lines, failing on a nonzero exit.</summary>
    private static string[] Git(string root, string[] arguments)
    {
        var start = new ProcessStartInfo("git") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, WorkingDirectory = root };
        foreach (string argument in arguments)
            start.ArgumentList.Add(argument);
        using Process process = Process.Start(start) ?? throw new InvalidOperationException("git could not be started.");
        Task<string> error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed ({process.ExitCode}): {error.Result}");
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>One registered module: its repository-relative path, role, declared references and shared-source imports.</summary>
    private sealed record Module(string Path, string Role, string[]? References, string[] Support);
}
