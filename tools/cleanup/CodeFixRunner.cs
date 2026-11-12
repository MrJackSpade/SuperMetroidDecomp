#:package Microsoft.CodeAnalysis.Workspaces.MSBuild@5.9.0
#:package Microsoft.CodeAnalysis.CSharp.Workspaces@5.9.0
#:package Microsoft.Bcl.AsyncInterfaces@10.0.1
#:property PublishAot=false

// Applies the code fixes of the automatically fixed rules (csharp/.editorconfig names them) to a whole solution, the
// way the asynchronous cleanup needs them applied (tools/cleanup/README.md).
//
// It loads every project of a .slnx (or one .csproj) into one MSBuildWorkspace, runs each project's own analyzers for
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
// fails the run, naming the errors.
//
// IDE0079 (unnecessary suppression) has no command-line analyzer either. When it is requested, the runner runs every
// analyzer of each project with the whole cleanup group enabled in its scope and suppressed diagnostics reported, and
// finds each #pragma warning disable code and [SuppressMessage] that suppressed nothing (UnnecessarySuppressions). Only
// a suppression of a rule enabled in its file is judged; one of an unknown or disabled rule is left alone.
//
// A file compiled into several projects (test-support through .projitems, or a <Compile Link>) is edited only where
// every project compiling it reports the same finding at the same place, and not at all while any of them has a
// compiler error, because a semantic model with errors reads needed code as unnecessary. Where the projects disagree
// about a file's unnecessary usings, only the usings every project reports are removed; other disagreeing findings
// are left and counted as kept.
//
// A fix may change several files (a field and its property in different parts of a partial type). It is accepted only
// when every file it changes belongs to an analyzed project that compiles, and when no other fix in the same pass has
// touched one of those files; a file touched in this pass waits for the next. Every project compiling a file of such a
// fix must still compile after it, or the run fails naming the fix.
//
// Fixes cascade (a removed cast can leave a using unnecessary), so the analysis repeats until a pass finds nothing to
// apply. Each pass applies one rule per file, because the other findings' spans are stale once a file changes. The
// loop fails, writing nothing, if a file returns to an earlier text (a cycle) or the passes exceed MaxPasses.
//
// Usage: dotnet run tools/cleanup/CodeFixRunner.cs -- <solution.slnx | project.csproj> <summary.json> <rule,rule,...> [--dry-run]
// --dry-run prints every finding with all its locations and writes nothing.

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

    /// <summary>Runs the fixes behind the no-dialog boundary; any failure is written in full with a nonzero exit.</summary>
    public static async Task<int> RunAsync(string[] args)
    {
        if (OperatingSystem.IsWindows())
            _ = SetErrorMode(NoDialogs);
        try
        {
            bool dryRun = args.Length == 4 && args[3] == "--dry-run";
            if (args.Length != 3 && !dryRun)
                throw new ArgumentException("usage: CodeFixRunner <solution.slnx | project.csproj> <summary.json> <rule,rule,...> [--dry-run]");
            string[] rules = args[2].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (rules.Length == 0)
                throw new ArgumentException("No rules named.");
            var fixer = new SolutionFixer(Path.GetFullPath(args[0]), rules, dryRun);
            CleanupSummary summary = await fixer.RunAsync();
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
}

/// <summary>What one run fixed and left, for the cleanup's log and commit message.</summary>
/// <param name="Passes">The analysis passes run, the last of which found nothing to apply.</param>
/// <param name="Fixed">Sites fixed per rule; for unused usings, the using directives removed.</param>
/// <param name="Kept">Findings per rule left unapplied: in files compiled by several projects that do not all report
/// them, or whose fix changes a file that a project with compiler errors compiles.</param>
/// <param name="ChangedFiles">Repository-relative paths of the files written.</param>
/// <param name="ProjectsWithCompilerErrors">Projects whose findings were not used because they do not compile.</param>
internal sealed record CleanupSummary(
    int Passes,
    SortedDictionary<string, int> Fixed,
    SortedDictionary<string, int> Kept,
    List<string> ChangedFiles,
    List<string> ProjectsWithCompilerErrors);

/// <summary>One project's analysis: whether it compiles, and its findings of the requested rules by file.</summary>
/// <param name="Project">The analyzed project.</param>
/// <param name="CompilerErrors">The compiler errors, empty when it compiles.</param>
/// <param name="Findings">The unsuppressed findings of the requested rules, by file path.</param>
/// <param name="FixerOnly">Hidden findings an analyzer reports only for a rule's fixer to fix, by file path.</param>
internal sealed record ProjectAnalysis(Project Project, List<Diagnostic> CompilerErrors, Dictionary<string, List<Diagnostic>> Findings,
    Dictionary<string, List<Diagnostic>> FixerOnly);

/// <summary>One accepted fix: the solution after it and every file it changed.</summary>
/// <param name="Solution">The solution with the fix applied in every project that compiles a changed file.</param>
/// <param name="Paths">The full paths of the files the fix changed.</param>
internal sealed record FileFix(Solution Solution, List<string> Paths);

/// <summary>Whether a computed fix is applied now, left for the next pass, or kept unapplied.</summary>
/// <param name="Fix">The accepted fix, or null when it is not applied in this pass.</param>
/// <param name="Kept">Whether the findings are kept (a project compiling a changed file does not compile) rather
/// than retried in the next pass.</param>
internal sealed record FixDecision(FileFix? Fix, bool Kept);

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

    /// <summary>The order rules are applied in within a file: suppressions first, then code, then the usings that
    /// the other fixes can leave unnecessary.</summary>
    private static readonly CleanupRule[] RuleOrder =
    [
        new("IDE0076", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("IDE0077", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("IDE0079", FindingSource.UnnecessarySuppression, FixerSource.Repository),
        new("IDE0004", FindingSource.Analyzer, FixerSource.CodeStyle),
        new("CS0219", FindingSource.Compiler, FixerSource.Repository),
        new("CS0168", FindingSource.Compiler, FixerSource.Repository),
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

    private readonly string _target;
    private readonly HashSet<string> _rules;
    private readonly bool _dryRun;
    private readonly string _root;
    private readonly Dictionary<string, List<CodeFixProvider>> _fixerAssemblies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates a fixer for a solution or project file and the rules to fix.</summary>
    public SolutionFixer(string target, string[] rules, bool dryRun)
    {
        _target = target;
        _rules = rules.ToHashSet(StringComparer.Ordinal);
        foreach (string rule in _rules)
        {
            if (!RuleOrder.Any(ordered => ordered.Id == rule))
                throw new ArgumentException($"{rule} is not one of the rules this runner orders: {string.Join(", ", RuleOrder.Select(r => r.Id))}.");
        }
        _dryRun = dryRun;
        _root = FindRepositoryRoot(Path.GetDirectoryName(target) ?? throw new ArgumentException($"{target} has no directory."));
    }

    /// <summary>Runs the passes and writes the changed files.</summary>
    public async Task<CleanupSummary> RunAsync()
    {
        using var workspace = MSBuildWorkspace.Create();
        var failures = new List<string>();
        workspace.RegisterWorkspaceFailedHandler(e =>
        {
            if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                failures.Add(e.Diagnostic.Message);
        });
        await LoadAsync(workspace);
        if (failures.Count > 0)
            throw new InvalidOperationException("Workspace load failed:\n" + string.Join('\n', failures));

        Solution original = workspace.CurrentSolution;
        Solution solution = original;
        var fixedCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var keptCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);
        var history = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var analyses = new Dictionary<ProjectId, ProjectAnalysis>();
        var changedSinceAnalysis = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var broken = new SortedSet<string>(StringComparer.Ordinal);
        var compiledAtStart = new HashSet<ProjectId>();
        int pass = 0;
        while (true)
        {
            pass++;
            if (pass > MaxPasses)
                throw new InvalidOperationException($"The fixes did not converge within {MaxPasses} passes.");
            await AnalyzeAsync(solution, analyses, changedSinceAnalysis, pass == 1);
            changedSinceAnalysis.Clear();
            if (pass == 1)
                compiledAtStart.UnionWith(analyses.Values.Where(a => a.CompilerErrors.Count == 0).Select(a => a.Project.Id));
            if (analyses.Values.FirstOrDefault(a => a.CompilerErrors.Count > 0 && compiledAtStart.Contains(a.Project.Id)) is { } brokenByFixes)
                throw new InvalidOperationException($"The fixes of pass {pass - 1} left {brokenByFixes.Project.Name} with compiler errors:\n" +
                    string.Join('\n', brokenByFixes.CompilerErrors.Take(20)));
            broken.UnionWith(analyses.Values.Where(a => a.CompilerErrors.Count > 0).Select(a => a.Project.Name));
            if (_dryRun)
            {
                PrintFindings(analyses.Values);
                return new CleanupSummary(pass, fixedCounts, keptCounts, [], [.. broken]);
            }
            var passKept = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var touched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int applied = 0;
            foreach (string path in analyses.Values.SelectMany(a => a.Findings.Keys).Distinct(StringComparer.OrdinalIgnoreCase)
                         .Order(StringComparer.OrdinalIgnoreCase).ToList())
            {
                if (touched.Contains(path))
                    continue;
                FileFix? fix = await FixFileAsync(solution, path, analyses, fixedCounts, passKept, touched);
                if (fix is null)
                    continue;
                solution = fix.Solution;
                applied++;
                foreach (string changedPath in fix.Paths)
                {
                    touched.Add(changedPath);
                    changedSinceAnalysis.Add(changedPath);
                    string hash = Hash(await TextAtAsync(solution, changedPath));
                    if (!history.TryGetValue(changedPath, out var seen))
                        history[changedPath] = seen = [Hash(await TextAtAsync(original, changedPath))];
                    if (!seen.Add(hash))
                        throw new InvalidOperationException($"{Relative(changedPath)} returned to an earlier text in pass {pass}: the fixes cycle.");
                }
            }
            Console.WriteLine($"pass {pass}: fixed {applied} files; kept {Format(passKept)}");
            if (applied == 0)
            {
                foreach (var (rule, count) in passKept)
                    keptCounts[rule] = count;
                break;
            }
        }
        var changed = await WriteAsync(original, solution);
        Console.WriteLine($"fixed {Format(fixedCounts)} in {changed.Count} files after {pass} passes; kept {Format(keptCounts)}"
            + (broken.Count > 0 ? $"; projects with compiler errors: {string.Join(", ", broken)}" : ""));
        return new CleanupSummary(pass, fixedCounts, keptCounts, changed, [.. broken]);
    }

    /// <summary>Opens the target: every project a .slnx lists, or one project.</summary>
    private async Task LoadAsync(MSBuildWorkspace workspace)
    {
        if (_target.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            await workspace.OpenProjectAsync(_target);
            return;
        }
        if (!_target.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"{_target} is neither a .slnx nor a .csproj.");
        string directory = Path.GetDirectoryName(_target)!;
        foreach (string path in XDocument.Load(_target).Descendants("Project")
                     .Select(p => Path.GetFullPath(Path.Combine(directory,
                         (string?)p.Attribute("Path") ?? throw new InvalidOperationException($"{_target} lists a project without a Path.")))))
        {
            if (workspace.CurrentSolution.Projects.Any(p => string.Equals(p.FilePath, path, StringComparison.OrdinalIgnoreCase)))
                continue;
            await workspace.OpenProjectAsync(path);
        }
    }

    /// <summary>Analyzes every project on the first pass, then only the projects that compile a changed file. The
    /// fixes never change what another project can see (they remove usings, casts and suppressions, or make a private
    /// field read-only), so a project with no changed file keeps its findings.</summary>
    private async Task AnalyzeAsync(Solution solution, Dictionary<ProjectId, ProjectAnalysis> analyses, HashSet<string> changed, bool all)
    {
        foreach (Project project in solution.Projects.OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            if (!all && !project.Documents.Any(d => d.FilePath is { } path && changed.Contains(path)))
                continue;
            analyses[project.Id] = await AnalyzeProjectAsync(project);
        }
    }

    /// <summary>Compiles one project with the requested rules enabled as warnings where their scope allows, and runs
    /// its analyzers for the requested analyzer rules; the compiler's own rules come with the compilation. When
    /// IDE0079 is requested, every analyzer runs, with the whole cleanup group enabled and suppressed diagnostics
    /// reported, so that a suppression's use can be seen.</summary>
    private async Task<ProjectAnalysis> AnalyzeProjectAsync(Project project)
    {
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
        var loaded = project.AnalyzerReferences.SelectMany(reference => reference.GetAnalyzers(project.Language)).ToImmutableArray();
        var analyzers = suppressions
            ? loaded
            : loaded.Where(analyzer => analyzer.SupportedDiagnostics.Any(descriptor => analyzerRules.Contains(descriptor.Id))).ToImmutableArray();
        var supported = analyzers.SelectMany(a => a.SupportedDiagnostics).Select(d => d.Id).ToHashSet(StringComparer.Ordinal);
        var unsupported = analyzerRules.Where(rule => !supported.Contains(rule)).Order(StringComparer.Ordinal).ToList();
        if (unsupported.Count > 0)
            throw new InvalidOperationException(
                $"{project.Name} references no analyzer for {string.Join(", ", unsupported)}; the code-style analyzers are not loaded, " +
                "or the repository analyzer is not built in the configuration the workspace opens.");
        if (suppressions && analyzers.IsEmpty)
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
            var withAnalyzers = compilation.WithAnalyzers(analyzers, new CompilationWithAnalyzersOptions(analyzerOptions,
                onAnalyzerException: null, concurrentAnalysis: true, logAnalyzerExecutionTime: false, reportSuppressedDiagnostics: suppressions));
            diagnostics = await withAnalyzers.GetAllDiagnosticsAsync();
            analyzed = withAnalyzers.Compilation;
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
        Console.WriteLine($"  analyzed {project.Name}: {findings.Values.Sum(l => l.Count)} findings"
            + (errors.Count > 0 ? $", {errors.Count} compiler errors (its files are left alone)" : ""));
        return new ProjectAnalysis(project, errors, findings, fixable);
    }

    /// <summary>Applies one rule's agreed fixes to one file, or returns null when nothing in it may be fixed in this
    /// pass: a fix that would change a file another fix of this pass changed waits for the next pass.</summary>
    private async Task<FileFix?> FixFileAsync(Solution solution, string path, Dictionary<ProjectId, ProjectAnalysis> analyses,
        SortedDictionary<string, int> fixedCounts, SortedDictionary<string, int> kept, HashSet<string> touched)
    {
        var compilers = solution.GetDocumentIdsWithFilePath(path)
            .Select(id => analyses[id.ProjectId]).OrderBy(a => a.Project.Name, StringComparer.Ordinal).ToList();
        var reported = compilers.Select(a => a.Findings.GetValueOrDefault(path) ?? []).ToList();
        if (compilers.Any(a => a.CompilerErrors.Count > 0))
        {
            Count(kept, reported.SelectMany(l => l).DistinctBy(Key));
            return null;
        }
        var agreed = reported.Select(l => l.Select(Key).ToHashSet(StringComparer.Ordinal))
            .Aggregate((left, right) => { left.IntersectWith(right); return left; });
        Count(kept, reported.SelectMany(l => l).Where(d => !agreed.Contains(Key(d))).DistinctBy(Key));
        ProjectAnalysis chosen = compilers[0];
        Document document = solution.GetDocument(solution.GetDocumentIdsWithFilePath(path).Single(id => id.ProjectId == chosen.Project.Id))!;
        var own = reported[0];
        foreach (CleanupRule cleanupRule in RuleOrder)
        {
            string rule = cleanupRule.Id;
            var diagnostics = own.Where(d => d.Id == rule && agreed.Contains(Key(d))).ToList();
            if (diagnostics.Count > 0)
            {
                Solution changed = rule == UnusedUsing
                    ? await ApplyFixAllAsync(document, cleanupRule, UnusedUsingFixable, chosen.FixerOnly.GetValueOrDefault(path)
                        ?? throw new InvalidOperationException($"{Relative(path)} reports {UnusedUsing} without {UnusedUsingFixable} in {chosen.Project.Name}."))
                    : await ApplyFixAllAsync(document, cleanupRule, rule, diagnostics);
                FixDecision decision = await AcceptAsync(solution, changed, document, rule, analyses, touched);
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
            if (rule == UnusedUsing && compilers.Count > 1)
            {
                // The projects disagree on the file's unused usings: remove only the usings every one of them reports.
                var lines = reported.Select(l => UsingLines(l.Where(d => d.Id == UnusedUsing)))
                    .Aggregate((left, right) => { left.IntersectWith(right); return left; });
                if (lines.Count == 0)
                    continue;
                SourceText text = await document.GetTextAsync();
                fixedCounts[rule] = fixedCounts.GetValueOrDefault(rule) + lines.Count;
                return new FileFix(WithText(solution, path, RemoveLines(text, lines, path)), [path]);
            }
        }
        return null;
    }

    /// <summary>
    /// Decides whether a fix computed in one project may be applied, and applies it to every project that compiles
    /// a file it changed. A fix of one file is always accepted. A fix of several files is accepted only when every file
    /// it changes is compiled by analyzed projects without compiler errors (otherwise its findings are kept) and when no
    /// other fix of this pass touched one of them (otherwise it waits for the next pass); every project compiling one
    /// of its files must then still compile, or the run fails.
    /// </summary>
    private async Task<FixDecision> AcceptAsync(Solution solution, Solution changed, Document document, string rule,
        Dictionary<ProjectId, ProjectAnalysis> analyses, HashSet<string> touched)
    {
        string source = document.FilePath!;
        var texts = new SortedDictionary<string, SourceText>(StringComparer.OrdinalIgnoreCase);
        foreach (ProjectChanges project in changed.GetChanges(solution).GetProjectChanges())
        {
            if (project.GetAddedDocuments().Any() || project.GetRemovedDocuments().Any())
                throw new InvalidOperationException($"The {rule} fix for {Relative(source)} adds or removes files, which this runner does not apply.");
            foreach (DocumentId id in project.GetChangedDocuments())
            {
                Document fixedDocument = changed.GetDocument(id)!;
                string path = fixedDocument.FilePath
                    ?? throw new InvalidOperationException($"The {rule} fix for {Relative(source)} changed {fixedDocument.Name}, which has no file.");
                SourceText text = await fixedDocument.GetTextAsync();
                if (texts.TryGetValue(path, out SourceText? other) && !other.ContentEquals(text))
                    throw new InvalidOperationException($"The {rule} fix for {Relative(source)} gave {Relative(path)} two different texts.");
                texts[path] = text;
            }
        }
        if (texts.Count == 0)
            throw new InvalidOperationException($"The {rule} fix for {Relative(source)} changed nothing.");
        if (texts.Count == 1 && texts.ContainsKey(source))
            return new FixDecision(new FileFix(WithText(solution, source, texts[source]), [source]), false);

        if (texts.Keys.Any(touched.Contains))
            return new FixDecision(null, false);
        foreach (string path in texts.Keys)
        {
            var ids = solution.GetDocumentIdsWithFilePath(path);
            if (ids.IsEmpty)
                throw new InvalidOperationException($"The {rule} fix for {Relative(source)} changed {Relative(path)}, which no project of the solution compiles.");
            if (ids.Any(id => !analyses.TryGetValue(id.ProjectId, out var analysis) || analysis.CompilerErrors.Count > 0))
                return new FixDecision(null, true);
        }
        Solution result = solution;
        foreach (var (path, text) in texts)
            result = WithText(result, path, text);
        await RequireCompilesAsync(result, texts.Keys, $"The {rule} fix for {Relative(source)}");
        return new FixDecision(new FileFix(result, [.. texts.Keys]), false);
    }

    /// <summary>Fails when a project compiling one of the files has a compiler error in the given solution.</summary>
    private async Task RequireCompilesAsync(Solution solution, IEnumerable<string> paths, string fix)
    {
        var projects = paths.SelectMany(path => solution.GetDocumentIdsWithFilePath(path)).Select(id => id.ProjectId).Distinct();
        foreach (ProjectId id in projects)
        {
            Project project = solution.GetProject(id)!;
            Compilation compilation = await project.GetCompilationAsync()
                ?? throw new InvalidOperationException($"{project.Name} produced no compilation.");
            var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
                throw new InvalidOperationException($"{fix} leaves {project.Name} with compiler errors:\n" + string.Join('\n', errors.Take(20)));
        }
    }

    /// <summary>A file's text in a solution, or null when no project compiles that file.</summary>
    private static async Task<SourceText?> TextAtAsync(Solution solution, string path)
    {
        var ids = solution.GetDocumentIdsWithFilePath(path);
        return ids.IsEmpty ? null : await solution.GetDocument(ids[0])!.GetTextAsync();
    }

    /// <summary>A file's text in a solution, which must compile it.</summary>
    private async Task<SourceText> RequiredTextAsync(Solution solution, string path) =>
        await TextAtAsync(solution, path) ?? throw new InvalidOperationException($"No project compiles {Relative(path)}.");

    /// <summary>The SHA-256 of a file's text, or a marker for a file no project compiles.</summary>
    private static string Hash(SourceText? text) =>
        text is null ? "absent" : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())));

    /// <summary>Runs the rule's fixer, for the diagnostic id it fixes, over exactly the given findings of one document
    /// and returns the solution it produces, which may change other documents too.</summary>
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
            throw new InvalidOperationException($"{provider.GetType().Name} offers no fix for {rule} at {Describe(diagnostics[0])}.");
        FixAllProvider fixAll = provider.GetFixAllProvider()
            ?? throw new InvalidOperationException($"{provider.GetType().Name} has no fix-all provider.");
        var context = new FixAllContext(document, provider, FixAllScope.Document, actions[0].EquivalenceKey, [rule],
            new FixedDiagnostics(document, diagnostics), CancellationToken.None);
        CodeAction action = await fixAll.GetFixAsync(context)
            ?? throw new InvalidOperationException($"{provider.GetType().Name} produced no fix-all action for {Relative(document.FilePath!)}.");
        var operations = await action.GetOperationsAsync(CancellationToken.None);
        return operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
    }

    /// <summary>The code fix providers for a rule, from the fixer assemblies of the given source that the project
    /// references.</summary>
    private List<CodeFixProvider> FixersFor(Project project, FixerSource source, string rule)
    {
        var providers = new List<CodeFixProvider>();
        foreach (string path in project.AnalyzerReferences.Select(r => r.FullPath)
                     .Where(p => p is not null && IsFixerAssembly(Path.GetFileName(p), source))
                     .Select(p => p!))
        {
            if (!_fixerAssemblies.TryGetValue(path, out var loaded))
                _fixerAssemblies[path] = loaded = LoadProviders(path, project.Language);
            providers.AddRange(loaded.Where(provider => provider.FixableDiagnosticIds.Contains(rule)));
        }
        return providers;
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

    /// <summary>Writes every file whose text changed, keeping its byte-order mark, and returns their paths.</summary>
    private async Task<List<string>> WriteAsync(Solution original, Solution solution)
    {
        var changed = new List<string>();
        foreach (DocumentId id in solution.GetChanges(original).GetProjectChanges().SelectMany(p => p.GetChangedDocuments()))
        {
            string path = solution.GetDocument(id)!.FilePath!;
            if (changed.Contains(path, StringComparer.OrdinalIgnoreCase))
                continue;
            string text = (await solution.GetDocument(id)!.GetTextAsync()).ToString();
            bool bom = File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];
            File.WriteAllText(path, text, new UTF8Encoding(bom));
            changed.Add(path);
        }
        changed.Sort(StringComparer.OrdinalIgnoreCase);
        return [.. changed.Select(Relative)];
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

    /// <summary>The nearest directory at or above the start that holds .git, or the start itself outside a repository.</summary>
    private static string FindRepositoryRoot(string start)
    {
        for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")) || File.Exists(Path.Combine(directory.FullName, ".git")))
                return directory.FullName;
        }
        return start;
    }
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
    private readonly Dictionary<string, AnalyzerConfigOptionsResult> _byPath = new(StringComparer.OrdinalIgnoreCase);

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
        if (!_byPath.TryGetValue(path, out AnalyzerConfigOptionsResult options))
            _byPath[path] = options = _configuration.GetOptionsForSourcePath(path);
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
    private static string CodeOf(ExpressionSyntax code) => code switch
    {
        LiteralExpressionSyntax { Token.Value: int number } => $"CS{number:D4}",
        _ => code.ToString().Trim(),
    };
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
