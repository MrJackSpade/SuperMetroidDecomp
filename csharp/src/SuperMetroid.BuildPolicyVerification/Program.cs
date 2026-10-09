using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SuperMetroid.BuildPolicyVerification;

/// <summary>
/// Confirms the #142 quality gates fail the ordinary build: a fixture violating each gated
/// diagnostic must fail <c>dotnet build</c> with that diagnostic as an error, a compliant
/// fixture of the same shapes must build, and nothing in maintained source may lower them.
/// </summary>
internal static partial class Program
{
    private static int Main(string[] args)
    {
        try
        {
            if (OperatingSystem.IsWindows())
                NativeConsoleErrors.DisableDialogs();
            string root = args switch
            {
                [] => FindRepositoryRoot(AppContext.BaseDirectory),
                ["--root", string path] => Path.GetFullPath(path),
                _ => throw new ArgumentException("Usage: [--root REPOSITORY]", nameof(args)),
            };
            VerifyPolicyConfiguration(root);
            VerifyNoLocalOverrides(root);
            VerifyViolationsFail(root);
            VerifyCompliantBuilds(root);
            Console.WriteLine(
                $"Build policy: {BuildPolicyDiagnostics.Gated.Length} gated diagnostics fail the normal build " +
                "as errors, the compliant fixture builds cleanly, and no maintained source lowers them.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    private static void VerifyPolicyConfiguration(string root)
    {
        string editorConfig = File.ReadAllText(Path.Combine(root, "csharp", ".editorconfig"));
        foreach (string id in BuildPolicyDiagnostics.Gated)
        {
            MatchCollection settings = Regex.Matches(
                editorConfig, $@"^\s*dotnet_diagnostic\.{id}\.severity\s*=\s*(\w+)\s*$", RegexOptions.Multiline);
            if (settings.Count != 1 || settings[0].Groups[1].Value != "error")
                throw new InvalidDataException($"csharp/.editorconfig must set {id} to error exactly once.");
        }
        if (!Regex.IsMatch(editorConfig, @"^\s*csharp_style_prefer_top_level_statements\s*=\s*false\b", RegexOptions.Multiline))
            throw new InvalidDataException("IDE0211 only reports when top-level statements are not preferred.");

        string props = File.ReadAllText(Path.Combine(root, "csharp", "Directory.Build.props"));
        if (!props.Contains("<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>", StringComparison.Ordinal))
            throw new InvalidDataException("Directory.Build.props must enforce code style in every build.");
    }

    /// <summary>
    /// Rejects every repository mechanism that could silently lower a gated diagnostic for one
    /// project, file, member, or command line.
    /// </summary>
    private static void VerifyNoLocalOverrides(string root)
    {
        string csharp = Path.Combine(root, "csharp");
        string ids = string.Join('|', BuildPolicyDiagnostics.Gated);
        var gated = new Regex($@"\b({ids})\b");
        var findings = new List<string>();
        string sharedEditorConfig = Path.Combine(csharp, ".editorconfig");
        foreach (string file in MaintainedFiles(root))
        {
            string name = Path.GetFileName(file);
            string extension = Path.GetExtension(file);
            if (name == ".editorconfig" || extension == ".globalconfig")
            {
                if (!string.Equals(Path.GetFullPath(file), sharedEditorConfig, StringComparison.OrdinalIgnoreCase) &&
                    gated.IsMatch(File.ReadAllText(file)))
                    findings.Add($"{Relative(root, file)} configures a gated diagnostic outside csharp/.editorconfig");
                continue;
            }
            if (extension is not (".cs" or ".csproj" or ".props" or ".targets" or ".yml" or ".ps1"))
                continue;
            string[] lines = File.ReadAllLines(file);
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (!gated.IsMatch(line))
                    continue;
                if (extension == ".cs" && (line.Contains("#pragma warning disable", StringComparison.Ordinal) ||
                                           line.Contains("SuppressMessage", StringComparison.Ordinal)))
                    findings.Add($"{Relative(root, file)}:{index + 1} suppresses a gated diagnostic");
                else if (extension != ".cs" && (line.Contains("NoWarn", StringComparison.Ordinal) ||
                                                line.Contains("WarningsNotAsErrors", StringComparison.Ordinal) ||
                                                line.Contains("nowarn", StringComparison.Ordinal)))
                    findings.Add($"{Relative(root, file)}:{index + 1} lowers a gated diagnostic");
            }
            if (extension is ".csproj" or ".props" or ".targets" or ".yml" or ".ps1" &&
                Regex.IsMatch(File.ReadAllText(file), @"EnforceCodeStyleInBuild\W+false|RunAnalyzers\w*\W+false", RegexOptions.IgnoreCase))
                findings.Add($"{Relative(root, file)} disables build analysis");
        }
        if (findings.Count != 0)
            throw new InvalidDataException("Gated diagnostics are lowered locally:" + Environment.NewLine +
                string.Join(Environment.NewLine, findings));
    }

    private static void VerifyViolationsFail(string root)
    {
        BuildResult result = Build(root, "Violations");
        if (result.ExitCode == 0)
            throw new InvalidOperationException("The violating fixture built successfully.");
        var missing = BuildPolicyDiagnostics.Gated
            .Where(id => !Regex.IsMatch(result.Output, $@"\berror {id}\b"))
            .ToArray();
        if (missing.Length != 0)
            throw new InvalidOperationException(
                $"The violating fixture did not report {string.Join(", ", missing)} as build errors:" +
                Environment.NewLine + result.Output);
        Match unexpected = Regex.Match(result.Output, @"\berror (?!(" + string.Join('|', BuildPolicyDiagnostics.Gated) + @")\b)\w+");
        if (unexpected.Success)
            throw new InvalidOperationException(
                $"The violating fixture failed for an unrelated reason ({unexpected.Value}):" +
                Environment.NewLine + result.Output);
    }

    private static void VerifyCompliantBuilds(string root)
    {
        BuildResult result = Build(root, "Compliant");
        if (result.ExitCode != 0)
            throw new InvalidOperationException("The compliant fixture failed to build:" + Environment.NewLine + result.Output);
        string[] reported = BuildPolicyDiagnostics.Gated
            .Where(id => Regex.IsMatch(result.Output, $@"\b{id}\b"))
            .ToArray();
        if (reported.Length != 0)
            throw new InvalidOperationException(
                $"The compliant fixture reported {string.Join(", ", reported)}:" + Environment.NewLine + result.Output);
    }

    /// <summary>Runs the plain build a developer runs; the fixture inherits only repository settings.</summary>
    private static BuildResult Build(string root, string fixture)
    {
        string project = Path.Combine(root, "csharp", "test-fixtures", "build-policy", fixture,
            $"BuildPolicy{fixture}.csproj");
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        // --no-incremental keeps a previous successful build from hiding analyzer output.
        foreach (string argument in new[] { "build", project, "-nologo", "--no-incremental" })
            start.ArgumentList.Add(argument);
        using Process process = Process.Start(start)
            ?? throw new InvalidOperationException("dotnet could not be started.");
        Task<string> error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return new BuildResult(process.ExitCode, output + error.Result);
    }

    private static IEnumerable<string> MaintainedFiles(string root)
    {
        var pending = new Stack<string>([Path.Combine(root, "csharp"), Path.Combine(root, ".github")]);
        while (pending.TryPop(out string? directory))
        {
            foreach (string child in Directory.EnumerateDirectories(directory))
            {
                string name = Path.GetFileName(child);
                if (name is "bin" or "obj" or "test-temp" or "build-policy" || name.StartsWith('.') && name != ".github")
                    continue;
                pending.Push(child);
            }
            foreach (string file in Directory.EnumerateFiles(directory))
                yield return file;
        }
    }

    private static string FindRepositoryRoot(string start)
    {
        for (DirectoryInfo? directory = new(start); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "csharp", "SuperMetroid.Full.slnx")))
                return directory.FullName;
        }
        throw new DirectoryNotFoundException($"No repository root above {start}.");
    }

    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private sealed record BuildResult(int ExitCode, string Output);
}

/// <summary>The diagnostics #142 makes build-breaking in every maintained C# project.</summary>
internal static class BuildPolicyDiagnostics
{
    /// <summary>The gated IDs, each with a violating and a compliant fixture file.</summary>
    internal static readonly string[] Gated =
    [
        "IDE0211", "CA1512", "CA1822", "CA1859", "CA1826",
        "IDE0032", "IDE0060", "IDE0078", "SYSLIB1054", "IDE0330",
    ];
}
