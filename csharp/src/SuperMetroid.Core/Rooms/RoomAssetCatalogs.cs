using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>The installed catalogs one room's character, palette, block, layout and level-allocation graph is compiled from.</summary>
public sealed record RoomAssetCatalogs
{
    /// <summary>Groups the five complete installed catalogs required to compile a room state without cartridge reads.</summary>
    /// <param name="characters">Character artwork selected by the room graphics-set source.</param>
    /// <param name="palettes">Static RGB5 palettes selected by the room graphics-set source.</param>
    /// <param name="metatiles">Visual 16-by-16 block definitions selected by the room graphics-set source.</param>
    /// <param name="visualLayouts">Compiled room layouts selected by their 24-bit level-data source.</param>
    /// <param name="levelStreams">Stock native level allocations selected by their 24-bit level-data source.</param>
    public RoomAssetCatalogs(RoomCharacterAtlasCatalog characters, RoomStaticPaletteCatalog palettes,
        RoomMetatileCatalog metatiles, RoomVisualLayoutCatalog visualLayouts, RoomLevelStreamCatalog levelStreams)
    {
        Characters = Ensure.NotNull(characters);
        Palettes = Ensure.NotNull(palettes);
        Metatiles = Ensure.NotNull(metatiles);
        VisualLayouts = Ensure.NotNull(visualLayouts);
        LevelStreams = Ensure.NotNull(levelStreams);
    }

    /// <summary>Gets the complete room-character artwork installation.</summary>
    public RoomCharacterAtlasCatalog Characters { get; init; }
    /// <summary>Gets the complete static room-palette installation.</summary>
    public RoomStaticPaletteCatalog Palettes { get; init; }
    /// <summary>Gets the complete visual room-metatile installation, excluding collision and BTS data.</summary>
    public RoomMetatileCatalog Metatiles { get; init; }
    /// <summary>Gets the complete source-keyed room visual-layout installation.</summary>
    public RoomVisualLayoutCatalog VisualLayouts { get; init; }
    /// <summary>Gets the stock native collision/BTS and streaming allocations, which visual overrides never alter.</summary>
    public RoomLevelStreamCatalog LevelStreams { get; init; }
}
