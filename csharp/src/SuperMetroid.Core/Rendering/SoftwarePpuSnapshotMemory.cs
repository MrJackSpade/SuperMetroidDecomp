using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Per-render scratch readers for legacy software raster kernels.</summary>
internal sealed class SoftwarePpuSnapshotMemory
{
    /// <summary>Scratch VRAM loaded from the captured frame image.</summary>
    internal SnesVram Vram { get; } = new();
    /// <summary>Scratch color palette loaded from the captured frame image.</summary>
    internal SnesCgram Cgram { get; } = new();
    /// <summary>Scratch OAM loaded from the captured frame image.</summary>
    internal OamBuffer Oam { get; } = new();

    /// <summary>Creates scratch hardware readers initialized from one immutable memory snapshot.</summary>
    internal SoftwarePpuSnapshotMemory(PpuMemorySnapshot snapshot) => Load(snapshot);

    /// <summary>Replaces every modeled byte, so a pooled reader carries nothing between frames.</summary>
    internal void Load(PpuMemorySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        Vram.LoadBytes(0, snapshot.Vram);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            Cgram.SetColor(index, snapshot.Cgram[index]);
        Oam.LoadUploadPayload(snapshot.Oam, snapshot.ModeledSpriteCount);
    }
}
