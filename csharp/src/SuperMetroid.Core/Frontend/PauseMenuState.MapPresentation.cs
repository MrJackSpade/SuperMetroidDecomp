using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Buffers.Binary;

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
        LoadPauseBackdrop();
        RefreshPauseButtonArtwork();
        // Reapply only the wireframe patch, in its native footprint. The surrounding
        // mutable labels include intentional cartridge overruns and must not be rebuilt.
        WriteSamusWireframe();
        if (ScreenMode != 0) UploadEquipmentTilemap();
        // The serialized live button palette rows still own their overlay on
        // the refreshed backdrop, including a Start/fade highlight.
        // Do not rerun controls or rebuild equipment: that would erase native
        // same-frame label overruns and change the state being restored.
        vram.LoadBytes(PauseMenuLayout.ButtonRowsDestinationWord * 2,
            pauseButtonTilemap.AsSpan(PauseMenuLayout.ButtonRowsSourceOffset, PauseMenuLayout.ButtonRowsByteCount));
        // Preserve the serialized scroll position and transition timing. Only refresh
        // BG1 when it currently contains the map; the equipment page shares that VRAM.
        if (ScreenMode == 0) LoadPauseMapTilemap();
    }

    private void LoadPauseBackdrop()
    {
        if (mapPresentation is not null)
            mapPresentation.PauseBackdrops.LoadTo(vram, PauseMenuLayout.Bg2TilemapWord * 2, area);
        else
        {
            vram.LoadBytes(PauseMenuLayout.Bg2TilemapWord * 2,
                SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus, PauseBackdropDefinitions.FrameSource, PauseBackdropDefinitions.ByteCount));
            LoadNativePauseAreaLabel();
        }
    }

    private void RefreshPauseButtonArtwork()
    {
        byte[] replacement = mapPresentation?.PauseBackdrops.CreateButtonTilemap() ??
            SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus,
                PauseBackdropDefinitions.ButtonSource, PauseBackdropDefinitions.ButtonCells * 2);
        // Only these palette bits are owned by the live menu. Carry them forward
        // word-by-word (including restored historical states) rather than inferring
        // a mode from ScreenMode, which lags the button highlight during fades.
        foreach (var span in PauseMenuLayout.ButtonLabelSpans)
        for (int index = 0; index < span.Count; index++)
        {
            int offset = (span.Word - PauseMenuLayout.ButtonSourceWordOrigin + index) * 2;
            var previous = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(pauseButtonTilemap.AsSpan(offset)));
            var current = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(replacement.AsSpan(offset)));
            BinaryPrimitives.WriteUInt16LittleEndian(replacement.AsSpan(offset), current.WithPaletteIndex(previous.PaletteIndex).Raw);
        }
        replacement.CopyTo(pauseButtonTilemap, 0);
    }
}
