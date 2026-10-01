using System.Diagnostics;
using System.IO.Compression;
using SuperMetroid.Desktop;

/// <summary>Confirms only the requested automatic console-to-ZIP lifecycle, without gameplay.</summary>
internal static class DesktopSessionLogSmokeTest
{
    internal static void Run(string gameAssemblyPath)
    {
        string root = Directory.CreateTempSubdirectory("SuperMetroid-session-log-").FullName;
        TextWriter originalOutput = Console.Out;
        TextWriter originalError = Console.Error;
        try
        {
            VerifyLifecycle(root);
            VerifyArchiveFailure(root);
            VerifyGameEntry(root, gameAssemblyPath);
        }
        finally
        {
            UnhandledExceptionConsole.SetFatalDiagnosticCheckpoint(null);
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
            // This path was created by this fixture, never player data or a shared workspace.
            Directory.Delete(root, recursive: true);
        }
        Console.WriteLine("PASS automatic session ZIP: stdout/stderr, nested fatal report before acknowledgment, concurrent writes, recoverable/report-worker output, normal close, IO failure preservation, executable-relative startup failure.");
    }

    private static void VerifyLifecycle(string root)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);
        string executable = Path.Combine(root, "fixture-executable");
        string archive;
        string live;
        using (var session = DesktopSessionLog.Start(executable))
        {
            archive = session.ArchivePath;
            live = session.LiveLogPath;
            Require(Path.GetDirectoryName(archive) == Path.Combine(executable, "logs"), "Wrong session directory.");
            Console.WriteLine("ordinary output: 日本語");
            Console.Error.WriteLine("stderr-only marker");
            // AutoFlush preserves even a fragment without a newline if the process is killed.
            Console.Write("unterminated marker");
            using (var liveStream = new FileStream(live, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var liveReader = new StreamReader(liveStream))
                Require(liveReader.ReadToEnd().Contains("unterminated marker"), "Live output was not flushed to disk.");
            Console.WriteLine();
            Parallel.For(0, 64, index =>
            {
                Console.WriteLine($"out-worker-{index:D2}");
                Console.Error.WriteLine($"error-worker-{index:D2}");
            });
            UnhandledExceptionConsole.SetFatalDiagnosticCheckpoint(session.Checkpoint);
            Exception nested = CaptureFailure();
            UnhandledExceptionConsole.ReportAndWait(nested, Console.Error, () =>
            {
                string checkpoint = ReadArchive(archive);
                Require(checkpoint.Contains(nested.ToString()) && checkpoint.Contains("Press Enter to exit."),
                    "Fatal ZIP was not complete before acknowledgment.");
            });
            UnhandledExceptionConsole.SetFatalDiagnosticCheckpoint(null);
            // No network is used. Exercise the real reporter's asynchronous worker output.
            using (var reporter = new GitHubErrorReporter("fixture/diagnostics", new LocalIssueClient()))
            {
                reporter.Report(nested, new GitHubErrorContext("fixture frame boundary", RoomPointer: 0x1234,
                    RoomStatePointer: 0x5678, InputRecordingPath: "fixture.smrec"));
            }
            Console.WriteLine("after checkpoint and reporter shutdown");
        }
        string text = ReadArchive(archive);
        Require(!File.Exists(live), "Successful archive retained unnecessary interim text.");
        Require(text.Contains("Game version:") && text.Contains("Session started (UTC):") &&
            text.Contains("Session ended (UTC):"), "Session metadata was omitted.");
        Require(text.Contains("ordinary output: 日本語") && text.Contains("stderr-only marker") &&
            text.Contains("RECOVERABLE ERROR [SMERR-") && text.Contains("Room $8F:1234") &&
            text.Contains("Created GitHub error [SMERR-") && text.Contains("after checkpoint and reporter shutdown"),
            "Final ZIP omitted console, failure context, or shutdown output.");
        string[] lines = text.Split(Environment.NewLine);
        for (int index = 0; index < 64; index++)
        {
            Require(lines.Count(line => line == $"out-worker-{index:D2}") == 1 &&
                lines.Count(line => line == $"error-worker-{index:D2}") == 1,
                "Concurrent output interleaved or was lost.");
        }
        Require(output.ToString().Contains("ordinary output") && !output.ToString().Contains("stderr-only marker") &&
            error.ToString().Contains("stderr-only marker"), "The tee changed console destinations.");
        Console.WriteLine("restored console marker");
        Require(!ReadArchive(archive).Contains("restored console marker"), "Disposed tee still captured console output.");
    }

    private static void VerifyArchiveFailure(string root)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);
        string live;
        using (var session = DesktopSessionLog.Start(Path.Combine(root, "blocked-archive")))
        {
            live = session.LiveLogPath;
            Console.Error.WriteLine("preserve this diagnostic");
            // An existing directory blocks atomic ZIP publication, faithfully exercising IO failure.
            Directory.CreateDirectory(session.ArchivePath);
            session.Checkpoint();
        }
        Require(File.ReadAllText(live).Contains("preserve this diagnostic"), "Failed archive lost interim text.");
        Require(error.ToString().Contains("SESSION LOG FAILURE:") && error.ToString().Contains("Exception"),
            "Archive failure was silently swallowed or lacked its full exception.");
    }

    private static void VerifyGameEntry(string root, string gameAssemblyPath)
    {
        string executable = Path.GetDirectoryName(Path.GetFullPath(gameAssemblyPath))!;
        string logs = Path.Combine(executable, "logs");
        bool logsExisted = Directory.Exists(logs);
        HashSet<string> before = logsExisted
            ? Directory.GetFiles(logs, "*.zip").ToHashSet(StringComparer.OrdinalIgnoreCase) : [];
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add(Path.GetFullPath(gameAssemblyPath));
        // The existing invalid-option boundary fails before asset installation or any game window.
        start.ArgumentList.Add("--fixture-invalid-session-log-option");
        using Process child = Process.Start(start) ?? throw new InvalidOperationException("Could not launch log fixture.");
        Task<string> output = child.StandardOutput.ReadToEndAsync();
        Task<string> error = child.StandardError.ReadToEndAsync();
        child.StandardInput.Close();
        if (!child.WaitForExit(30000))
        {
            child.Kill(entireProcessTree: true);
            child.WaitForExit();
            throw new InvalidOperationException("Session-log fixture startup boundary did not exit.");
        }
        string stdout = output.GetAwaiter().GetResult();
        string stderr = error.GetAwaiter().GetResult();
        Require(child.ExitCode == 1 && stderr.Contains("FATAL:") && stderr.Contains("Unknown game option."),
            $"Packaged fatal boundary changed: exit={child.ExitCode}; stderr={stderr}");
        string archive = Directory.GetFiles(logs, "*.zip").Single(path => !before.Contains(path));
        string text = ReadArchive(archive);
        Require(text.Contains(stdout.Trim()) && text.Contains(stderr.Trim()) && text.Contains("exit code: 1"),
            "Actual entry-point ZIP omitted stdout, fatal stderr, or exit status.");
        Require(!Directory.Exists(Path.Combine(root, "logs")), "Game logs followed the working directory, not the executable.");
        // Remove only the archive produced by this child. Never package diagnostic output
        // with the build or touch an earlier tester session beside this executable.
        File.Delete(archive);
        if (!logsExisted && !Directory.EnumerateFileSystemEntries(logs).Any())
            Directory.Delete(logs);
    }

    private static string ReadArchive(string path)
    {
        using ZipArchive zip = ZipFile.OpenRead(path);
        Require(zip.Entries.Count == 1 && zip.Entries[0].FullName == "session.log",
            "Diagnostic archive contains unexpected files (saves, assets or recordings).");
        using var reader = new StreamReader(zip.Entries[0].Open());
        return reader.ReadToEnd();
    }

    private static Exception CaptureFailure()
    {
        try { throw new InvalidOperationException("fixture fatal", new InvalidDataException("fixture inner")); }
        catch (Exception error) { return error; }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class LocalIssueClient : IGitHubIssueClient
    {
        public Task<string?> FindByFingerprintAsync(string repository, string fingerprint) => Task.FromResult<string?>(null);
        public Task<string> CreateAsync(string repository, string title, string body) => Task.FromResult("fixture-created-issue");
        public Task CommentAsync(string repository, string issueUrl, string body) => Task.CompletedTask;
    }
}
