using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>Per-render scratch readers for legacy software raster kernels.</summary>
[PpuLayoutBoundary("codec: restores a render snapshot into software PPU memory")]
internal sealed class SoftwarePpuSnapshotMemory
{
    /// <summary>Palette copy populated color by color from the current snapshot.</summary>
    internal SnesCgram Cgram { get; } = new();
    /// <summary>Whole-frame sprite draws restored from the snapshot.</summary>
    internal SpriteFrameBuffer Sprites { get; } = new();

    /// <summary>Creates scratch PPU memory already populated from one render snapshot.</summary>
    internal SoftwarePpuSnapshotMemory(FramePaint snapshot) => Load(snapshot);

    /// <summary>Replaces every modeled byte, so a pooled reader carries nothing between frames.</summary>
    internal void Load(FramePaint snapshot)
    {
        Ensure.NotNull(snapshot);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            Cgram.SetColor(index, snapshot.Colors[index]);
        Sprites.LoadDraws(snapshot.SpriteDraws);
    }
}
