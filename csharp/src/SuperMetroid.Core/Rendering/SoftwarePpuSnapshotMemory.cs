using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Per-render scratch readers for legacy software raster kernels.</summary>
[PpuLayoutBoundary("codec: restores a render snapshot into software PPU memory")]
internal sealed class SoftwarePpuSnapshotMemory
{
    /// <summary>Gets vram.</summary>
    internal SnesVram Vram { get; } = new();
    /// <summary>Gets cgram.</summary>
    internal SnesCgram Cgram { get; } = new();
    /// <summary>Gets oam.</summary>
    internal OamBuffer Oam { get; } = new();

    /// <summary>Initializes a new software ppu snapshot memory instance.</summary>
    internal SoftwarePpuSnapshotMemory(PpuMemorySnapshot snapshot) => Load(snapshot);

    /// <summary>Replaces every modeled byte, so a pooled reader carries nothing between frames.</summary>
    internal void Load(PpuMemorySnapshot snapshot)
    {
        Ensure.NotNull(snapshot);
        Vram.LoadBytes(0, snapshot.Vram);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            Cgram.SetColor(index, snapshot.Cgram[index]);
        Oam.LoadUploadPayload(snapshot.Oam, snapshot.ModeledSpriteCount);
    }
}
