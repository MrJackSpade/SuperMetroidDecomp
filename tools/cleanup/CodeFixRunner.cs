#:package Microsoft.CodeAnalysis.Workspaces.MSBuild@5.9.0
#:package Microsoft.CodeAnalysis.CSharp.Workspaces@5.9.0
#:package Microsoft.Bcl.AsyncInterfaces@10.0.1
#:property PublishAot=false

// Applies the code fixes of the automatically fixed rules (csharp/.editorconfig names them) to a whole solution, the
// way the asynchronous cleanup needs them applied (tools/cleanup/README.md).
//
// It loads every project of a .slnx (or one .csproj) into one MSBuildWorkspace, runs each project's own analyzers for
// the requested rules with the project's own configuration, and applies the fixes of Roslyn's code-style fixers, the
// same fixers `dotnet format style` uses, from the code-style package the projects reference.
//
// csharp/.editorconfig sets the group to none, so the build skips their analyzers and they cost it no time. The runner
// therefore enables exactly the requested rules, as warnings, in its own analysis: in the compilation's per-file
// diagnostic options, where the .editorconfig severity would otherwise win over any compilation-wide setting, and in
// the analyzer configuration the code-style analyzers read their own severity from. Every other rule keeps the
// project's configuration. The caller (tools/cleanup/run.ps1) reads the group from csharp/.editorconfig.
//
// IDE0079 (unnecessary suppression) is in the group, but no command-line analyzer reports it (README.md), so the
// runner refuses to be asked for it rather than report that it found nothing.
//
// A file compiled into several projects (test-support through .projitems, or a <Compile Link>) is edited only where
// every project compiling it reports the same finding at the same place, and not at all while any of them has a
// compiler error, because a semantic model with errors reads needed code as unnecessary. Where the projects disagree
// about a file's unnecessary usings, only the usings every project reports are removed; other disagreeing findings
// are left and counted as kept.
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
/// <param name="Kept">Findings per rule left in files compiled by several projects that do not all report them.</param>
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

/// <summary>Loads a solution and applies the requested rules' fixes until none remain.</summary>
internal sealed class SolutionFixer
{
    /// <summary>The bound on analysis passes. Every fix removes text or adds a modifier once, so real cascades settle
    /// in a few passes; reaching this many means the fixes are not converging.</summary>
    private const int MaxPasses = 20;

    /// <summary>The order rules are applied in within a file: suppressions first, then code, then the usings that
    /// the other fixes can leave unnecessary.</summary>
    private static readonly string[] RuleOrder = ["IDE0076", "IDE0077", "IDE0004", "IDE0044", "IDE0005"];

    /// <summary>The rule of the automatically fixed group that no command-line analyzer reports (README.md).</summary>
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
    private readonly Dictionary<string, Assembly> _fixerAssemblies = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates a fixer for a solution or project file and the rules to fix.</summary>
    public SolutionFixer(string target, string[] rules, bool dryRun)
    {
        _target = target;
        _rules = rules.ToHashSet(StringComparer.Ordinal);
        foreach (string rule in _rules)
        {
            if (rule == UnnecessarySuppression)
                throw new ArgumentException($"{rule} has no command-line analyzer: the code-style package reports no unnecessary " +
                    "suppression outside the IDE, so this runner cannot find its sites (tools/cleanup/README.md).");
            if (!RuleOrder.Contains(rule))
                throw new ArgumentException($"{rule} is not one of the rules this runner orders: {string.Join(", ", RuleOrder)}.");
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
        int pass = 0;
        while (true)
        {
            pass++;
            if (pass > MaxPasses)
                throw new InvalidOperationException($"The fixes did not converge within {MaxPasses} passes.");
            await AnalyzeAsync(solution, analyses, changedSinceAnalysis, pass == 1);
            changedSinceAnalysis.Clear();
            broken.UnionWith(analyses.Values.Where(a => a.CompilerErrors.Count > 0).Select(a => a.Project.Name));
            if (_dryRun)
            {
                PrintFindings(analyses.Values);
                return new CleanupSummary(pass, fixedCounts, keptCounts, [], [.. broken]);
            }
            var passKept = new SortedDictionary<string, int>(StringComparer.Ordinal);
            int applied = 0;
            foreach (string path in analyses.Values.SelectMany(a => a.Findings.Keys).Distinct(StringComparer.OrdinalIgnoreCase)
                         .Order(StringComparer.OrdinalIgnoreCase).ToList())
            {
                Solution? next = await FixFileAsync(solution, path, analyses, fixedCounts, passKept);
                if (next is null)
                    continue;
                solution = next;
                applied++;
                changedSinceAnalysis.Add(path);
                string text = (await solution.GetDocument(solution.GetDocumentIdsWithFilePath(path)[0])!.GetTextAsync()).ToString();
                string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
                if (!history.TryGetValue(path, out var seen))
                {
                    seen = [Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                        (await original.GetDocument(original.GetDocumentIdsWithFilePath(path)[0])!.GetTextAsync()).ToString())))];
                    history[path] = seen;
                }
                if (!seen.Add(hash))
                    throw new InvalidOperationException($"{Relative(path)} returned to an earlier text in pass {pass}: the fixes cycle.");
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

    /// <summary>Compiles one project and runs its analyzers for the requested rules, enabled as warnings.</summary>
    private async Task<ProjectAnalysis> AnalyzeProjectAsync(Project project)
    {
        Compilation configured = await project.GetCompilationAsync()
            ?? throw new InvalidOperationException($"{project.Name} produced no compilation.");
        SyntaxTreeOptionsProvider treeOptions = configured.Options.SyntaxTreeOptionsProvider
            ?? throw new InvalidOperationException($"{project.Name} has no per-file analyzer configuration; csharp/.editorconfig is not loaded.");
        Compilation compilation = configured.WithOptions(configured.Options
            .WithSpecificDiagnosticOptions(configured.Options.SpecificDiagnosticOptions.SetItems(
                _rules.Select(rule => KeyValuePair.Create(rule, ReportDiagnostic.Warn))))
            .WithSyntaxTreeOptionsProvider(new EnabledRuleTreeOptions(treeOptions, _rules)));
        var analyzerOptions = new AnalyzerOptions(project.AnalyzerOptions.AdditionalFiles,
            new EnabledRuleConfigOptions(project.AnalyzerOptions.AnalyzerConfigOptionsProvider, _rules));
        var analyzers = project.AnalyzerReferences
            .SelectMany(reference => reference.GetAnalyzers(project.Language))
            .Where(analyzer => analyzer.SupportedDiagnostics.Any(descriptor => _rules.Contains(descriptor.Id)))
            .ToImmutableArray();
        var supported = analyzers.SelectMany(a => a.SupportedDiagnostics).Select(d => d.Id).ToHashSet(StringComparer.Ordinal);
        var unsupported = _rules.Where(rule => !supported.Contains(rule)).Order(StringComparer.Ordinal).ToList();
        if (unsupported.Count > 0)
            throw new InvalidOperationException(
                $"{project.Name} references no analyzer for {string.Join(", ", unsupported)}; the code-style analyzers are not loaded.");
        var diagnostics = await compilation.WithAnalyzers(analyzers, analyzerOptions).GetAllDiagnosticsAsync();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error && d.Id.StartsWith("CS", StringComparison.Ordinal)).ToList();
        var findings = new Dictionary<string, List<Diagnostic>>(StringComparer.OrdinalIgnoreCase);
        var fixable = new Dictionary<string, List<Diagnostic>>(StringComparer.OrdinalIgnoreCase);
        foreach (Diagnostic diagnostic in diagnostics)
        {
            bool finding = _rules.Contains(diagnostic.Id) && diagnostic.Severity != DiagnosticSeverity.Hidden;
            bool fixerOnly = diagnostic.Id == UnusedUsingFixable && _rules.Contains(UnusedUsing);
            if (!finding && !fixerOnly || diagnostic.IsSuppressed || !diagnostic.Location.IsInSource)
                continue;
            string path = diagnostic.Location.SourceTree!.FilePath;
            if (project.Documents.All(d => !string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase)))
                continue;
            var target = finding ? findings : fixable;
            if (!target.TryGetValue(path, out var list))
                target[path] = list = [];
            list.Add(diagnostic);
        }
        Console.WriteLine($"  analyzed {project.Name}: {findings.Values.Sum(l => l.Count)} findings"
            + (errors.Count > 0 ? $", {errors.Count} compiler errors (its files are left alone)" : ""));
        return new ProjectAnalysis(project, errors, findings, fixable);
    }

    /// <summary>Applies one rule's agreed fixes to one file, or returns null when nothing in it may be fixed.</summary>
    private async Task<Solution?> FixFileAsync(Solution solution, string path, Dictionary<ProjectId, ProjectAnalysis> analyses,
        SortedDictionary<string, int> fixedCounts, SortedDictionary<string, int> kept)
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
        foreach (string rule in RuleOrder)
        {
            var diagnostics = own.Where(d => d.Id == rule && agreed.Contains(Key(d))).ToList();
            if (diagnostics.Count > 0)
            {
                SourceText fixedText = rule == UnusedUsing
                    ? await ApplyFixAllAsync(document, UnusedUsingFixable, chosen.FixerOnly.GetValueOrDefault(path)
                        ?? throw new InvalidOperationException($"{Relative(path)} reports {UnusedUsing} without {UnusedUsingFixable} in {chosen.Project.Name}."))
                    : await ApplyFixAllAsync(document, rule, diagnostics);
                int count = rule == UnusedUsing
                    ? CountUsings(await document.GetTextAsync()) - CountUsings(fixedText)
                    : diagnostics.Count;
                fixedCounts[rule] = fixedCounts.GetValueOrDefault(rule) + count;
                return WithText(solution, path, fixedText);
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
                return WithText(solution, path, RemoveLines(text, lines, path));
            }
        }
        return null;
    }

    /// <summary>Runs the rule's fixer over exactly the given findings of one document and returns the fixed text.</summary>
    private async Task<SourceText> ApplyFixAllAsync(Document document, string rule, List<Diagnostic> diagnostics)
    {
        var providers = FixersFor(document.Project, rule);
        if (providers.Count != 1)
            throw new InvalidOperationException($"{rule} has {providers.Count} code fix providers in {document.Project.Name}; expected exactly one.");
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
        Solution changed = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var changedDocuments = changed.GetChanges(document.Project.Solution).GetProjectChanges()
            .SelectMany(p => p.GetChangedDocuments()).ToList();
        if (changedDocuments.Count != 1 || changedDocuments[0] != document.Id)
            throw new InvalidOperationException($"The {rule} fix for {Relative(document.FilePath!)} changed other documents too.");
        return await changed.GetDocument(document.Id)!.GetTextAsync();
    }

    /// <summary>The code fix providers for a rule, from the code-style fixer assemblies the project references.</summary>
    private List<CodeFixProvider> FixersFor(Project project, string rule)
    {
        var providers = new List<CodeFixProvider>();
        foreach (string path in project.AnalyzerReferences.Select(r => r.FullPath)
                     .Where(p => p is not null && Path.GetFileName(p).EndsWith(".CodeStyle.Fixes.dll", StringComparison.OrdinalIgnoreCase))
                     .Select(p => p!))
        {
            if (!_fixerAssemblies.TryGetValue(path, out var assembly))
                _fixerAssemblies[path] = assembly = Assembly.LoadFrom(path);
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || !typeof(CodeFixProvider).IsAssignableFrom(type)
                    || type.GetCustomAttribute<ExportCodeFixProviderAttribute>() is not { } export
                    || !export.Languages.Contains(project.Language)
                    || type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.EmptyTypes) is not { } constructor)
                    continue;
                var provider = (CodeFixProvider)constructor.Invoke(null);
                if (provider.FixableDiagnosticIds.Contains(rule))
                    providers.Add(provider);
            }
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

/// <summary>A compilation's per-file diagnostic options with the requested rules enabled as warnings. The per-file
/// options carry csharp/.editorconfig's severities and win over the compilation-wide options, so the group's
/// <c>none</c> is replaced here; every other rule and file reads the project's own configuration.</summary>
internal sealed class EnabledRuleTreeOptions(SyntaxTreeOptionsProvider configured, IReadOnlySet<string> rules) : SyntaxTreeOptionsProvider
{
    /// <summary>The project's own answer: enabling a rule does not change which files are generated.</summary>
    public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken) =>
        configured.IsGenerated(tree, cancellationToken);

    /// <summary>Warning for a requested rule in every file; the project's configuration for any other.</summary>
    public override bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
    {
        if (rules.Contains(diagnosticId))
        {
            severity = ReportDiagnostic.Warn;
            return true;
        }
        return configured.TryGetDiagnosticValue(tree, diagnosticId, cancellationToken, out severity);
    }

    /// <summary>Warning for a requested rule; the project's global configuration for any other.</summary>
    public override bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
    {
        if (rules.Contains(diagnosticId))
        {
            severity = ReportDiagnostic.Warn;
            return true;
        }
        return configured.TryGetGlobalDiagnosticValue(diagnosticId, cancellationToken, out severity);
    }
}

/// <summary>The analyzer configuration with <c>dotnet_diagnostic.&lt;rule&gt;.severity = warning</c> for each requested
/// rule. The code-style analyzers read a rule's severity from this configuration to decide whether to analyze at all,
/// so the compilation's options alone would leave them skipping the group's <c>none</c>.</summary>
internal sealed class EnabledRuleConfigOptions(AnalyzerConfigOptionsProvider configured, IReadOnlySet<string> rules) : AnalyzerConfigOptionsProvider
{
    /// <summary>The severity keys of the requested rules, compared the way analyzer configuration keys are.</summary>
    private readonly HashSet<string> _keys = rules.Select(rule => $"dotnet_diagnostic.{rule}.severity").ToHashSet(AnalyzerConfigOptions.KeyComparer);

    /// <summary>The project's global options with the requested rules enabled.</summary>
    public override AnalyzerConfigOptions GlobalOptions => new EnabledRuleConfig(configured.GlobalOptions, _keys);

    /// <summary>A file's options with the requested rules enabled.</summary>
    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new EnabledRuleConfig(configured.GetOptions(tree), _keys);

    /// <summary>An additional file's options with the requested rules enabled.</summary>
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => new EnabledRuleConfig(configured.GetOptions(textFile), _keys);
}

/// <summary>One set of analyzer options answering <c>warning</c> for the requested rules' severity keys.</summary>
internal sealed class EnabledRuleConfig(AnalyzerConfigOptions configured, HashSet<string> keys) : AnalyzerConfigOptions
{
    /// <summary>Warning for a requested rule's severity key; the configured value for any other key.</summary>
    public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
    {
        if (keys.Contains(key))
        {
            value = "warning";
            return true;
        }
        return configured.TryGetValue(key, out value);
    }

    /// <summary>The configured keys and the requested rules' severity keys.</summary>
    public override IEnumerable<string> Keys => configured.Keys.Union(keys, KeyComparer);
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
