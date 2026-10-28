namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Complete production room-layout installation, separate from explicitly partial fixtures.</summary>
internal static class RoomLayoutClosedContractDefinitions
{
    /// <summary>Requires the production room visual layout to install its complete compiled source set before resolving a room.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomLevelArtworks", "room-layout-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomLevelArtworks.cs"),
             new("csharp/src/SuperMetroid.Core/World/RoomStates/RoomVisualLayoutSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/World/RoomStates/RoomStateDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/World/RoomStates/SelectedRoom.cs")]),
    ];
}
