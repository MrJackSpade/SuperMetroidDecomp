using System.Diagnostics;

internal static partial class Program
{
    /// <summary>Suites slower than this are logged as needing to be broken down. Reporting only.</summary>
    private static readonly TimeSpan SuiteBreakdownThreshold = TimeSpan.FromSeconds(30);

    /// <summary>A flag whose peak working set, or a suite whose allocation, exceeds this is marked over budget.</summary>
    private const long MemoryBudgetBytes = 1L << 30;

    private static int suiteDepth;
    // Elapsed time of suites nested directly inside each open suite, so a bundle's own work
    // (its self time) can be told apart from the sum of the suites it runs.
    private static readonly Stack<TimeSpan> nestedSuiteTime = new();
    private static readonly Stack<long> nestedSuiteAllocation = new();

    /// <summary>
    /// Runs one named verification suite and logs its elapsed time, nested under any enclosing
    /// suite. The time is logged even when the suite fails; the failure still propagates. The
    /// breakdown marker applies to self time: a bundle made of fast suites is already broken down.
    /// </summary>
    private static void Suite(string name, Action body)
    {
        var watch = Stopwatch.StartNew();
        long allocatedBefore = GC.GetTotalAllocatedBytes();
        suiteDepth++;
        nestedSuiteTime.Push(TimeSpan.Zero);
        nestedSuiteAllocation.Push(0);
        try
        {
            body();
        }
        finally
        {
            suiteDepth--;
            watch.Stop();
            TimeSpan self = watch.Elapsed - nestedSuiteTime.Pop();
            if (nestedSuiteTime.Count > 0) nestedSuiteTime.Push(nestedSuiteTime.Pop() + watch.Elapsed);
            long allocated = GC.GetTotalAllocatedBytes() - allocatedBefore;
            long selfAllocated = allocated - nestedSuiteAllocation.Pop();
            if (nestedSuiteAllocation.Count > 0) nestedSuiteAllocation.Push(nestedSuiteAllocation.Pop() + allocated);
            string marker = (self > SuiteBreakdownThreshold ? "  [over 30 s: break down]" : "") +
                (selfAllocated > MemoryBudgetBytes ? "  [over 1 GB allocated: reduce]" : "");
            double allocatedMegabytes = allocated / (1024.0 * 1024.0);
            Console.WriteLine($"TIME {new string(' ', 2 * suiteDepth)}{name}: {watch.Elapsed.TotalSeconds:0.00} s (self {self.TotalSeconds:0.00} s), " +
                $"{allocatedMegabytes:0.0} MB allocated (self {selfAllocated / (1024.0 * 1024.0):0.0} MB){marker}");
            // Suites directly under a flag or its bundle start from their own live data only:
            // collect and return the previous suite's garbage instead of letting the heap grow.
            if (suiteDepth <= 1) GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        }
    }

    /// <summary>The flag's peak working set, with the over-budget marker.</summary>
    private static string PeakMemoryReport()
    {
        long peak = Process.GetCurrentProcess().PeakWorkingSet64;
        return $"peak {peak / (1024.0 * 1024.0):0} MB" + (peak > MemoryBudgetBytes ? "  [over 1 GB: reduce]" : "");
    }

    private static readonly Stopwatch checkpointWatch = Stopwatch.StartNew();
    private static long checkpointAllocated;

    /// <summary>
    /// Logs the time and allocations since the previous checkpoint, to locate the slow block
    /// inside a suite that is being broken down.
    /// </summary>
    private static void Checkpoint(string label)
    {
        long allocated = GC.GetTotalAllocatedBytes();
        Console.WriteLine($"TIME {new string(' ', 2 * suiteDepth)}. {label}: {checkpointWatch.Elapsed.TotalSeconds:0.00} s, " +
            $"{(allocated - checkpointAllocated) / (1024.0 * 1024.0):0.0} MB allocated");
        checkpointWatch.Restart();
        checkpointAllocated = allocated;
    }
}
