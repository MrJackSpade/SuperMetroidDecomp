using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>The installed catalogs one room's character, palette, block and layout graph is compiled from.</summary>
public sealed record RoomAssetCatalogs
{
    /// <summary>Groups the four complete installed catalogs required to compile a room state without cartridge reads.</summary>
    /// <param name="characters">Character artwork selected by the room graphics-set source.</param>
    /// <param name="palettes">Static RGB5 palettes selected by the room graphics-set source.</param>
    /// <param name="metatiles">Visual 16-by-16 block definitions selected by the room graphics-set source.</param>
    /// <param name="visualLayouts">Compiled room layouts selected by their 24-bit level-data source.</param>
    public RoomAssetCatalogs(RoomCharacterAtlasCatalog characters, RoomStaticPaletteCatalog palettes,
        RoomMetatileCatalog metatiles, RoomVisualLayoutCatalog visualLayouts)
    {
        Characters = characters ?? throw new ArgumentNullException(nameof(characters));
        Palettes = palettes ?? throw new ArgumentNullException(nameof(palettes));
        Metatiles = metatiles ?? throw new ArgumentNullException(nameof(metatiles));
        VisualLayouts = visualLayouts ?? throw new ArgumentNullException(nameof(visualLayouts));
    }

    /// <summary>Gets the complete room-character artwork installation.</summary>
    public RoomCharacterAtlasCatalog Characters { get; init; }
    /// <summary>Gets the complete static room-palette installation.</summary>
    public RoomStaticPaletteCatalog Palettes { get; init; }
    /// <summary>Gets the complete visual room-metatile installation, excluding collision and BTS data.</summary>
    public RoomMetatileCatalog Metatiles { get; init; }
    /// <summary>Gets the complete source-keyed room visual-layout installation.</summary>
    public RoomVisualLayoutCatalog VisualLayouts { get; init; }
}
