using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Core.Rooms;

/// <summary>Verification's native view of <see cref="RoomCharacterAtlases"/>: character sheets by cartridge source.</summary>
internal static class RoomCharacterAtlasesNative
{
    /// <summary>Lookups by cartridge source and the installed content identity.</summary>
    extension(RoomCharacterAtlases catalog)
    {
        /// <summary>The installed entry stored at <paramref name="address"/> on the cartridge.</summary>
        internal RoomCharacterAtlas GetAt(int address) => catalog.Get(RoomTilesetNative.CharacterSetAt(address));

        /// <summary>
        /// The content identity of the catalog: the shared characters, then each sheet's characters by its name in ordinal
        /// order.
        /// </summary>
        internal string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomCharacterAtlases), content =>
        {
            content.Append("shared", catalog.Cre.Characters.Span);
            foreach ((string source, RoomCharacterAtlas selected) in catalog.Sheets.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value)
                .OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                content.Append("source", System.Text.Encoding.UTF8.GetBytes(source));
                content.Append("transfer", selected.Characters.Span);
            }
        });
    }
}
