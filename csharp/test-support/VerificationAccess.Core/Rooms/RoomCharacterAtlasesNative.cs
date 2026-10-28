using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomCharacterAtlases"/>: character sheets by cartridge source.</summary>
internal static class RoomCharacterAtlasesNative
{
    /// <summary>Lookups by cartridge source and the installed content identity.</summary>
    extension(RoomCharacterAtlases catalog)
    {
        /// <summary>The installed entry stored at <paramref name="address"/> on the cartridge.</summary>
        internal RoomCharacterAtlas GetAt(int address) => catalog.Get(RoomTilesetNative.CharacterSetAt(address));

        /// <summary>The content identity the installation records for the catalog.</summary>
        internal string ContentIdentity => RoomTilesetSources.CharacterIdentity(catalog);
    }
}
