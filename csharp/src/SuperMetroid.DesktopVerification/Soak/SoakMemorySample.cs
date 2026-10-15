using System.Diagnostics;

namespace SuperMetroid.DesktopVerification.Soak;

/// <summary>Whole-process sampled memory, including runtime/driver caches; no forced collection.</summary>
internal readonly record struct SoakMemorySample(double Seconds, long PrivateBytes, long WorkingSetBytes, long ManagedBytes)
{
    /// <summary>Refreshes process counters and samples private, working-set, and managed bytes without forcing collection.</summary>
    internal static SoakMemorySample Capture(Process process, double seconds)
    {
        process.Refresh();
        return new(seconds, process.PrivateMemorySize64, process.WorkingSet64, GC.GetTotalMemory(forceFullCollection: false));
    }
}
