namespace SuperMetroid.Core.Hardware;

/// <summary>Typed view of ordinary gameplay's VRAM planes (#630).</summary>
[PpuLayoutBoundary("rendering view: ordinary gameplay VRAM")]
public readonly struct GameplayVram(SnesVram vram)
{
    /// <summary>BG3 HUD/FX characters.</summary>
    public VramRegion HudCharacters => new(vram, GameplayVramLayout.HudCharacters);

    /// <summary>The standard BG3 upload: HUD characters followed by the cleared BG2 tilemap.</summary>
    public VramRegion HudCharacterUpload => new(vram, GameplayVramLayout.HudCharacterUpload);

    /// <summary>BG1's level tilemap.</summary>
    public VramRegion Bg1Tilemap => new(vram, GameplayVramLayout.Bg1Tilemap);

    /// <summary>BG2's background tilemap.</summary>
    public VramRegion Bg2Tilemap => new(vram, GameplayVramLayout.Bg2Tilemap);

    /// <summary>The queued-DMA destination <paramref name="wordOffset"/> words into the BG3 HUD tilemap.</summary>
    public static VramDmaDestination HudTilemapDma(int wordOffset = 0) => VramDmaDestination.At(GameplayVramLayout.HudTilemap, wordOffset);

    /// <summary>The queued-DMA destination at the start of the OBJ character table, where the standard OBJ upload lands.</summary>
    public static VramDmaDestination ObjectCharactersDma() => VramDmaDestination.At(GameplayVramLayout.ObjectCharacters);
}
