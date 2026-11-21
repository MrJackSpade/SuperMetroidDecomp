#:package Microsoft.CodeAnalysis.Workspaces.MSBuild@5.9.0
#:package Microsoft.CodeAnalysis.CSharp.Workspaces@5.9.0
#:package Microsoft.Bcl.AsyncInterfaces@10.0.1
#:property PublishAot=false
#:property ServerGarbageCollection=true
#:project ../../csharp/src/SuperMetroid.ReachabilityAudit/SuperMetroid.ReachabilityAudit.csproj
#:project ../../csharp/src/SuperMetroid.ArchitectureAudit/SuperMetroid.ArchitectureAudit.csproj

// Applies the code fixes of the automatically fixed rules (csharp/.editorconfig names them) to a whole solution, the
// way the asynchronous cleanup needs them applied (tools/cleanup/README.md).
//
// It loads every project of a .slnx (or one .csproj) into one workspace (WorkspaceLoad), runs each project's own analyzers for
// the requested rules with the project's own configuration, and applies each rule's fixer from the assembly RuleOrder
// names: Roslyn's code-style fixers (the same fixers `dotnet format style` uses) from the code-style package the
// projects reference, or the repository's own fixers from SuperMetroid.EnsureAnalyzer.CodeFixes.dll, which the
// projects reference as an analyzer and which must be built in the configuration the workspace opens (Debug; run.ps1
// builds it first).
//
// csharp/.editorconfig sets the group to none, so the build skips their analyzers and they cost it no time. The runner
// therefore enables exactly the requested rules, as warnings, in its own analysis: in the compilation's per-file
// diagnostic options, where the .editorconfig severity would otherwise win over any compilation-wide setting, and in
// the analyzer configuration the code-style analyzers read their own severity from. A rule is enabled only where the
// project's own configuration would enable it if its line in the group said warning instead of none: a section that
// turns the rule off for some files (the SME rules outside Core and AssetExtraction) keeps it off there. Every other
// rule keeps the project's configuration. The caller (tools/cleanup/run.ps1) reads the group from csharp/.editorconfig.
//
// A compiler warning in the group (CS0168, ...) has no analyzer: the compiler reports it while it binds. The runner
// reads it from the compilation's own diagnostics, with the rule enabled in the per-file options as above, and applies
// the repository's fixer for it. A project that compiled when the run started and has compiler errors after a pass
// is recorded as a problem, naming the errors, and its files are left alone from then on; the run goes on.
//
// IDE0079 (unnecessary suppression) has no command-line analyzer either. When it is requested, the runner runs, with
// the whole cleanup group enabled in its scope and suppressed diagnostics reported, every analyzer that reports an id one
// of the project's suppressions names and every diagnostic suppressor (SuppressionEvidence), and finds each #pragma
// warning disable code and [SuppressMessage] that suppressed nothing (UnnecessarySuppressions). Only a suppression of a
// rule enabled in its file is judged; one of an unknown or disabled rule is left alone.
//
// A file compiled into several projects (test-support through .projitems, or a <Compile Link>) is edited only where
// every project compiling it reports the same finding at the same place, and not at all while any of them has a
// compiler error, because a semantic model with errors reads needed code as unnecessary. Where the projects disagree
// about a file's unnecessary usings, only the usings every project reports are removed; other disagreeing findings
// are left and counted as kept.
//
// A fix may change several files (a field and its property in different parts of a partial type), and may add,
// rename or delete files (SME6400, SME6401, SME6360). It is applied once every file it changes belongs to an
// analyzed project that compiles; until then its findings are kept, and a fix touching a file another fix of the same
// pass touched waits for the next pass. A project compiling a file of such a fix that no longer compiles after it is
// recorded as a problem naming the fix, and the fix is kept. An added file joins every project that compiles the file its finding is in; when the files are
// written, the explicit source lists (.projitems and <Compile Include> items) follow the files created and deleted
// (ProjectLists). A rename that changes only the case of a file's name is the same file on a file system that ignores
// case: it is deleted before it is created, and the cleanup loop tells git the index's old name is gone.
//
// Fixes cascade (a removed cast can leave a using unnecessary), so the analysis repeats until a pass finds nothing to
// apply. Each pass applies one rule per file, because the other findings' spans are stale once a file changes. The
// loop stops, keeping the tree it has and recording a problem, if a file returns to an earlier text (a cycle) or the
// passes exceed MaxPasses.
//
// The owner's rule (2026-11-15, #2599): a fix is always applied, and when it cannot be applied without breaking the
// code, it breaks the code; the build then fails at that site and the cleanup commits it as it is. Nothing is left
// unfixed: no site is skipped or recorded as unfixed, and no fixer's failure is caught. A fixer that offers no fix,
// has no fix-all, produces no usable change or throws is a defect of that fixer, and fails the run with its full
// exception (written to standard error, nonzero exit); so does a fix the runner cannot apply.
//
// Usage: dotnet run tools/cleanup/CodeFixRunner.cs -- <solution.slnx | project.csproj> <summary.json> <rule,rule,...> [--dry-run]
// --dry-run prints every finding with all its locations and writes nothing.
//
// With --cleanup it runs the asynchronous cleanup's whole fix and prune loop and its inventories in this one process, on
// one load of the solution (CleanupLoop): the code fixes, the reachability audit's prune of dead code and the
// architecture audit's prune of stale reviews take turns, each handed the solution as the steps before it left it, and
// the outcome is written as JSON for tools/cleanup/run.ps1's commit message. The audits are referenced as projects:
//   dotnet run tools/cleanup/CodeFixRunner.cs -- --cleanup <solution.slnx> <artifacts directory> <outcome.json> <rule,rule,...>
//     <prune categories> <prune path prefix> <prune destination> <max rounds>

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

return await CodeFixRun.RunAsync(args);

/// <summary>The runner's process boundary.</summary>
internal static class CodeFixRun
{
    /// <summary>SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX: no Windows error dialogs.</summary>
    private const uint NoDialogs = 0x0001 | 0x0002 | 0x8000;

    /// <summary>Disables Windows error dialogs for this process and the processes it starts.</summary>
    [DllImport("kernel32.dll")]
    private static extern uint SetErrorMode(uint mode);

    /// <summary>The usage line.</summary>
    private const string Usage = "usage: CodeFixRunner <solution.slnx | project.csproj> <summary.json> <rule,rule,...> [--dry-run]\n" +
        "   or: CodeFixRunner --cleanup <solution.slnx> <artifacts directory> <result.json> <rule,rule,...> " +
        "<prune categories> <prune path prefix> <prune destination> <max rounds>";

    /// <summary>Runs the fixes behind the no-dialog boundary; any failure is written in full with a nonzero exit.</summary>
    public static async Task<int> RunAsync(string[] args)
    {
        if (OperatingSystem.IsWindows())
            _ = SetErrorMode(NoDialogs);
        try
        {
            if (args is ["--cleanup", var solution, var artifacts, var result, var ruleList, var categories, var prefix, var destination, var rounds])
            {
                var loop = new CleanupLoop(Path.GetFullPath(solution), Path.GetFullPath(artifacts), Rules(ruleList), categories, prefix,
                    Path.GetFullPath(destination), int.Parse(rounds, System.Globalization.CultureInfo.InvariantCulture));
                CleanupOutcome outcome = await loop.RunAsync();
                File.WriteAllText(Path.GetFullPath(result),
                    JsonSerializer.Serialize(outcome, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
                return 0;
            }
            bool dryRun = args.Length == 4 && args[3] == "--dry-run";
            if (args.Length != 3 && !dryRun)
                throw new ArgumentException(Usage);
            string target = Path.GetFullPath(args[0]);
            using var workspace = await WorkspaceLoad.LoadAsync(target);
            var fixer = new SolutionFixer(WorkspaceLoad.RepositoryRoot(target), Rules(args[2]), dryRun);
            (_, CleanupSummary summary) = await fixer.RunAsync(workspace.CurrentSolution);
            File.WriteAllText(Path.GetFullPath(args[1]),
                JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>The rules of a comma-separated list, of which there must be at least one.</summary>
    private static string[] Rules(string list)
    {
        string[] rules = list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return rules.Length == 0 ? throw new ArgumentException("No rules named.") : rules;
    }
}

/// <summary>
/// Opens a .slnx's projects, or one project, in the configuration MSBuildWorkspace opens by default (Debug). A solution's
/// projects are loaded in one batch (<see cref="MSBuildProjectLoader.LoadSolutionInfoAsync"/>), which reads their
/// design-time builds together, instead of opening one project after another, each its own design-time build in turn;
/// they are then placed in the order opening them one at a time gives (<see cref="OpeningOrder"/>), since the audits
/// take the first project that compiles a shared file.
/// </summary>
internal static class WorkspaceLoad
{
    /// <summary>Opens the target: every project a .slnx lists, or one project; any load failure fails.</summary>
    public static async Task<Workspace> LoadAsync(string target)
    {
        using var msbuild = MSBuildWorkspace.Create();
        var failures = new List<string>();
        msbuild.RegisterWorkspaceFailedHandler(e =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                failures.Add(e.Diagnostic.Message);
        });
        SolutionInfo loaded;
        List<string> listed;
        if (target.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            loaded = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), null,
                await new MSBuildProjectLoader(msbuild).LoadProjectInfoAsync(target));
            listed = [Path.GetFullPath(target)];
        }
        else if (target.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
        {
            loaded = await new MSBuildProjectLoader(msbuild).LoadSolutionInfoAsync(target);
            string directory = Path.GetDirectoryName(target)!;
            listed = [.. XDocument.Load(target).Descendants("Project")
                .Select(p => Path.GetFullPath(Path.Combine(directory,
                    (string?)p.Attribute("Path") ?? throw new InvalidOperationException($"{target} lists a project without a Path."))))];
        }
        else
        {
            throw new ArgumentException($"{target} is neither a .slnx nor a .csproj.");
        }
        if (failures.Count > 0)
            throw new InvalidOperationException("Workspace load failed:\n" + string.Join('\n', failures));
        var ordered = await OpeningOrder(loaded.Projects, listed);
        var workspace = new AdhocWorkspace(MSBuildMefHostServices.DefaultServices);
        workspace.AddSolution(SolutionInfo.Create(loaded.Id, loaded.Version, loaded.FilePath, ordered, loaded.AnalyzerReferences));
        return workspace;
    }

    /// <summary>
    /// The projects in the order opening the listed projects one at a time puts them: each listed project, followed by
    /// the projects its project references (evaluated, in their order, analyzer references included) reach that are not
    /// placed yet, depth first. Every loaded project must be placed exactly once.
    /// </summary>
    private static async Task<List<ProjectInfo>> OpeningOrder(IReadOnlyList<ProjectInfo> projects, List<string> listed)
    {
        var byPath = projects.ToDictionary(project => project.FilePath
            ?? throw new InvalidOperationException($"{project.Name} has no project file."), StringComparer.OrdinalIgnoreCase);
        var references = await ProjectItems.EvaluateAsync([.. byPath.Keys]);
        var ordered = new List<ProjectInfo>();
        var placed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Place(string path)
        {
            if (!byPath.TryGetValue(path, out ProjectInfo? project) || !placed.Add(path))
                return;
            ordered.Add(project);
            foreach (string reference in references[path].ProjectReferences)
                Place(reference);
        }
        foreach (string path in listed)
            Place(path);
        if (ordered.Count != projects.Count)
            throw new InvalidOperationException("The load holds projects the solution neither lists nor reaches: " +
                string.Join(", ", projects.Where(project => !placed.Contains(project.FilePath!)).Select(project => project.Name)));
        return ordered;
    }

    /// <summary>The nearest directory at or above the target's that holds .git, or the target's own directory outside a
    /// repository.</summary>
    public static string RepositoryRoot(string target)
    {
        string start = Path.GetDirectoryName(target) ?? throw new ArgumentException($"{target} has no directory.");
        for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")) || File.Exists(Path.Combine(directory.FullName, ".git")))
                return directory.FullName;
        }
        return start;
    }
}

/// <summary>A project's evaluated source items and project references, in order.</summary>
/// <param name="Compile">The full paths of its Compile items.</param>
/// <param name="ProjectReferences">The full paths of the projects its ProjectReference items name.</param>
internal sealed record ProjectItems(List<string> Compile, List<string> ProjectReferences)
{
    /// <summary>Every given project's items, evaluated in parallel (<c>dotnet msbuild -getItem</c>), by project path.</summary>
    public static async Task<Dictionary<string, ProjectItems>> EvaluateAsync(IReadOnlyList<string> projects)
    {
        var items = await Task.WhenAll(projects.Select(project => Task.Run(() => Evaluate(project))));
        return projects.Select((project, index) => (project, items[index]))
            .ToDictionary(entry => entry.project, entry => entry.Item2, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>One project's items, as <c>dotnet msbuild -getItem:Compile -getItem:ProjectReference</c> evaluates them.</summary>
    private static ProjectItems Evaluate(string project)
    {
        var start = new System.Diagnostics.ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string argument in new[] { "msbuild", project, "-getItem:Compile", "-getItem:ProjectReference", "-nologo" })
            start.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(start) ?? throw new InvalidOperationException("dotnet msbuild did not start.");
        Task<string> error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet msbuild {project} -getItem failed (exit {process.ExitCode}): {output}{error.Result}");
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement found = document.RootElement.GetProperty("Items");
        List<string> Paths(string item) => found.TryGetProperty(item, out JsonElement list)
            ? [.. list.EnumerateArray().Select(entry => entry.GetProperty("FullPath").GetString()
                ?? throw new InvalidOperationException($"{project} has a {item} item without a path."))]
            : [];
        return new ProjectItems(Paths("Compile"), Paths("ProjectReference"));
    }
}

/// <summary>What the cleanup's fix and prune loop and its inventories did, for tools/cleanup/run.ps1's commit message.</summary>
/// <param name="CodeFixes">Sites fixed per rule over every code-fix step.</param>
/// <param name="Kept">Findings per rule the last code-fix step left in files compiled by several projects that do not
/// all report them, or by a project that does not compile.</param>
/// <param name="ProjectsWithCompilerErrors">Projects a code-fix step found with compiler errors, whose files it left alone.</param>
/// <param name="Problems">Every problem the run carried on past, in order.</param>
/// <param name="FixedPaths">Repository-relative paths the code-fix steps changed.</param>
/// <param name="PrunedPaths">Paths the dead-code prunes changed.</param>
/// <param name="PrunedSummaries">The prunes' own summary lines.</param>
/// <param name="RetiredFields">The instance fields the prunes retired.</param>
/// <param name="ReviewPaths">Paths the stale-review prunes changed.</param>
/// <param name="StaleReviews">Each stale review removed.</param>
/// <param name="InventoryPaths">Paths the inventories changed.</param>
/// <param name="Tree">The git tree the run leaves in the working copy.</param>
/// <param name="Steps">Each step's kind, duration and changed file count, in order.</param>
/// <param name="BuildExit">tools/build.ps1's exit code when the solution was built while the inventories were written
/// (its full output is the build log it leaves in the temporary folder), or null when it was not built.</param>
internal sealed record CleanupOutcome(
    SortedDictionary<string, int> CodeFixes,
    SortedDictionary<string, int> Kept,
    List<string> ProjectsWithCompilerErrors,
    List<string> Problems,
    List<string> FixedPaths,
    List<string> PrunedPaths,
    List<string> PrunedSummaries,
    List<string> RetiredFields,
    List<string> ReviewPaths,
    List<string> StaleReviews,
    List<string> InventoryPaths,
    string Tree,
    List<CleanupStep> Steps,
    int? BuildExit);

/// <summary>One step of the loop, or the inventories, as the log and the result report it.</summary>
/// <param name="Number">The step's number; the inventories have none.</param>
/// <param name="Kind">fix, prune, reviews, inventories or build.</param>
/// <param name="Seconds">How long the step took.</param>
/// <param name="ChangedFiles">How many files it changed.</param>
internal sealed record CleanupStep(int? Number, string Kind, double Seconds, int ChangedFiles);

/// <summary>
/// The asynchronous cleanup's fix and prune loop and its inventories (tools/cleanup/README.md), in one process on one
/// load of the solution. The code fixes, the reachability prune of dead code and the architecture audit's prune of stale
/// reviews take turns until one of each in a row changes nothing; then the inventories are written. Each step is handed
/// the solution as the previous steps left it: the code fixes return theirs, and what a prune writes is read back from
/// the files git reports it changed (<see cref="SourceFiles"/>), so no step loads the solution again.
/// </summary>
/// <remarks>The owner's rulings (2026-11-15, #2599): a fix is always applied, breaking the code if it must, and the
/// cleanup commits what it has. A failed prune, review prune or inventory step, steps that do not settle and steps that
/// undo each other are each recorded as a problem and the run goes on. A fixer that offers no fix or throws is a defect
/// of that fixer, never a site left unfixed, and fails the run, as a failure to load the solution or the analyzers, or
/// an unusable command line, does.</remarks>
internal sealed class CleanupLoop(string solutionPath, string artifacts, string[] rules, string pruneCategories, string prunePrefix,
    string pruneDestination, int maxRounds)
{
    /// <summary>The steps the loop takes turns at.</summary>
    private static readonly string[] StepKinds = ["fix", "prune", "reviews"];

    /// <summary>The line the stale-review prune prints for each review it removes.</summary>
    private const string StaleReviewLine = "Removed stale review: ";

    /// <summary>The reachability prune's summary lines.</summary>
    private static readonly Regex PruneSummaryLine = new(@"^(Removed|Moved|Relocated|Deleted)\b", RegexOptions.CultureInvariant);

    /// <summary>Runs the loop and the inventories; returns what they did.</summary>
    public async Task<CleanupOutcome> RunAsync()
    {
        string root = WorkspaceLoad.RepositoryRoot(solutionPath);
        var git = new Git(root);
        var codeFixes = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var kept = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var broken = new SortedSet<string>(StringComparer.Ordinal);
        var problems = new List<string>();
        var fixedPaths = new SortedSet<string>(StringComparer.Ordinal);
        var prunedPaths = new SortedSet<string>(StringComparer.Ordinal);
        var prunedSummaries = new List<string>();
        var retiredFields = new List<string>();
        var reviewPaths = new SortedSet<string>(StringComparer.Ordinal);
        var staleReviews = new List<string>();
        var steps = new List<CleanupStep>();
        void Problem(string text)
        {
            problems.Add(text);
            Console.WriteLine($"problem: {text}");
        }

        Heading("load the solution, once for every step");
        var loading = System.Diagnostics.Stopwatch.StartNew();
        using Workspace workspace = await WorkspaceLoad.LoadAsync(solutionPath);
        Solution solution = workspace.CurrentSolution;
        SourceFiles files = await SourceFiles.CreateAsync(solution, root);
        Console.WriteLine($"--- load {solution.ProjectIds.Count} projects after {loading.Elapsed.TotalSeconds:F0} s");
        string tree = git.TreeId();
        string initial = tree;
        string synced = tree;
        // The solution with every file the steps have written so far: what git reports changed since the solution was
        // last brought up to date is read back into it.
        async Task<Solution> CurrentAsync()
        {
            string now = git.TreeId();
            if (now != synced)
            {
                solution = await files.SyncAsync(solution, git.Changes(synced, now));
                synced = now;
            }
            return solution;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal) { tree };
        int unchangedInARow = 0;
        int maxSteps = maxRounds * StepKinds.Length;
        for (int step = 1; unchangedInARow < StepKinds.Length; step++)
        {
            if (step > maxSteps)
            {
                Problem($"The fix and prune steps did not settle within {maxSteps} steps; the run keeps the tree it has.");
                break;
            }
            string kind = StepKinds[(step - 1) % StepKinds.Length];
            var started = System.Diagnostics.Stopwatch.StartNew();
            if (kind == "fix")
            {
                Heading($"step {step}: code fixes ({string.Join(", ", rules)})");
                var fixer = new SolutionFixer(root, rules, dryRun: false);
                (solution, CleanupSummary summary) = await fixer.RunAsync(await CurrentAsync());
                File.WriteAllText(Path.Combine(artifacts, $"code-fixes-{step}.json"),
                    JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
                foreach (var (rule, count) in summary.Fixed)
                    codeFixes[rule] = codeFixes.GetValueOrDefault(rule) + count;
                kept = summary.Kept;
                if (summary.ProjectsWithCompilerErrors.Count > 0)
                    Console.WriteLine($"projects with compiler errors, whose files were left alone: {string.Join(", ", summary.ProjectsWithCompilerErrors)}");
                broken.UnionWith(summary.ProjectsWithCompilerErrors);
                // A file renamed only in case keeps its old name in an index that ignores case until git is told it is gone.
                git.Untrack(summary.RenamedCase);
                foreach (string problem in summary.Problems)
                    problems.Add($"Code fixes, step {step}: {problem}");
            }
            else if (kind == "prune")
            {
                Heading($"step {step}: reachability prune of dead code");
                string retired = Path.Combine(artifacts, $"retired-fields-{step}.txt");
                File.WriteAllText(retired, "");
                // The prune applies every removal and move it selects, and the build fails where one needs a choice; a prune
                // that throws is a defect of the prune and fails the run with its exception (#2599): nothing carries on past it.
                using (var capture = new ConsoleCapture())
                {
                    await SuperMetroid.ReachabilityAudit.Program.PruneKeepingVerificationUsedAsync(
                        CurrentAsync, root, pruneCategories, prunePrefix, pruneDestination, retired);
                    prunedSummaries.AddRange(capture.Lines().Where(line => PruneSummaryLine.IsMatch(line)));
                }
                Console.WriteLine("--- reachability prune done");
                retiredFields.AddRange(File.ReadAllLines(retired).Where(line => line.Length > 0));
            }
            else
            {
                Heading($"step {step}: stale architecture reviews (ArchitectureAudit --prune-stale-reviews)");
                int exit;
                using (var capture = new ConsoleCapture())
                {
                    exit = await ExitOfAsync(async () => await SuperMetroid.ArchitectureAudit.Program.RunAsync(
                        await CurrentAsync(), root, SuperMetroid.ArchitectureAudit.AuditMode.PruneStaleReviews));
                    staleReviews.AddRange(capture.Lines().Where(line => line.StartsWith(StaleReviewLine, StringComparison.Ordinal))
                        .Select(line => line[StaleReviewLine.Length..]));
                }
                Console.WriteLine($"--- stale review prune exit {exit}");
                if (exit != 0)
                    Problem($"The stale review prune failed (exit {exit}) at step {step}; see the log.");
            }
            string next = git.TreeId();
            if (next == tree)
            {
                unchangedInARow++;
                steps.Add(new CleanupStep(step, kind, started.Elapsed.TotalSeconds, 0));
                Console.WriteLine($"no change ({started.Elapsed.TotalSeconds:F0} s)");
                continue;
            }
            var paths = git.ChangedPaths(tree, next);
            (kind == "fix" ? fixedPaths : kind == "prune" ? prunedPaths : reviewPaths).UnionWith(paths);
            steps.Add(new CleanupStep(step, kind, started.Elapsed.TotalSeconds, paths.Count));
            Console.WriteLine($"changed {paths.Count} files ({started.Elapsed.TotalSeconds:F0} s)");
            tree = next;
            if (!seen.Add(next))
            {
                Problem($"Step {step} returned the tree to an earlier state ({next}): the fixes and the prunes undo each other; the run keeps that tree.");
                break;
            }
            unchangedInARow = 0;
        }

        // The code the run commits is final once the loop ends: the inventories write only docs/, which no build reads.
        // When the loop changed anything, the solution is built while they are written; otherwise run.ps1 builds after
        // them, when they changed anything.
        Task<BuildResult>? build = null;
        if (tree != initial)
        {
            Heading("build (tools/build.ps1, the whole solution), while the inventories are written");
            build = BuildAsync(root, solutionPath);
        }
        Heading("inventories (ArchitectureAudit --write)");
        var inventory = System.Diagnostics.Stopwatch.StartNew();
        int inventoryExit = await ExitOfAsync(async () => await SuperMetroid.ArchitectureAudit.Program.RunAsync(
            await CurrentAsync(), root, SuperMetroid.ArchitectureAudit.AuditMode.Write));
        Console.WriteLine($"--- inventories exit {inventoryExit}");
        if (inventoryExit != 0)
            Problem($"The inventories were not regenerated (exit {inventoryExit}); see the log.");
        string inventoried = git.TreeId();
        var inventoryPaths = git.ChangedPaths(tree, inventoried);
        steps.Add(new CleanupStep(null, "inventories", inventory.Elapsed.TotalSeconds, inventoryPaths.Count));
        Console.WriteLine($"inventories changed {inventoryPaths.Count} files ({inventory.Elapsed.TotalSeconds:F0} s)");
        string final = inventoried;
        BuildResult? built = null;
        if (build is not null)
        {
            built = await build;
            foreach (string line in built.Output)
                Console.WriteLine(line);
            steps.Add(new CleanupStep(null, "build", built.Seconds, 0));
            final = git.TreeId();
            // tools/build.ps1 never changes a tracked file; the inventories change only docs/.
            var changedByBuild = git.ChangedPaths(tree, final).Where(path => !inventoryPaths.Contains(path)).ToList();
            if (final != inventoried || changedByBuild.Count > 0)
                throw new InvalidOperationException($"The build changed the tree, which tools/build.ps1 never does: {string.Join(", ", changedByBuild)}");
        }
        return new CleanupOutcome(codeFixes, kept, [.. broken], problems, [.. fixedPaths], [.. prunedPaths], prunedSummaries,
            retiredFields, [.. reviewPaths], staleReviews, inventoryPaths, final, steps, built?.Exit);
    }

    /// <summary>What tools/build.ps1 printed and how it exited.</summary>
    /// <param name="Exit">Its exit code.</param>
    /// <param name="Output">Every line it printed.</param>
    /// <param name="Seconds">How long it took.</param>
    private sealed record BuildResult(int Exit, List<string> Output, double Seconds);

    /// <summary>Builds the solution with tools/build.ps1 in a process of its own, which keeps the build's full output
    /// in its log, as run.ps1 does.</summary>
    private static Task<BuildResult> BuildAsync(string root, string solution) => Task.Run(() =>
    {
        var started = System.Diagnostics.Stopwatch.StartNew();
        var start = new System.Diagnostics.ProcessStartInfo("pwsh")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = root,
        };
        foreach (string argument in new[] { "-NoProfile", "-NonInteractive", "-File", Path.Combine(root, "tools", "build.ps1"), solution })
            start.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(start) ?? throw new InvalidOperationException("tools/build.ps1 did not start.");
        Task<string> error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        var lines = (output + error.Result).Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        return new BuildResult(process.ExitCode, lines, started.Elapsed.TotalSeconds);
    });

    /// <summary>
    /// A step's exit code as its command line gave it: what the step returns, or 1 when it throws, after writing the
    /// exception in full to the log. The owner's ruling (2026-11-15): a failed prune or inventory step is a problem the
    /// run records and carries on past, as it was when each step was a process of its own.
    /// </summary>
    private static async Task<int> ExitOfAsync(Func<Task<int>> step)
    {
        try
        {
            return await step();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>A step that returns nothing exits 0 when it does not throw.</summary>
    private static Task<int> ExitOfAsync(Func<Task> step) => ExitOfAsync(async () =>
    {
        await step();
        return 0;
    });

    /// <summary>Prints a step's heading as run.ps1 prints its own.</summary>
    private static void Heading(string text)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {text}");
    }
}

/// <summary>The git commands the loop runs in the repository: tree ids of the working copy and the paths between them.</summary>
internal sealed class Git(string root)
{
    /// <summary>The tree the working copy holds now, including new and deleted files.</summary>
    public string TreeId()
    {
        _ = Run("add", "--all");
        return Run("write-tree");
    }

    /// <summary>Takes repository-relative paths out of the index, leaving the working copy as it is: a file renamed only
    /// in case keeps its old name in an index that ignores case, and the next <see cref="TreeId"/> then adds its new one.</summary>
    public void Untrack(IReadOnlyCollection<string> paths)
    {
        if (paths.Count > 0)
            _ = Run(["rm", "--cached", "--quiet", "--", .. paths]);
    }

    /// <summary>The paths that differ between two trees, as <c>git diff --name-only</c> lists them.</summary>
    public List<string> ChangedPaths(string from, string to) =>
        [.. Run("diff", "--name-only", from, to).Split('\n', StringSplitOptions.RemoveEmptyEntries)];

    /// <summary>Each path added (A), modified (M) or deleted (D) between two trees, without rename detection.</summary>
    public List<(char Status, string Path)> Changes(string from, string to) =>
        [.. Run("diff", "--name-status", "--no-renames", from, to).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split('\t', 2) is [var status, var path] ? (status[0], path)
                : throw new InvalidOperationException($"Unexpected git diff line: {line}"))];

    /// <summary>Runs git in the repository; a nonzero exit fails with git's output.</summary>
    private string Run(params string[] arguments)
    {
        var start = new System.Diagnostics.ProcessStartInfo("git")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-C");
        start.ArgumentList.Add(root);
        foreach (string argument in arguments)
            start.ArgumentList.Add(argument);
        using var process = System.Diagnostics.Process.Start(start) ?? throw new InvalidOperationException("git did not start.");
        Task<string> error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed (exit {process.ExitCode}): {output}{error.Result}");
        return output.Replace("\r", "").Trim();
    }
}

/// <summary>
/// Keeps the loaded solution in step with the files the steps write, as a fresh load would see them. A changed file's
/// text is read back where it differs. When files are added or deleted, or a project file changes, every project's
/// source list is evaluated again (<c>dotnet msbuild -getItem:Compile</c>, the items a load takes before the build adds
/// its generated files), and each project gets exactly those files, in that order, followed by the generated files it
/// was loaded with. <see cref="CreateAsync"/> confirms the load holds that shape, so the rule reproduces a load.
/// </summary>
internal sealed class SourceFiles
{
    private readonly string _root;

    /// <summary>Each project's documents a load adds after its evaluated source items (generated files under obj/).</summary>
    private readonly Dictionary<ProjectId, List<string>> _generated;

    private SourceFiles(string root, Dictionary<ProjectId, List<string>> generated)
    {
        _root = root;
        _generated = generated;
    }

    /// <summary>Records each project's generated documents, failing unless every project's documents are its evaluated
    /// source items, in order, followed by them.</summary>
    public static async Task<SourceFiles> CreateAsync(Solution solution, string root)
    {
        var evaluated = await EvaluateAsync(solution);
        var generated = new Dictionary<ProjectId, List<string>>();
        foreach (Project project in solution.Projects)
        {
            var documents = project.Documents.Select(document => document.FilePath
                ?? throw new InvalidOperationException($"{project.Name} has a document without a file.")).ToList();
            List<string> items = evaluated[project.Id];
            if (documents.Count < items.Count || !documents.Take(items.Count).SequenceEqual(items, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{project.Name}'s loaded documents are not its evaluated source items in order, so " +
                    "files the steps add or delete cannot be placed as a load would place them.");
            generated[project.Id] = documents.Skip(items.Count).ToList();
        }
        return new SourceFiles(root, generated);
    }

    /// <summary>The solution with the given changes (git's status and repository-relative path) read back.</summary>
    public async Task<Solution> SyncAsync(Solution solution, IReadOnlyList<(char Status, string Path)> changes)
    {
        bool reevaluate = false;
        foreach (var (status, relative) in changes)
        {
            string path = Path.GetFullPath(Path.Combine(_root, relative));
            string extension = Path.GetExtension(path);
            if (status != 'M' || extension is not ".cs")
                reevaluate |= extension is ".cs" or ".csproj" or ".projitems" or ".props" or ".targets";
            if (status != 'M')
                continue;
            var ids = solution.GetDocumentIdsWithFilePath(path);
            if (ids.IsEmpty)
                continue;
            SourceText text = Read(path);
            if ((await solution.GetDocument(ids[0])!.GetTextAsync()).ContentEquals(text))
                continue;
            foreach (DocumentId id in ids)
                solution = solution.WithDocumentText(id, text);
        }
        return reevaluate ? await PlaceAsync(solution) : solution;
    }

    /// <summary>Gives every project exactly its evaluated source items, in order, followed by its generated files.</summary>
    private async Task<Solution> PlaceAsync(Solution solution)
    {
        var evaluated = await EvaluateAsync(solution);
        foreach (ProjectId projectId in solution.ProjectIds)
        {
            Project project = solution.GetProject(projectId)!;
            var wanted = evaluated[projectId].Concat(_generated[projectId]).ToList();
            var wantedSet = wanted.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (Document document in project.Documents.Where(document => !wantedSet.Contains(document.FilePath!)))
                solution = solution.RemoveDocument(document.Id);
            var present = solution.GetProject(projectId)!.Documents
                .ToDictionary(document => document.FilePath!, document => document.Id, StringComparer.OrdinalIgnoreCase);
            foreach (string path in wanted.Where(path => !present.ContainsKey(path)))
            {
                DocumentId id = DocumentId.CreateNewId(projectId, path);
                solution = solution.AddDocument(DocumentInfo.Create(id, Path.GetFileName(path), Folders(project, path), SourceCodeKind.Regular,
                    TextLoader.From(TextAndVersion.Create(Read(path), VersionStamp.Create(), path)), path));
                present[path] = id;
            }
            var order = wanted.Select(path => present[path]).ToImmutableList();
            if (!solution.GetProject(projectId)!.DocumentIds.SequenceEqual(order))
                solution = solution.WithProjectDocumentsOrder(projectId, order);
        }
        return solution;
    }

    /// <summary>A new document's folders: its directory's path below the project's, or none outside it.</summary>
    private static IEnumerable<string> Folders(Project project, string path)
    {
        string relative = Path.GetRelativePath(Path.GetDirectoryName(project.FilePath!)!, Path.GetDirectoryName(path)!);
        return relative == "." || relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative)
            ? [] : relative.Split(Path.DirectorySeparatorChar);
    }

    /// <summary>A file's text as a load reads it.</summary>
    private static SourceText Read(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return SourceText.From(stream, encoding: null, checksumAlgorithm: SourceHashAlgorithm.Sha256);
    }

    /// <summary>Every project's evaluated source items, in order, evaluated in parallel.</summary>
    private static async Task<Dictionary<ProjectId, List<string>>> EvaluateAsync(Solution solution)
    {
        var projects = solution.Projects.ToList();
        var items = await ProjectItems.EvaluateAsync([.. projects.Select(project => project.FilePath!)]);
        return projects.ToDictionary(project => project.Id, project => items[project.FilePath!].Compile);
    }
}

/// <summary>Copies everything written to the console while it is open, so a step's own output lines can be read back;
/// the output still reaches the log.</summary>
internal sealed class ConsoleCapture : IDisposable
{
    private readonly TextWriter _out = Console.Out;
    private readonly TextWriter _error = Console.Error;
    private readonly StringBuilder _captured = new();
    private readonly Lock _gate = new();

    /// <summary>Starts copying standard output and standard error.</summary>
    public ConsoleCapture()
    {
        Console.SetOut(new Tee(_out, this));
        Console.SetError(new Tee(_error, this));
    }

    /// <summary>The lines written so far.</summary>
    public List<string> Lines()
    {
        lock (_gate)
            return [.. _captured.ToString().Replace("\r", "").Split('\n')];
    }

    /// <summary>Stops copying.</summary>
    public void Dispose()
    {
        Console.SetOut(_out);
        Console.SetError(_error);
    }

    /// <summary>Writes to one console stream and to the capture.</summary>
    private sealed class Tee(TextWriter target, ConsoleCapture capture) : TextWriter
    {
        public override Encoding Encoding => target.Encoding;

        public override void Write(char value)
        {
            lock (capture._gate)
            {
                target.Write(value);
                capture._captured.Append(value);
            }
        }

        public override void Write(string? value)
        {
            lock (capture._gate)
            {
                target.Write(value);
                capture._captured.Append(value);
            }
        }

        public override void WriteLine(string? value)
        {
            lock (capture._gate)
            {
                target.WriteLine(value);
                capture._captured.Append(value).Append('\n');
            }
        }

        public override void Flush() => target.Flush();
    }
}

/// <summary>What one run fixed and left, for the cleanup's log and commit message.</summary>
/// <param name="Passes">The analysis passes run, the last of which found nothing to apply.</param>
/// <param name="Fixed">Sites fixed per rule; for unused usings, the using directives removed.</param>
/// <param name="Kept">Findings per rule left unapplied: in files compiled by several projects that do not all report
/// them, or whose fix changes a file that a project with compiler errors compiles.</param>
/// <param name="ChangedFiles">Repository-relative paths of the files written, created or deleted, project lists included.</param>
/// <param name="ProjectsWithCompilerErrors">Projects whose findings were not used because they do not compile.</param>
/// <param name="Problems">What went wrong in the run as a whole and was carried on past: fixes that left a project with
/// compiler errors (they are kept, the owner's rule: a fix breaks the code rather than leave a site unfixed), fixes that
/// cycle, and passes that did not converge.</param>
/// <param name="RenamedCase">Repository-relative paths of files renamed only in case, by their old names.</param>
internal sealed record CleanupSummary(
    int Passes,
    SortedDictionary<string, int> Fixed,
    SortedDictionary<string, int> Kept,
    List<string> ChangedFiles,
    List<string> ProjectsWithCompilerErrors,
    List<string> Problems,
    List<string> RenamedCase);

/// <summary>One project's analysis: whether it compiles, and its findings of the requested rules by file.</summary>
/// <param name="Project">The analyzed project.</param>
/// <param name="CompilerErrors">The compiler errors, empty when it compiles.</param>
/// <param name="Findings">The unsuppressed findings of the requested rules, by file path.</param>
/// <param name="FixerOnly">Hidden findings an analyzer reports only for a rule's fixer to fix, by file path.</param>
internal sealed record ProjectAnalysis(Project Project, List<Diagnostic> CompilerErrors, Dictionary<string, List<Diagnostic>> Findings,
    Dictionary<string, List<Diagnostic>> FixerOnly);

/// <summary>One accepted fix: the solution after it and every file it changed.</summary>
/// <param name="Solution">The solution with the fix applied in every project that compiles a changed file.</param>
/// <param name="Paths">The full paths of the files the fix changed, added or removed.</param>
/// <param name="Origins">Each added file's path, with the path of the file whose finding added it.</param>
internal sealed record FileFix(Solution Solution, List<string> Paths, Dictionary<string, string> Origins);

/// <summary>Whether a computed fix is applied now, left for the next pass, or kept unapplied.</summary>
/// <param name="Fix">The accepted fix, or null when it is not applied in this pass.</param>
/// <param name="Kept">Whether the findings are kept (a project compiling a changed file does not compile) rather
/// than retried in the next pass.</param>
internal sealed record FixDecision(FileFix? Fix, bool Kept);

/// <summary>The fix a file's plan holds: the first rule in <c>RuleOrder</c> with agreed findings in it, those findings
/// and the solution its fixer produced for them.</summary>
/// <param name="Rule">The rule.</param>
/// <param name="Diagnostics">The agreed findings of the rule in the file.</param>
/// <param name="Changed">The solution after the fix.</param>
internal sealed record PlannedFix(string Rule, List<Diagnostic> Diagnostics, Solution Changed);

/// <summary>The assembly a rule's code fix provider comes from.</summary>
internal enum FixerSource
{
    /// <summary>Roslyn's code-style fixers: an analyzer reference whose file name ends <c>.CodeStyle.Fixes.dll</c>.</summary>
    CodeStyle,

    /// <summary>The repository's own fixers: the analyzer reference <c>SuperMetroid.EnsureAnalyzer.CodeFixes.dll</c>.</summary>
    Repository,
}

/// <summary>What reports a rule's findings.</summary>
internal enum FindingSource
{
    /// <summary>An analyzer the project references, which must support the rule.</summary>
    Analyzer,

    /// <summary>The compiler itself, while it binds: the findings are read from the compilation's own diagnostics.</summary>
    Compiler,

    /// <summary>The runner: a suppression that suppressed nothing in a run of every analyzer with suppressed
    /// diagnostics reported (<see cref="UnnecessarySuppressions"/>).</summary>
    UnnecessarySuppression,
}

/// <summary>A rule the runner applies, what reports it, and where its fixer comes from.</summary>
/// <param name="Id">The diagnostic id.</param>
/// <param name="Findings">What reports its findings.</param>
/// <param name="Fixer">The assembly its one code fix provider is taken from.</param>
internal sealed record CleanupRule(string Id, FindingSource Findings, FixerSource Fixer);

/// <summary>Loads a solution and applies the requested rules' fixes until none remain.</summary>
internal sealed class SolutionFixer
{
    /// <summary>The bound on analysis passes. Every fix removes text or adds a modifier once, so real cascades settle
    /// in a few passes; reaching this many means the fixes are not converging.</summary>
    private const int MaxPasses = 20;

    /// <summary>The file name of the repository's code fix assembly.</summary>
    private const string RepositoryFixes = "SuperMetroid.EnsureAnalyzer.CodeFixes.dll";

    /// <summary>The order rules are applied in within a file: suppressions first, then the file layout (merged parts,
    /// one type per file, the file's name), then code (casts, unreachable and dead code, then the rewrites of what is
    /// left), then the usings that the other fixes can leave unnecessary.</summary>
    private static readonly CleanupRule[] RuleOrder =
    [
        new("IDE0076", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("IDE0077", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("IDE0079", FindingSource.UnnecessarySuppression, FixerSource.Repository),
        new("SME6360", FindingSource.Analyzer, FixerSource.Repository),
        new("SME6400", FindingSource.Analyzer, FixerSource.Repository),
        new("SME6401", FindingSource.Analyzer, FixerSource.Repository),
        new("IDE0004", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("CS0162", FindingSource.Compiler, FixerSource.Repository),
        new("IDE0059", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("CS0219", FindingSource.Compiler, FixerSource.Repository),
        new("CS0168", FindingSource.Compiler, FixerSource.Repository),
        new("CA1823", FindingSource.Analyzer, FixerSource.Repository),
        new("IDE0051", FindingSource.Analyzer, FixerSource.Repository),
        new("CS0169", FindingSource.Compiler, FixerSource.Repository),
        new("IDE0052", FindingSource.Analyzer, FixerSource.Repository),
        new("CS0414", FindingSource.Compiler, FixerSource.Repository),
        new("IDE0330", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("IDE0032", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("IDE0044", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("SME6391", FindingSource.Analyzer, FixerSource.Repository),
        new("IDE0005", FindingSource.Analyzer, FixerSource.CodeStyle),
    ];

    /// <summary>The unnecessary-suppression rule, whose findings the runner computes itself.</summary>
    private const string UnnecessarySuppression = "IDE0079";

    /// <summary>The unused-using rule, which reports one finding per file covering every unnecessary using.</summary>
    private const string UnusedUsing = "IDE0005";

    /// <summary>The hidden finding the unused-using analyzer reports beside IDE0005 for its fixer, which fixes that id
    /// rather than IDE0005 itself.</summary>
    private const string UnusedUsingFixable = "RemoveUnnecessaryImportsFixable";

    private readonly HashSet<string> _rules;
    private readonly bool _dryRun;
    private readonly string _root;
    private readonly Dictionary<string, List<CodeFixProvider>> _fixerAssemblies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>What went wrong in the run as a whole and was carried on past, in the order it happened.</summary>
    private readonly List<string> _problems = [];

    /// <summary>The repository-relative old paths of the files the run renamed only in case.</summary>
    private readonly List<string> _renamedCase = [];

    /// <summary>Creates a fixer for the rules to fix in the repository at <paramref name="root"/>.</summary>
    public SolutionFixer(string root, string[] rules, bool dryRun)
    {
        _rules = rules.ToHashSet(StringComparer.Ordinal);
        foreach (string rule in _rules)
        {
            if (!RuleOrder.Any(ordered => ordered.Id == rule))
                throw new ArgumentException($"{rule} is not one of the rules this runner orders: {string.Join(", ", RuleOrder.Select(r => r.Id))}.");
        }
        _dryRun = dryRun;
        _root = root;
    }

    /// <summary>Runs the passes on a loaded solution and writes the changed files; returns the solution as written.</summary>
    public async Task<(Solution Fixed, CleanupSummary Summary)> RunAsync(Solution original)
    {
        Solution solution = original;
        var fixedCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var keptCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var history = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var analyses = new Dictionary<ProjectId, ProjectAnalysis>();
        var changedSinceAnalysis = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var broken = new SortedSet<string>(StringComparer.Ordinal);
        var compiledAtStart = new HashSet<ProjectId>();
        var origins = new Dictionary<string, string>(StringComparer.Ordinal);
        var brokenByFixes = new HashSet<ProjectId>();
        int pass = 0;
        while (true)
        {
            pass++;
            if (pass > MaxPasses)
            {
                // The owner's ruling (2026-11-15): the run keeps what the passes produced rather than failing.
                Problem($"The fixes did not converge within {MaxPasses} passes; the run keeps the tree after pass {MaxPasses}.");
                pass--;
                break;
            }
            var passWatch = System.Diagnostics.Stopwatch.StartNew();
            await AnalyzeAsync(solution, analyses, changedSinceAnalysis, pass == 1);
            changedSinceAnalysis.Clear();
            double analysisSeconds = passWatch.Elapsed.TotalSeconds;
            if (pass == 1)
                compiledAtStart.UnionWith(analyses.Values.Where(a => a.CompilerErrors.Count == 0).Select(a => a.Project.Id));
            foreach (ProjectAnalysis newlyBroken in analyses.Values.Where(a =>
                         a.CompilerErrors.Count > 0 && compiledAtStart.Contains(a.Project.Id) && brokenByFixes.Add(a.Project.Id)))
                Problem($"The fixes of pass {pass - 1} left {newlyBroken.Project.Name} with compiler errors (kept; its files are left " +
                    "alone from now on):\n" + string.Join('\n', newlyBroken.CompilerErrors.Take(20)));
            broken.UnionWith(analyses.Values.Where(a => a.CompilerErrors.Count > 0).Select(a => a.Project.Name));
            if (_dryRun)
            {
                PrintFindings(analyses.Values);
                return (solution, new CleanupSummary(pass, fixedCounts, keptCounts, [], [.. broken], [.. _problems], []));
            }
            var passKept = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var touched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool cycled = false;
            int applied = 0;
            var paths = analyses.Values.SelectMany(a => a.Findings.Keys).Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase).ToList();
            Solution analyzed = solution;
            var plans = await PlanAsync(analyzed, paths, analyses);
            _passProblems = [];
            foreach (string path in paths)
            {
                if (touched.Contains(path))
                    continue;
                FileFix? fix = await FixFileAsync(solution, analyzed, path, plans[path], analyses, fixedCounts, passKept, touched);
                if (fix is null)
                    continue;
                solution = fix.Solution;
                applied++;
                foreach (var (added, origin) in fix.Origins)
                    origins[added] = origin;
                foreach (string changedPath in fix.Paths)
                {
                    touched.Add(changedPath);
                    changedSinceAnalysis.Add(changedPath);
                    string hash = Hash(await TextAtAsync(solution, changedPath));
                    if (!history.TryGetValue(changedPath, out var seen))
                        history[changedPath] = seen = [Hash(await TextAtAsync(original, changedPath))];
                    if (!seen.Add(hash) && !cycled)
                    {
                        Problem($"{Relative(changedPath)} returned to an earlier text in pass {pass}: the fixes cycle; the run " +
                            $"keeps the tree after pass {pass}.");
                        cycled = true;
                    }
                }
            }
            // The pass's problems are recorded in the order they arose; the compiler-error checks of its fixes ran while
            // the pass went on.
            var passProblems = _passProblems;
            _passProblems = null;
            foreach (Task<List<string>> problems in passProblems)
            {
                foreach (string problem in await problems)
                    Problem(problem);
            }
            Console.WriteLine($"pass {pass}: fixed {applied} files; kept {Format(passKept)} (analysis {analysisSeconds:F0} s, fixes {passWatch.Elapsed.TotalSeconds - analysisSeconds:F0} s)");
            if (applied == 0 || cycled)
            {
                foreach (var (rule, count) in passKept)
                    keptCounts[rule] = count;
                break;
            }
        }
        var changed = await WriteAsync(original, solution, origins);
        Console.WriteLine($"fixed {Format(fixedCounts)} in {changed.Count} files after {pass} passes; kept {Format(keptCounts)}"
            + (broken.Count > 0 ? $"; projects with compiler errors: {string.Join(", ", broken)}" : ""));
        return (solution, new CleanupSummary(pass, fixedCounts, keptCounts, changed, [.. broken], [.. _problems], [.. _renamedCase]));
    }

    /// <summary>The problems of the pass applying its fixes, in the order they arise; null outside that loop.</summary>
    private List<Task<List<string>>>? _passProblems;

    /// <summary>Records a problem the run carries on past, and prints it to the log: at once, or, while a pass applies
    /// its fixes, when the pass ends, in the order the problems arose.</summary>
    private void Problem(string text)
    {
        if (_passProblems is { } pending)
        {
            pending.Add(Task.FromResult<List<string>>([text]));
            return;
        }
        _problems.Add(text);
        Console.WriteLine($"problem: {text}");
    }

    /// <summary>Analyzes every project on the first pass, then only the projects that compile a changed file. The
    /// fixes never change what another project can see (they change a file's own code, private members, or the files
    /// of one type, and an added file joins every project compiling its origin), so a project with no changed file
    /// keeps its findings.</summary>
    /// <remarks>The projects are analyzed in parallel, each on its own compilation, and their results and log lines
    /// taken in name order, so the outcome and the log do not depend on which finishes first. Their analyzers are
    /// loaded first, one project at a time, since a load failure is reported through an event of the analyzer
    /// reference, which projects share.</remarks>
    private async Task AnalyzeAsync(Solution solution, Dictionary<ProjectId, ProjectAnalysis> analyses, HashSet<string> changed, bool all)
    {
        var projects = solution.Projects.OrderBy(p => p.Name, StringComparer.Ordinal)
            .Where(project => all || project.Documents.Any(d => d.FilePath is { } path && changed.Contains(path))).ToList();
        var loaded = projects.Select(LoadedAnalyzers).ToList();
        var results = await Task.WhenAll(projects.Select((project, index) => Task.Run(() => AnalyzeProjectAsync(project, loaded[index]))));
        foreach (var (analysis, log) in results)
        {
            foreach (string line in log)
                Console.WriteLine(line);
            analyses[analysis.Project.Id] = analysis;
        }
    }

    /// <summary>The analyzer file names whose assemblies hold analyzers, so a reference to one that yields none failed to load.</summary>
    private static readonly string[] AnalyzerAssemblyNames =
        ["SuperMetroid.EnsureAnalyzer.dll", "Microsoft.CodeAnalysis.CSharp.CodeStyle.dll", "Microsoft.CodeAnalysis.CodeStyle.dll",
            "Microsoft.CodeAnalysis.NetAnalyzers.dll", "Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll"];

    /// <summary>
    /// Every analyzer the project's references load, as its build loads them. Fails on a reference that does not resolve,
    /// one whose assembly or analyzer types fail to load, and one of the known analyzer assemblies that yields no analyzer,
    /// so that a rule missing from the result is outside the project rather than lost to a broken workspace.
    /// </summary>
    private static ImmutableArray<DiagnosticAnalyzer> LoadedAnalyzers(Project project)
    {
        var failures = new List<string>();
        var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
        foreach (AnalyzerReference reference in project.AnalyzerReferences)
        {
            if (reference is UnresolvedAnalyzerReference unresolved)
            {
                failures.Add($"{unresolved.Display} does not resolve");
                continue;
            }
            void Failed(object? sender, AnalyzerLoadFailureEventArgs failure) =>
                failures.Add($"{reference.Display}: {failure.ErrorCode} {failure.TypeName} {failure.Message}");
            var file = reference as AnalyzerFileReference;
            if (file is not null)
                file.AnalyzerLoadFailed += Failed;
            ImmutableArray<DiagnosticAnalyzer> found = reference.GetAnalyzers(project.Language);
            if (file is not null)
                file.AnalyzerLoadFailed -= Failed;
            if (found.IsEmpty && file is not null && AnalyzerAssemblyNames.Contains(Path.GetFileName(file.FullPath), StringComparer.OrdinalIgnoreCase))
                failures.Add($"{file.FullPath} loads no analyzer");
            analyzers.AddRange(found);
        }
        if (failures.Count > 0)
            throw new InvalidOperationException($"{project.Name}'s analyzers do not all load:\n" + string.Join('\n', failures));
        return analyzers.ToImmutable();
    }

    /// <summary>Compiles one project with the requested rules enabled as warnings where their scope allows, and runs
    /// its analyzers for the requested analyzer rules; the compiler's own rules come with the compilation. When
    /// IDE0079 is requested, the whole cleanup group is enabled, suppressed diagnostics are reported, and every analyzer
    /// that can show a suppression of the project to be needed runs too (<see cref="SuppressionEvidence"/>). Returns the
    /// analysis and the lines it logs.</summary>
    private async Task<(ProjectAnalysis Analysis, List<string> Log)> AnalyzeProjectAsync(Project project, ImmutableArray<DiagnosticAnalyzer> loaded)
    {
        var log = new List<string>();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Compilation configured = await project.GetCompilationAsync()
            ?? throw new InvalidOperationException($"{project.Name} produced no compilation.");
        SyntaxTreeOptionsProvider treeOptions = configured.Options.SyntaxTreeOptionsProvider
            ?? throw new InvalidOperationException($"{project.Name} has no per-file analyzer configuration; csharp/.editorconfig is not loaded.");
        bool suppressions = _rules.Contains(UnnecessarySuppression);
        RuleScope scope = await RuleScope.CreateAsync(project, _rules, wholeGroup: suppressions);
        Compilation compilation = configured.WithOptions(configured.Options
            .WithSpecificDiagnosticOptions(configured.Options.SpecificDiagnosticOptions.SetItems(
                scope.Rules.Select(rule => KeyValuePair.Create(rule, ReportDiagnostic.Warn))))
            .WithSyntaxTreeOptionsProvider(new EnabledRuleTreeOptions(treeOptions, scope)));
        var analyzerOptions = new AnalyzerOptions(project.AnalyzerOptions.AdditionalFiles,
            new EnabledRuleConfigOptions(project.AnalyzerOptions.AnalyzerConfigOptionsProvider, scope));
        var analyzerRules = RuleOrder.Where(rule => rule.Findings == FindingSource.Analyzer && _rules.Contains(rule.Id))
            .Select(rule => rule.Id).ToHashSet(StringComparer.Ordinal);
        var ruleAnalyzers = loaded.Where(analyzer => analyzer.SupportedDiagnostics.Any(descriptor => analyzerRules.Contains(descriptor.Id)))
            .ToImmutableArray();
        var (evidence, suppressors) = suppressions ? SuppressionEvidence.Analyzers(configured, loaded, ruleAnalyzers) : ([], []);
        var analyzers = ruleAnalyzers.AddRange(evidence).AddRange(suppressors);
        var supported = loaded.SelectMany(a => a.SupportedDiagnostics).Select(d => d.Id).ToHashSet(StringComparer.Ordinal);
        // A project is held to the rules its own analyzers report, as its build is: the analyzer projects do not load the
        // repository analyzer (Directory.Build.targets) and, as netstandard2.0 projects, not the .NET analyzers either, so
        // their builds never reported those rules. LoadedAnalyzers fails on any reference that does not load, so a rule
        // missing here is outside the project, not lost to a misconfigured workspace.
        var outside = analyzerRules.Where(rule => !supported.Contains(rule)).Order(StringComparer.Ordinal).ToList();
        if (outside.Count > 0)
            log.Add($"  {project.Name} has no analyzer for {string.Join(", ", outside)}; its build never reports them, so neither does this run");
        if (suppressions && loaded.IsEmpty)
            throw new InvalidOperationException($"{project.Name} references no analyzer, so its suppressions' use cannot be seen.");
        // The compiler reports its own rules (CS0168, ...) while it binds; with the rule enabled in the per-file options
        // above, they are among the compilation's diagnostics, which the analyzer run includes.
        ImmutableArray<Diagnostic> diagnostics;
        Compilation analyzed = compilation;
        if (analyzers.IsEmpty)
        {
            diagnostics = compilation.GetDiagnostics();
        }
        else
        {
            // The requested rules' analyzers run concurrently: each of them declares concurrent execution and keeps no
            // state between callbacks, and the options the runner answers them from (RuleScope) are safe to read from
            // several threads. Run concurrently over Core with the whole analyzer set, diagnostics were once
            // intermittently lost: in one of four runs the suppressed SME6426 findings in StationObjectDraws did not come
            // back, so their needed [SuppressMessage]s looked unused and IDE0079 removed them (the cleanup run at
            // 549a352794 failed its build on this). RuleScope's per-file cache was then a plain dictionary, written from
            // the analysis threads; a lost entry turned a rule off for a file. An analyzer that runs only as evidence for
            // a suppression still runs serially with the rest.
            var withAnalyzers = compilation.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(analyzerOptions,
                onAnalyzerException: null, concurrentAnalysis: evidence.IsEmpty, logAnalyzerExecutionTime: false,
                reportSuppressedDiagnostics: suppressions));
            diagnostics = await withAnalyzers.GetAllDiagnosticsAsync();
            analyzed = withAnalyzers.Compilation;
            // An analyzer that throws reports AD0001 instead of its findings; the log shows it.
            foreach (Diagnostic failure in diagnostics.Where(d => d.Id == "AD0001"))
                log.Add($"  {project.Name}: {failure}");
        }
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && !d.IsSuppressed && d.Id.StartsWith("CS", StringComparison.Ordinal)).ToList();
        var findings = new Dictionary<string, List<Diagnostic>>(StringComparer.OrdinalIgnoreCase);
        var fixable = new Dictionary<string, List<Diagnostic>>(StringComparer.OrdinalIgnoreCase);
        var documents = project.Documents.Select(d => d.FilePath).OfType<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (Diagnostic diagnostic in diagnostics)
        {
            bool finding = _rules.Contains(diagnostic.Id) && diagnostic.Severity != DiagnosticSeverity.Hidden;
            bool fixerOnly = diagnostic.Id == UnusedUsingFixable && _rules.Contains(UnusedUsing);
            if (!finding && !fixerOnly || diagnostic.IsSuppressed || !diagnostic.Location.IsInSource)
                continue;
            string path = diagnostic.Location.SourceTree!.FilePath;
            if (!documents.Contains(path))
                continue;
            var target = finding ? findings : fixable;
            if (!target.TryGetValue(path, out var list))
                target[path] = list = [];
            list.Add(diagnostic);
        }
        if (suppressions)
        {
            var descriptors = loaded.SelectMany(a => a.SupportedDiagnostics).GroupBy(d => d.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            bool IsEnabled(SyntaxTree tree, string id)
            {
                string? rule = scope.Rules.FirstOrDefault(r => string.Equals(r, id, StringComparison.OrdinalIgnoreCase));
                if (rule is not null)
                    return scope.IsEnabled(tree.FilePath, rule);
                if (treeOptions.TryGetDiagnosticValue(tree, id, CancellationToken.None, out ReportDiagnostic severity) ||
                    configured.Options.SpecificDiagnosticOptions.TryGetValue(id, out severity) ||
                    treeOptions.TryGetGlobalDiagnosticValue(id, CancellationToken.None, out severity))
                    return severity != ReportDiagnostic.Suppress;
                if (descriptors.TryGetValue(id, out DiagnosticDescriptor? descriptor))
                    return descriptor.IsEnabledByDefault;
                return id.StartsWith("CS", StringComparison.OrdinalIgnoreCase);
            }
            foreach (Diagnostic unnecessary in UnnecessarySuppressions.Find(analyzed, diagnostics, IsEnabled))
            {
                string path = unnecessary.Location.SourceTree!.FilePath;
                if (!documents.Contains(path))
                    continue;
                if (!findings.TryGetValue(path, out var list))
                    findings[path] = list = [];
                list.Add(unnecessary);
            }
        }
        log.Add($"  analyzed {project.Name} in {watch.Elapsed.TotalSeconds:F0} s: {findings.Values.Sum(l => l.Count)} findings"
            + (errors.Count > 0 ? $", {errors.Count} compiler errors (its files are left alone)" : ""));
        return (new ProjectAnalysis(project, errors, findings, fixable), log);
    }

    /// <summary>
    /// What one file's fix will be computed from: the projects compiling it, what each reports, the findings they agree
    /// on, the document of the first, and the fix of the first rule in order with agreed findings, which is always
    /// applied. The fixes are computed for every file at once, on all processors, from the solution the pass analyzed.
    /// Each pass fixes at most one rule per file and a fix that would change a file another fix of the pass changed waits
    /// for the next pass, so the file a fix is computed for holds the same text as in that solution when the fix is applied.
    /// </summary>
    private sealed record FilePlan(List<ProjectAnalysis> Compilers, List<List<Diagnostic>> Reported, HashSet<string> Agreed,
        Document? Document, PlannedFix? Fix);

    /// <summary>Plans every file of the pass, on all processors; the plans are used in path order.</summary>
    private async Task<Dictionary<string, FilePlan>> PlanAsync(Solution solution, List<string> paths, Dictionary<ProjectId, ProjectAnalysis> analyses)
    {
        var plans = await Task.WhenAll(paths.Select(path => Task.Run(() => PlanFileAsync(solution, path, analyses))));
        return paths.Select((path, index) => (path, plans[index])).ToDictionary(entry => entry.path, entry => entry.Item2, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>One file's plan (<see cref="FilePlan"/>); a file a project with compiler errors compiles has no document.</summary>
    private async Task<FilePlan> PlanFileAsync(Solution solution, string path, Dictionary<ProjectId, ProjectAnalysis> analyses)
    {
        var compilers = solution.GetDocumentIdsWithFilePath(path)
            .Select(id => analyses[id.ProjectId]).OrderBy(a => a.Project.Name, StringComparer.Ordinal).ToList();
        var reported = compilers.Select(a => a.Findings.GetValueOrDefault(path) ?? []).ToList();
        var agreed = reported.Select(l => l.Select(Key).ToHashSet(StringComparer.Ordinal))
            .Aggregate((left, right) => { left.IntersectWith(right); return left; });
        if (compilers.Any(a => a.CompilerErrors.Count > 0))
            return new FilePlan(compilers, reported, agreed, null, null);
        ProjectAnalysis chosen = compilers[0];
        Document document = solution.GetDocument(solution.GetDocumentIdsWithFilePath(path).Single(id => id.ProjectId == chosen.Project.Id))!;
        foreach (CleanupRule cleanupRule in RuleOrder)
        {
            var diagnostics = reported[0].Where(d => d.Id == cleanupRule.Id && agreed.Contains(Key(d))).ToList();
            if (diagnostics.Count > 0)
                return new FilePlan(compilers, reported, agreed, document,
                    new PlannedFix(cleanupRule.Id, diagnostics, await FixOfAsync(document, chosen, cleanupRule, diagnostics)));
        }
        return new FilePlan(compilers, reported, agreed, document, null);
    }

    /// <summary>The fixer's fix of one rule's agreed findings in a document.</summary>
    private async Task<Solution> FixOfAsync(Document document, ProjectAnalysis chosen, CleanupRule cleanupRule, List<Diagnostic> diagnostics)
    {
        string path = document.FilePath!;
        if (cleanupRule.Id != UnusedUsing)
            return await ApplyFixAllAsync(document, cleanupRule, cleanupRule.Id, diagnostics);
        return chosen.FixerOnly.GetValueOrDefault(path) is { } fixable
            ? await ApplyFixAllAsync(document, cleanupRule, UnusedUsingFixable, fixable)
            : throw new InvalidOperationException($"{Relative(path)} reports {UnusedUsing} without {UnusedUsingFixable} in {chosen.Project.Name}, " +
                "so the code-style fixer has nothing to fix it from.");
    }

    /// <summary>Applies one rule's agreed fixes to one file, or returns null when nothing in it may be fixed in this
    /// pass: a fix that would change a file another fix of this pass changed waits for the next pass. The fix comes from
    /// the file's plan, computed from <paramref name="analyzed"/>, and is applied to <paramref name="solution"/>, which
    /// holds the fixes applied before it in this pass.</summary>
    private async Task<FileFix?> FixFileAsync(Solution solution, Solution analyzed, string path, FilePlan plan,
        Dictionary<ProjectId, ProjectAnalysis> analyses, SortedDictionary<string, int> fixedCounts, SortedDictionary<string, int> kept,
        HashSet<string> touched)
    {
        var (compilers, reported, agreed, planned, planFix) = plan;
        if (planned is not { } document)
        {
            Count(kept, reported.SelectMany(l => l).DistinctBy(Key));
            return null;
        }
        Count(kept, reported.SelectMany(l => l).Where(d => !agreed.Contains(Key(d))).DistinctBy(Key));
        if (planFix is { Rule: var rule, Diagnostics: var diagnostics, Changed: var changed })
        {
            // The fix is always applied, now or in a later pass; it is never skipped.
            FixDecision decision = await AcceptAsync(solution, analyzed, changed, document, rule, analyses, touched);
            if (decision.Fix is not { } fix)
            {
                if (decision.Kept)
                    Count(kept, diagnostics);
                return null;
            }
            int count = rule == UnusedUsing
                ? CountUsings(await document.GetTextAsync()) - CountUsings(await RequiredTextAsync(fix.Solution, path))
                : diagnostics.Count;
            fixedCounts[rule] = fixedCounts.GetValueOrDefault(rule) + count;
            return fix;
        }
        if (compilers.Count > 1)
        {
            // The projects disagree on the file's unused usings: remove only the usings every one of them reports.
            var lines = reported.Select(l => UsingLines(l.Where(d => d.Id == UnusedUsing)))
                .Aggregate((left, right) => { left.IntersectWith(right); return left; });
            if (lines.Count == 0)
                return null;
            SourceText text = await document.GetTextAsync();
            fixedCounts[UnusedUsing] = fixedCounts.GetValueOrDefault(UnusedUsing) + lines.Count;
            return new FileFix(WithText(solution, path, RemoveLines(text, lines, path)), [path], new());
        }
        return null;
    }

    /// <summary>
    /// Decides whether a fix computed in one project may be applied, and applies it to every project that compiles
    /// a file it changed. A fix of one file's text is always accepted. A fix of several files, or one that adds or
    /// removes files, is accepted only when every file it changes or removes, and the file its finding is in, is
    /// compiled by analyzed projects without compiler errors (otherwise its findings are kept) and when no other fix of
    /// this pass touched one of its files (otherwise it waits for the next pass); a project compiling one of its files
    /// that no longer compiles afterwards is recorded as a problem and the fix is kept (the owner's ruling, 2026-11-15:
    /// commit broken code rather than fail). An added file joins every project that compiles the finding's file; it must
    /// not exist yet, unless the fix removes it under a name that differs only in case. A fix that changes anything but
    /// source documents, or changes nothing, is a defect of its fixer and fails the run.
    /// </summary>
    private async Task<FixDecision> AcceptAsync(Solution solution, Solution analyzed, Solution changed, Document document, string rule,
        Dictionary<ProjectId, ProjectAnalysis> analyses, HashSet<string> touched)
    {
        string source = document.FilePath!;
        string fix = $"The {rule} fix for {Relative(source)}";
        var texts = new SortedDictionary<string, SourceText>(StringComparer.OrdinalIgnoreCase);
        var added = new SortedDictionary<string, Document>(StringComparer.OrdinalIgnoreCase);
        var removed = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        // The fix was computed from the solution the pass analyzed; what it changed is read against that solution and
        // applied to the current one.
        SolutionChanges changes = changed.GetChanges(analyzed);
        if (changes.GetAddedProjects().Any() || changes.GetRemovedProjects().Any())
            throw Defect($"{fix} adds or removes projects.");
        foreach (ProjectChanges project in changes.GetProjectChanges())
        {
            if (project.GetAddedAdditionalDocuments().Any() || project.GetRemovedAdditionalDocuments().Any() ||
                project.GetChangedAdditionalDocuments().Any() || project.GetAddedMetadataReferences().Any() ||
                project.GetRemovedMetadataReferences().Any() || project.GetAddedProjectReferences().Any() ||
                project.GetRemovedProjectReferences().Any())
                throw Defect($"{fix} changes {project.NewProject.Name} beyond its source files.");
            foreach (DocumentId id in project.GetChangedDocuments())
            {
                Document fixedDocument = changed.GetDocument(id)!;
                if (fixedDocument.FilePath is not { } path)
                    throw Defect($"{fix} changed {fixedDocument.Name}, which has no file.");
                SourceText text = await fixedDocument.GetTextAsync();
                if (texts.TryGetValue(path, out SourceText? other) && !other.ContentEquals(text))
                    throw Defect($"{fix} gave {Relative(path)} two different texts.");
                texts[path] = text;
            }
            foreach (DocumentId id in project.GetAddedDocuments())
            {
                Document addedDocument = changed.GetDocument(id)!;
                if (addedDocument.FilePath is not { } path)
                    throw Defect($"{fix} added {addedDocument.Name} without a file.");
                added[path] = addedDocument;
            }
            foreach (DocumentId id in project.GetRemovedDocuments())
            {
                if (analyzed.GetDocument(id)!.FilePath is not { } path)
                    throw Defect($"{fix} removed a document without a file.");
                removed.Add(path);
            }
        }
        if (texts.Count + added.Count + removed.Count == 0)
            throw Defect($"{fix} changed nothing.");
        // A file a fix adds must not exist yet, unless the fix removes the same file under a name that differs only in
        // case: a rename on a file system that ignores case.
        foreach (string path in added.Keys)
        {
            bool renamedCase = removed.Any(gone => string.Equals(gone, path, StringComparison.OrdinalIgnoreCase));
            if (!renamedCase && (File.Exists(path) || !solution.GetDocumentIdsWithFilePath(path).IsEmpty))
                throw Defect($"{fix} adds {Relative(path)}, which exists already.");
        }
        if (added.Count == 0 && removed.Count == 0 && texts.Count == 1 && texts.ContainsKey(source))
            return new FixDecision(new FileFix(WithText(solution, source, texts[source]), [source], new()), false);

        var paths = texts.Keys.Concat(added.Keys).Concat(removed).ToList();
        if (paths.Any(touched.Contains))
            return new FixDecision(null, false);
        foreach (string path in texts.Keys.Concat(removed).Append(source))
        {
            var ids = solution.GetDocumentIdsWithFilePath(path);
            if (ids.IsEmpty)
                throw Defect($"{fix} changed {Relative(path)}, which no project of the solution compiles.");
            if (ids.Any(id => !analyses.TryGetValue(id.ProjectId, out var analysis) || analysis.CompilerErrors.Count > 0))
                return new FixDecision(null, true);
        }
        var sourceProjects = solution.GetDocumentIdsWithFilePath(source).Select(id => id.ProjectId).ToList();
        var affected = paths.SelectMany(path => solution.GetDocumentIdsWithFilePath(path)).Select(id => id.ProjectId)
            .Concat(sourceProjects).ToHashSet();
        Solution result = solution;
        foreach (var (path, text) in texts)
            result = WithText(result, path, text);
        foreach (string path in removed)
        {
            foreach (DocumentId id in result.GetDocumentIdsWithFilePath(path))
                result = result.RemoveDocument(id);
        }
        // After the removals, so a rename that changes only case keeps the file it adds.
        foreach (var (path, addedDocument) in added)
        {
            SourceText text = await addedDocument.GetTextAsync();
            foreach (ProjectId project in sourceProjects)
                result = result.AddDocument(DocumentId.CreateNewId(project), Path.GetFileName(path), text, addedDocument.Folders, path);
        }
        // The solution after this fix is checked for compiler errors while the pass goes on; its problems are recorded
        // in their place when the pass ends.
        Solution checkedSolution = result;
        _passProblems!.Add(Task.Run(() => CompilerErrorProblemsAsync(checkedSolution, affected, fix)));
        return new FixDecision(new FileFix(result, paths, added.Keys.ToDictionary(path => path, _ => source, StringComparer.Ordinal)), false);
    }

    /// <summary>A fix the runner cannot apply: a defect of its fixer, which fails the run (#2599).</summary>
    private static InvalidOperationException Defect(string reason) => new(reason + " A fix is always applied, so this is a defect of the fixer.");

    /// <summary>A problem for each project with a compiler error in the given solution; the fix is kept (the owner's
    /// ruling, 2026-11-15: commit broken code rather than fail).</summary>
    private static async Task<List<string>> CompilerErrorProblemsAsync(Solution solution, IEnumerable<ProjectId> projects, string fix)
    {
        var problems = new List<string>();
        foreach (ProjectId id in projects)
        {
            Project project = solution.GetProject(id)!;
            if (await project.GetCompilationAsync() is not { } compilation)
            {
                problems.Add($"{fix} leaves {project.Name} with no compilation; the fix is kept.");
                continue;
            }
            var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
                problems.Add($"{fix} leaves {project.Name} with compiler errors; the fix is kept:\n" + string.Join('\n', errors.Take(20)));
        }
        return problems;
    }

    /// <summary>A file's text in a solution, or null when no project compiles that file under that name, its case
    /// included: a file renamed only in case is absent under its old name.</summary>
    private static async Task<SourceText?> TextAtAsync(Solution solution, string path)
    {
        var ids = solution.GetDocumentIdsWithFilePath(path).Where(id => string.Equals(solution.GetDocument(id)!.FilePath, path, StringComparison.Ordinal)).ToList();
        return ids.Count == 0 ? null : await solution.GetDocument(ids[0])!.GetTextAsync();
    }

    /// <summary>A file's text in a solution, which must compile it.</summary>
    private async Task<SourceText> RequiredTextAsync(Solution solution, string path) =>
        await TextAtAsync(solution, path) ?? throw new InvalidOperationException($"No project compiles {Relative(path)}.");

    /// <summary>The SHA-256 of a file's text, or a marker for a file no project compiles.</summary>
    private static string Hash(SourceText? text) =>
        text is null ? "absent" : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())));

    /// <summary>Runs the rule's fixer, for the diagnostic id it fixes, over exactly the given findings of one document
    /// and returns the solution it produces, which may change other documents too.</summary>
    /// <remarks>The owner's rule (#2599): a fix is always applied, breaking the code if it must, and nothing is left
    /// unfixed. A fixer that offers no fix, has no fix-all, produces no usable change or throws is a defect of that
    /// fixer: nothing here is caught, and the run fails with the full exception.</remarks>
    private async Task<Solution> ApplyFixAllAsync(Document document, CleanupRule cleanupRule, string rule, List<Diagnostic> diagnostics)
    {
        var providers = FixersFor(document.Project, cleanupRule.Fixer, rule);
        if (providers.Count != 1)
            throw new InvalidOperationException(
                $"{rule} has {providers.Count} {cleanupRule.Fixer} code fix providers in {document.Project.Name}; expected exactly one.");
        CodeFixProvider provider = providers[0];
        var actions = new List<CodeAction>();
        await provider.RegisterCodeFixesAsync(new CodeFixContext(document, diagnostics[0], (action, _) => actions.Add(action), CancellationToken.None));
        if (actions.Count == 0)
            throw Defect($"{provider.GetType().Name} offers no fix for {rule} at {Describe(diagnostics[0])}.");
        if (provider.GetFixAllProvider() is not { } fixAll)
            throw Defect($"{provider.GetType().Name} has no fix-all provider.");
        var context = new FixAllContext(document, provider, FixAllScope.Document, actions[0].EquivalenceKey, [rule],
            new FixedDiagnostics(document, diagnostics), CancellationToken.None);
        if (await fixAll.GetFixAsync(context) is not { } fix)
            throw Defect($"{provider.GetType().Name} produced no fix-all action for {rule} in {Relative(document.FilePath!)}.");
        var operations = await fix.GetOperationsAsync(CancellationToken.None);
        var applies = operations.OfType<ApplyChangesOperation>().ToList();
        return applies.Count == 1
            ? applies[0].ChangedSolution
            : throw Defect($"{provider.GetType().Name} produced {applies.Count} solution changes for {rule} in {Relative(document.FilePath!)}; expected one.");
    }

    /// <summary>The code fix providers for a rule, from the fixer assemblies of the given source the project's fixes
    /// load (<see cref="FixerAssemblies"/>).</summary>
    private List<CodeFixProvider> FixersFor(Project project, FixerSource source, string rule)
    {
        var providers = new List<CodeFixProvider>();
        foreach (string path in FixerAssemblies(project, source))
        {
            List<CodeFixProvider>? loaded;
            // Files are planned on several threads at once; each fixer assembly is loaded once.
            lock (_fixerAssemblies)
            {
                if (!_fixerAssemblies.TryGetValue(path, out loaded))
                    _fixerAssemblies[path] = loaded = LoadProviders(path, project.Language);
            }
            providers.AddRange(loaded.Where(provider => provider.FixableDiagnosticIds.Contains(rule)));
        }
        return providers;
    }

    /// <summary>
    /// The fixer assemblies of a source whose providers fix a project's findings: those it references. A project that
    /// builds the repository analyzer itself (SuperMetroid.EnsureAnalyzer, its code fixes and EnsureVerification,
    /// csharp/Directory.Build.targets) cannot reference the repository's fixes, while the code-style analyzers still
    /// report there the rules those fixes are for (IDE0051 in the code fixes, #2599): it takes the one build of them the
    /// rest of the solution references. A fixer works on the document it is given, whoever references it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The project references none and the solution not exactly one.</exception>
    private static List<string> FixerAssemblies(Project project, FixerSource source)
    {
        static List<string> Referenced(Project project, FixerSource source) => [.. project.AnalyzerReferences.Select(r => r.FullPath)
            .Where(p => p is not null && IsFixerAssembly(Path.GetFileName(p), source)).Select(p => p!)];
        var referenced = Referenced(project, source);
        if (referenced.Count > 0 || source != FixerSource.Repository)
            return referenced;
        var solutionWide = project.Solution.Projects.SelectMany(other => Referenced(other, source))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        return solutionWide.Count == 1 ? solutionWide
            : throw new InvalidOperationException($"{project.Name} references no {RepositoryFixes}, and the solution references " +
                $"{solutionWide.Count} builds of it; expected one.");
    }

    /// <summary>Whether an analyzer reference's file name is a fixer assembly of the given source.</summary>
    private static bool IsFixerAssembly(string fileName, FixerSource source) => source switch
    {
        FixerSource.CodeStyle => fileName.EndsWith(".CodeStyle.Fixes.dll", StringComparison.OrdinalIgnoreCase),
        FixerSource.Repository => string.Equals(fileName, RepositoryFixes, StringComparison.OrdinalIgnoreCase),
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown fixer source."),
    };

    /// <summary>Every exported code fix provider for the language in one fixer assembly, constructed once.</summary>
    private static List<CodeFixProvider> LoadProviders(string path, string language)
    {
        var providers = new List<CodeFixProvider>();
        foreach (Type type in Assembly.LoadFrom(path).GetTypes())
        {
            if (type.IsAbstract || !typeof(CodeFixProvider).IsAssignableFrom(type)
                || type.GetCustomAttribute<ExportCodeFixProviderAttribute>() is not { } export
                || !export.Languages.Contains(language)
                || type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) is not { } constructor)
                continue;
            providers.Add((CodeFixProvider)constructor.Invoke(null));
        }
        return providers;
    }

    /// <summary>
    /// Writes every file whose text changed, keeping its byte-order mark; creates every added file, with the byte-order
    /// mark of the file it came from; deletes every removed file; and updates the explicit project lists
    /// (<see cref="ProjectLists"/>). Returns the repository-relative paths of every file written, created or deleted.
    /// Files are deleted before any is written, so a file renamed only in case is created after its old name is gone.
    /// </summary>
    private async Task<List<string>> WriteAsync(Solution original, Solution solution, Dictionary<string, string> origins)
    {
        var paths = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ProjectChanges project in solution.GetChanges(original).GetProjectChanges())
        {
            foreach (DocumentId id in project.GetChangedDocuments().Concat(project.GetAddedDocuments()))
                paths.Add(solution.GetDocument(id)!.FilePath!);
            foreach (DocumentId id in project.GetRemovedDocuments())
                paths.Add(original.GetDocument(id)!.FilePath!);
        }
        var writes = new List<(string Path, string Text, bool Bom)>();
        var created = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var deleted = new List<string>();
        foreach (string path in paths)
        {
            SourceText? before = await TextAtAsync(original, path);
            SourceText? after = await TextAtAsync(solution, path);
            if (after is null)
            {
                if (before is not null)
                    deleted.Add(path);
                continue;
            }
            if (before is not null && before.ContentEquals(after))
                continue;
            string bomSource = path;
            if (before is null)
            {
                bomSource = OriginOf(path, origins);
                created[path] = bomSource;
            }
            writes.Add((path, after.ToString(), File.ReadAllBytes(bomSource) is [0xEF, 0xBB, 0xBF, ..]));
        }
        // Deleted first, so a file renamed only in case is gone before its new name is created.
        foreach (string path in deleted)
            File.Delete(path);
        foreach (var (path, text, bom) in writes)
        {
            if (created.ContainsKey(path))
            {
                if (File.Exists(path))
                    throw new InvalidOperationException($"{Relative(path)} exists already, so the file a fix added cannot be created.");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            }
            File.WriteAllText(path, text, new UTF8Encoding(bom));
        }
        _renamedCase.AddRange(deleted.Where(gone => created.Keys.Any(path => string.Equals(path, gone, StringComparison.OrdinalIgnoreCase)))
            .Select(Relative));
        var changed = writes.Select(write => write.Path).Concat(deleted)
            .Concat(ProjectLists.Update(original, created, deleted, Relative)).ToList();
        changed.Sort(StringComparer.OrdinalIgnoreCase);
        return [.. changed.Select(Relative)];
    }

    /// <summary>The file of the original solution an added file came from, through any file added before it.</summary>
    private static string OriginOf(string path, Dictionary<string, string> origins)
    {
        string origin = path;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (origins.TryGetValue(origin, out string? from))
        {
            if (!seen.Add(origin))
                throw new InvalidOperationException($"The added files' origins form a cycle at {origin}, reached from {path}: " +
                    string.Join("; ", origins.Select(entry => $"{entry.Key} from {entry.Value}")));
            origin = from;
        }
        if (origin == path)
            throw new InvalidOperationException($"{path} was added by no fix.");
        return origin;
    }

    /// <summary>The solution with a file's text replaced in every project that compiles it.</summary>
    private static Solution WithText(Solution solution, string path, SourceText text)
    {
        foreach (DocumentId id in solution.GetDocumentIdsWithFilePath(path))
            solution = solution.WithDocumentText(id, text);
        return solution;
    }

    /// <summary>The 1-based lines of the using directives an unused-using finding covers.</summary>
    private static HashSet<int> UsingLines(IEnumerable<Diagnostic> diagnostics)
    {
        var lines = new HashSet<int>();
        foreach (Diagnostic diagnostic in diagnostics)
        {
            SyntaxNode root = diagnostic.Location.SourceTree!.GetRoot();
            foreach (Location location in diagnostic.AdditionalLocations.Prepend(diagnostic.Location))
            {
                foreach (UsingDirectiveSyntax directive in root.DescendantNodes(location.SourceSpan).OfType<UsingDirectiveSyntax>()
                             .Where(u => location.SourceSpan.Contains(u.Span)))
                    lines.Add(directive.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
            }
        }
        return lines;
    }

    /// <summary>The text with the given 1-based lines removed; each must hold exactly one using directive.</summary>
    private static SourceText RemoveLines(SourceText text, HashSet<int> lines, string path)
    {
        var changes = new List<TextChange>();
        foreach (int line in lines.Order())
        {
            TextLine textLine = text.Lines[line - 1];
            string content = textLine.ToString().Trim();
            if (!content.StartsWith("using ", StringComparison.Ordinal) && !content.StartsWith("global using ", StringComparison.Ordinal)
                || !content.EndsWith(';') || content.Count(c => c == ';') != 1)
                throw new InvalidOperationException($"{path}:{line} is not a single using directive: {content}");
            changes.Add(new TextChange(textLine.SpanIncludingLineBreak, ""));
        }
        return text.WithChanges(changes);
    }

    /// <summary>The number of using directives in a text.</summary>
    private static int CountUsings(SourceText text) =>
        Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(text).GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>().Count();

    /// <summary>Identifies a finding by rule and every location, so the same finding in two projects compares equal.</summary>
    private static string Key(Diagnostic diagnostic) =>
        diagnostic.Id + "@" + string.Join(";", diagnostic.AdditionalLocations.Prepend(diagnostic.Location)
            .Select(l => $"{l.SourceSpan.Start}+{l.SourceSpan.Length}"));

    /// <summary>Adds findings to per-rule counts.</summary>
    private static void Count(SortedDictionary<string, int> counts, IEnumerable<Diagnostic> diagnostics)
    {
        foreach (Diagnostic diagnostic in diagnostics)
            counts[diagnostic.Id] = counts.GetValueOrDefault(diagnostic.Id) + 1;
    }

    /// <summary>Prints every finding with its locations, for --dry-run.</summary>
    private void PrintFindings(IEnumerable<ProjectAnalysis> analyses)
    {
        foreach (ProjectAnalysis analysis in analyses.OrderBy(a => a.Project.Name, StringComparer.Ordinal))
        {
            foreach (Diagnostic error in analysis.CompilerErrors.Take(20))
                Console.WriteLine($"{analysis.Project.Name}\tCOMPILER\t{error}");
            foreach (Diagnostic diagnostic in analysis.Findings.Values.SelectMany(l => l))
                Console.WriteLine($"{analysis.Project.Name}\t{diagnostic.Id}\t{Describe(diagnostic)}\t{diagnostic.GetMessage()}");
        }
    }

    /// <summary>A finding's repository path and every location's line and column.</summary>
    private string Describe(Diagnostic diagnostic) =>
        Relative(diagnostic.Location.SourceTree!.FilePath) + ":" + string.Join(",", diagnostic.AdditionalLocations.Prepend(diagnostic.Location)
            .Select(l => l.GetLineSpan().StartLinePosition).Select(p => $"{p.Line + 1}:{p.Character + 1}"));

    /// <summary>A path relative to the repository root, with forward slashes.</summary>
    private string Relative(string path) => Path.GetRelativePath(_root, path).Replace('\\', '/');

    /// <summary>Per-rule counts as text.</summary>
    private static string Format(SortedDictionary<string, int> counts) =>
        counts.Count == 0 ? "none" : string.Join(", ", counts.Select(p => $"{p.Key} {p.Value}"));

}


/// <summary>The asynchronous cleanup's group in an analyzer configuration file: the
/// <c>dotnet_diagnostic.&lt;rule&gt;.severity = none</c> lines directly under the comment that names the cleanup, read
/// the same way tools/cleanup/run.ps1 reads it.</summary>
internal static class CleanupGroup
{
    /// <summary>The comment that names the cleanup.</summary>
    private static readonly Regex Comment = new(@"^#.*\basynchronous cleanup\b", RegexOptions.CultureInvariant);

    /// <summary>One rule of the group.</summary>
    private static readonly Regex Member = new(@"^dotnet_diagnostic\.(\w+)\.severity = none$", RegexOptions.CultureInvariant);

    /// <summary>The configuration text with each requested rule's group line (every rule's, when
    /// <paramref name="rules"/> is null) set to warning, adding those rules to <paramref name="grouped"/>; a text
    /// without the cleanup's comment is returned unchanged.</summary>
    public static string Enable(string text, IReadOnlySet<string>? rules, HashSet<string> grouped, string path)
    {
        string[] lines = text.Split('\n');
        int comment = -1;
        for (int index = 0; index < lines.Length; index++)
        {
            if (!Comment.IsMatch(lines[index].TrimEnd('\r')))
                continue;
            if (comment >= 0)
                throw new InvalidOperationException($"{path} has two comments naming the asynchronous cleanup.");
            comment = index;
        }
        if (comment < 0)
            return text;
        int line = comment;
        while (line < lines.Length && lines[line].StartsWith('#'))
            line++;
        for (; line < lines.Length && Member.Match(lines[line].TrimEnd('\r')) is { Success: true } match; line++)
        {
            string rule = match.Groups[1].Value;
            if (rules is not null && !rules.Contains(rule))
                continue;
            grouped.Add(rule);
            lines[line] = $"dotnet_diagnostic.{rule}.severity = warning" + (lines[line].EndsWith('\r') ? "\r" : "");
        }
        return string.Join('\n', lines);
    }
}

/// <summary>
/// Where a project's configuration enables each requested rule once its line in the cleanup's group says warning
/// instead of none. The project's analyzer configuration files are read again with that one change, so a later section
/// that turns the rule off for some files (the SME rules outside Core and AssetExtraction) keeps it off there.
/// </summary>
internal sealed class RuleScope
{
    private readonly AnalyzerConfigSet _configuration;

    /// <summary>Each file's options, read once. The compiler and the analyzers ask from several threads at once when a
    /// project is analyzed concurrently, so the cache is a concurrent one: a plain dictionary written from several
    /// threads loses entries and answers, and a lost answer turned a rule off for a file, which is how whole projects'
    /// findings went missing in concurrent runs.</summary>
    private readonly ConcurrentDictionary<string, AnalyzerConfigOptionsResult> _byPath = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates the scope from the project's configuration.</summary>
    private RuleScope(AnalyzerConfigSet configuration, IReadOnlySet<string> rules)
    {
        _configuration = configuration;
        Rules = rules;
    }

    /// <summary>The rules enabled in their scope: the requested ones, or the whole group.</summary>
    public IReadOnlySet<string> Rules { get; }

    /// <summary>Reads the project's analyzer configuration with the requested rules' group lines enabled, or the whole
    /// group's when <paramref name="wholeGroup"/> is set. Every requested rule must be in the cleanup's group of one of
    /// them, or its scope is unknown and the run fails.</summary>
    public static async Task<RuleScope> CreateAsync(Project project, IReadOnlySet<string> rules, bool wholeGroup)
    {
        var configs = new List<AnalyzerConfig>();
        var grouped = new HashSet<string>(StringComparer.Ordinal);
        foreach (AnalyzerConfigDocument document in project.AnalyzerConfigDocuments)
        {
            string path = document.FilePath
                ?? throw new InvalidOperationException($"{project.Name} has an analyzer configuration document without a file.");
            string text = (await document.GetTextAsync()).ToString();
            configs.Add(AnalyzerConfig.Parse(CleanupGroup.Enable(text, wholeGroup ? null : rules, grouped, path), path));
        }
        var outside = rules.Where(rule => !grouped.Contains(rule)).Order(StringComparer.Ordinal).ToList();
        if (outside.Count > 0)
            throw new InvalidOperationException($"No analyzer configuration of {project.Name} puts {string.Join(", ", outside)} in the " +
                "asynchronous cleanup's group, so where the rule applies is unknown.");
        return new RuleScope(AnalyzerConfigSet.Create(configs), wholeGroup ? grouped : rules);
    }

    /// <summary>Whether the requested rule is enabled in the file at <paramref name="path"/>.</summary>
    public bool IsEnabled(string path, string rule)
    {
        AnalyzerConfigOptionsResult options = _byPath.GetOrAdd(path, _configuration.GetOptionsForSourcePath);
        return options.TreeOptions.TryGetValue(rule, out ReportDiagnostic severity) && severity != ReportDiagnostic.Suppress;
    }
}

/// <summary>A compilation's per-file diagnostic options with the requested rules enabled as warnings where their scope
/// allows and off elsewhere. The per-file options carry csharp/.editorconfig's severities and win over the
/// compilation-wide options, so the group's <c>none</c> is replaced here; every other rule reads the project's own
/// configuration.</summary>
internal sealed class EnabledRuleTreeOptions(SyntaxTreeOptionsProvider configured, RuleScope scope) : SyntaxTreeOptionsProvider
{
    /// <summary>The project's own answer: enabling a rule does not change which files are generated.</summary>
    public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken) =>
        configured.IsGenerated(tree, cancellationToken);

    /// <summary>Warning or off for a requested rule, by its scope; the project's configuration for any other.</summary>
    public override bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
    {
        if (scope.Rules.Contains(diagnosticId))
        {
            severity = scope.IsEnabled(tree.FilePath, diagnosticId) ? ReportDiagnostic.Warn : ReportDiagnostic.Suppress;
            return true;
        }
        return configured.TryGetDiagnosticValue(tree, diagnosticId, cancellationToken, out severity);
    }

    /// <summary>Warning for a requested rule, so its analyzer runs; the project's global configuration for any other.</summary>
    public override bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
    {
        if (scope.Rules.Contains(diagnosticId))
        {
            severity = ReportDiagnostic.Warn;
            return true;
        }
        return configured.TryGetGlobalDiagnosticValue(diagnosticId, cancellationToken, out severity);
    }
}

/// <summary>The analyzer configuration with <c>dotnet_diagnostic.&lt;rule&gt;.severity</c> answered for each requested
/// rule: warning where its scope enables it, none elsewhere. The code-style analyzers read a rule's severity from this
/// configuration to decide whether to analyze at all, so the compilation's options alone would leave them skipping the
/// group's <c>none</c>.</summary>
internal sealed class EnabledRuleConfigOptions(AnalyzerConfigOptionsProvider configured, RuleScope scope) : AnalyzerConfigOptionsProvider
{
    /// <summary>The project's global options with the requested rules enabled.</summary>
    public override AnalyzerConfigOptions GlobalOptions => new EnabledRuleConfig(configured.GlobalOptions, scope, null);

    /// <summary>A file's options with the requested rules enabled where their scope allows.</summary>
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new EnabledRuleConfig(configured.GetOptions(tree), scope, tree.FilePath);

    /// <summary>An additional file's options with the requested rules enabled.</summary>
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new EnabledRuleConfig(configured.GetOptions(textFile), scope, null);
}

/// <summary>One set of analyzer options answering the requested rules' severity keys: warning where the rule's scope
/// enables it in the file (or always, for options that belong to no source file), none elsewhere.</summary>
internal sealed class EnabledRuleConfig(AnalyzerConfigOptions configured, RuleScope scope, string? path) : AnalyzerConfigOptions
{
    /// <summary>The severity key prefix and suffix around a rule id.</summary>
    private const string Prefix = "dotnet_diagnostic.", Suffix = ".severity";

    /// <summary>The scope's answer for a requested rule's severity key; the configured value for any other key.</summary>
    public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
    {
        if (RuleOf(key) is { } rule)
        {
            value = path is null || scope.IsEnabled(path, rule) ? "warning" : "none";
            return true;
        }
        return configured.TryGetValue(key, out value);
    }

    /// <summary>The configured keys and the requested rules' severity keys.</summary>
    public override IEnumerable<string> Keys => configured.Keys.Union(scope.Rules.Select(rule => Prefix + rule + Suffix), KeyComparer);

    /// <summary>The requested rule a severity key names, or null for any other key.</summary>
    private string? RuleOf(string key)
    {
        if (key.Length <= Prefix.Length + Suffix.Length || !key.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) ||
            !key.EndsWith(Suffix, StringComparison.OrdinalIgnoreCase))
            return null;
        string id = key[Prefix.Length..^Suffix.Length];
        return scope.Rules.FirstOrDefault(rule => string.Equals(rule, id, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Finds the suppressions that suppressed nothing (IDE0079), from a run of every analyzer with suppressed diagnostics
/// reported. A <c>#pragma warning disable</c> code is needed when a diagnostic of that id that no attribute suppressed
/// lies between it and the next <c>restore</c> of that code (or the end of the file); a <c>[SuppressMessage]</c> or
/// <c>[UnconditionalSuppressMessage]</c> is needed when it is the attribute that suppressed a diagnostic. A suppression
/// without codes, of a rule the file does not enable, in build output, or over code an inactive <c>#if</c> branch leaves
/// out is left alone. Each finding is at the
/// suppression's code or attribute; a pragma's finding also locates the matching <c>restore</c> code, when no other
/// disable of that code comes between them.
/// </summary>
internal static class UnnecessarySuppressions
{
    /// <summary>The finding's descriptor.</summary>
    private static readonly DiagnosticDescriptor Rule = new("IDE0079", "Remove unnecessary suppression",
        "The suppression of {0} suppresses nothing", "Style", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    /// <summary>The attributes that suppress diagnostics.</summary>
    private static readonly string[] SuppressionAttributes =
        ["System.Diagnostics.CodeAnalysis.SuppressMessageAttribute", "System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute"];

    /// <summary>The unnecessary suppressions in <paramref name="compilation"/>'s source, given every diagnostic it
    /// reported, suppressed ones included, and which rules each file enables.</summary>
    public static List<Diagnostic> Find(Compilation compilation, ImmutableArray<Diagnostic> diagnostics, Func<SyntaxTree, string, bool> isEnabled)
    {
        var suppressed = diagnostics.Where(d => d.IsSuppressed && d.Location.IsInSource).ToList();
        var byAttribute = new HashSet<(string, TextSpan)>();
        var byPragma = new List<Diagnostic>();
        foreach (Diagnostic diagnostic in suppressed)
        {
            if (diagnostic.GetSuppressionInfo(compilation)?.Attribute?.ApplicationSyntaxReference is { } reference)
                byAttribute.Add((reference.SyntaxTree.FilePath, reference.Span));
            else
                byPragma.Add(diagnostic);
        }
        var found = new List<Diagnostic>();
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            if (tree.FilePath.Replace('\\', '/').Contains("/obj/", StringComparison.OrdinalIgnoreCase))
                continue;
            SyntaxNode root = tree.GetRoot();
            FindPragmas(tree, root, byPragma, isEnabled, found);
            FindAttributes(compilation.GetSemanticModel(tree), root, byAttribute, isEnabled, found);
        }
        return found;
    }

    /// <summary>Adds each disabled code no diagnostic needed.</summary>
    private static void FindPragmas(SyntaxTree tree, SyntaxNode root, List<Diagnostic> byPragma, Func<SyntaxTree, string, bool> isEnabled, List<Diagnostic> found)
    {
        var pragmas = root.DescendantTrivia(descendIntoTrivia: true)
            .Where(trivia => trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PragmaWarningDirectiveTrivia))
            .Select(trivia => (PragmaWarningDirectiveTriviaSyntax)trivia.GetStructure()!)
            .OrderBy(pragma => pragma.SpanStart).ToList();
        for (int index = 0; index < pragmas.Count; index++)
        {
            PragmaWarningDirectiveTriviaSyntax disable = pragmas[index];
            if (!disable.DisableOrRestoreKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DisableKeyword))
                continue;
            foreach (ExpressionSyntax code in disable.ErrorCodes)
            {
                string id = CodeOf(code);
                if (!isEnabled(tree, id))
                    continue;
                ExpressionSyntax? restoreCode = null;
                int end = tree.Length;
                bool reopened = false;
                for (int later = index + 1; later < pragmas.Count; later++)
                {
                    PragmaWarningDirectiveTriviaSyntax next = pragmas[later];
                    bool names = next.ErrorCodes.Count == 0 || next.ErrorCodes.Any(c => string.Equals(CodeOf(c), id, StringComparison.OrdinalIgnoreCase));
                    if (!names)
                        continue;
                    if (next.DisableOrRestoreKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DisableKeyword))
                    {
                        reopened = true;
                        continue;
                    }
                    end = next.SpanStart;
                    restoreCode = reopened ? null : next.ErrorCodes.FirstOrDefault(c => string.Equals(CodeOf(c), id, StringComparison.OrdinalIgnoreCase));
                    break;
                }
                int start = disable.Span.End;
                bool needed = HoldsInactiveCode(root, TextSpan.FromBounds(start, end)) || byPragma.Any(d => string.Equals(d.Location.SourceTree?.FilePath, tree.FilePath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase) &&
                    d.Location.SourceSpan.Start >= start && d.Location.SourceSpan.Start < end);
                if (needed)
                    continue;
                Location[] restore = restoreCode is null ? [] : [restoreCode.GetLocation()];
                found.Add(Diagnostic.Create(Rule, code.GetLocation(), restore, id));
            }
        }
    }

    /// <summary>Adds each suppression attribute that suppressed no diagnostic.</summary>
    private static void FindAttributes(SemanticModel model, SyntaxNode root, HashSet<(string, TextSpan)> byAttribute,
        Func<SyntaxTree, string, bool> isEnabled, List<Diagnostic> found)
    {
        foreach (AttributeSyntax attribute in root.DescendantNodes().OfType<AttributeSyntax>())
        {
            string name = attribute.Name.ToString();
            if (!name.Contains("SuppressMessage", StringComparison.Ordinal) ||
                model.GetSymbolInfo(attribute).Symbol is not IMethodSymbol constructor ||
                !SuppressionAttributes.Contains(constructor.ContainingType.ToDisplayString()) ||
                attribute.ArgumentList is not { Arguments.Count: >= 2 } arguments ||
                model.GetConstantValue(arguments.Arguments[1].Expression).Value is not string checkId)
                continue;
            string id = checkId.Split(':')[0].Trim();
            if (id.Length == 0 || !isEnabled(model.SyntaxTree, id) || byAttribute.Contains((model.SyntaxTree.FilePath, attribute.Span)) ||
                attribute.Parent?.Parent is MemberDeclarationSyntax member && HoldsInactiveCode(root, member.FullSpan))
                continue;
            found.Add(Diagnostic.Create(Rule, attribute.GetLocation(), id));
        }
    }

    /// <summary>Whether the span holds code an inactive <c>#if</c> branch leaves out, whose diagnostics this
    /// configuration cannot see.</summary>
    private static bool HoldsInactiveCode(SyntaxNode root, TextSpan span) =>
        root.DescendantTrivia(span).Any(trivia => trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DisabledTextTrivia));

    /// <summary>The diagnostic id a pragma code names: an identifier as written, or a number as its compiler warning.</summary>
    internal static string CodeOf(ExpressionSyntax code) => code switch
    {
        LiteralExpressionSyntax { Token.Value: int number } => $"CS{number:D4}",
        _ => code.ToString().Trim(),
    };
}

/// <summary>
/// The analyzers beyond the requested rules' own that IDE0079 needs in one project. A suppression is needed only when it
/// suppresses a diagnostic of the id it names, so only an analyzer that reports an id one of the project's suppressions
/// names (a <c>#pragma warning disable</c> code, or a <c>[SuppressMessage]</c> or <c>[UnconditionalSuppressMessage]</c>
/// check id) can show one to be needed; the compiler reports its own ids anyway. Every diagnostic suppressor runs too,
/// since it decides what is suppressed, as it did when every analyzer ran. Any other analyzer's diagnostics are
/// neither findings nor evidence, so leaving it out changes nothing the run decides, and spares the run the whole
/// analyzer set over every project. When an attribute's check id is not a string literal, every analyzer runs.
/// </summary>
internal static class SuppressionEvidence
{
    /// <summary>The evidence analyzers (not among <paramref name="ruleAnalyzers"/>) and every diagnostic suppressor.</summary>
    public static (ImmutableArray<DiagnosticAnalyzer> Evidence, ImmutableArray<DiagnosticAnalyzer> Suppressors) Analyzers(
        Compilation compilation, ImmutableArray<DiagnosticAnalyzer> loaded, ImmutableArray<DiagnosticAnalyzer> ruleAnalyzers)
    {
        HashSet<string>? ids = SuppressedIds(compilation);
        var others = loaded.Where(analyzer => !ruleAnalyzers.Contains(analyzer)).ToList();
        return ([.. others.Where(analyzer => analyzer is not DiagnosticSuppressor &&
                    (ids is null || analyzer.SupportedDiagnostics.Any(descriptor => ids.Contains(descriptor.Id))))],
            [.. others.OfType<DiagnosticSuppressor>()]);
    }

    /// <summary>Every id a suppression in the compilation's source names, or null when an attribute names one this
    /// cannot read without binding.</summary>
    private static HashSet<string>? SuppressedIds(Compilation compilation)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SyntaxNode root = tree.GetRoot();
            foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: true))
            {
                if (trivia.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PragmaWarningDirectiveTrivia) &&
                    trivia.GetStructure() is PragmaWarningDirectiveTriviaSyntax pragma &&
                    pragma.DisableOrRestoreKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DisableKeyword))
                    ids.UnionWith(pragma.ErrorCodes.Select(UnnecessarySuppressions.CodeOf));
            }
            foreach (AttributeSyntax attribute in root.DescendantNodes().OfType<AttributeSyntax>())
            {
                if (!attribute.Name.ToString().Contains("SuppressMessage", StringComparison.Ordinal) ||
                    attribute.ArgumentList is not { Arguments.Count: >= 2 } arguments)
                    continue;
                if (arguments.Arguments[1].Expression is not LiteralExpressionSyntax { Token.Value: string checkId })
                    return null;
                ids.Add(checkId.Split(':')[0].Trim());
            }
        }
        return ids;
    }
}
/// <summary>
/// Keeps the explicit source lists in step with the files the fixes create and delete: a project file's or an imported
/// <c>.projitems</c> file's single-line <c>&lt;Compile Include="..."/&gt;</c> item that names a file by path (no
/// wildcard, no property but <c>$(MSBuildThisFileDirectory)</c>). A deleted file's item goes; a file renamed (created
/// from a file that was deleted) has its item retargeted, <c>Link</c> included; a file created beside one that stays gets
/// a copy of that file's item right after it. A wildcard item already covers a file created in its folder. A new file
/// in another folder than its origin fails the run.
/// </summary>
internal static class ProjectLists
{
    /// <summary>A single-line compile item that names its file.</summary>
    private static readonly Regex Item = new(@"<Compile\s+Include=""(?<include>[^""]+)""[^>]*/>", RegexOptions.CultureInvariant);

    /// <summary>An import of a shared-source item file.</summary>
    private static readonly Regex Import = new(@"<Import\s+Project=""(?<project>[^""]+\.projitems)""", RegexOptions.CultureInvariant);

    /// <summary>Updates the lists for <paramref name="created"/> (each with its original file) and
    /// <paramref name="deleted"/>, and returns the full paths of the list files written.</summary>
    public static List<string> Update(Solution original, Dictionary<string, string> created, List<string> deleted, Func<string, string> relative)
    {
        if (created.Count == 0 && deleted.Count == 0)
            return [];
        var gone = new HashSet<string>(deleted, StringComparer.OrdinalIgnoreCase);
        var renamed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var besides = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, origin) in created.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (!string.Equals(Path.GetDirectoryName(path), Path.GetDirectoryName(origin), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"{relative(path)} was created from {relative(origin)} in another folder; its project lists cannot follow.");
            if (gone.Contains(origin) && !renamed.ContainsKey(origin))
                renamed[origin] = path;
            else if (besides.TryGetValue(origin, out var list))
                list.Add(path);
            else
                besides[origin] = [path];
        }
        var written = new List<string>();
        foreach (string file in ListFiles(original))
        {
            string text = File.ReadAllText(file);
            string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            var output = new List<string>();
            bool changed = false;
            foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
            {
                Match match = Item.Match(line);
                string? named = match.Success ? Resolve(file, match.Groups["include"].Value) : null;
                if (named is null)
                {
                    output.Add(line);
                    continue;
                }
                if (renamed.TryGetValue(named, out string? target))
                {
                    output.Add(Retarget(line, named, target));
                    changed = true;
                    continue;
                }
                if (gone.Contains(named))
                {
                    changed = true;
                    continue;
                }
                output.Add(line);
                if (besides.TryGetValue(named, out var siblings))
                {
                    output.AddRange(siblings.Select(sibling => Retarget(line, named, sibling)));
                    changed = true;
                }
            }
            if (!changed)
                continue;
            bool bom = File.ReadAllBytes(file) is [0xEF, 0xBB, 0xBF, ..];
            File.WriteAllText(file, string.Join(newline, output), new UTF8Encoding(bom));
            written.Add(file);
        }
        return written;
    }

    /// <summary>Every project file of the solution and every <c>.projitems</c> file one of them imports.</summary>
    private static SortedSet<string> ListFiles(Solution original)
    {
        var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Project project in original.Projects)
        {
            if (project.FilePath is not { } path)
                continue;
            files.Add(path);
            foreach (Match import in Import.Matches(File.ReadAllText(path)))
            {
                if (Resolve(path, import.Groups["project"].Value) is { } imported)
                    files.Add(imported);
            }
        }
        return files;
    }

    /// <summary>The full path a list file's item names, or null for a wildcard or a property the list cannot resolve.</summary>
    private static string? Resolve(string listFile, string include)
    {
        string directory = Path.GetDirectoryName(listFile)!;
        string value = include.Replace("$(MSBuildThisFileDirectory)", directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        if (value.Contains("$(", StringComparison.Ordinal) || value.IndexOfAny(['*', '?', ';']) >= 0)
            return null;
        return Path.GetFullPath(Path.Combine(directory, value.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)));
    }

    /// <summary>An item line naming <paramref name="target"/>'s file name where it named <paramref name="named"/>'s,
    /// in its Include and its Link.</summary>
    private static string Retarget(string line, string named, string target)
    {
        string oldName = Path.GetFileName(named);
        string newName = Path.GetFileName(target);
        return Regex.Replace(line, @"(?<attribute>\b(Include|Link)="")(?<path>[^""]*)""", match =>
        {
            string value = match.Groups["path"].Value;
            int cut = Math.Max(value.LastIndexOf('/'), value.LastIndexOf('\\')) + 1;
            if (!string.Equals(value[cut..], oldName, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"The item '{line.Trim()}' does not end its {match.Groups["attribute"].Value.TrimEnd('=', '"')} with {oldName}.");
            return match.Groups["attribute"].Value + value[..cut] + newName + "\"";
        });
    }
}

/// <summary>Hands a fix-all exactly the findings chosen for one document, and nothing else.</summary>
internal sealed class FixedDiagnostics(Document document, List<Diagnostic> diagnostics) : FixAllContext.DiagnosticProvider
{
    /// <summary>The chosen findings when asked for the chosen document; none for any other.</summary>
    public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document requested, CancellationToken cancellationToken) =>
        Task.FromResult<IEnumerable<Diagnostic>>(requested.Id == document.Id ? diagnostics : []);

    /// <summary>No project-level findings: every chosen finding has a document location.</summary>
    public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
        Task.FromResult<IEnumerable<Diagnostic>>([]);

    /// <summary>The chosen findings when asked for the chosen document's project.</summary>
    public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken) =>
        Task.FromResult<IEnumerable<Diagnostic>>(project.Id == document.Project.Id ? diagnostics : []);
}
