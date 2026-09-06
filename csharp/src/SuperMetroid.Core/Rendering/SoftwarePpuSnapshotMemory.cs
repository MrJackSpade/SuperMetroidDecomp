using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Per-render scratch readers for legacy software raster kernels.</summary>
internal sealed class SoftwarePpuSnapshotMemory
{
    internal SnesVram Vram { get; } = new();
    internal SnesCgram Cgram { get; } = new();
    internal OamBuffer Oam { get; } = new();

    internal SoftwarePpuSnapshotMemory(PpuMemorySnapshot snapshot)
    {
        Vram.LoadBytes(0, snapshot.Vram);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            Cgram.SetColor(index, snapshot.Cgram[index]);
        Oam.LoadUploadPayload(snapshot.Oam, snapshot.ModeledSpriteCount);
    }
}
