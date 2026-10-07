using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>The installed catalogs one room's character, palette, block and layout graph is compiled from.</summary>
public sealed record RoomAssetCatalogs
{
    public RoomAssetCatalogs(RoomCharacterAtlasCatalog characters, RoomStaticPaletteCatalog palettes,
        RoomMetatileCatalog metatiles, RoomVisualLayoutCatalog visualLayouts)
    {
        Characters = characters ?? throw new ArgumentNullException(nameof(characters));
        Palettes = palettes ?? throw new ArgumentNullException(nameof(palettes));
        Metatiles = metatiles ?? throw new ArgumentNullException(nameof(metatiles));
        VisualLayouts = visualLayouts ?? throw new ArgumentNullException(nameof(visualLayouts));
    }

    public RoomCharacterAtlasCatalog Characters { get; init; }
    public RoomStaticPaletteCatalog Palettes { get; init; }
    public RoomMetatileCatalog Metatiles { get; init; }
    public RoomVisualLayoutCatalog VisualLayouts { get; init; }
}
