using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    /// <summary>Shared PLM draw-run shape required by every reviewed room-actor provider.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/ShotBlockPlmDraws.cs");
    /// <summary>The level words PLM draws share.</summary>
    private static readonly ReviewedSource SharedLevelWords = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmLevelWordDefinitions.cs");

    /// <summary>Downward gate, elevator, cannon, statue, and linked-restore
    /// visual domains with their complete source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmDownwardGateVisuals", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmDownwardGateVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/DownwardGatePlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/DownwardGatePlmDraws.cs")]),
        new("SuperMetroid.Core.Enemies.Elevator.Artwork.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/Artwork/RoomPlmElevatorPlatformVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/ElevatorPlatformPlmDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/ElevatorPlatformDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/PlmBlockStrip.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/Artwork/ElevatorPlatformArtwork.cs")]),
        new("SuperMetroid.Core.Bosses.Draygon.Artwork.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/Artwork/RoomPlmDraygonCannonVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/Artwork/DraygonCannonArtwork.cs")]),
        new("SuperMetroid.Core.Enemies.Chozo.Artwork.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/Artwork/RoomPlmChozoStatueVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStatuePlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStatueDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/PlmBlockStrip.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/Artwork/ChozoStatueArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmLinkedRestoreVisuals", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmLinkedRestoreVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmLinkedRestoreDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/LinkedRestorePlmDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/LinkedRestorePlmFamily.cs")]),
    ];
}
