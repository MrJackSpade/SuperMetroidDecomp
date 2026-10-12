using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    [NonSerialized] private bool hudArtworkRefreshPending;
    /// <summary>Gets or binds the host-owned presentation catalog used by maps, HUD, messages, room effects, Samus/enemy colors, and named VRAM assets.</summary>
    /// <remarks>Binding nonnull artwork schedules an initialized HUD refresh and rebinds compiled bus-source identities; assigning null removes presentation owners without rewriting already published VRAM.</remarks>
    [field: NonSerialized]     public AreaMapPresentationCatalog? MapPresentation
    {
        get;
        set
        {
            field = value;
            Hud.BindPresentation(value?.GameplayHud, Samus);
            MessageBox.BindPresentation(value?.GameplayMessageTitles, value?.GameplayMessagePanels,
                value?.GameplayMessageNotices);
            RoomLayer3Fx.AnimatedTileArtwork = value?.RoomFxAnimatedTiles;
            RoomLayer3Fx.Layer3Tilemaps = value?.RoomFxLayer3Tilemaps;
            RoomLayer3Fx.PaletteBlendColors = value?.RoomFxPaletteBlends;
            BombProjectiles.PowerBombExplosion.PresentationColors = value?.PowerBombFixedColors;
            BindSamusPalettePresentation();
            Enemies.BindMapPresentation(value);
            hudArtworkRefreshPending = value is not null && Hud.IsInitialized;
            if (value is not null)
                RebindLegacyArtworkTransfers();
        }
    }

    /// <summary>
    /// Points pending transfers a restored legacy queue still sources from the cartridge bus at the
    /// installed HUD, Kraid BG3 and escape-timer artwork, matched by their native source and size.
    /// </summary>
    [SuperMetroid.Core.Hardware.PpuLayoutBoundary("codec: legacy queued VRAM DMA records rebound from native bus sources to installed artwork")]
    private void RebindLegacyArtworkTransfers()
    {
        VramWrites.RebindBusSource(HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount,
            SuperMetroid.Core.Hardware.VramAssetId.StandardHudTiles);
        for (int quarter = 0; quarter < Game.KraidBackgroundRomData.StandardBg3TransferCount;
             quarter++)
            VramWrites.RebindBusSource(
                Game.KraidBackgroundRomData.StandardBg3TilesAddress +
                    quarter * Game.KraidBackgroundRomData.StandardBg3TransferBytes,
                Game.KraidBackgroundRomData.StandardBg3TransferBytes,
                Game.KraidBackgroundRomData.StandardBg3AssetForQuarter(quarter));
        VramWrites.RebindBusSource(EscapeTimerTileRomData.FirstSourceAddress,
            EscapeTimerTileAtlasFormat.FirstByteCount,
            SuperMetroid.Core.Hardware.VramAssetId.EscapeTimerFirstTiles);
        VramWrites.RebindBusSource(EscapeTimerTileRomData.SecondSourceAddress,
            EscapeTimerTileAtlasFormat.SecondByteCount,
            SuperMetroid.Core.Hardware.VramAssetId.EscapeTimerSecondTiles);
    }

    /// <summary>Rebinds current artwork when a saved or newly constructed Samus becomes active.</summary>
    private void BindSamusPalettePresentation()
    {
        if (Samus is null)
            return;
        Samus.VisorPalette.PresentationColors = MapPresentation?.SamusVisorColors;
        Samus.Xray.PresentationColors = MapPresentation?.SamusVisorColors;
        Samus.Drained.PresentationColors = MapPresentation?.SamusHyperBeamColors;
        Samus.SuitColors = MapPresentation?.SamusSuitColors;
        Samus.FullBodyCycleColors = MapPresentation?.SamusFullBodyCycleColors;
        Samus.ChargeColors = MapPresentation?.SamusChargeColors;
        // All Samus constructors call this rebind before priming the first visual DMA.
        Samus.TileTransfers.BindArtwork(SamusBodyArt);
        Samus.ArmCannon.Artwork = SamusBodyArt?.ArmCannon;
    }

    private void PublishReboundHudArtwork()
    {
        if (!hudArtworkRefreshPending) return;
        // Binding a restored session changes content ownership, not the already
        // published frame. Refresh at accepted NMI, before native queued writes.
        // The original upload's padding may now contain live BG2 tilemaps.
        // Kraid's room changes BG34NBA to place HUD characters at $2000 while
        // his private BG2 tilemap occupies the ordinary $4000 HUD character
        // address. A restored-state art rebind must honor the *current* PPU
        // character base, or the next NMI paints HUD pixels across Kraid.
        MapPresentation!.HudTiles.LoadCharactersTo(HudCharactersAtCurrentBase());
        hudArtworkRefreshPending = false;
    }
}
