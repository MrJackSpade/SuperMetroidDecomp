namespace SuperMetroid.ResourceAudit;

/// <summary>Complete production room-layout installation, separate from explicitly partial fixtures.</summary>
internal static class RoomLayoutClosedContractDefinitions
{
    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog", "room-layout-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs")]),
    ];
}
