namespace SuperMetroid.ResourceAudit;

/// <summary>Complete graphics-set source membership and the contiguous sky transfer store.</summary>
internal static class RoomArtworkClosedContractDefinitions
{
    /// <summary>Stores tileset sources.</summary>
    private static readonly ReviewedSource TilesetSources = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs");
    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.RoomCharacterAtlasCatalog", "room-complete-tileset-character-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlasCatalog.cs")]),
        new("SuperMetroid.Core.Assets.RoomMetatileCatalog", "room-complete-tileset-metatile-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomMetatileCatalog.cs")]),
        new("SuperMetroid.Core.Assets.RoomStaticPaletteCatalog", "room-complete-tileset-palette-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomStaticPaletteCatalog.cs")]),
        new("SuperMetroid.Core.Assets.RoomSkyTilemapCatalog", "room-complete-seven-page-sky-transfer-store", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs")]),
    ];
}
