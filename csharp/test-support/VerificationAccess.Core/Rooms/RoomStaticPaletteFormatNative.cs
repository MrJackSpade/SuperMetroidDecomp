using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomStaticPaletteFormat"/>: file names by cartridge source.</summary>
internal static class RoomStaticPaletteFormatNative
{
    /// <summary>File names by cartridge source.</summary>
    extension(RoomStaticPaletteFormat)
    {
        /// <summary>The file name of the palette stored at <paramref name="address"/>.</summary>
        internal static string SourceFileName(int address) => RoomStaticPaletteFormat.FileName(RoomTilesetNative.PaletteSetAt(address));
    }
}
