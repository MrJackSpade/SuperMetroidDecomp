using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomCharacterSheetFiles"/>: file names by cartridge source.</summary>
internal static class RoomCharacterAtlasFormatNative
{
    /// <summary>The installed name of the character sheet the cartridge stores at a source address.</summary>
    extension(RoomCharacterSheetFiles)
    {
        /// <summary>The installed file name of the character sheet stored at <paramref name="address"/>.</summary>
        internal static string SourceFileName(int address) => RoomCharacterSheetFiles.FileName(RoomTilesetNative.CharacterSetAt(address));
    }
}
