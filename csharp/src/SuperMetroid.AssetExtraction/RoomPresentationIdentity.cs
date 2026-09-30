using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Named identities of the room catalogs actually bound by a playable host.</summary>
public static class RoomPresentationIdentity
{
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
