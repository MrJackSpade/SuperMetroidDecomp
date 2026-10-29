using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Complete graphics-set source membership.</summary>
internal static class RoomArtworkClosedContractDefinitions
{
    /// <summary>Common retail tileset definition source reviewed by the character and palette contracts.</summary>
    private static readonly ReviewedSource TilesetSources = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTilesetDefinitions.cs");
    /// <summary>The retail tileset selection the character and palette contracts read the definitions through.</summary>
    private static readonly ReviewedSource TilesetSelection = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTilesets.cs");
    /// <summary>Closed source sets for all retail tileset artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Artwork.RoomCharacterAtlases", "room-complete-tileset-character-sources", ["Get"],
            [TilesetSources, TilesetSelection, new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomCharacterAtlases.cs")]),
        new("SuperMetroid.Core.Rooms.Artwork.RoomStaticPalettes", "room-complete-tileset-palette-sources", ["Get"],
            [TilesetSources, TilesetSelection, new("csharp/src/SuperMetroid.Core/Rooms/Artwork/RoomStaticPalettes.cs")]),
    ];
}
