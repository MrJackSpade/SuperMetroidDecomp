using System.Diagnostics;

/// <summary>Whole-process sampled memory, including runtime/driver caches; no forced collection.</summary>
/// <param name="Seconds">Elapsed soak time when the counters were sampled.</param>
/// <param name="PrivateBytes">The process's private memory size in bytes.</param>
/// <param name="WorkingSetBytes">The process's current working-set size in bytes.</param>
/// <param name="ManagedBytes">Managed heap bytes reported without forcing a full collection.</param>
internal readonly record struct SoakMemorySample(double Seconds, long PrivateBytes, long WorkingSetBytes, long ManagedBytes)
{
    /// <summary>Refreshes the process counters and captures them with the current managed-heap estimate.</summary>
    /// <param name="process">The process whose private-memory and working-set counters are sampled.</param>
    /// <param name="seconds">Elapsed soak time to associate with the sample.</param>
    /// <returns>A snapshot of process and managed memory usage at that elapsed time.</returns>
    internal static SoakMemorySample Capture(Process process, double seconds)
    {
        process.Refresh();
        return new(seconds, process.PrivateMemorySize64, process.WorkingSet64, GC.GetTotalMemory(forceFullCollection: false));
    }
}
