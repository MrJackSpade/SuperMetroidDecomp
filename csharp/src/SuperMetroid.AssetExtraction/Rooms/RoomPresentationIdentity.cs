using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Named identities of the room catalogs actually bound by a playable host.</summary>
public static class RoomPresentationIdentity
{
    /// <summary>
    /// Captures five named identities for the room presentation catalogs actually selected by a host. How a room's blocks
    /// look is the block sheets' identity (#1431), which the host records with them.
    /// </summary>
    /// <param name="characters">Non-null selected room character atlases.</param>
    /// <param name="palettes">Non-null selected static room palette rows, not current animated CGRAM state.</param>
    /// <param name="backgrounds">Non-null selected background tilemaps.</param>
    /// <param name="skies">Non-null selected compiled sky tilemap catalog.</param>
    /// <param name="layouts">Non-null selected initial foreground/background room visual layouts.</param>
    /// <returns>A new ordinal-keyed dictionary of decoded-content identities, using installation directory names except the sky key <c>RoomSkyTilemaps</c>.</returns>
    /// <remarks>Includes already selected overrides, reads no files, and retains no catalog references. These component identities remain separate from cartridge/engine provenance, room collision, and active mutable room state.</remarks>
    public static IReadOnlyDictionary<string, string> Create(
        RoomCharacterAtlases characters, RoomStaticPalettes palettes,
        RoomBackgroundTilemaps backgrounds,
        RoomSkyTilemaps skies, RoomLevelArtworks layouts) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomCharacterDirectoryName] = RoomTilesetSources.CharacterIdentity(characters),
            [GameInstallationLayout.RoomPaletteDirectoryName] = RoomTilesetSources.PaletteIdentity(palettes),
            [GameInstallationLayout.RoomBackgroundTilemapDirectoryName] = RoomBackgroundTilemapArtworkFiles.ContentIdentity(backgrounds),
            [nameof(RoomSkyTilemaps)] = SkyIdentity(skies),
            [GameInstallationLayout.RoomVisualLayoutDirectoryName] = layouts.ContentIdentity,
        };

    /// <summary>The content identity of the seven selected sky pages, hashed as the cartridge's tilemap words in scrolling order.</summary>
    public static string SkyIdentity(RoomSkyTilemaps skies) => SelectedPresentationHash.Create(nameof(RoomSkyTilemaps),
        content => content.Append("pages", BackgroundTileWords.ToBytes(Ensure.NotNull(skies).Pages.Span)));
}
