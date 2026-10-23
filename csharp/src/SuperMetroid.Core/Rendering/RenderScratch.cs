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
    /// <summary>Per-thread color-plane pools keyed by exact pixel count.</summary>
    [ThreadStatic] private static Dictionary<int, Stack<Rgba32[]>>? freeColors;
    /// <summary>Per-thread byte-plane pools keyed by exact element count.</summary>
    [ThreadStatic] private static Dictionary<int, Stack<byte[]>>? freeBytes;
    /// <summary>Per-thread PPU reader models available for reloading from a new snapshot.</summary>
    [ThreadStatic] private static Stack<SoftwarePpuSnapshotMemory>? freeMemory;

    /// <summary>Color planes checked out for this render scope.</summary>
    private readonly List<Rgba32[]> colors = [];
    /// <summary>Byte planes checked out for this render scope.</summary>
    private readonly List<byte[]> bytes = [];
    /// <summary>PPU snapshot readers checked out for this render scope.</summary>
    private readonly List<SoftwarePpuSnapshotMemory> memories = [];
    /// <summary>Prevents a completed scope from returning its planes twice.</summary>
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
    public SoftwarePpuSnapshotMemory Memory(FramePaint snapshot)
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

    /// <summary>The frame's sprite draws resolved into scratch color and priority planes.</summary>
    public ResolvedObjFrame ResolveObjects(SoftwarePpuSnapshotMemory memory) =>
        ResolveObjects(memory.Sprites, memory.Cgram);

    /// <summary>The frame's sprite draws resolved into scratch color and priority planes.</summary>
    public ResolvedObjFrame ResolveObjects(SpriteFrameBuffer sprites, Hardware.IPaletteColors cgram)
    {
        Extent16<ScreenPixels> screen = Hardware.SnesPpuLayout.ScreenSize;
        int count = screen.X * screen.Y;
        Rgba32[] pixels = Colors(count);
        byte[] priorities = Bytes(count);
        SnesObjRenderer.RenderResolved(DrawColours.ResolveAll(sprites.Draws, cgram.Colors), new ObjRasterTargets(pixels, priorities), screen);
        return new ResolvedObjFrame(pixels, priorities);
    }

    /// <summary>Returns every checked-out intermediate plane and memory model to its thread-local pool.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        foreach (Rgba32[] plane in colors) Give(freeColors ??= [], plane);
        foreach (byte[] plane in bytes) Give(freeBytes ??= [], plane);
        foreach (SoftwarePpuSnapshotMemory memory in memories) (freeMemory ??= new()).Push(memory);
        colors.Clear();
        bytes.Clear();
        memories.Clear();
    }

    /// <summary>Reuses an exact-size plane or allocates one on the thread's first demand.</summary>
    private static T[] Take<T>(Dictionary<int, Stack<T[]>> free, int length) =>
        free.TryGetValue(length, out Stack<T[]>? stack) && stack.TryPop(out T[]? plane) ? plane : new T[length];

    /// <summary>Places a returned plane into the pool for its exact element count.</summary>
    private static void Give<T>(Dictionary<int, Stack<T[]>> free, T[] plane)
    {
        if (!free.TryGetValue(plane.Length, out Stack<T[]>? stack))
            free.Add(plane.Length, stack = new Stack<T[]>());
        stack.Push(plane);
    }
}
