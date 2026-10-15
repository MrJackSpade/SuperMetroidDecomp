using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Per-render scratch readers for legacy software raster kernels.</summary>
[PpuLayoutBoundary("codec: restores a render snapshot into software PPU memory")]
internal sealed class SoftwarePpuSnapshotMemory
{
    /// <summary>Writable 64 KiB VRAM copy loaded from the current render snapshot.</summary>
    internal SnesVram Vram { get; } = new();
    /// <summary>Palette copy populated color by color from the current snapshot.</summary>
    internal SnesCgram Cgram { get; } = new();
    /// <summary>Sprite tables, modeled sprite count and whole-frame draws restored from the snapshot.</summary>
    internal SpriteFrameBuffer Sprites { get; } = new();

    /// <summary>Creates scratch PPU memory already populated from one render snapshot.</summary>
    internal SoftwarePpuSnapshotMemory(PpuMemorySnapshot snapshot) => Load(snapshot);

    /// <summary>Replaces every modeled byte, so a pooled reader carries nothing between frames.</summary>
    internal void Load(PpuMemorySnapshot snapshot)
    {
        Ensure.NotNull(snapshot);
        Vram.LoadBytes(0, snapshot.Vram);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            Cgram.SetColor(index, snapshot.Cgram[index]);
        Sprites.Oam.LoadUploadPayload(snapshot.Oam, snapshot.ModeledSpriteCount);
        Sprites.LoadDraws(snapshot.SpriteDraws);
    }
}
