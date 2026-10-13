using System.Diagnostics;

namespace SuperMetroid.Core.Rendering;

/// <summary>Opt-in owner-thread diagnostic. No profiler is stored in game or saved state.</summary>
internal sealed class RenderPublicationProfile : IDisposable
{
    /// <summary>The measurement scope currently active on this thread.</summary>
    [ThreadStatic] private static RenderPublicationProfile? active;
    /// <summary>The thread on which this scope was created and must be disposed.</summary>
    private readonly int owner = Environment.CurrentManagedThreadId;
    /// <summary>Elapsed timestamp ticks accumulated by completed publication samples.</summary>
    internal long Ticks { get; set; }
    /// <summary>Current-thread allocations accumulated while publication samples were open.</summary>
    internal long AllocatedBytes { get; set; }
    /// <summary>Number of completed publication samples.</summary>
    internal int Publications { get; set; }

    /// <summary>Starts a sample only when a publication profile is active on this thread.</summary>
    internal static Sample Measure() => active is { } profile ? new(profile) : default;

    /// <summary>Measures the wall-clock ticks and thread allocations within one publication operation.</summary>
    internal readonly struct Sample : IDisposable
    {
        /// <summary>The active scope to credit when this sample ends.</summary>
        private readonly RenderPublicationProfile? profile;
        /// <summary>Timestamp and allocation baseline captured at the start of the sample.</summary>
        private readonly long start, allocation;
        /// <summary>Captures the current-thread counters for an active publication scope.</summary>
        internal Sample(RenderPublicationProfile profile)
        {
            this.profile = profile;
            allocation = GC.GetAllocatedBytesForCurrentThread();
            start = Stopwatch.GetTimestamp();
        }
        /// <summary>Credits elapsed ticks and allocations to the owning scope once the publication completes.</summary>
        public void Dispose()
        {
            if (profile is null) return;
            profile.VerifyOwner();
            profile.Ticks += Stopwatch.GetTimestamp() - start;
            profile.AllocatedBytes += GC.GetAllocatedBytesForCurrentThread() - allocation;
            profile.Publications++;
        }
    }

    /// <summary>Rejects samples disposed on a different thread or outside this profile's active scope.</summary>
    internal void VerifyOwner()
    {
        if (Environment.CurrentManagedThreadId != owner || !ReferenceEquals(active, this))
            throw new InvalidOperationException("Publication profiler used outside its owner-thread scope.");
    }

    /// <summary>Ends the owner-thread profiling scope and clears its thread-local registration.</summary>
    public void Dispose() { VerifyOwner(); active = null; }
}
