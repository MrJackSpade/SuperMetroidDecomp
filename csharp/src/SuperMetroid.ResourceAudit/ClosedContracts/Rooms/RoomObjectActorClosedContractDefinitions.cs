using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class RoomObjectActorClosedContractDefinitions
{
    /// <summary>Shared PLM draw-run shape required by every reviewed room-actor provider.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ShotBlockObjectDraws.cs");
    /// <summary>The terrain blocks PLM draws share.</summary>
    private static readonly ReviewedSource SharedTerrainBlocks = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTerrainBlocks.cs");

    /// <summary>Downward gate, elevator, cannon, statue, and linked-restore
    /// visual domains with their complete source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectDownwardGateVisuals", "plm-downward-gate-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectDownwardGateVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/DownwardGateObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/DownwardGateObjectDraws.cs")]),
        new("SuperMetroid.Core.Enemies.Elevator.Artwork.RoomObjectElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/Artwork/RoomObjectElevatorPlatformVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/ElevatorPlatformObjectDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/ElevatorPlatformDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectBlockStrip.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/Artwork/ElevatorPlatformArtwork.cs")]),
        new("SuperMetroid.Core.Bosses.Draygon.Artwork.RoomObjectDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/Artwork/RoomObjectDraygonCannonVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/Artwork/DraygonCannonArtwork.cs")]),
        new("SuperMetroid.Core.Enemies.Chozo.Artwork.RoomObjectChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/Artwork/RoomObjectChozoStatueVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStatueObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStatueDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectBlockStrip.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/Artwork/ChozoStatueArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectLinkedRestoreVisuals", "plm-linked-restore-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectLinkedRestoreVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/LinkedRestoreObjectFamily.cs")]),
    ];
}
