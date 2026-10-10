using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Optional installed room base-palette catalog retained for runtime construction and rebinding after state restoration.</summary>
    [NonSerialized] private RoomStaticPaletteCatalog? roomPaletteArt;

    /// <summary>Attaches installed base room colors after construction or state restoration.</summary>
    public void BindRoomPaletteArt(RoomStaticPaletteCatalog? catalog)
    {
        roomPaletteArt = catalog;
        if (runtime is not null) runtime.RoomPaletteArt = catalog;
    }
}
