using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>External map content supplied by the host; never serialized into a debugger snapshot.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    [NonSerialized] private bool hudArtworkRefreshPending;
    public AreaMapPresentationCatalog? MapPresentation
    {
        get => mapPresentation;
        set
        {
            mapPresentation = value;
            Hud.BindPresentation(value?.GameplayHud, Samus);
            MessageBox.BindPresentation(value?.GameplayMessageTitles, value?.GameplayMessagePanels,
                value?.GameplayMessageNotices);
            RoomPaletteFx.BindPresentationColors(value?.RoomPaletteFx);
            RoomLayer3Fx.AnimatedTileArtwork = value?.RoomFxAnimatedTiles;
            RoomLayer3Fx.Layer3Tilemaps = value?.RoomFxLayer3Tilemaps;
            RoomLayer3Fx.PaletteBlendColors = value?.RoomFxPaletteBlends;
            BombProjectiles.PowerBombExplosion.PresentationColors = value?.PowerBombFixedColors;
            BindSamusVisorPresentation();
            Enemies.MotherBrainHealthColors = value?.MotherBrainHealthPalette;
            Enemies.MotherBrainRainbowColors = value?.MotherBrainRainbowPalette;
            Enemies.EscapeTimerArtwork = value?.EscapeTimerTiles;
            Enemies.EscapeTypewriterPresentation = value?.EscapeTypewriter;
            hudArtworkRefreshPending = value is not null && Hud.IsInitialized;
            if (value is not null)
                VramWrites.RebindBusSource(HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount,
                    SuperMetroid.Core.Hardware.VramAssetId.StandardHudTiles);
            if (value is not null)
            {
                VramWrites.RebindBusSource(EscapeTimerTileRomData.FirstSourceAddress,
                    EscapeTimerTileAtlasFormat.FirstByteCount,
                    SuperMetroid.Core.Hardware.VramAssetId.EscapeTimerFirstTiles);
                VramWrites.RebindBusSource(EscapeTimerTileRomData.SecondSourceAddress,
                    EscapeTimerTileAtlasFormat.SecondByteCount,
                    SuperMetroid.Core.Hardware.VramAssetId.EscapeTimerSecondTiles);
            }
        }
    }

    /// <summary>Rebinds current artwork when a saved or newly constructed Samus becomes active.</summary>
    private void BindSamusVisorPresentation()
    {
        if (Samus is null)
            return;
        Samus.VisorPalette.PresentationColors = mapPresentation?.SamusVisorColors;
        Samus.Xray.PresentationColors = mapPresentation?.SamusVisorColors;
    }

    private void PublishReboundHudArtwork()
    {
        if (!hudArtworkRefreshPending) return;
        // Binding a restored session changes content ownership, not the already
        // published frame. Refresh at accepted NMI, before native queued writes.
        // The original upload's padding may now contain live BG2 tilemaps.
        mapPresentation!.HudTiles.LoadCharactersTo(Vram, HudTileAtlasFormat.DestinationWord * 2);
        hudArtworkRefreshPending = false;
    }
}
