namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>The complete stock-only installed terrain of every room level, keyed by level.</summary>
internal static class RoomLevelStreamClosedContractDefinitions
{
    /// <summary>The closed-provider contract for the installed room-level stream catalog.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.LevelData.InstalledRoomLevelStreams", "room-level-stream-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/LevelData/InstalledRoomLevelStreams.cs"),
             new("csharp/src/SuperMetroid.Core/World/RoomStates/RoomVisualLayoutSourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/World/RoomStates/RoomStateDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/World/RoomStates/SelectedRoom.cs")]),
    ];
}
