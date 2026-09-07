namespace SuperMetroid.Core.Rendering;

/// <summary>Serializes the final presentation call with load/reset generation changes.</summary>
/// <remarks>
/// This lock is separate from the visual mailbox. Ordinary publication and simulation
/// must never acquire it. A load/reset asynchronously awaits an already-entered presentation
/// call, then advances this gate before restoring/publishing the new generation. Rendering,
/// waitable-swapchain waits and GPU completion waits belong outside TryPresent.
/// </remarks>
public sealed class RenderPresentationGate
{
    private readonly SemaphoreSlim sync = new(1, 1);
    private long generation;
    private int presentingThread;

    public RenderPresentationGate(long initialGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialGeneration);
        generation = initialGeneration;
    }

    /// <summary>Checks and submits atomically relative to AdvanceGeneration; stale work is rejected.</summary>
    public bool TryPresent(RenderFrameIdentity identity, Action present)
    {
        ArgumentNullException.ThrowIfNull(present);
        RejectReentrancy();
        sync.Wait();
        try
        {
            if (identity.Generation != generation) return false;
            Volatile.Write(ref presentingThread, Environment.CurrentManagedThreadId);
            try { present(); }
            finally { Volatile.Write(ref presentingThread, 0); }
            return true;
        }
        finally { sync.Release(); }
    }

    /// <summary>Linearizes reset after any already-entered presentation and rejects old work thereafter.</summary>
    public void AdvanceGeneration(long nextGeneration)
    {
        RejectReentrancy();
        sync.Wait();
        try
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(nextGeneration, generation);
            generation = nextGeneration;
        }
        finally { sync.Release(); }
    }

    /// <summary>Preserves reset ordering without blocking the caller while Present is in flight.</summary>
    public async Task AdvanceGenerationAsync(long nextGeneration)
    {
        RejectReentrancy();
        await sync.WaitAsync().ConfigureAwait(false);
        try
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(nextGeneration, generation);
            generation = nextGeneration;
        }
        finally { sync.Release(); }
    }

    private void RejectReentrancy()
    {
        if (Volatile.Read(ref presentingThread) == Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Presentation callbacks cannot reenter or reset the gate.");
    }
}
