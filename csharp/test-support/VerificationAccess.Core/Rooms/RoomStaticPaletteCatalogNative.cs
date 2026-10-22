using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomStaticPaletteCatalog"/>: palettes by cartridge source.</summary>
internal static class RoomStaticPaletteCatalogNative
{
    /// <summary>Lookups by cartridge source and the installed content identity.</summary>
    extension(RoomStaticPaletteCatalog catalog)
    {
        /// <summary>The installed entry stored at <paramref name="address"/> on the cartridge.</summary>
        internal RoomStaticPalette GetAt(int address) => catalog.Get(RoomTilesetNative.PaletteSetAt(address));

        /// <summary>The content identity the installation records for the catalog.</summary>
        internal string ContentIdentity => RoomTilesetSources.PaletteIdentity(catalog);
    }
}
