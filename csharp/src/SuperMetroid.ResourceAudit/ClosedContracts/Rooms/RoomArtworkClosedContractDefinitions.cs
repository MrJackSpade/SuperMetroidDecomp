using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Complete graphics-set source membership.</summary>
internal static class RoomArtworkClosedContractDefinitions
{
    /// <summary>The cartridge's graphics-set numbering the palette contract's import reads the tileset table by.</summary>
    private static readonly ReviewedSource TilesetSources = new(
        "csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomGraphicsSetCodec.cs");
    /// <summary>The retail tileset selection the palette contract reads the definitions through.</summary>
    private static readonly ReviewedSource TilesetSelection = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTilesets.cs");
    /// <summary>Closed source sets for all retail tileset artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Rooms.RoomStaticPalettes", "room-complete-tileset-palette-sources", ["Get"],
            [TilesetSources, TilesetSelection, new("csharp/src/SuperMetroid.AssetExtraction/Rooms/RoomStaticPalettes.cs")]),
    ];
}
