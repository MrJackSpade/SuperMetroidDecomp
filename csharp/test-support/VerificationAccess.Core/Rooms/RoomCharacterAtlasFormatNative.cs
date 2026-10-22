using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomCharacterAtlasFormat"/>: file names by cartridge source.</summary>
internal static class RoomCharacterAtlasFormatNative
{
    /// <summary>File names by cartridge source.</summary>
    extension(RoomCharacterAtlasFormat)
    {
        /// <summary>The file name of the character sheet stored at <paramref name="address"/>.</summary>
        internal static string SourceFileName(int address) => RoomCharacterAtlasFormat.FileName(RoomTilesetNative.CharacterSetAt(address));
    }
}
