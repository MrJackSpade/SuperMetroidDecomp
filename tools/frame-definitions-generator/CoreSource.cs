using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>SuperMetroid.Core's compilation and the directory its source paths are reported against.</summary>
/// <param name="Compilation">The error-free Core compilation.</param>
/// <param name="Directory">The Core project directory.</param>
internal sealed record CoreSource(Compilation Compilation, string Directory)
{
    /// <summary>Loads and compiles SuperMetroid.Core, failing on any workspace failure or compile error.</summary>
    /// <param name="repository">The repository root.</param>
    public static async Task<CoreSource> LoadAsync(string repository)
    {
        string projectPath = Path.Combine(repository, "csharp", "src", "SuperMetroid.Core", "SuperMetroid.Core.csproj");
        if (!File.Exists(projectPath))
            throw new FileNotFoundException("Run from the repository root: SuperMetroid.Core's project is not where it was expected.", projectPath);

        using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string> { ["Configuration"] = "Release" });
        var failures = new List<string>();
        workspace.RegisterWorkspaceFailedHandler(e =>
        {
            Console.Error.WriteLine($"WORKSPACE {e.Diagnostic.Kind}: {e.Diagnostic.Message}");
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                failures.Add(e.Diagnostic.Message);
        });
        Console.WriteLine($"Loading {Path.GetFileName(projectPath)}");
        Project project = await workspace.OpenProjectAsync(projectPath);
        if (failures.Count > 0)
            throw new InvalidOperationException("Workspace load failed:\n" + string.Join('\n', failures));

        Compilation compilation = await project.GetCompilationAsync()
            ?? throw new InvalidOperationException($"{project.Name} produced no compilation.");
        // Symbol binding is only trustworthy for code that compiles.
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (errors.Count > 0)
            throw new InvalidOperationException($"{project.Name} has {errors.Count} compile errors:\n" + string.Join('\n', errors));
        return new CoreSource(compilation, Path.GetDirectoryName(projectPath)!);
    }
}
