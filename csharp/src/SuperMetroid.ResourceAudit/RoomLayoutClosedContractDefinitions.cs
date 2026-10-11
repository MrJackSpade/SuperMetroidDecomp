namespace SuperMetroid.ResourceAudit;

/// <summary>Complete production room-layout installation, separate from explicitly partial fixtures.</summary>
internal static class RoomLayoutClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog", "room-layout-complete-required-source-installation", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs", "33DF05BD05F6BAE2025D879EB2F8A0CDF53E04A3D0188958BE8A9FEC9D93666A"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutSourceDefinitions.cs", "7565C39F5976EADE518FEB6642B1342B23FFE3A16EDBB445CC602548D5DEA241"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "A81815A52EF266CE17FB4EE569D9E07B1A0F67D47F0F914591E5D5FD0E92B159"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "8A23BCAE4F3DC70AAF97817F3E7BD5A12C89B320477BAEB99CFDFCB4827E1378")]),
    ];
}
