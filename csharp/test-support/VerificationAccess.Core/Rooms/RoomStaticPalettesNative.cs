using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomStaticPalettes"/>: palettes by cartridge source.</summary>
internal static class RoomStaticPalettesNative
{
    /// <summary>Lookups by cartridge source.</summary>
    extension(RoomStaticPalettes catalog)
    {
        /// <summary>The installed entry stored at <paramref name="address"/> on the cartridge.</summary>
        internal RoomStaticPalette GetAt(int address) => catalog.Get(RoomTilesetNative.PaletteSetAt(address));
    }
}
