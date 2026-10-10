using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Exact-size, zero-filled intermediate planes for one software render. Planes come from a
/// per-thread free list and all return to it on <see cref="Dispose"/>, so a layer raster
/// costs nothing after a thread's first frame. A plane must never outlive its scope: the
/// returned frame is always caller- or renderer-owned storage, never a scratch plane.
/// </summary>
internal sealed class RenderScratch : IDisposable
{
    /// <summary>Per-thread reusable color planes grouped by exact pixel count.</summary>
    [ThreadStatic] private static Dictionary<int, Stack<Rgba32[]>>? freeColors;
    /// <summary>Per-thread reusable byte planes grouped by exact element count.</summary>
    [ThreadStatic] private static Dictionary<int, Stack<byte[]>>? freeBytes;
    /// <summary>Per-thread reusable VRAM/CGRAM/OAM snapshot readers.</summary>
    [ThreadStatic] private static Stack<SoftwarePpuSnapshotMemory>? freeMemory;

    /// <summary>Planes borrowed by this scope and returned together when disposed.</summary>
    private readonly List<Rgba32[]> colors = [];
    /// <summary>Byte planes borrowed by this scope and returned together when disposed.</summary>
    private readonly List<byte[]> bytes = [];
    /// <summary>Snapshot readers borrowed by this scope and returned together when disposed.</summary>
    private readonly List<SoftwarePpuSnapshotMemory> memories = [];
    /// <summary>Whether this scope has returned its borrowed storage.</summary>
    private bool disposed;

    /// <summary>A cleared color plane (every pixel transparent).</summary>
    public Rgba32[] Colors(int length)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        Rgba32[] plane = Take(freeColors ??= [], length);
        Array.Clear(plane);
        colors.Add(plane);
        return plane;
    }

    /// <summary>A zero-filled byte plane.</summary>
    public byte[] Bytes(int length)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        byte[] plane = Take(freeBytes ??= [], length);
        Array.Clear(plane);
        bytes.Add(plane);
        return plane;
    }

    /// <summary>VRAM/CGRAM/OAM reader models loaded from one immutable snapshot.</summary>
    public SoftwarePpuSnapshotMemory Memory(PpuMemorySnapshot snapshot)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        SoftwarePpuSnapshotMemory memory;
        if ((freeMemory ??= new()).TryPop(out SoftwarePpuSnapshotMemory? pooled))
        {
            pooled.Load(snapshot);
            memory = pooled;
        }
        else memory = new SoftwarePpuSnapshotMemory(snapshot);
        memories.Add(memory);
        return memory;
    }

    /// <summary>OAM resolved into scratch color and priority planes.</summary>
    public ResolvedObjFrame ResolveObjects(SoftwarePpuSnapshotMemory memory, byte objectSelection) =>
        ResolveObjects(memory.Oam, memory.Vram, memory.Cgram, objectSelection);

    /// <summary>OAM resolved into scratch color and priority planes.</summary>
    public ResolvedObjFrame ResolveObjects(Hardware.OamBuffer oam, Hardware.SnesVram vram, Hardware.SnesCgram cgram,
        byte objectSelection, int width = Hardware.SnesPpuLayout.ScreenWidthPixels,
        int height = Hardware.SnesPpuLayout.ScreenHeightPixels)
    {
        int count = checked(width * height);
        Rgba32[] pixels = Colors(count);
        byte[] priorities = Bytes(count);
        SnesObjRenderer.RenderResolved(oam, vram, cgram, objectSelection, pixels, priorities, width, height);
        return new ResolvedObjFrame(pixels, priorities);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        foreach (Rgba32[] plane in colors) Give(freeColors!, plane);
        foreach (byte[] plane in bytes) Give(freeBytes!, plane);
        foreach (SoftwarePpuSnapshotMemory memory in memories) freeMemory!.Push(memory);
        colors.Clear();
        bytes.Clear();
        memories.Clear();
    }

    /// <summary>Takes an exact-length plane from its free list or allocates one when none is available.</summary>
    private static T[] Take<T>(Dictionary<int, Stack<T[]>> free, int length) =>
        free.TryGetValue(length, out Stack<T[]>? stack) && stack.TryPop(out T[]? plane) ? plane : new T[length];

    /// <summary>Returns an exact-length plane to its per-thread free list.</summary>
    private static void Give<T>(Dictionary<int, Stack<T[]>> free, T[] plane)
    {
        if (!free.TryGetValue(plane.Length, out Stack<T[]>? stack))
            free.Add(plane.Length, stack = new Stack<T[]>());
        stack.Push(plane);
    }
}
