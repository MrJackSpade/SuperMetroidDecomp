namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    /// <summary>Stores shared draw shape.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs");

    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDownwardGateVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/DownwardGatePlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmElevatorPlatformVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/ElevatorPlatformPlmDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDraygonCannonVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmChozoStatueVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/ChozoStatuePlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombBlockRestoreDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs")]),
    ];
}
