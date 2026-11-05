using System.Diagnostics;

namespace SuperMetroid.Verification.Harness;

/// <summary>Measures named verification suites, reports failures, and tracks time and allocation budgets.</summary>
internal static class SuiteTimingVerification
{
    /// <summary>
    /// The longest an outermost suite may run in a solo run; one that runs longer fails, except the movie replays (#1532).
    /// </summary>
    private static readonly TimeSpan SuiteTimeLimit = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Whether an outermost suite over <see cref="SuiteTimeLimit"/> fails. True for a solo run; a parallel worker only
    /// reports its suites' times, which there measure contention with the other workers (owner ruling on #1532).
    /// </summary>
    private static bool suiteTimeLimitApplies = true;

    /// <summary>Makes this process report its suites' times without failing a suite over the limit, as a parallel worker does.</summary>
    internal static void ReportSuiteTimesOnly() => suiteTimeLimitApplies = false;

    /// <summary>A flag whose peak working set, or a suite whose allocation, exceeds this is marked over budget.</summary>
    internal const long MemoryBudgetBytes = 1L << 30;

    /// <summary>Current nesting level used for failure handling and indented timing output.</summary>
    private static int suiteDepth;
    /// <summary>Elapsed child-suite time subtracted from each enclosing suite's self time.</summary>
    private static readonly Stack<TimeSpan> nestedSuiteTime = new();
    /// <summary>Child-suite allocations subtracted from each enclosing suite's self allocation.</summary>
    private static readonly Stack<long> nestedSuiteAllocation = new();

    /// <summary>Whether a suite is running, so a shared fixture's load can be timed under it.</summary>
    internal static bool InSuite => suiteDepth > 0;

    /// <summary>Total time spent loading process-wide shared fixtures, which no suite is charged for.</summary>
    private static TimeSpan sharedFixtureLoadTime;
    /// <summary>Shared fixture loads in progress; a fixture another fixture loads is counted once, in the outer load.</summary>
    private static int sharedFixtureDepth;

    /// <summary>
    /// Loads a process-wide shared fixture inside the running suite, timed as its own entry. Whichever suite first needs
    /// the fixture pays its load, so the load does not count toward that suite's time limit.
    /// </summary>
    internal static void SharedFixtureLoad(string name, Action load)
    {
        var watch = Stopwatch.StartNew();
        sharedFixtureDepth++;
        try
        {
            Suite(name, load);
        }
        finally
        {
            sharedFixtureDepth--;
            if (sharedFixtureDepth == 0)
                sharedFixtureLoadTime += watch.Elapsed;
        }
    }

    /// <summary>Outermost suites that failed during this run, in the order they failed.</summary>
    internal static readonly List<string> failedSuites = [];

    /// <summary>
    /// Runs one named verification suite and logs its elapsed time, nested under any enclosing
    /// suite. The time is logged even when the suite fails. A failure inside a nested suite
    /// propagates to its outermost suite, whose remaining steps depend on it; the outermost
    /// suite then writes the complete exception and records the failure so the run continues
    /// with the next suite. <c>Main</c> reports every recorded failure and exits nonzero.
    /// In a solo run, an outermost suite other than a movie replay that runs longer than <see cref="SuiteTimeLimit"/> fails.
    /// </summary>
    internal static void Suite(string name, Action body)
    {
        SuperMetroid.TestSupport.RunTimeLimit.VerificationRunTimeLimit.Activity = name;
        var watch = Stopwatch.StartNew();
        TimeSpan fixtureLoadsBefore = sharedFixtureLoadTime;
        long allocatedBefore = GC.GetTotalAllocatedBytes();
        bool outermost = suiteDepth == 0;
        bool failed = false;
        suiteDepth++;
        nestedSuiteTime.Push(TimeSpan.Zero);
        nestedSuiteAllocation.Push(0);
        try
        {
            body();
        }
        catch (Exception exception) when (outermost)
        {
            failed = true;
            failedSuites.Add(name);
            Console.Error.WriteLine($"FAIL {name}");
            Console.Error.WriteLine(exception);
        }
        finally
        {
            suiteDepth--;
            long allocated = GC.GetTotalAllocatedBytes() - allocatedBefore;
            // Each outermost suite starts from its own live data only: it collects after itself, returning its garbage
            // instead of letting the heap grow. A nested suite collects only when it allocated past the budget, and
            // inside its own time, so the collection is charged to the suite (or shared fixture load) whose garbage
            // it returns. A full compacting collection walks every fixture the process holds (#1532): one after each
            // small nested check made definition suites spend nearly all their time in the collector.
            if (suiteDepth > 0 && allocated > MemoryBudgetBytes)
                Collect();
            watch.Stop();
            TimeSpan charged = watch.Elapsed - (sharedFixtureLoadTime - fixtureLoadsBefore);
            if (suiteTimeLimitApplies && outermost && !failed && charged > SuiteTimeLimit && !FullRunSuitesVerification.MovieReplaySuites.Contains(name))
            {
                failedSuites.Add(name);
                Console.Error.WriteLine($"FAIL {name}: ran {charged.TotalSeconds:0.00} s besides shared fixture loads, over the {SuiteTimeLimit.TotalSeconds:0} s suite limit");
            }
            TimeSpan self = watch.Elapsed - nestedSuiteTime.Pop();
            if (nestedSuiteTime.Count > 0) nestedSuiteTime.Push(nestedSuiteTime.Pop() + watch.Elapsed);
            long selfAllocated = allocated - nestedSuiteAllocation.Pop();
            if (nestedSuiteAllocation.Count > 0) nestedSuiteAllocation.Push(nestedSuiteAllocation.Pop() + allocated);
            string marker = (selfAllocated > MemoryBudgetBytes ? "  [over 1 GB allocated: reduce]" : "");
            double allocatedMegabytes = allocated / (1024.0 * 1024.0);
            Console.WriteLine($"TIME {new string(' ', 2 * suiteDepth)}{name}: {watch.Elapsed.TotalSeconds:0.00} s (self {self.TotalSeconds:0.00} s), " +
                $"{allocatedMegabytes:0.0} MB allocated (self {selfAllocated / (1024.0 * 1024.0):0.0} MB){marker}");
            if (suiteDepth == 0)
                Collect();
        }
    }

    /// <summary>Runs a full blocking compacting collection that returns freed memory to the system.</summary>
    private static void Collect() => GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);

    /// <summary>The flag's peak working set, with the over-budget marker.</summary>
    internal static string PeakMemoryReport()
    {
        long peak = Process.GetCurrentProcess().PeakWorkingSet64;
        return $"peak {peak / (1024.0 * 1024.0):0} MB" + (peak > MemoryBudgetBytes ? "  [over 1 GB: reduce]" : "");
    }
}
