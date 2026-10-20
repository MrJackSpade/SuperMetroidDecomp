namespace SuperMetroid.Core.Hardware;

/// <summary>Whole-VRAM operations a screen performs when it takes over the store (#630).</summary>
[PpuLayoutBoundary("rendering view: whole VRAM")]
public readonly struct VramStore(SnesVram vram)
{
    /// <summary>All 32,768 words.</summary>
    public VramRegion All => new(vram, PpuMemoryLayout.Vram);
}
