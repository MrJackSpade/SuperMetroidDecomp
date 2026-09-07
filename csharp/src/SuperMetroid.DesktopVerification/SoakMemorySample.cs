using System.Diagnostics;

/// <summary>Whole-process sampled memory, including runtime/driver caches; no forced collection.</summary>
internal readonly record struct SoakMemorySample(double Seconds, long PrivateBytes, long WorkingSetBytes, long ManagedBytes)
{
    internal static SoakMemorySample Capture(Process process, double seconds)
    {
        process.Refresh();
        return new(seconds, process.PrivateMemorySize64, process.WorkingSet64, GC.GetTotalMemory(forceFullCollection: false));
    }
}
