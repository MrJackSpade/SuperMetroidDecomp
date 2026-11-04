using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Runs an exhaustive check over every input of a pure function across the machine's cores (#1532). Each outer input runs
/// on one worker with its own state; the sweep stops at the first failure and rethrows the failure of the lowest failing
/// input, unchanged, so the report names that input and its expected and actual values as a sequential sweep would.
/// Only functions that read no shared mutable state and write no static caches may be swept this way.
/// </summary>
internal static class ParallelSweep
{
    /// <summary>One input's failure.</summary>
    /// <param name="Input">The outer input that failed.</param>
    /// <param name="Failure">What it threw.</param>
    private sealed record SweepFailure(int Input, Exception Failure);

    /// <summary>Runs <paramref name="check"/> for every input from <paramref name="fromInclusive"/> below <paramref name="toExclusive"/>.</summary>
    /// <exception cref="Exception">The lowest failing input's own failure, rethrown with its stack.</exception>
    internal static void For(int fromInclusive, int toExclusive, Action<int> check)
    {
        ArgumentNullException.ThrowIfNull(check);
        var failures = new ConcurrentQueue<SweepFailure>();
        Parallel.For(fromInclusive, toExclusive, (input, loop) =>
        {
            try
            {
                check(input);
            }
            catch (Exception failure)
            {
                // Recorded for the report below, made once every worker has stopped.
                failures.Enqueue(new SweepFailure(input, failure));
                loop.Stop();
            }
        });
        if (failures.IsEmpty)
            return;
        SweepFailure first = failures.MinBy(failure => failure.Input)
            ?? throw new InvalidOperationException("A failed sweep recorded no failure.");
        ExceptionDispatchInfo.Capture(first.Failure).Throw();
    }
}
