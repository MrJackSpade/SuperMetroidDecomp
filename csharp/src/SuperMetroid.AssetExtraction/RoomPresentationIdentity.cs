using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Named identities of the room catalogs actually bound by a playable host.</summary>
public static class RoomPresentationIdentity
{
    /// <summary>Captures six named identities for the room presentation catalogs actually selected by a host.</summary>
    /// <param name="characters">Non-null selected room character atlases.</param>
    /// <param name="palettes">Non-null selected static room palette rows, not current animated CGRAM state.</param>
    /// <param name="metatiles">Non-null selected metatile character/attribute compositions.</param>
    /// <param name="backgrounds">Non-null selected background tilemaps.</param>
    /// <param name="skies">Non-null selected compiled sky tilemap catalog.</param>
    /// <param name="layouts">Non-null selected initial foreground/background room visual layouts.</param>
    /// <returns>A new ordinal-keyed dictionary of decoded-content identities, using installation directory names except the sky key <c>RoomSkyTilemapCatalog</c>.</returns>
    /// <remarks>Includes already selected overrides, reads no files, and retains no catalog references. These component identities remain separate from cartridge/engine provenance, room collision, and active mutable room state.</remarks>
    public static IReadOnlyDictionary<string, string> Create(
        RoomCharacterAtlasCatalog characters, RoomStaticPaletteCatalog palettes,
        RoomMetatileCatalog metatiles, RoomBackgroundTilemapCatalog backgrounds,
        RoomSkyTilemapCatalog skies, RoomVisualLayoutCatalog layouts) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GameInstallationLayout.RoomCharacterDirectoryName] = characters.ContentIdentity,
            [GameInstallationLayout.RoomPaletteDirectoryName] = palettes.ContentIdentity,
            [GameInstallationLayout.RoomMetatileDirectoryName] = metatiles.ContentIdentity,
            [GameInstallationLayout.RoomBackgroundTilemapDirectoryName] = backgrounds.ContentIdentity,
            [nameof(RoomSkyTilemapCatalog)] = skies.ContentIdentity,
            [GameInstallationLayout.RoomVisualLayoutDirectoryName] = layouts.ContentIdentity,
        };
}
