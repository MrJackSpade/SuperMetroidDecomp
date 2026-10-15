namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Complete stock-only installed room-level allocations keyed by the retail level sources.</summary>
internal static class RoomLevelStreamClosedContractDefinitions
{
    /// <summary>The closed-provider contract for the installed room-level stream catalog.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.LevelData.RoomLevelStreamCatalog", "room-level-stream-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomLevelStreamCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs")]),
    ];
}
