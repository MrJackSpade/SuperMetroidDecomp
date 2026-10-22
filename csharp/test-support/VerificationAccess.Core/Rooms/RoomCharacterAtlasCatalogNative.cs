using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomCharacterAtlasCatalog"/>: character sheets by cartridge source.</summary>
internal static class RoomCharacterAtlasCatalogNative
{
    /// <summary>Lookups by cartridge source and the installed content identity.</summary>
    extension(RoomCharacterAtlasCatalog catalog)
    {
        /// <summary>The installed entry stored at <paramref name="address"/> on the cartridge.</summary>
        internal RoomCharacterAtlas GetAt(int address) => catalog.Get(RoomTilesetNative.CharacterSetAt(address));

        /// <summary>The content identity the installation records for the catalog.</summary>
        internal string ContentIdentity => RoomTilesetSources.CharacterIdentity(catalog);
    }
}
