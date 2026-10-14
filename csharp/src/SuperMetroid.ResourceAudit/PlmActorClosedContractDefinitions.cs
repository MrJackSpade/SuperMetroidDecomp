namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    /// <summary>Shared PLM draw-run shape required by every reviewed room-actor provider.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");

    /// <summary>Downward gate, elevator, cannon, statue, and linked-restore
    /// visual domains with their complete source dependencies.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmDownwardGateVisualCatalog", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDownwardGateVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/DownwardGatePlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmElevatorPlatformVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Elevator/ElevatorPlatformPlmDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDraygonCannonVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonCannonPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmChozoStatueVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStatuePlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmLinkedRestoreVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmLinkedRestoreDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmBombBlockRestoreDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs")]),
    ];
}
