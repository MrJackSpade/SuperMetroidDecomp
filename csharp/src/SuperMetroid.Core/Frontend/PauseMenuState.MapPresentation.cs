using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog;
        paletteAnimation.Bind(catalog?.HighlightCycle);
        if (catalog is not null) catalog.PauseTiles.LoadTo(vram, PauseTileAtlasFormat.DestinationByte);
        else vram.LoadBytes(PauseTileAtlasFormat.DestinationByte,
            SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus, PauseTileAtlasFormat.SourceAddress, PauseTileAtlasFormat.ByteCount));
        if (catalog is not null) catalog.Sprites.LoadArtworkTo(vram, MapSpriteFormat.PauseDestination);
        else vram.LoadBytes(MapSpriteFormat.PauseDestination,
            SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus, MapSpriteFormat.SourceAddress, MapSpriteFormat.ByteCount));
        if (catalog is not null)
            for (int color = 0; color < SuperMetroid.Core.Hardware.SnesCgram.ColorCount; color++)
                // Live highlights and reserve-arrow colors belong to their animation
                // owners. Replacing the static base must not reset their phase.
                if ((color < MapAnimationRomData.PaletteDestination || color >= MapAnimationRomData.PaletteDestination + MapPaletteCycleFormat.ColorCount) &&
                    color != PauseReserveArrowRomData.Color6Index && color != PauseReserveArrowRomData.Color11Index)
                    cgram.SetColor(color, catalog.Palettes.Pause[color]);
        if (catalog is not null) catalog.Tiles.LoadTo(vram, 0);
        else vram.LoadBytes(0, SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus,
            MapTileAtlasFormat.SourceAddress, MapTileAtlasFormat.ByteCount));
        if (catalog is not null) catalog.HudTiles.LoadTo(vram, HudTileAtlasFormat.DestinationWord * 2);
        else vram.LoadBytes(HudTileAtlasFormat.DestinationWord * 2,
            SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus, HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount));
        // Preserve the serialized scroll position and transition timing. Only refresh
        // BG1 when it currently contains the map; the equipment page shares that VRAM.
        if (ScreenMode == 0) LoadPauseMapTilemap();
    }
}
