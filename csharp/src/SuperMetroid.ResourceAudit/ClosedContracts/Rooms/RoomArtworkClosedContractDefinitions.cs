using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Complete graphics-set source membership and the contiguous sky transfer store.</summary>
internal static class RoomArtworkClosedContractDefinitions
{
    /// <summary>Common retail tileset definition source reviewed by the character and palette contracts.</summary>
    private static readonly ReviewedSource TilesetSources = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTilesetDefinitions.cs");
    /// <summary>Closed source sets for all retail tileset artwork and the seven-page room sky transfer store.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Artwork.RoomCharacterAtlases", "room-complete-tileset-character-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomCharacterAtlases.cs")]),
        new("SuperMetroid.Core.Rooms.Artwork.RoomStaticPalettes", "room-complete-tileset-palette-sources", ["Get"],
            [TilesetSources, new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomStaticPalettes.cs")]),
        new("SuperMetroid.Core.Rooms.Artwork.RoomSkyTilemaps", "room-complete-seven-page-sky-transfer-store",
            ["Page", "Tilemap", "Row"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomSkyTilemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomSkyTilemapFormat.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Scrolling/SkyTilemap.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Scrolling/ScrollingSkyChunks.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomBackgroundTilemapAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/ScrollingSkyBands.cs"),
             new("csharp/src/SuperMetroid.Core/Fx/ScrollingSkyBandDefinitions.cs")]),
    ];
}
