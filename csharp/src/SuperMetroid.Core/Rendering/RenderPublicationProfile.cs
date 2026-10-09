using System.Diagnostics;

namespace SuperMetroid.Core.Rendering;

/// <summary>Opt-in owner-thread diagnostic. No profiler is stored in game or saved state.</summary>
internal sealed class RenderPublicationProfile : IDisposable
{
    /// <summary>Profiler scope currently enabled on this managed thread, if any.</summary>
    [ThreadStatic] private static RenderPublicationProfile? active;
    /// <summary>Thread that opened this diagnostic scope.</summary>
    private readonly int owner = Environment.CurrentManagedThreadId;
    /// <summary>Elapsed stopwatch ticks accumulated by completed samples.</summary>
    internal long Ticks { get; private set; }
    /// <summary>Managed bytes allocated on the owner thread during completed samples.</summary>
    internal long AllocatedBytes { get; private set; }
    /// <summary>Number of measured publication scopes completed.</summary>
    internal int Publications { get; private set; }

    /// <summary>Begins a sample for the active thread-local profiler, or a no-op sample when profiling is disabled.</summary>
    internal static Sample Measure() => active is { } profile ? new(profile) : default;

    /// <summary>One measured owner-thread publication interval, or a no-op when profiling is disabled.</summary>
    internal readonly struct Sample : IDisposable
    {
        /// <summary>Profiler receiving this sample; null represents the disabled no-op case.</summary>
        private readonly RenderPublicationProfile? profile;
        /// <summary>Stopwatch timestamp and owner-thread allocation counter captured at sample start.</summary>
        private readonly long start, allocation;
        /// <summary>Starts timing and allocation accounting for one publication scope.</summary>
        internal Sample(RenderPublicationProfile profile)
        {
            this.profile = profile;
            allocation = GC.GetAllocatedBytesForCurrentThread();
            start = Stopwatch.GetTimestamp();
        }
        /// <summary>Accumulates elapsed ticks, allocated bytes, and one completed publication.</summary>
        public void Dispose()
        {
            if (profile is null) return;
            profile.VerifyOwner();
            profile.Ticks += Stopwatch.GetTimestamp() - start;
            profile.AllocatedBytes += GC.GetAllocatedBytesForCurrentThread() - allocation;
            profile.Publications++;
        }
    }

    /// <summary>Ensures this profiler is accessed only by its active creating thread.</summary>
    private void VerifyOwner()
    {
        if (Environment.CurrentManagedThreadId != owner || !ReferenceEquals(active, this))
            throw new InvalidOperationException("Publication profiler used outside its owner-thread scope.");
    }

    /// <summary>Ends this profiler scope and clears the thread-local active instance.</summary>
    public void Dispose() { VerifyOwner(); active = null; }
}
