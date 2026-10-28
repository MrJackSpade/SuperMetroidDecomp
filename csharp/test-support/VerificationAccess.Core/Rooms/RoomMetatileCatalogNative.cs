using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomMetatileCatalog"/>: block tables by cartridge source.</summary>
internal static class RoomMetatileCatalogNative
{
    /// <summary>Lookups by cartridge source.</summary>
    extension(RoomMetatileCatalog catalog)
    {
        /// <summary>The installed entry stored at <paramref name="address"/> on the cartridge.</summary>
        internal RoomMetatileAtlas GetAt(int address) => catalog.Get(RoomTilesetNative.BlockSetAt(address));
    }
}
