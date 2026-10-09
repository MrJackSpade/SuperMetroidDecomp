using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Desktop;

/// <summary>Exact catalog layouts that briefly serialized a derived presentation fingerprint.</summary>
internal static class DebuggerPresentationIdentityFieldDefinitions
{
    /// <summary>The retired compiler-generated string field, never executable game state.</summary>
    internal const string RetiredFieldName = "<ContentIdentity>k__BackingField";

    /// <summary>SHA-256 serialized as two hexadecimal characters per digest byte.</summary>
    internal const int DigestHexLength = 64;

    // This is an explicit historical schema inventory, not permission to discard
    // arbitrary unknown fields or every property named ContentIdentity.
    /// <summary>Catalog types from historical states that carried the retired derived fingerprint field.</summary>
    private static readonly HashSet<Type> Catalogs =
    [
        typeof(RoomPlmBlueDoorVisualCatalog),
        typeof(RoomPlmBombTorizoHandVisualCatalog),
        typeof(RoomPlmChozoStatueVisualCatalog),
        typeof(RoomPlmColoredDoorVisualCatalog),
        typeof(RoomPlmBotwoonWallVisualCatalog),
        typeof(RoomPlmCollectibleVisualCatalog),
        typeof(RoomPlmEyeDoorVisualCatalog),
        typeof(RoomPlmElevatorPlatformVisualCatalog),
        typeof(RoomPlmDynamicCollectibleArtCatalog),
        typeof(RoomPlmEscapeGateVisualCatalog),
        typeof(RoomPlmDraygonCannonVisualCatalog),
        typeof(RoomPlmDownwardGateVisualCatalog),
        typeof(RoomPlmCrocomireVisualCatalog),
        typeof(RoomPlmLinkedRestoreVisualCatalog),
        typeof(RoomPlmGreyDoorVisualCatalog),
        typeof(RoomPlmGrappleBlockVisualCatalog),
        typeof(RoomPlmKraidVisualCatalog),
        typeof(RoomPlmNoobTubeVisualCatalog),
        typeof(RoomPlmMotherBrainGlassVisualCatalog),
        typeof(RoomPlmMaridiaElevatubeVisualCatalog),
        typeof(RoomPlmMotherBrainFakeDeathVisualCatalog),
        typeof(RoomPlmSamusEaterVisualCatalog),
        typeof(RoomPlmShotBlockVisualCatalog),
        typeof(RoomBackgroundTilemapCatalog),
        typeof(RoomPlmStationVisualCatalog),
        typeof(RoomVisualLayoutCatalog),
        typeof(RoomPlmSpeedBoosterVisualCatalog),
        typeof(RoomPlmSporeSpawnCeilingVisualCatalog),
        typeof(RoomPlmTourianAccessVisualCatalog),
        typeof(RoomCharacterAtlasCatalog),
        typeof(RoomMetatileCatalog),
        typeof(RoomSkyTilemapCatalog),
        typeof(RoomStaticPaletteCatalog),
        typeof(SamusArmCannonArtworkCatalog),
        typeof(SamusAtmosphericArtworkCatalog),
        typeof(SamusBodyArtworkCatalog),
        typeof(SamusDeathPaletteArtworkCatalog),
        typeof(SamusDeathTileAtlas),
        typeof(SamusSpritemapArtworkCatalog),
    ];

    /// <summary>Checks whether an exact catalog type belonged to the retired-field schema.</summary>
    /// <param name="type">Runtime type whose serialized layout is being migrated.</param>
    /// <returns><see langword="true"/> only for an explicitly listed catalog type.</returns>
    internal static bool Contains(Type type) => Catalogs.Contains(type);
}
