namespace SuperMetroid.ResourceAudit;

/// <summary>Complete graphics-set source membership and the contiguous sky transfer store.</summary>
internal static class RoomArtworkClosedContractDefinitions
{
    /// <summary>Fingerprint for the source list that maps room tilesets to their native graphics addresses.</summary>
    private static readonly ReviewedSource TilesetSources = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs", "AF68FB26662D9CF4BD94B6DFD4BAF632089B67FFD45BA7E7C6FB377B1281B810");
    /// <summary>Fingerprints closing room character, metatile, palette and sky transfer sources.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.RoomCharacterAtlasCatalog", "room-complete-tileset-character-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlasCatalog.cs", "09D1137401131608F581E6012F1D805B0D9F39C1D5669B73AAEA596F5AD11A39")]),
        new("SuperMetroid.Core.Assets.RoomMetatileCatalog", "room-complete-tileset-metatile-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomMetatileCatalog.cs", "5D88A49F70038EFEB8210667DDE989DFC239464A46F378ABF74C19C7B79DF9CA")]),
        new("SuperMetroid.Core.Assets.RoomStaticPaletteCatalog", "room-complete-tileset-palette-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Assets/RoomStaticPaletteCatalog.cs", "3EC1FDFDF20B3B3AAA7EC7AD4F64403883739359D31D069ACF7E5C3E46572BB6")]),
        new("SuperMetroid.Core.Assets.RoomSkyTilemapCatalog", "room-complete-seven-page-sky-transfer-store", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "E3F623660F01CB63B35C26C82589FC4A3832460D29C7BAA9D6707B601C92A2F5"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs", "3555499032A6A3B2A2E603076176F39E6177645970E04CD6D021489855EC4335"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "AA01AC4CED546BD6E38C37778DBC9EDF179B42843D3B26C4651FD835B51E822D")]),
    ];
}
