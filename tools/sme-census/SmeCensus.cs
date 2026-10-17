#:package Microsoft.CodeAnalysis.Workspaces.MSBuild@5.9.0
#:package Microsoft.CodeAnalysis.CSharp.Workspaces@5.9.0
#:property PublishAot=false

// IDE-equivalent census of SME analyzer findings. Loads every project of a .slnx in one MSBuildWorkspace, where
// project references are source compilations exactly as in Visual Studio, runs the repository's EnsureAnalyzer on
// each compilation with the project's own analyzer configuration, and writes every SME diagnostic to a TSV file.
// It needs no clean build: analyzer findings are counted separately from compile errors, so it lists every finding
// while the analyzers keep the build red. A project with real compile errors is reported and the run fails.
// Build the analyzer first, since projects load it from its output:
//   dotnet build csharp/src/SuperMetroid.EnsureAnalyzer -c Release
//   dotnet run tools/sme-census/SmeCensus.cs -- csharp/SuperMetroid.Full.slnx <output.tsv>
// Each TSV row: rule, severity, project, repository path:line, message.

using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

return await Census.RunAsync(args);

/// <summary>The census process boundary and analysis.</summary>
internal static class Census
{
    /// <summary>SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX: no Windows error dialogs.</summary>
    private const uint NoDialogs = 0x0001 | 0x0002 | 0x8000;

    /// <summary>Disables Windows error dialogs for this process.</summary>
    [DllImport("kernel32.dll")]
    private static extern uint SetErrorMode(uint mode);

    /// <summary>Runs the census behind the no-dialog boundary; any failure is written in full with a nonzero exit.</summary>
    public static async Task<int> RunAsync(string[] args)
    {
        if (OperatingSystem.IsWindows())
            _ = SetErrorMode(NoDialogs);
        try
        {
            if (args.Length != 2)
                throw new ArgumentException("usage: SmeCensus <solution.slnx> <output.tsv>");
            await AnalyzeAsync(args[0], args[1]);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>Loads the solution, analyzes each project, and writes the findings and their tallies.</summary>
    private static async Task AnalyzeAsync(string solutionPath, string outputPath)
    {
        string solutionDirectory = Path.GetDirectoryName(Path.GetFullPath(solutionPath))!;
        using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string> { ["Configuration"] = "Release" });
        var failures = new List<string>();
        workspace.RegisterWorkspaceFailedHandler(e =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                failures.Add(e.Diagnostic.Message);
        });
        foreach (string path in XDocument.Load(solutionPath).Descendants("Project")
                     .Select(p => Path.GetFullPath(Path.Combine(solutionDirectory, (string)p.Attribute("Path")!))))
        {
            if (workspace.CurrentSolution.Projects.Any(p => string.Equals(p.FilePath, path, StringComparison.OrdinalIgnoreCase)))
                continue;
            await workspace.OpenProjectAsync(path);
        }
        if (failures.Count > 0)
            throw new InvalidOperationException("Workspace load failed:\n" + string.Join('\n', failures));

        var rows = new List<string>();
        var byRule = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var byProject = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var uncompiled = new List<string>();
        foreach (Project project in workspace.CurrentSolution.Projects)
        {
            Compilation compilation = await project.GetCompilationAsync()
                ?? throw new InvalidOperationException($"{project.Name} produced no compilation.");
            var compileErrors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (compileErrors.Count > 0)
            {
                // A project that does not compile is reported; its analyzer results are not counted.
                Console.WriteLine($"{project.Name}: DOES NOT COMPILE\n  " + string.Join("\n  ", compileErrors.Take(20)));
                uncompiled.Add(project.Name);
                continue;
            }
            var analyzers = project.AnalyzerReferences
                .Where(reference => reference.Display?.Contains("SuperMetroid.EnsureAnalyzer", StringComparison.Ordinal) == true &&
                    !reference.Display.Contains("CodeFixes", StringComparison.Ordinal))
                .SelectMany(reference => reference.GetAnalyzers(project.Language))
                .ToArray();
            if (analyzers.Length == 0)
            {
                Console.WriteLine($"{project.Name}: no EnsureAnalyzer reference");
                continue;
            }
            var withAnalyzers = compilation.WithAnalyzers([.. analyzers], project.AnalyzerOptions);
            var result = await withAnalyzers.GetAnalysisResultAsync(CancellationToken.None);
            var diagnostics = result.GetAllDiagnostics()
                .Where(d => d.Id.StartsWith("SME", StringComparison.Ordinal) || d.Id == "AD0001")
                .Where(d => !d.IsSuppressed && d.Severity != DiagnosticSeverity.Hidden)
                .ToList();
            foreach (Diagnostic diagnostic in diagnostics)
            {
                var span = diagnostic.Location.GetLineSpan();
                string file = span.Path.Replace('\\', '/');
                int index = file.IndexOf("/csharp/", StringComparison.Ordinal);
                rows.Add(string.Join('\t', diagnostic.Id, diagnostic.Severity, project.Name,
                    (index >= 0 ? file[(index + 1)..] : file) + ":" + (span.StartLinePosition.Line + 1),
                    diagnostic.GetMessage().Replace('\t', ' ').Replace('\n', ' ')));
                byRule[diagnostic.Id] = byRule.GetValueOrDefault(diagnostic.Id) + 1;
                byProject[project.Name] = byProject.GetValueOrDefault(project.Name) + 1;
            }
            Console.WriteLine($"{project.Name}: {diagnostics.Count}");
        }
        File.WriteAllLines(outputPath, rows.OrderBy(row => row, StringComparer.Ordinal));
        Console.WriteLine($"total {rows.Count}");
        if (uncompiled.Count > 0)
            throw new InvalidOperationException("Projects that do not compile: " + string.Join(", ", uncompiled));
        Console.WriteLine("by rule: " + string.Join(", ", byRule.OrderByDescending(p => p.Value).Select(p => $"{p.Key} {p.Value}")));
        Console.WriteLine("by project: " + string.Join(", ", byProject.OrderByDescending(p => p.Value).Select(p => $"{p.Key} {p.Value}")));
    }
}
