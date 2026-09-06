namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A single pending visual packet, with explicit replacement and invalidation counts.
/// No game/input/audio data belongs here. Slow rendering never creates a video backlog.
/// </summary>
/// <remarks>
/// The lock protects only pointer/counter changes. No rendering, callbacks, waiting,
/// GPU disposal or I/O occurs while it is held. A consumer takes ownership of its
/// immutable packet then releases the lock before doing any work. Host shutdown and
/// the final generation check before presentation are separate lifecycle concerns.
/// </remarks>
public sealed class LatestRenderFrameMailbox
{
    private readonly object sync = new();
    private RenderFrameSnapshot? pending;
    private long generation;
    private long lastSequence;
    private long published;
    private long taken;
    private long replaced;
    private long invalidated;

    public LatestRenderFrameMailbox(long initialGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialGeneration);
        generation = initialGeneration;
    }

    /// <summary>
    /// Publishes a complete packet from the simulation owner. Sequence may skip but
    /// never rewind, even across generations; malformed publication is a loud error.
    /// </summary>
    public void Publish(RenderFrameSnapshot frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        lock (sync)
        {
            if (frame.Identity.Generation != generation)
                throw new InvalidOperationException("Render publication has the wrong load/reset generation.");
            if (frame.Identity.Sequence <= lastSequence)
                throw new InvalidOperationException("Render publication sequence did not increase.");
            if (pending is not null) replaced++;
            pending = frame;
            lastSequence = frame.Identity.Sequence;
            published++;
        }
    }

    /// <summary>Returns the newest complete frame once, without waiting for a producer.</summary>
    public RenderFrameSnapshot? TakeLatest()
    {
        lock (sync)
        {
            RenderFrameSnapshot? frame = pending;
            pending = null;
            if (frame is not null) taken++;
            return frame;
        }
    }

    /// <summary>
    /// Invalidates queued work at a simulation load/reset boundary. Already-taken
    /// packets remain immutable but must be rejected by the render owner's generation
    /// check. This does not pretend to cancel a GPU submission already in flight.
    /// </summary>
    public void AdvanceGeneration(long nextGeneration)
    {
        lock (sync)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(nextGeneration, generation);
            if (pending is not null) invalidated++;
            pending = null;
            generation = nextGeneration;
        }
    }

    /// <summary>
    /// Tests a consumer-held packet against the current generation. The future
    /// presenter must coordinate its final submission with reset/resize lifecycle;
    /// this observation alone is not an atomic check-and-Present operation.
    /// </summary>
    public bool IsCurrent(RenderFrameSnapshot frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        lock (sync) return frame.Identity.Generation == generation;
    }

    public RenderMailboxMetrics Metrics
    {
        get
        {
            lock (sync) return new(generation, published, taken, replaced, invalidated, pending is not null);
        }
    }
}

/// <summary>Visual-only counters; none represents skipped emulation ticks or audio.</summary>
public readonly record struct RenderMailboxMetrics(long Generation, long Published, long Taken,
    long Replaced, long Invalidated, bool HasPendingFrame);
