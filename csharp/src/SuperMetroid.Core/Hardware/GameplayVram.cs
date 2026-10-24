namespace SuperMetroid.Core.Hardware;

/// <summary>Typed view of ordinary gameplay's VRAM planes (#630).</summary>
[PpuLayoutBoundary("rendering view: ordinary gameplay VRAM")]
public readonly struct GameplayVram(SnesVram vram)
{

    /// <summary>Queued-DMA destination of the standard BG3 upload (HUD characters then the cleared BG2 tilemap).</summary>
    public static VramDmaDestination HudCharacterUploadDma => VramDmaDestination.At(GameplayVramLayout.HudCharacterUpload);
}
