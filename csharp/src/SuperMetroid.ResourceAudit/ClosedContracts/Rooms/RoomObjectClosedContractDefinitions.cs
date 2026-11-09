using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class RoomObjectClosedContractDefinitions
{
    /// <summary>Common draw-list structure reviewed for all five bank-$84 PLM visual catalogs below.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ShotBlockObjectDraws.cs");
    /// <summary>The terrain blocks PLM draws share.</summary>
    private static readonly ReviewedSource SharedTerrainBlocks = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTerrainBlocks.cs");

    /// <summary>Limits shot-block, station, Torizo hand, Mother Brain glass, and Noob Tube draws to reviewed artwork owners.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectShotBlockVisuals", "plm-shot-block-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectShotBlockVisuals.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectStationVisuals", "plm-station-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectStationVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectStationDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/StationObjectDraws.cs")]),
        new("SuperMetroid.Core.Bosses.Torizo.Artwork.RoomObjectBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/RoomObjectBombTorizoHandVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/BombTorizoHandDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectBlockStrip.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/BombTorizoHandArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectNoobTubeVisuals", "plm-noob-tube-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectNoobTubeVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/NoobTubeObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/NoobTubeObjectDraws.cs")]),
    ];
}
