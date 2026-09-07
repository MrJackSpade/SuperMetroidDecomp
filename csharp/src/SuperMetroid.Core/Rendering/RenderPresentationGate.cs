namespace SuperMetroid.Core.Rendering;

/// <summary>Serializes the final presentation call with load/reset generation changes.</summary>
/// <remarks>
/// This lock is separate from the visual mailbox. Ordinary publication and simulation
/// must never acquire it. A load/reset waits for an already-entered presentation call,
/// then advances this gate before publishing frames of the new generation. Rendering,
/// waitable-swapchain waits and GPU completion waits belong outside TryPresent.
/// </remarks>
public sealed class RenderPresentationGate
{
    private readonly object sync = new();
    private long generation;
    private bool presenting;

    public RenderPresentationGate(long initialGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialGeneration);
        generation = initialGeneration;
    }

    /// <summary>Checks and submits atomically relative to AdvanceGeneration; stale work is rejected.</summary>
    public bool TryPresent(RenderFrameIdentity identity, Action present)
    {
        ArgumentNullException.ThrowIfNull(present);
        lock (sync)
        {
            if (presenting) throw new InvalidOperationException("Presentation callbacks cannot reenter the gate.");
            if (identity.Generation != generation) return false;
            presenting = true;
            try { present(); }
            finally { presenting = false; }
            return true;
        }
    }

    /// <summary>Linearizes reset after any already-entered presentation and rejects old work thereafter.</summary>
    public void AdvanceGeneration(long nextGeneration)
    {
        lock (sync)
        {
            if (presenting) throw new InvalidOperationException("A presentation callback cannot reset its generation.");
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(nextGeneration, generation);
            generation = nextGeneration;
        }
    }
}
