using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Owner-thread timestamp ring. Full rings skip telemetry, never wait for the GPU.</summary>
internal sealed class D3D11GpuTimer : IDisposable
{
    /// <summary>Render device and immediate context whose owner thread issues every query.</summary>
    private readonly D3D11RenderDevice owner;
    /// <summary>Reusable disjoint and timestamp query sets for pending frame samples.</summary>
    internal readonly Slot[] slots;
    /// <summary>Ring insertion, removal, and pending-sample cursors.</summary>
    internal int head, tail, pending;
    /// <summary>Tracks an open interval and final disposal of its query resources.</summary>
    private bool active, disposed;
    /// <summary>Intervals omitted because all query slots were awaiting GPU results.</summary>
    internal long SkippedSamples { get; private set; }

    /// <summary>Creates a fixed number of query slots on the render device's owner thread.</summary>
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

    /// <summary>Starts a disjoint clock interval unless the ring is full; a full ring increments skipped telemetry.</summary>
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

    /// <summary>Closes the current GPU interval and queues its query slot for nonblocking retrieval.</summary>
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

    /// <summary>Reads the oldest ready query set without flushing the GPU and discards disjoint or reversed timestamps.</summary>
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

    /// <summary>Queries data without a GPU flush; S_FALSE leaves the interval pending.</summary>
    private unsafe bool Read<T>(ID3D11Query query, out T data) where T : unmanaged
    {
        T value = default;
        var result = owner.Context.GetData(query, (nint)(&value), (uint)sizeof(T), AsyncGetDataFlags.DoNotFlush);
        result.CheckError();
        data = value;
        return result.Code == 0; // S_FALSE is pending, not a zero-duration measurement.
    }

    /// <summary>Requires the rendering thread and a live timer before touching D3D queries.</summary>
    private void CheckOwner()
    {
        owner.VerifyOwner();
        ObjectDisposedException.ThrowIf(disposed, this);
    }

    /// <summary>Releases every query slot on the rendering thread.</summary>
    public void Dispose()
    {
        owner.VerifyOwner();
        if (disposed) return;
        foreach (var slot in slots) slot.Dispose();
        disposed = true;
    }

    /// <summary>Four queries bounding one frame and its optional composition split.</summary>
    internal sealed class Slot : IDisposable
    {
        /// <summary>Disjoint clock plus start, finish, and intermediate composition timestamps.</summary>
        internal readonly ID3D11Query Disjoint, Start, Finish, CompositionFinished;
        /// <summary>Whether the composition timestamp belongs to this interval.</summary>
        internal bool HasCompositionBoundary;
        /// <summary>Frame identity supplied when the interval was opened.</summary>
        internal RenderFrameIdentity Identity;
        /// <summary>Allocates all queries, releasing earlier ones if a later allocation fails.</summary>
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
        /// <summary>Releases this slot's query objects in reverse allocation order.</summary>
        public void Dispose() { CompositionFinished.Dispose(); Finish.Dispose(); Start.Dispose(); Disjoint.Dispose(); }
    }
}
