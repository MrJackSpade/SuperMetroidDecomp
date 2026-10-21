namespace SuperMetroid.Core.Enemies;

/// <summary>Typed view of enemy VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: enemy VRAM")]
public readonly struct EnemyVram(SnesVram vram)
{

    /// <summary>The enemy BG2 image.</summary>
    public VramRegion EnemyBg2Tilemap => new(vram, EnemyVramLayout.EnemyBg2Tilemap);

    /// <summary>The enemy graphics-set characters.</summary>
    public VramRegion EnemyTiles => new(vram, EnemyVramLayout.EnemyTiles);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the enemy graphics-set characters.</summary>
    public static VramDmaDestination EnemyTilesDma(int wordOffset) => VramDmaDestination.At(EnemyVramLayout.EnemyTiles, wordOffset);



}
