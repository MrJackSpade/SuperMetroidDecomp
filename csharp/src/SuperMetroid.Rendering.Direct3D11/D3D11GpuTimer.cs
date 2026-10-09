using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Owner-thread timestamp ring. Full rings skip telemetry, never wait for the GPU.</summary>
internal sealed class D3D11GpuTimer : IDisposable
{
    /// <summary>The Direct3D device and context whose owner thread must perform all query operations.</summary>
    private readonly D3D11RenderDevice owner;
    /// <summary>Reusable query sets forming the bounded ring of in-flight GPU measurements.</summary>
    private readonly Slot[] slots;
    /// <summary>Ring positions and outstanding count; a full ring causes samples to be skipped rather than blocking.</summary>
    private int head, tail, pending;
    /// <summary>Tracks an open interval and whether disposal has already released the query resources.</summary>
    private bool active, disposed;
    /// <summary>Number of requested samples omitted because every ring slot was still pending.</summary>
    internal long SkippedSamples { get; private set; }

    /// <summary>Creates timestamp query slots for measuring GPU work without waiting when the ring is full.</summary>
    /// <param name="owner">The render device providing the D3D11 device, context, and owning-thread check.</param>
    /// <param name="capacity">Number of concurrent measurements the ring can hold; must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is zero or negative.</exception>
    internal D3D11GpuTimer(D3D11RenderDevice owner, int capacity = 8)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        this.owner = owner;
        owner.VerifyOwner();
        slots = new Slot[capacity];
        try
        {
            for (int i = 0; i < capacity; i++) slots[i] = new Slot(owner);
        }
        catch { foreach (var slot in slots) slot?.Dispose(); throw; }
    }

    /// <summary>Begins a timestamp interval for a frame unless all query slots are still in flight.</summary>
    /// <param name="identity">The frame identity associated with this measurement.</param>
    /// <returns><see langword="true"/> when a query interval was opened; otherwise increments <see cref="SkippedSamples"/> and returns <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">An interval is already active.</exception>
    /// <exception cref="ObjectDisposedException">The timer has been disposed.</exception>
    internal bool TryBegin(RenderFrameIdentity identity)
    {
        CheckOwner();
        if (active) throw new InvalidOperationException("Nested GPU timing interval.");
        if (pending == slots.Length) { SkippedSamples++; return false; }
        Slot slot = slots[head];
        slot.Identity = identity;
        slot.HasCompositionBoundary = false;
        owner.Context.Begin(slot.Disjoint);
        owner.Context.End(slot.Start);
        active = true;
        return true;
    }

    /// <summary>Splits composition from display inside the same disjoint clock interval.</summary>
    internal void MarkCompositionFinished()
    {
        CheckOwner();
        if (!active || slots[head].HasCompositionBoundary)
            throw new InvalidOperationException("GPU composition boundary requires one active, unmarked interval.");
        owner.Context.End(slots[head].CompositionFinished);
        slots[head].HasCompositionBoundary = true;
    }

    /// <summary>Closes the active timestamp interval and queues its slot for a later nonblocking read.</summary>
    /// <exception cref="InvalidOperationException">No interval is active.</exception>
    /// <exception cref="ObjectDisposedException">The timer has been disposed.</exception>
    internal void End()
    {
        CheckOwner();
        if (!active) throw new InvalidOperationException("GPU timing End without Begin.");
        Slot slot = slots[head];
        owner.Context.End(slot.Finish);
        owner.Context.End(slot.Disjoint);
        active = false;
        head = (head + 1) % slots.Length;
        pending++;
    }

    /// <summary>Attempts to read the oldest completed interval without flushing pending GPU work.</summary>
    /// <param name="sample">Receives the elapsed timing when available; otherwise receives the default value.</param>
    /// <returns><see langword="true"/> when one pending interval was read and removed from the ring.</returns>
    /// <exception cref="ObjectDisposedException">The timer has been disposed.</exception>
    internal bool TryRead(out GpuTimingSample sample)
    {
        CheckOwner();
        sample = default;
        if (pending == 0) return false;
        Slot slot = slots[tail];
        if (!Read(slot.Disjoint, out QueryDataTimestampDisjoint clock) ||
            !Read(slot.Start, out ulong start) || !Read(slot.Finish, out ulong finish)) return false;
        ulong compositionFinished = finish;
        if (slot.HasCompositionBoundary && !Read(slot.CompositionFinished, out compositionFinished)) return false;
        bool valid = !clock.Disjoint && clock.Frequency != 0 && finish >= compositionFinished && compositionFinished >= start;
        sample = new(valid, valid ? (finish - start) * 1000.0 / clock.Frequency : double.NaN,
            valid ? (compositionFinished - start) * 1000.0 / clock.Frequency : double.NaN);
        tail = (tail + 1) % slots.Length;
        pending--;
        return true;
    }

    /// <summary>Reads query data once with the do-not-flush flag, distinguishing a pending result from a completed zero value.</summary>
    /// <typeparam name="T">The unmanaged native data shape returned by the query.</typeparam>
    /// <param name="query">The D3D11 query to poll.</param>
    /// <param name="data">Receives the query payload even when the runtime reports that it is still pending.</param>
    /// <returns><see langword="true"/> when the query result is ready.</returns>
    private unsafe bool Read<T>(ID3D11Query query, out T data) where T : unmanaged
    {
        T value = default;
        var result = owner.Context.GetData(query, (nint)(&value), (uint)sizeof(T), AsyncGetDataFlags.DoNotFlush);
        result.CheckError();
        data = value;
        return result.Code == 0; // S_FALSE is pending, not a zero-duration measurement.
    }

    /// <summary>Enforces the render-device thread affinity and rejects use after disposal.</summary>
    /// <exception cref="ObjectDisposedException">The timer has already been disposed.</exception>
    private void CheckOwner()
    {
        owner.VerifyOwner();
        ObjectDisposedException.ThrowIf(disposed, this);
    }

    /// <summary>Releases every timestamp query on the device owner thread; repeated calls are harmless.</summary>
    public void Dispose()
    {
        owner.VerifyOwner();
        if (disposed) return;
        foreach (var slot in slots) slot.Dispose();
        disposed = true;
    }

    private sealed class Slot : IDisposable
    {
        /// <summary>Timestamp queries for the whole interval and its optional composition boundary.</summary>
        internal readonly ID3D11Query Disjoint, Start, Finish, CompositionFinished;
        /// <summary>Indicates that a composition timestamp was recorded between start and finish.</summary>
        internal bool HasCompositionBoundary;
        /// <summary>Frame associated with these query results for render telemetry correlation.</summary>
        internal RenderFrameIdentity Identity;
        /// <summary>Allocates the disjoint-frequency query and the three timestamp queries as one cleanup unit.</summary>
        /// <param name="owner">The device used to create the queries.</param>
        internal Slot(D3D11RenderDevice owner)
        {
            Disjoint = owner.Device.CreateQuery(new QueryDescription(QueryType.TimestampDisjoint));
            try
            {
                Start = owner.Device.CreateQuery(new QueryDescription(QueryType.Timestamp));
                try
                {
                    Finish = owner.Device.CreateQuery(new QueryDescription(QueryType.Timestamp));
                    try { CompositionFinished = owner.Device.CreateQuery(new QueryDescription(QueryType.Timestamp)); }
                    catch { Finish.Dispose(); throw; }
                }
                catch { Start.Dispose(); throw; }
            }
            catch { Disjoint.Dispose(); throw; }
        }
        /// <summary>Releases all queries owned by this ring slot.</summary>
        public void Dispose() { CompositionFinished.Dispose(); Finish.Dispose(); Start.Dispose(); Disjoint.Dispose(); }
    }
}

/// <summary>GPU elapsed times for a frame, with a validity flag for disjoint clocks or inconsistent timestamp ordering.</summary>
/// <param name="Valid">Whether the timestamps describe a usable measurement.</param>
/// <param name="Milliseconds">Elapsed GPU time from interval start through finish.</param>
/// <param name="CompositionMilliseconds">Elapsed GPU time from interval start through the optional composition boundary.</param>
internal readonly record struct GpuTimingSample(bool Valid, double Milliseconds, double CompositionMilliseconds);
