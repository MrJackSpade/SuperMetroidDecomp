using System.Diagnostics;

namespace SuperMetroid.Core.Rendering;

/// <summary>Opt-in owner-thread diagnostic. No profiler is stored in game or saved state.</summary>
internal sealed class RenderPublicationProfile : IDisposable
{
    /// <summary>Stores active.</summary>
    [ThreadStatic] private static RenderPublicationProfile? active;
    /// <summary>Stores owner.</summary>
    private readonly int owner = Environment.CurrentManagedThreadId;
    /// <summary>Gets or sets ticks.</summary>
    internal long Ticks { get; private set; }
    /// <summary>Gets or sets allocated bytes.</summary>
    internal long AllocatedBytes { get; private set; }
    /// <summary>Gets or sets publications.</summary>
    internal int Publications { get; private set; }

    /// <summary>Measure.</summary>
    internal static Sample Measure() => active is { } profile ? new(profile) : default;

    /// <summary>Represents sample.</summary>
    internal readonly struct Sample : IDisposable
    {
        /// <summary>Stores profile.</summary>
        private readonly RenderPublicationProfile? profile;
        /// <summary>Stores start.</summary>
        private readonly long start, allocation;
        /// <summary>Initializes a new sample instance.</summary>
        internal Sample(RenderPublicationProfile profile)
        {
            this.profile = profile;
            allocation = GC.GetAllocatedBytesForCurrentThread();
            start = Stopwatch.GetTimestamp();
        }
        /// <summary>Dispose.</summary>
        public void Dispose()
        {
            if (profile is null) return;
            profile.VerifyOwner();
            profile.Ticks += Stopwatch.GetTimestamp() - start;
            profile.AllocatedBytes += GC.GetAllocatedBytesForCurrentThread() - allocation;
            profile.Publications++;
        }
    }

    /// <summary>Verifies owner.</summary>
    private void VerifyOwner()
    {
        if (Environment.CurrentManagedThreadId != owner || !ReferenceEquals(active, this))
            throw new InvalidOperationException("Publication profiler used outside its owner-thread scope.");
    }

    /// <summary>Dispose.</summary>
    public void Dispose() { VerifyOwner(); active = null; }
}
