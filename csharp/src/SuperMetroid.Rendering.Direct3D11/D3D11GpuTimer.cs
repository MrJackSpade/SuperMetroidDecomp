using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>Owner-thread timestamp ring. Full rings skip telemetry, never wait for the GPU.</summary>
internal sealed class D3D11GpuTimer : IDisposable
{
    private readonly D3D11RenderDevice owner;
    private readonly Slot[] slots;
    private int head, tail, pending;
    private bool active, disposed;
    internal long SkippedSamples { get; private set; }
    internal int PendingSamples => pending;

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

    internal bool TryBegin(RenderFrameIdentity identity)
    {
        CheckOwner();
        if (active) throw new InvalidOperationException("Nested GPU timing interval.");
        if (pending == slots.Length) { SkippedSamples++; return false; }
        Slot slot = slots[head];
        slot.Identity = identity;
        owner.Context.Begin(slot.Disjoint);
        owner.Context.End(slot.Start);
        active = true;
        return true;
    }

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

    internal bool TryRead(out GpuTimingSample sample)
    {
        CheckOwner();
        sample = default;
        if (pending == 0) return false;
        Slot slot = slots[tail];
        if (!Read(slot.Disjoint, out QueryDataTimestampDisjoint clock) ||
            !Read(slot.Start, out ulong start) || !Read(slot.Finish, out ulong finish)) return false;
        bool valid = !clock.Disjoint && clock.Frequency != 0 && finish >= start;
        sample = new(slot.Identity, valid, valid ? (finish - start) * 1000.0 / clock.Frequency : double.NaN);
        tail = (tail + 1) % slots.Length;
        pending--;
        return true;
    }

    private unsafe bool Read<T>(ID3D11Query query, out T data) where T : unmanaged
    {
        T value = default;
        var result = owner.Context.GetData(query, (nint)(&value), (uint)sizeof(T), AsyncGetDataFlags.DoNotFlush);
        result.CheckError();
        data = value;
        return result.Code == 0; // S_FALSE is pending, not a zero-duration measurement.
    }

    private void CheckOwner()
    {
        owner.VerifyOwner();
        ObjectDisposedException.ThrowIf(disposed, this);
    }

    public void Dispose()
    {
        owner.VerifyOwner();
        if (disposed) return;
        foreach (var slot in slots) slot.Dispose();
        disposed = true;
    }

    private sealed class Slot : IDisposable
    {
        internal readonly ID3D11Query Disjoint, Start, Finish;
        internal RenderFrameIdentity Identity;
        internal Slot(D3D11RenderDevice owner)
        {
            Disjoint = owner.Device.CreateQuery(new QueryDescription(QueryType.TimestampDisjoint));
            try
            {
                Start = owner.Device.CreateQuery(new QueryDescription(QueryType.Timestamp));
                try { Finish = owner.Device.CreateQuery(new QueryDescription(QueryType.Timestamp)); }
                catch { Start.Dispose(); throw; }
            }
            catch { Disjoint.Dispose(); throw; }
        }
        public void Dispose() { Finish.Dispose(); Start.Dispose(); Disjoint.Dispose(); }
    }
}

internal readonly record struct GpuTimingSample(RenderFrameIdentity Identity, bool Valid, double Milliseconds);
