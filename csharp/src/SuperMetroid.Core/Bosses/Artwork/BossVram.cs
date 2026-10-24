using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>The bosses' VRAM destinations and background bindings (#1395).</summary>
[PpuLayoutBoundary("rendering view: boss VRAM")]
public static class BossVram
{
    /// <summary>Binds the Kraid room's backdrop BG characters at their VRAM word (#1352).</summary>
    public static void BindKraidRoomBackgroundTiles(BgArtworkBindings backgrounds, BgArtworkIdentity identity, ReadOnlySpan<byte> characters) =>
        backgrounds.Bind(identity, BossVramLayout.KraidRoomBackgroundTiles.Offset, characters);

    /// <summary>
    /// Binds quarter <paramref name="quarter"/> of the standard BG3 characters Kraid's death restores, as the VRAM queue binds
    /// the asset it uploads (#1352).
    /// </summary>
    public static void BindKraidBg3Restore(BgArtworkBindings backgrounds, int quarter, ReadOnlySpan<byte> characters) =>
        KraidBg3Restore(quarter, characters.ToArray()).BindTo(backgrounds);

    /// <summary>Quarter <paramref name="quarter"/> of the standard BG3 characters Kraid's death restores, at its VRAM word.</summary>
    public static BgCharacterUpload KraidBg3Restore(int quarter, ReadOnlyMemory<byte> characters) =>
        new(BgArtworkIdentity.Asset(KraidBackgroundRomData.StandardBg3AssetForQuarter(quarter)),
            KraidBg3RestoreDma(KraidBg3RestoreQuarterWord(quarter)).Word, characters);

    /// <summary>The first word of quarter <paramref name="quarter"/> within the BG3 characters Kraid's death restores.</summary>
    public static int KraidBg3RestoreQuarterWord(int quarter) => quarter * (KraidBackgroundRomData.StandardBg3QuarterBytes / 2);

    /// <summary>Destination of Draygon's opening Evir character sheet.</summary>
    public static VramDmaDestination DraygonEvirTilesDma => VramDmaDestination.At(BossVramLayout.DraygonEvirTiles);

    /// <summary>Queued-DMA destination <paramref name="wordOffset"/> words into the BG3 characters Kraid's death restores.</summary>
    public static VramDmaDestination KraidBg3RestoreDma(int wordOffset) => VramDmaDestination.At(BossVramLayout.KraidBg3Restore, wordOffset);
}
