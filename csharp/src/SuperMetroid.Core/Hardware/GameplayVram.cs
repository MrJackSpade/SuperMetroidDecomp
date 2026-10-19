namespace SuperMetroid.Core.Hardware;

/// <summary>Typed view of ordinary gameplay's VRAM planes (#630).</summary>
[PpuLayoutBoundary("rendering view: ordinary gameplay VRAM")]
public readonly struct GameplayVram(SnesVram vram)
{
    /// <summary>BG3 HUD/FX characters.</summary>
    public VramRegion HudCharacters => new(vram, GameplayVramLayout.HudCharacters);

    /// <summary>The standard BG3 upload: HUD characters followed by the cleared BG2 tilemap.</summary>
    public VramRegion HudCharacterUpload => new(vram, GameplayVramLayout.HudCharacterUpload);

    /// <summary>The queued-DMA destination <paramref name="wordOffset"/> words into the BG3 HUD tilemap.</summary>
    public static VramDmaDestination HudTilemapDma(int wordOffset = 0) => VramDmaDestination.At(GameplayVramLayout.HudTilemap, wordOffset);

    /// <summary>Queued-DMA destination of the standard BG3 upload (HUD characters then the cleared BG2 tilemap).</summary>
    public static VramDmaDestination HudCharacterUploadDma => VramDmaDestination.At(GameplayVramLayout.HudCharacterUpload);

    /// <summary>
    /// The queued-DMA destination <paramref name="wordOffset"/> words into the OBJ character table; the start is where the
    /// standard OBJ upload lands.
    /// </summary>
    public static VramDmaDestination ObjectCharactersDma(int wordOffset = 0) =>
        VramDmaDestination.At(GameplayVramLayout.ObjectCharacters, wordOffset);
}
