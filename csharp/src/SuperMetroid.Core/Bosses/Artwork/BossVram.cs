namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>The bosses' VRAM destinations and background bindings (#1395).</summary>
[PpuLayoutBoundary("rendering view: boss VRAM")]
public static class BossVram
{
    /// <summary>Binds the Kraid room's backdrop BG characters at their VRAM word (#1352).</summary>
    public static void BindKraidRoomBackgroundTiles(BgArtworkBindings backgrounds, BgArtworkIdentity identity, ReadOnlySpan<byte> characters) =>
        backgrounds.Bind(identity, BossVramLayout.KraidRoomBackgroundTiles.Offset, characters);

    /// <summary>Binds standard BG3 characters Kraid's death restores, <paramref name="wordOffset"/> words into their region (#1352).</summary>
    public static void BindKraidBg3Restore(BgArtworkBindings backgrounds, BgArtworkIdentity identity, int wordOffset, ReadOnlySpan<byte> characters) =>
        backgrounds.Bind(identity, BossVramLayout.KraidBg3Restore.Offset + wordOffset, characters);

    /// <summary>Destination of Draygon's opening Evir character sheet.</summary>
    public static VramDmaDestination DraygonEvirTilesDma => VramDmaDestination.At(BossVramLayout.DraygonEvirTiles);

    /// <summary>Destination of one Crocomire skeleton chunk <paramref name="wordOffset"/> words into the OBJ characters.</summary>
    /// <param name="wordOffset">The chunk's word offset from the OBJ character base.</param>
    public static VramDmaDestination CrocomireSkeletonTilesDma(int wordOffset) => VramDmaDestination.At(BossVramLayout.CrocomireSkeletonTiles, wordOffset);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the BG3 characters Kraid's death restores.</summary>
    public static VramDmaDestination KraidBg3RestoreDma(int wordOffset) => VramDmaDestination.At(BossVramLayout.KraidBg3Restore, wordOffset);
}
