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

    internal static bool Contains(Type type) => Catalogs.Contains(type);
}
