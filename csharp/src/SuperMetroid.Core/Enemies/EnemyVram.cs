namespace SuperMetroid.Core.Enemies;

/// <summary>Typed view of enemy and boss VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: enemy and boss VRAM")]
public readonly struct EnemyVram(SnesVram vram)
{
    /// <summary>The upper page of Kraid's BG2.</summary>
    public VramRegion KraidTopPage => new(vram, EnemyVramLayout.KraidTopPage);

    /// <summary>The lower page of Kraid's BG2.</summary>
    public VramRegion KraidBottomPage => new(vram, EnemyVramLayout.KraidBottomPage);

    /// <summary>The enemy BG2 image.</summary>
    public VramRegion EnemyBg2Tilemap => new(vram, EnemyVramLayout.EnemyBg2Tilemap);

    /// <summary>The OBJ table addressed by Crocomire's skeleton uploads.</summary>
    public VramRegion CrocomireSkeletonTiles => new(vram, EnemyVramLayout.CrocomireSkeletonTiles);

    /// <summary>The enemy graphics-set characters.</summary>
    public VramRegion EnemyTiles => new(vram, EnemyVramLayout.EnemyTiles);

    /// <summary>The Kraid room's backdrop BG characters.</summary>
    public VramRegion KraidRoomBackgroundTiles => new(vram, EnemyVramLayout.KraidRoomBackgroundTiles);

    /// <summary>Draygon's opening Evir characters.</summary>
    public VramRegion DraygonEvirTiles => new(vram, EnemyVramLayout.DraygonEvirTiles);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the enemy graphics-set characters.</summary>
    public static VramDmaDestination EnemyTilesDma(int wordOffset) => VramDmaDestination.At(EnemyVramLayout.EnemyTiles, wordOffset);

    /// <summary>The BG3 characters Kraid's death restores.</summary>
    public VramRegion KraidBg3Restore => new(vram, EnemyVramLayout.KraidBg3Restore);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the BG3 characters Kraid's death restores.</summary>
    public static VramDmaDestination KraidBg3RestoreDma(int wordOffset) => VramDmaDestination.At(EnemyVramLayout.KraidBg3Restore, wordOffset);

    /// <summary>Queued-DMA destination of the English EMERGENCY title row.</summary>
    public static VramDmaDestination CeresEmergencyTextDma => VramDmaDestination.At(EnemyVramLayout.CeresEmergencyText);
}
