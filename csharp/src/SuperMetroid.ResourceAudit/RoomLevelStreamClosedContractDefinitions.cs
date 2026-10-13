namespace SuperMetroid.ResourceAudit;

/// <summary>Complete stock-only installed room-level allocations keyed by the retail level sources.</summary>
internal static class RoomLevelStreamClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomLevelStreamCatalog", "room-level-stream-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreamCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs")]),
    ];
}
