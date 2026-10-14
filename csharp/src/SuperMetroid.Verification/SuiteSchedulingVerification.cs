using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using static FullRunSuitesVerification;
using static SuiteTimingVerification;

/// <summary>Runs registered verification suites serially or through isolated worker processes.</summary>
internal static class SuiteSchedulingVerification
{
    /// <summary>
    /// Previous run's elapsed time per suite, read to start the longest suites first. Kept in the
    /// ignored <see cref="RepositoryInstallation.CacheRoot"/>, so a missing file only means registry
    /// order for that run.
    /// </summary>
    private const string SuiteDurationsPath = RepositoryInstallation.CacheRoot + "/verification-suite-durations.json";

    /// <summary>One suite's result, sent by a worker to the coordinator as a single JSON line.</summary>
    internal sealed record SuiteReport(int Index, bool Passed, double Seconds, string Output, string Error, long PeakWorkingSetBytes);

    /// <summary>Runs every suite in this process, in registry order (<c>--serial</c>, <c>--render-contract</c>).</summary>
    internal static void RunSuitesSerially(RegisteredSuite[] suites)
    {
        foreach (RegisteredSuite suite in suites)
            Suite(suite.Name, suite.Body);
    }

    /// <summary>
    /// The full run: one worker process per logical processor, each running suites handed to it from
    /// a shared queue, longest previous duration first. Each suite runs exactly once. Its console
    /// output is printed as one block when it finishes, and its failure is recorded exactly as
    /// <see cref="Suite"/> records it in a serial run. Separate processes keep every suite's static
    /// and runtime state isolated from suites running at the same time.
    /// </summary>
    internal static void RunSuitesInParallel(RegisteredSuite[] suites)
    {
        // Workers only read the shared installation; extraction must finish before any of them opens it.
        _ = RepositoryInstallation.Installation;
        string[] keys = SuiteDurationKeys(suites);
        Dictionary<string, double> previous = ReadSuiteDurations();
        var queue = new Queue<int>(Enumerable.Range(0, suites.Length)
            .OrderByDescending(index => previous.GetValueOrDefault(keys[index], double.MaxValue)));
        var durations = new double[suites.Length];
        var outputLock = new object();
        long peakWorker = 0;
        int workerCount = Math.Min(Environment.ProcessorCount, suites.Length);
        Console.WriteLine($"Running {suites.Length} suites across {workerCount} worker processes.");

        void Record(RegisteredSuite suite, SuiteReport report)
        {
            lock (outputLock)
            {
                Console.Out.Write(report.Output);
                Console.Out.Flush();
                Console.Error.Write(report.Error);
                Console.Error.Flush();
                durations[report.Index] = report.Seconds;
                peakWorker = Math.Max(peakWorker, report.PeakWorkingSetBytes);
                if (!report.Passed) failedSuites.Add(suite.Name);
            }
        }

        bool TryTake(out int index)
        {
            lock (queue) return queue.TryDequeue(out index);
        }

        var faults = new List<System.Runtime.ExceptionServices.ExceptionDispatchInfo>();
        var threads = Enumerable.Range(0, workerCount).Select(_ => new Thread(() =>
        {
            SuiteWorkerProcess? worker = null;
            try
            {
                while (TryTake(out int index))
                {
                    worker ??= SuiteWorkerProcess.Start();
                    if (worker.Run(index) is { } report)
                    {
                        Record(suites[index], report);
                        continue;
                    }
                    // The worker process ended mid-suite (a crash the in-process suite boundary
                    // cannot catch). That suite failed; later suites get a fresh worker.
                    Record(suites[index], new SuiteReport(index, false, 0, "",
                        $"FAIL {suites[index].Name}{Environment.NewLine}{worker.DescribeExit()}{Environment.NewLine}", 0));
                    worker.Dispose();
                    worker = null;
                }
            }
            catch (Exception exception)
            {
                // A broken worker protocol ends the run; it is rethrown on the coordinator thread so the
                // process boundary reports it instead of the runtime's unhandled-exception path.
                lock (faults) faults.Add(System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception));
            }
            finally
            {
                worker?.Dispose();
            }
        })).ToArray();
        foreach (Thread thread in threads) thread.Start();
        foreach (Thread thread in threads) thread.Join();
        if (faults.Count == 1) faults[0].Throw();
        if (faults.Count > 1) throw new AggregateException(faults.Select(fault => fault.SourceException));

        WriteSuiteDurations(keys, durations);
        Console.WriteLine($"TIME workers: {workerCount} processes, peak worker {peakWorker / (1024.0 * 1024.0):0} MB" +
            (peakWorker > MemoryBudgetBytes ? "  [over 1 GB: reduce]" : ""));
    }

    /// <summary>
    /// Worker side of <see cref="RunSuitesInParallel"/> (<c>--suite-worker</c>): reads suite indices
    /// from standard input until it closes and answers each with one <see cref="SuiteReport"/> line.
    /// Console output is captured per suite so the coordinator can print it as one block.
    /// </summary>
    internal static int RunSuiteWorker(RegisteredSuite[] suites)
    {
        TextWriter protocol = Console.Out;
        TextWriter processError = Console.Error;
        var output = new StringWriter(CultureInfo.InvariantCulture);
        var error = new StringWriter(CultureInfo.InvariantCulture);
        Console.SetOut(TextWriter.Synchronized(output));
        Console.SetError(TextWriter.Synchronized(error));
        try
        {
            RunWorkerSuites(suites, protocol, output, error);
        }
        finally
        {
            // A failure outside a suite reaches Main's boundary on the real streams.
            Console.SetOut(protocol);
            Console.SetError(processError);
        }
        // The coordinator records each failure from its report; the worker itself always succeeds.
        failedSuites.Clear();
        return 0;
    }

    /// <summary>Executes requested suite indices and emits one JSON result per input line.</summary>
    private static void RunWorkerSuites(RegisteredSuite[] suites, TextWriter protocol, StringWriter output, StringWriter error)
    {
        while (Console.In.ReadLine() is { } line)
        {
            int index = int.Parse(line, NumberStyles.None, CultureInfo.InvariantCulture);
            int failuresBefore = failedSuites.Count;
            var watch = Stopwatch.StartNew();
            Suite(suites[index].Name, suites[index].Body);
            watch.Stop();
            Console.Out.Flush();
            Console.Error.Flush();
            var report = new SuiteReport(index, failedSuites.Count == failuresBefore, watch.Elapsed.TotalSeconds,
                output.ToString(), error.ToString(), Process.GetCurrentProcess().PeakWorkingSet64);
            output.GetStringBuilder().Clear();
            error.GetStringBuilder().Clear();
            protocol.WriteLine(JsonSerializer.Serialize(report));
            protocol.Flush();
        }
    }

    /// <summary>Stable timing keys: the suite name, numbered when the registry repeats it with other arguments.</summary>
    private static string[] SuiteDurationKeys(RegisteredSuite[] suites)
    {
        var seen = new Dictionary<string, int>();
        return suites.Select(suite =>
        {
            int occurrence = seen[suite.Name] = seen.GetValueOrDefault(suite.Name) + 1;
            return occurrence == 1 ? suite.Name : $"{suite.Name}#{occurrence}";
        }).ToArray();
    }

    /// <summary>Loads prior suite timing hints used only to order the worker queue.</summary>
    private static Dictionary<string, double> ReadSuiteDurations() =>
        File.Exists(SuiteDurationsPath)
            ? JsonSerializer.Deserialize<Dictionary<string, double>>(File.ReadAllText(SuiteDurationsPath))
                ?? throw new InvalidDataException($"{SuiteDurationsPath} is empty.")
            : [];

    /// <summary>Persists per-suite elapsed time hints for the next parallel run.</summary>
    private static void WriteSuiteDurations(string[] keys, double[] durations)
    {
        Directory.CreateDirectory((Path.GetDirectoryName(Path.GetFullPath(SuiteDurationsPath)) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(Path.GetFullPath(SuiteDurationsPath)).")));
        File.WriteAllText(SuiteDurationsPath, JsonSerializer.Serialize(
            keys.Zip(durations).ToDictionary(pair => pair.First, pair => pair.Second),
            new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>A <c>--suite-worker</c> child process of this verifier, driven over its standard streams.</summary>
    internal sealed class SuiteWorkerProcess : IDisposable
    {
        /// <summary>Child verifier process receiving suite indices over standard input.</summary>
        private readonly Process process;
        /// <summary>Runtime error text emitted outside the worker's per-suite capture.</summary>
        private readonly System.Text.StringBuilder unexpectedError = new();

        /// <summary>Captures unexpected child standard error without mixing it into suite protocol lines.</summary>
        private SuiteWorkerProcess(Process process)
        {
            this.process = process;
            // Suite output is captured inside the worker; anything here is the runtime's own crash report.
            process.ErrorDataReceived += (_, data) =>
            {
                if (data.Data is null) return;
                lock (unexpectedError) unexpectedError.AppendLine(data.Data);
            };
            process.BeginErrorReadLine();
        }

        /// <summary>Launches this verifier as a worker with redirected protocol streams.</summary>
        internal static SuiteWorkerProcess Start()
        {
            string host = Environment.ProcessPath ?? throw new InvalidOperationException("The verifier's host executable path is unknown.");
            var start = new ProcessStartInfo(host)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
            };
            // Run through dotnet: pass the verifier assembly; through the apphost: the executable is the verifier.
            if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
                start.ArgumentList.Add(typeof(Program).Assembly.Location);
            start.ArgumentList.Add("--suite-worker");
            return new SuiteWorkerProcess(Process.Start(start)
                ?? throw new InvalidOperationException($"Could not start verifier worker {host}."));
        }

        /// <summary>Runs one suite; null when the worker process ended before reporting it.</summary>
        internal SuiteReport? Run(int index)
        {
            process.StandardInput.WriteLine(index.ToString(CultureInfo.InvariantCulture));
            process.StandardInput.Flush();
            string? line = process.StandardOutput.ReadLine();
            if (line is null) return null;
            SuiteReport report = JsonSerializer.Deserialize<SuiteReport>(line)
                ?? throw new InvalidDataException($"Verifier worker sent an empty report for suite {index}.");
            if (report.Index != index)
                throw new InvalidDataException($"Verifier worker answered suite {report.Index} when asked for suite {index}.");
            return report;
        }

        /// <summary>Waits for an unreported worker exit and returns its code and unexpected error text.</summary>
        internal string DescribeExit()
        {
            process.WaitForExit();
            lock (unexpectedError)
                return $"Verifier worker process {process.Id} exited with code {process.ExitCode} before reporting the suite." +
                    (unexpectedError.Length == 0 ? "" : Environment.NewLine + unexpectedError);
        }

        /// <summary>Closes worker input, waits for a live child, and releases its process handle.</summary>
        public void Dispose()
        {
            if (!process.HasExited)
            {
                process.StandardInput.Close();
                process.WaitForExit();
            }
            process.Dispose();
        }
    }

    /// <summary>Runs the render-contract suites in registry order.</summary>
    [VerificationCommand("--render-contract")]
    internal static int RenderContractCommand()
    {
        Console.WriteLine("Verifying translated Super Metroid routines...");
        RunSuitesSerially(RenderContractSuites());
        if (failedSuites.Count == 0)
            Console.WriteLine($"Portable render contract passed on {RuntimeInformation.OSDescription}; {RuntimeInformation.FrameworkDescription}.");
        return 0;
    }

    /// <summary>The no-argument run: every registered suite, spread across worker processes.</summary>
    internal static int RunFullSuite() => RunFullSuite(serial: false);

    /// <summary>Every registered suite in this one process, in registration order.</summary>
    [VerificationCommand("--serial")]
    internal static int SerialFullSuiteCommand() => RunFullSuite(serial: true);

    /// <summary>The worker process <see cref="RunSuitesInParallel"/> starts.</summary>
    [VerificationCommand("--suite-worker")]
    internal static int SuiteWorkerCommand() => RunSuiteWorker(FullRunSuites());

    /// <summary>Runs every registered suite with the requested serial or worker-process scheduler.</summary>
    private static int RunFullSuite(bool serial)
    {
        Console.WriteLine("Verifying translated Super Metroid routines...");
        if (serial)
            RunSuitesSerially(FullRunSuites());
        else
            RunSuitesInParallel(FullRunSuites());
        if (failedSuites.Count == 0)
            Console.WriteLine("All bank $80 verification checks passed.");
        return 0;
    }
}
