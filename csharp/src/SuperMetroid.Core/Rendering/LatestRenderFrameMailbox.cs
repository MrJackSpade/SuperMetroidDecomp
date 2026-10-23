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
    /// <summary>Serializes ownership transfer and generation changes without holding the lock during rendering.</summary>
    internal readonly Lock sync = new();
    /// <summary>At most one unpublished immutable visual packet; a newer publish replaces it.</summary>
    internal DisplayPacket? Pending { get; private set; }
    /// <summary>Current simulation load/reset identity accepted for publication.</summary>
    internal long Generation { get; private set; }
    /// <summary>Highest published sequence, retained across generations to reject rewinds.</summary>
    private long lastSequence;
    /// <summary>Total valid publications observed by this mailbox.</summary>
    internal long Published { get; private set; }
    /// <summary>Total packets claimed by a consumer.</summary>
    internal long Taken { get; private set; }
    /// <summary>Packets superseded by a newer publication before consumption.</summary>
    internal long Replaced { get; private set; }
    /// <summary>Pending packets discarded on a load/reset generation change.</summary>
    internal long Invalidated { get; private set; }

    /// <summary>Creates an empty visual mailbox for one positive simulation load/reset generation.</summary>
    /// <param name="initialGeneration">Positive generation identity required on the first published packet.</param>
    /// <exception cref="ArgumentOutOfRangeException">The generation is zero or negative.</exception>
    public LatestRenderFrameMailbox(long initialGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialGeneration);
        Generation = initialGeneration;
    }

    /// <summary>
    /// Publishes a complete packet from the simulation owner. Sequence may skip but
    /// never rewind, even across generations; malformed publication is a loud error.
    /// </summary>
    public void Publish(DisplayPacket frame)
    {
        Ensure.NotNull(frame);
        lock (sync)
        {
            if (frame.Identity.Generation != Generation)
                throw new InvalidOperationException("Render publication has the wrong load/reset generation.");
            if (frame.Identity.Sequence <= lastSequence)
                throw new InvalidOperationException("Render publication sequence did not increase.");
            if (Pending is not null) Replaced++;
            Pending = frame;
            lastSequence = frame.Identity.Sequence;
            Published++;
        }
    }

    /// <summary>Returns the newest complete frame once, without waiting for a producer.</summary>
    public DisplayPacket? TakeLatest()
    {
        lock (sync)
        {
            DisplayPacket? frame = Pending;
            Pending = null;
            if (frame is not null) Taken++;
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
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(nextGeneration, Generation);
            if (Pending is not null) Invalidated++;
            Pending = null;
            Generation = nextGeneration;
        }
    }

    /// <summary>
    /// Tests a consumer-held packet against the current generation. The future
    /// presenter must coordinate its final submission with reset/resize lifecycle;
    /// this observation alone is not an atomic check-and-Present operation.
    /// </summary>
    public bool IsCurrent(DisplayPacket frame)
    {
        Ensure.NotNull(frame);
        lock (sync) return frame.Identity.Generation == Generation;
    }

    /// <summary>Reads a lock-consistent snapshot of visual packets replaced before a consumer took them.</summary>
    public RenderMailboxMetrics Metrics
    {
        get
        {
            lock (sync) return new(Replaced);
        }
    }
}
