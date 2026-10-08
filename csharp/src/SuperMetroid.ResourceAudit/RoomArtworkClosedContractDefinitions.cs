namespace SuperMetroid.ResourceAudit;

/// <summary>Complete graphics-set source membership and the contiguous sky transfer store.</summary>
internal static class RoomArtworkClosedContractDefinitions
{
    private static readonly ReviewedSource TilesetSources = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs", "8E4E51FF9113967EAE509E86D0AD47B7F9C29C55325D927CDFE68DD0DDD30F43");
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.RoomCharacterAtlasCatalog", "room-complete-tileset-character-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlasCatalog.cs", "E8800C6B90309ADC667C7014DDE81918307A5D56B2587BEF968E2A665B858BA6")]),
        new("SuperMetroid.Core.Assets.RoomMetatileCatalog", "room-complete-tileset-metatile-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomMetatileCatalog.cs", "A1BA5CE10E43B8B0FA70C8ECB783ADEA47F824B53466CC69101004E7596D2351")]),
        new("SuperMetroid.Core.Assets.RoomStaticPaletteCatalog", "room-complete-tileset-palette-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomStaticPaletteCatalog.cs", "77527587EDA3C37432261E2903334603D5A7EF75C4AFABADBB9108AADD488D62")]),
        new("SuperMetroid.Core.Assets.RoomSkyTilemapCatalog", "room-complete-seven-page-sky-transfer-store", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "76A235967E736A9B6FDC9CA2E67B26FEC0DBE71074D63F83A901608AC7E48ABA"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs", "9FA2182B31CC5B874EC8DD279E72208D7EAA60147C92BF6099B21DCF99863A07"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "B3F2088A2CBEA03FC3F2E392CDBC832109DFC1A1AFEA51C7456B20372AB4D1EC")]),
    ];
}
