using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Named identities of the room catalogs actually bound by a playable host.</summary>
public static class RoomPresentationIdentity
{
    /// <summary>
    /// Captures three named identities for the room presentation catalogs actually selected by a host. How a room's blocks
    /// and backgrounds look is the block sheets' identity (#1431, #1466), which the host records with them; the library
    /// background and sky tilemaps those images are composed from are the import pipeline's input, which no host binds.
    /// </summary>
    /// <param name="characters">Non-null selected room character atlases.</param>
    /// <param name="palettes">Non-null selected static room palette rows, not current animated CGRAM state.</param>
    /// <param name="layouts">Non-null selected initial foreground/background room visual layouts.</param>
    /// <returns>A new ordinal-keyed dictionary of decoded-content identities, using installation directory names.</returns>
    /// <remarks>Includes already selected overrides, reads no files, and retains no catalog references. These component identities remain separate from cartridge/engine provenance, room collision, and active mutable room state.</remarks>
    public static IReadOnlyDictionary<string, string> Create(RoomCharacterAtlases characters, RoomStaticPalettes palettes, RoomLevelArtworks layouts) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomCharacterDirectoryName] = RoomTilesetSources.CharacterIdentity(characters),
            [GameInstallationLayout.RoomPaletteDirectoryName] = RoomTilesetSources.PaletteIdentity(palettes),
            [GameInstallationLayout.RoomVisualLayoutDirectoryName] = layouts.ContentIdentity,
        };
}
