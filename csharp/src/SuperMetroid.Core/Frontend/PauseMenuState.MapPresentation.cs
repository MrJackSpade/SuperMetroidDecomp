using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog;
        paletteAnimation.Bind(catalog?.HighlightCycle);
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
