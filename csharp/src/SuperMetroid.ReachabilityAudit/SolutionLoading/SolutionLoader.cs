using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace SuperMetroid.ReachabilityAudit.SolutionLoading;

/// <summary>Loads and compiles every project listed in a .slnx solution, failing on any load or compile error.</summary>
internal static class SolutionLoader
{
    /// <summary>The compile errors of each live compilation, found once.</summary>
    private static readonly ConditionalWeakTable<Compilation, List<Diagnostic>> Errors = new();

    /// <summary>Loads every listed project once, reports workspace failures, and returns only error-free compilations.</summary>
    public static async Task<LoadedSolution> LoadAsync(string solutionPath)
    {
        using MSBuildWorkspace workspace = await OpenAsync(solutionPath);
        return await CompileAsync(workspace.CurrentSolution, RepositoryRoot(solutionPath));
    }

    /// <summary>The repository holding the solution: the directory above the solution's.</summary>
    public static string RepositoryRoot(string solutionPath)
    {
        string solutionDirectory = Path.GetDirectoryName(Path.GetFullPath(solutionPath)) ??
            throw new ArgumentException($"{solutionPath} has no containing directory.", nameof(solutionPath));
        return Path.GetDirectoryName(solutionDirectory) ??
            throw new InvalidOperationException($"{solutionDirectory} has no repository root above it.");
    }

    /// <summary>Opens every listed project once, in Release, failing on any workspace failure; nothing is compiled yet. The
    /// caller disposes the workspace once it is done with its solution.</summary>
    public static async Task<MSBuildWorkspace> OpenAsync(string solutionPath)
    {
        string solutionDirectory = Path.GetDirectoryName(Path.GetFullPath(solutionPath)) ??
            throw new ArgumentException($"{solutionPath} has no containing directory.", nameof(solutionPath));
        var workspace = MSBuildWorkspace.Create(new Dictionary<string, string> { ["Configuration"] = "Release" });
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
        {
            workspace.Dispose();
            throw new InvalidOperationException("Workspace load failed:\n" + string.Join('\n', failures));
        }
        return workspace;
    }

    /// <summary>
    /// Compiles every project of an already loaded solution, on all processors, and returns the error-free compilations
    /// in the solution's project order; any compile error in any project fails, listing every project's errors. The
    /// asynchronous cleanup loads the solution once and hands each of its steps the solution as the previous steps left
    /// it (tools/cleanup/README.md). A project <paramref name="mayFail"/> accepts is returned with its errors, which are
    /// counted on standard error: the test-only-member fix binds checks that name private members their deleted access
    /// stood in for, and regenerates that access.
    /// </summary>
    public static async Task<LoadedSolution> CompileAsync(Solution solution, string repositoryRoot, Func<Project, bool>? mayFail = null)
    {
        var projects = solution.Projects.ToList();
        var compiled = await Task.WhenAll(projects.Select(project => Task.Run(async () =>
        {
            var compilation = await project.GetCompilationAsync()
                ?? throw new InvalidOperationException($"{project.Name} produced no compilation.");
            // Symbol binding is only trustworthy for code that compiles. A project neither the cleanup's steps nor its
            // references changed keeps its compilation, whose errors are known already.
            var errors = Errors.GetValue(compilation, compiled =>
                compiled.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList());
            return (Compilation: new ProjectCompilation(project, compilation), Errors: errors);
        })));
        foreach (var result in compiled.Where(result => result.Errors.Count == 0))
            Console.WriteLine($"Compiled {result.Compilation.Project.Name}");
        // Every project's errors are reported together, so one load shows everything that must be fixed.
        foreach (var result in compiled.Where(result => result.Errors.Count != 0 && mayFail?.Invoke(result.Compilation.Project) == true))
            Console.Error.WriteLine($"{result.Compilation.Project.Name}: {result.Errors.Count} compile errors accepted:\n" +
                string.Join('\n', result.Errors));
        var compileErrors = compiled.Where(result => mayFail?.Invoke(result.Compilation.Project) != true)
            .SelectMany(result => result.Errors.Select(error => $"{result.Compilation.Project.Name}: {error}")).ToList();
        if (compileErrors.Count > 0)
            throw new InvalidOperationException(
                $"{compileErrors.Count} compile errors:\n" + string.Join('\n', compileErrors.Distinct(StringComparer.Ordinal)));
        return new LoadedSolution(repositoryRoot, [.. compiled.Select(result => result.Compilation)]);
    }
}
