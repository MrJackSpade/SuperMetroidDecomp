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
    /// <summary>Stores sync.</summary>
    private readonly Lock sync = new();
    /// <summary>Stores pending.</summary>
    private RenderFrameSnapshot? pending;
    /// <summary>Stores generation.</summary>
    private long generation;
    /// <summary>Stores last sequence.</summary>
    private long lastSequence;
    /// <summary>Stores published.</summary>
    private long published;
    /// <summary>Stores taken.</summary>
    private long taken;
    /// <summary>Stores replaced.</summary>
    private long replaced;
    /// <summary>Stores invalidated.</summary>
    private long invalidated;

    /// <summary>Creates an empty visual mailbox for one positive simulation load/reset generation.</summary>
    /// <param name="initialGeneration">Positive generation identity required on the first published packet.</param>
    /// <exception cref="ArgumentOutOfRangeException">The generation is zero or negative.</exception>
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
        Ensure.NotNull(frame);
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
        Ensure.NotNull(frame);
        lock (sync) return frame.Identity.Generation == generation;
    }

    /// <summary>Reads a lock-consistent snapshot of visual packets replaced before a consumer took them.</summary>
    public RenderMailboxMetrics Metrics
    {
        get
        {
            lock (sync) return new(replaced);
        }
    }
}

/// <summary>Visual-only counters; none represents skipped emulation ticks or audio.</summary>
public readonly record struct RenderMailboxMetrics(    long Replaced);
