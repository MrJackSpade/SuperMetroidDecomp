using System.Diagnostics;

namespace SuperMetroid.Core.Rendering;

/// <summary>Opt-in owner-thread diagnostic. No profiler is stored in game or saved state.</summary>
internal sealed class RenderPublicationProfile : IDisposable
{
    [ThreadStatic] private static RenderPublicationProfile? active;
    private readonly int owner = Environment.CurrentManagedThreadId;
    internal long Ticks { get; private set; }
    internal long AllocatedBytes { get; private set; }
    internal int Publications { get; private set; }

    internal RenderPublicationProfile()
    {
        if (active is not null) throw new InvalidOperationException("Nested render publication profiling.");
        active = this;
    }

    internal void Reset()
    {
        VerifyOwner(); Ticks = 0; AllocatedBytes = 0; Publications = 0;
    }

    internal static Sample Measure() => active is { } profile ? new(profile) : default;

    internal readonly struct Sample : IDisposable
    {
        private readonly RenderPublicationProfile? profile;
        private readonly long start, allocation;
        internal Sample(RenderPublicationProfile profile)
        {
            this.profile = profile;
            allocation = GC.GetAllocatedBytesForCurrentThread();
            start = Stopwatch.GetTimestamp();
        }
        public void Dispose()
        {
            if (profile is null) return;
            profile.VerifyOwner();
            profile.Ticks += Stopwatch.GetTimestamp() - start;
            profile.AllocatedBytes += GC.GetAllocatedBytesForCurrentThread() - allocation;
            profile.Publications++;
        }
    }

    private void VerifyOwner()
    {
        if (Environment.CurrentManagedThreadId != owner || !ReferenceEquals(active, this))
            throw new InvalidOperationException("Publication profiler used outside its owner-thread scope.");
    }

    public void Dispose() { VerifyOwner(); active = null; }
}
