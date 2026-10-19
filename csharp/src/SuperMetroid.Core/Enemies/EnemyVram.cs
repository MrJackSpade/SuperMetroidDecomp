namespace SuperMetroid.Core.Enemies;

/// <summary>Typed view of enemy and boss VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: enemy and boss VRAM")]
public readonly struct EnemyVram(SnesVram vram)
{

    /// <summary>The enemy BG2 image.</summary>
    public VramRegion EnemyBg2Tilemap => new(vram, EnemyVramLayout.EnemyBg2Tilemap);

    /// <summary>The enemy graphics-set characters.</summary>
    public VramRegion EnemyTiles => new(vram, EnemyVramLayout.EnemyTiles);

    /// <summary>Binds the Kraid room's backdrop BG characters at their VRAM word (#1352).</summary>
    public static void BindKraidRoomBackgroundTiles(BgArtworkBindings backgrounds, BgArtworkIdentity identity, ReadOnlySpan<byte> characters) =>
        backgrounds.Bind(identity, EnemyVramLayout.KraidRoomBackgroundTiles.Offset, characters);

    /// <summary>Binds standard BG3 characters Kraid's death restores, <paramref name="wordOffset"/> words into their region (#1352).</summary>
    public static void BindKraidBg3Restore(BgArtworkBindings backgrounds, BgArtworkIdentity identity, int wordOffset, ReadOnlySpan<byte> characters) =>
        backgrounds.Bind(identity, EnemyVramLayout.KraidBg3Restore.Offset + wordOffset, characters);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the enemy graphics-set characters.</summary>
    public static VramDmaDestination EnemyTilesDma(int wordOffset) => VramDmaDestination.At(EnemyVramLayout.EnemyTiles, wordOffset);

    /// <summary>Destination of Draygon's opening Evir character sheet.</summary>
    public static VramDmaDestination DraygonEvirTilesDma => VramDmaDestination.At(EnemyVramLayout.DraygonEvirTiles);

    /// <summary>Destination of one Crocomire skeleton chunk <paramref name="wordOffset"/> words into the OBJ characters.</summary>
    /// <param name="wordOffset">The chunk's word offset from the OBJ character base.</param>
    public static VramDmaDestination CrocomireSkeletonTilesDma(int wordOffset) => VramDmaDestination.At(EnemyVramLayout.CrocomireSkeletonTiles, wordOffset);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the BG3 characters Kraid's death restores.</summary>
    public static VramDmaDestination KraidBg3RestoreDma(int wordOffset) => VramDmaDestination.At(EnemyVramLayout.KraidBg3Restore, wordOffset);
}
