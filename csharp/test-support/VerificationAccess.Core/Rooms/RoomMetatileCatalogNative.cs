using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomMetatileCatalog"/>: block tables by cartridge source.</summary>
internal static class RoomMetatileCatalogNative
{
    /// <summary>Lookups by cartridge source and the installed content identity.</summary>
    extension(RoomMetatileCatalog catalog)
    {
        /// <summary>The installed entry stored at <paramref name="address"/> on the cartridge.</summary>
        internal RoomMetatileAtlas GetAt(int address) => catalog.Get(RoomTilesetNative.BlockSetAt(address));

        /// <summary>The content identity the installation records for the catalog.</summary>
        internal string ContentIdentity => RoomTilesetSources.BlockIdentity(catalog);
    }
}
