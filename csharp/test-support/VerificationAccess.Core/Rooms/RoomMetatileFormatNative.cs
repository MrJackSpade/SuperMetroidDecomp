using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomMetatileFormat"/>: file names by cartridge source.</summary>
internal static class RoomMetatileFormatNative
{
    /// <summary>File names by cartridge source.</summary>
    extension(RoomMetatileFormat)
    {
        /// <summary>The file name of the block table stored at <paramref name="address"/>.</summary>
        internal static string SourceFileName(int address) => RoomMetatileFormat.FileName(RoomTilesetNative.BlockSetAt(address));
    }
}
