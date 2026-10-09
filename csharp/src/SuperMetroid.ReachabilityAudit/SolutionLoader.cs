using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>The compiled repository: every project of the solution with its error-free compilation.</summary>
/// <param name="RepositoryRoot">The repository directory used to identify hand-written source.</param>
/// <param name="Projects">Each loaded project paired with the compilation used for symbol analysis.</param>
internal sealed record LoadedSolution(string RepositoryRoot, IReadOnlyList<(Project Project, Compilation Compilation)> Projects);

/// <summary>Loads and compiles every project listed in a .slnx solution, failing on any load or compile error.</summary>
internal static class SolutionLoader
{
    /// <summary>Loads the solution's projects and returns only after all available compilations are error-free.</summary>
    /// <param name="solutionPath">Absolute or relative path to the solution XML file.</param>
    /// <returns>The repository root and compiled projects for subsequent source analysis.</returns>
    /// <exception cref="InvalidOperationException">A workspace load failed or a project has compilation errors.</exception>
    public static async Task<LoadedSolution> LoadAsync(string solutionPath)
    {
        string solutionDirectory = Path.GetDirectoryName(solutionPath)!;
        using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string> { ["Configuration"] = "Release" });
        var failures = new List<string>();
        workspace.RegisterWorkspaceFailedHandler(e =>
        {
            Console.Error.WriteLine($"WORKSPACE {e.Diagnostic.Kind}: {e.Diagnostic.Message}");
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                failures.Add(e.Diagnostic.Message);
        });

        var projectPaths = XDocument.Load(solutionPath).Descendants("Project")
            .Select(p => Path.GetFullPath(Path.Combine(solutionDirectory, (string?)p.Attribute("Path")
                ?? throw new InvalidDataException($"{solutionPath}: project entry has no Path."))));
        foreach (string path in projectPaths)
        {
            // Project references load transitively; open each project only once.
            if (workspace.CurrentSolution.Projects.Any(p => string.Equals(p.FilePath, path, StringComparison.OrdinalIgnoreCase)))
                continue;
            Console.WriteLine($"Loading {Path.GetFileName(path)}");
            await workspace.OpenProjectAsync(path);
        }
        if (failures.Count > 0)
            throw new InvalidOperationException("Workspace load failed:\n" + string.Join('\n', failures));

        var compiled = new List<(Project, Compilation)>();
        foreach (var project in workspace.CurrentSolution.Projects)
        {
            var compilation = await project.GetCompilationAsync()
                ?? throw new InvalidOperationException($"{project.Name} produced no compilation.");
            // Symbol binding is only trustworthy for code that compiles.
            var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
                throw new InvalidOperationException($"{project.Name} has {errors.Count} compile errors; first: {errors[0]}");
            compiled.Add((project, compilation));
            Console.WriteLine($"Compiled {project.Name}");
        }
        return new LoadedSolution(Path.GetDirectoryName(solutionDirectory)!, compiled);
    }
}
